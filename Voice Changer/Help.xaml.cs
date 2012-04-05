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
using Microsoft.Phone.Tasks;

namespace Voice_Changer
{
    public partial class Help : PhoneApplicationPage
    {
        public Help()
        {
            InitializeComponent();
        }

        private void emailMe()
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "boredjonproductions@gmail.com";
            emailComposeTask.Subject = "Voice changer application support";
            emailComposeTask.Body = "";
            emailComposeTask.Show();
        }

        private void startpageButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }

        private void hyperlinkButton1_Click(object sender, RoutedEventArgs e)
        {
            emailMe();
        }
    }
}