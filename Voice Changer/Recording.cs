using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Voice_Changer
{
    public class Recording
    {
        public string FileName { get; set; }
        public string fullFileName { get; set; }
        public string DateCreated { get; set; }
        public string Pitch { get; set; }
    }
}
