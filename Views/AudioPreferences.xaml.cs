using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EncoderApp.Views
{
    /// <summary>
    /// Interaction logic for AudioPreferences.xaml
    /// </summary>
    public partial class AudioPreferences : UserControl
    {
        public AudioPreferences()
        {
            InitializeComponent();
            MainContent.Content = new AudioDevicesScreen();
        }

        private void AudioDevices_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new AudioDevicesScreen();
        }

        private void Recording_Click(object sender, RoutedEventArgs e)
        {
            MainContent.Content = new RecordingScreen();
        }

       

        //private void Networking_Click(object sender, RoutedEventArgs e)
        //{
        //    MainContent.Content = new NetworkingScreen();
        //}
    }
}
