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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EncoderApp.Views
{
    /// <summary>
    /// Interaction logic for Streams.xaml
    /// </summary>
    public partial class Streams : UserControl
    {
        public Streams()
        {
            InitializeComponent();
        }
        private void ComboBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.Focus();
                comboBox.IsDropDownOpen = true;
                e.Handled = true;
            }

        }
        private void StreamNameTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            if (StreamNameTxt.Text == "My Radio Station")
            {
                StreamNameTxt.Text = "";
                StreamNameTxt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#94a3b8"));
            }
        }
        private void StreamNameTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(StreamNameTxt.Text))
            {
                StreamNameTxt.Text = "My Radio Station";
                StreamNameTxt.Foreground = Brushes.LightGray;
            }
        }

        private void Advanced_Click(object sender, RoutedEventArgs e)
        {
            StreamsMetaDataModal.Visibility = Visibility.Visible;
        }
        private void Customize_Click(object sender, RoutedEventArgs e)
        {
            EncoderSettingModal.Visibility = Visibility.Visible;
        }
        
        private void MetaDataStream_CloseClicked(object sender, RoutedEventArgs e)
        {
            StreamsMetaDataModal.Visibility = Visibility.Collapsed;
        } 
        private void EncoderSettings_CloseClicked(object sender, RoutedEventArgs e)
        {
            EncoderSettingModal.Visibility = Visibility.Collapsed;
        }
        private void StreamDiagnostic_CloseClicked(object sender, RoutedEventArgs e)
        {
            StreamDiagnosticModal.Visibility = Visibility.Collapsed;
        }
        
        private void StreamsMetaDataModal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource == StreamsMetaDataModal)
            {
                StreamsMetaDataModal.Visibility = Visibility.Collapsed;
            }
        }
        private void StreamsMetaDataModal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                StreamsMetaDataModal.Visibility = Visibility.Collapsed;
            }
        }

        private void TestStream_Click(object sender, RoutedEventArgs e)
        {
            StreamDiagnosticModal.Visibility = Visibility.Visible;
        }
        private void MetaDataCancel_Click(object sender, RoutedEventArgs e)
        {
            StreamsMetaDataModal.Visibility = Visibility.Collapsed;
        }
        private void MetaDataOK_Click(object sender, RoutedEventArgs e)
        {
            StreamsMetaDataModal.Visibility = Visibility.Collapsed;
        }

        private void EncoderSettingOK_Click(object sender, RoutedEventArgs e)
        {
            EncoderSettingModal.Visibility = Visibility.Collapsed;
        }
        private void EncoderSettingCancel_Click(object sender, RoutedEventArgs e)
        {
            EncoderSettingModal.Visibility = Visibility.Collapsed;
        }
        
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            StreamDiagnosticModal.Visibility = Visibility.Collapsed;
        }

        private void IncrementPort_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(portTextBox.Text, out int port))
            {
                port++; 
                portTextBox.Text = port.ToString();
            }
        }

        private void DecrementPort_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(portTextBox.Text, out int port))
            {
                port--; 
                portTextBox.Text = port.ToString();
            }
        }

    }
}
