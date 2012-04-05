using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.Text;

namespace Voice_Changer
{
    public partial class MainPage : PhoneApplicationPage
    {

        #region Delcared Variables
        Microphone microphone = Microphone.Default;             // Object representing the physical microphone on the device
        byte[] buffer;                                          // Dynamic buffer to retrieve audio data from the microphone
        private MemoryStream stream = new MemoryStream();       // Stores the audio data for later playback
        private SoundEffectInstance soundInstance;              // Used to play back audio
        private bool soundIsPlaying = false;                    // Flag to monitor the state of sound playback
        private bool canvasOpen = false;                        // true if save canvas dialogue is open
        private double pitch;                                   // Assigns pitch to the soundInstance
        private string fileName;                                // fileName used when saving recordings

        // Status images
        private BitmapImage microphoneImage;
        private BitmapImage speakerImage;
        private BitmapImage darkVader;
        private BitmapImage lightVader;
        private BitmapImage darkMouse;
        private BitmapImage lightMouse;
        private BitmapImage mouth;
        #endregion

        #region Constructor
        public MainPage()
        {
            InitializeComponent();

            // Monitors state of audio playback so we can update the UI.
            DispatcherTimer dt = new DispatcherTimer();
            dt.Interval = TimeSpan.FromMilliseconds(33);
            dt.Tick += new EventHandler(dt_Tick);
            dt.Start();

            // Event handler for getting audio data when the buffer is full
            microphone.BufferReady += new EventHandler<EventArgs>(microphone_BufferReady);

            microphoneImage = new BitmapImage(new Uri("Images/microphone.png", UriKind.RelativeOrAbsolute));
            speakerImage = new BitmapImage(new Uri("Images/speaker.png", UriKind.RelativeOrAbsolute));
            darkVader = new BitmapImage(new Uri("Images/darkvader.png", UriKind.RelativeOrAbsolute));
            lightVader = new BitmapImage(new Uri("Images/lightvader.png", UriKind.RelativeOrAbsolute));
            darkMouse = new BitmapImage(new Uri("Images/darkmouse.png", UriKind.RelativeOrAbsolute));
            lightMouse = new BitmapImage(new Uri("Images/lightmouse.png", UriKind.RelativeOrAbsolute));
            mouth = new BitmapImage(new Uri("Images/mouth.png", UriKind.RelativeOrAbsolute));

            Visibility v = (Visibility)Resources["PhoneLightThemeVisibility"];
            if (v == System.Windows.Visibility.Visible)
            {
                // Light theme
                vaderImage.Source = lightVader;
                mouseImage.Source = lightMouse;
            }
            else
            {
                // Dark theme
                vaderImage.Source = darkVader;
                mouseImage.Source = darkMouse;
            }
        }
        #endregion

        #region Updaters
        // Updates the XNA FrameworkDispatcher and checks to see if a sound is playing.
        void dt_Tick(object sender, EventArgs e)
        {
            try { FrameworkDispatcher.Update(); }
            catch { }

            if (true == soundIsPlaying)
            {
                if (soundInstance.State != SoundState.Playing)
                {
                    soundIsPlaying = false;

                    UserHelp.Text = "press play";
                    SetButtonStates(true, true, false, true);
                    StatusImage.Source = mouth;
                }
            }
        }

        /// The Microphone.BufferReady event handler.
        /// Gets the audio data from the microphone and stores it in a buffer,
        /// then writes that buffer to a stream for later playback.
        void microphone_BufferReady(object sender, EventArgs e)
        {
            microphone.GetData(buffer);
            stream.Write(buffer, 0, buffer.Length);
        }

        //Just to make sure mic stops recording and sound stops playing when navigating away
        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            try
            {
                if (microphone.State == MicrophoneState.Started)
                {
                    // In RECORD mode
                    microphone.Stop();
                }
                else if (soundInstance.State == SoundState.Playing)
                {
                    // In PLAY mode
                    soundInstance.Stop();
                }
            }
            catch //in case the user hasn't played any sounds yet, soundInstance would return an error.
            { }
        }
        #endregion

        #region Record, Play, Stop, Save, Cancel
        private void recordButton_Click(object sender, EventArgs e)
        {
            microphone.BufferDuration = TimeSpan.FromMilliseconds(500);
            buffer = new byte[microphone.GetSampleSizeInBytes(microphone.BufferDuration)]; //Allocate memory to hold the audio data

            // Set the stream back to zero in case there is already something in it
            stream.SetLength(0);
            microphone.Start();

            SetButtonStates(false, false, true, false);
            UserHelp.Text = "recording";
            StatusImage.Source = microphoneImage;
        }

        private void stopButton_Click(object sender, EventArgs e)
        {
            if (microphone.State == MicrophoneState.Started)
            {
                // In RECORD mode
                microphone.Stop();
            }
            else if (soundInstance.State == SoundState.Playing)
            {
                // In PLAY mode
                soundInstance.Stop();
            }
            SetButtonStates(true, true, false, true);
            UserHelp.Text = "press play";
            StatusImage.Source = mouth;
        }

        private void playButton_Click(object sender, EventArgs e)
        {
            if (stream.Length > 0)
            {
                // Update the UI to reflect that
                // sound is playing
                SetButtonStates(false, false, true, false);
                UserHelp.Text = "playing";
                StatusImage.Source = speakerImage;
                pitch = (float)pitchSlider.Value;

                // Play the audio in a new thread so the UI can update.
                Thread soundThread = new Thread(new ThreadStart(playSound));
                soundThread.Start();
            }
            else
            {
                MessageBox.Show("Please record something before pressing play.");
            }
        }

        private void playSound()
        {
            SoundEffect sound = new SoundEffect(stream.ToArray(), microphone.SampleRate, AudioChannels.Mono);
            soundInstance = sound.CreateInstance();
            soundIsPlaying = true;
            soundInstance.Pitch = (float)pitch;
            soundInstance.Play();
        }

        private void saveButton_Click(object sender, EventArgs e)
        {
            saveCanvas.Visibility = System.Windows.Visibility.Visible;
            SetButtonStates(false, false, false, false);
            helpButton.IsEnabled = false;
            openButton.IsEnabled = false;
            canvasOpen = true;
        }

        private void saveButton2_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder date = new StringBuilder();
            date.Append(DateTime.Now.Year);
            date.Append("_");
            date.Append(String.Format("{0:00}", DateTime.Now.Month));
            date.Append("_");
            date.Append(String.Format("{0:00}", DateTime.Now.Day));
            date.Append("_");
            date.Append(String.Format("{0:00}", DateTime.Now.Hour));
            date.Append("_");
            date.Append(String.Format("{0:00}", DateTime.Now.Minute));
            date.Append("_");
            date.Append(String.Format("{0:00}", DateTime.Now.Second));
            pitch = pitchSlider.Value + 1; //I added 1 to avoid dealing with negative pitch values. (subtract 1 when re-assigning pitch)
            fileName = date.ToString() + pitch.ToString("0.0000") + FileNameTextBox.Text + ".wav";
            save(fileName);
            
            FileNameTextBox.Text = "";
            SetButtonStates(true, true, true, true);
            helpButton.IsEnabled = true;
            openButton.IsEnabled = true;
            saveCanvas.Visibility = System.Windows.Visibility.Collapsed;
            canvasOpen = false;
        }

        private void save(string fileName)
        {
            //saves the sound to isolated storage, with the selected pitch value and date created
            var appStorage = IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                using (var file = new IsolatedStorageFileStream(fileName, FileMode.Create, appStorage))
                {
                    file.Write(stream.ToArray(), 0, stream.ToArray().Length);
                }
            }
            catch
            {
                MessageBox.Show("Error saving the file. Try using only letters and numbers.");
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            cancel();
        }

        private void cancel()
        {
            saveCanvas.Visibility = System.Windows.Visibility.Collapsed;
            FileNameTextBox.Text = "";
            SetButtonStates(true, true, true, true);
            helpButton.IsEnabled = true;
            openButton.IsEnabled = true;
            canvasOpen = false;
        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        { // overrides the back button if the save canvas dialogue is open, so it closes the canvas instead of the application
            if (canvasOpen == true)
            {
                cancel();
                e.Cancel = true;
            }
            else if (canvasOpen == false)
            {
                base.OnBackKeyPress(e);
            }
        }
        #endregion

        #region Help, Open buttons
        private void helpButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Help.xaml", UriKind.RelativeOrAbsolute));
        }

        private void openButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Open.xaml", UriKind.RelativeOrAbsolute));
        }
        #endregion

        #region ButtonStates
        private void SetButtonStates(bool recordEnabled, bool playEnabled, bool stopEnabled, bool saveEnabled)
        {
            (ApplicationBar.Buttons[0] as ApplicationBarIconButton).IsEnabled = recordEnabled;
            (ApplicationBar.Buttons[1] as ApplicationBarIconButton).IsEnabled = playEnabled;
            (ApplicationBar.Buttons[2] as ApplicationBarIconButton).IsEnabled = stopEnabled;
            (ApplicationBar.Buttons[3] as ApplicationBarIconButton).IsEnabled = saveEnabled;
        }
        #endregion

        #region Slider Buttons
        private void HyperlinkButton_Click1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            pitchSlider.Value = -1;
        }

        private void HyperlinkButton_Click2(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            pitchSlider.Value = 1;
        }
        #endregion
    }
}