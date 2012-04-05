using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System.IO.IsolatedStorage;
using System.IO;

namespace Voice_Changer
{
    public partial class Open : PhoneApplicationPage
    {
        private byte[] buffer;
        private SoundEffectInstance soundInstance;
        private float pitch;
        private string fileName;
        private string newfileName;
        
        public Open()
        {
            InitializeComponent();
            bindList();
        }

        #region Loading/Deleting recordings
        private void bindList()
        {
            var appStorage = IsolatedStorageFile.GetUserStoreForApplication();

            try
            {
                string[] fileList = appStorage.GetFileNames();
                List<Recording> recordings = new List<Recording>();
                foreach (string file in fileList)
                {
                    // Retrieve the file
                    string fullfileName = file;
                    // Pluck out the date parts
                    string year = file.Substring(0, 4);
                    string month = file.Substring(5, 2);
                    string day = file.Substring(8, 2);
                    string hour = file.Substring(11, 2);
                    string minute = file.Substring(14, 2);
                    string second = file.Substring(17, 2);
                    //get pitch
                    string pitch = file.Substring(19, 6);
                    //get the "actual" file name
                    string shortfileName = file.Substring(25, file.Length - 29);

                    // Create a new DateTime object
                    DateTime dateCreated = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), int.Parse(second));

                    recordings.Add(new Recording() { FileName = shortfileName, fullFileName = fullfileName, DateCreated = dateCreated.ToLongDateString(), Pitch = pitch }); //
                }
                recordingListBox.ItemsSource = recordings;
            }
            catch
            {
                MessageBox.Show("There was an error loading the file names.");
            }
        }

        private void recordingFileName_Click(object sender, RoutedEventArgs e)
        {
            //finds the clicked file name
            HyperlinkButton clickedLink = (HyperlinkButton)sender;
            string fileName = clickedLink.Tag.ToString();

            //opens the and plays the file with pitch
            IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication();
            using (var file = new IsolatedStorageFileStream(fileName, FileMode.Open, appStorage))
            {
                buffer = new byte[file.Length];
                file.Read(buffer, 0, (int)file.Length);
                SoundEffect sound = new SoundEffect(buffer, 16000, AudioChannels.Mono);
                if (newPitchCheckBox.IsChecked == true)
                {
                    pitch = (float)pitchSlider.Value;
                }
                else
                {
                    pitch = Convert.ToSingle(fileName.Substring(19, 6)) - 1;
                }
                soundInstance = sound.CreateInstance();
                soundInstance.Pitch = pitch;
                soundInstance.Play();
            }
        }

        private void OnMenuClickedDelete(object sender, RoutedEventArgs e)
        {
            //finds the clicked file name
            MenuItem clickedLink = (MenuItem)sender;
            string fileName = clickedLink.Tag.ToString();

            //deletes file
            IsolatedStorageFile appStorage = IsolatedStorageFile.GetUserStoreForApplication();
            appStorage.DeleteFile(fileName);

            //refreshes list
            bindList();
        }

        private void OnMenuClickedRename(object sender, RoutedEventArgs e)
        {
            //finds the clicked file name
            MenuItem clickedLink = (MenuItem)sender;
            fileName = clickedLink.Tag.ToString();
            FileNameTextBox.Text = fileName.Substring(25, fileName.Length - 29);
            saveCanvas.Visibility = System.Windows.Visibility.Visible;
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            saveCanvas.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            newfileName = fileName.Substring(0, 25) + FileNameTextBox.Text + ".wav";
            FileNameTextBox.Text = "";
            renameFile();
            saveCanvas.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void renameFile()
        {
            //opens file and writes it to a new file with the new name, then deletes old file. Only way to "rename" for now
            var appStorage = IsolatedStorageFile.GetUserStoreForApplication();

            if (fileName != newfileName)
            {
                using (var file = new IsolatedStorageFileStream(fileName, FileMode.Open, appStorage))
                {
                    buffer = new byte[file.Length];
                    file.Read(buffer, 0, (int)file.Length);
                    using (var newfile = new IsolatedStorageFileStream(newfileName, FileMode.Create, appStorage))
                    {
                        newfile.Write(buffer, 0, buffer.Length);
                    }
                }
                appStorage.DeleteFile(fileName);
                bindList();
            }
            else
            {
            }
        }
        #endregion

        #region Navigation
        private void startpageButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
        #endregion
    }
}