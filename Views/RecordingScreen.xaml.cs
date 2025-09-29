using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EncoderApp.Views
{
    /// <summary>
    /// Interaction logic for RecordingScreen.xaml
    /// </summary>
    public partial class RecordingScreen : System.Windows.Controls.UserControl
    {
        public RecordingScreen()
        {
            InitializeComponent();
        }
        private void ComboBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is System.Windows.Controls.ComboBox comboBox)
            {
                comboBox.Focus();
                comboBox.IsDropDownOpen = true;
                e.Handled = true;
            }

        }
        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select folder to save recordings";
                dialog.ShowNewFolderButton = true;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    MyFolderPathTextBox.Text = dialog.SelectedPath;
                }
            }
        }
        private void IncrementMinutes_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MinutesTextBox.Text, out int value))
                MinutesTextBox.Text = (value + 1).ToString();
        }

        private void DecrementMinutes_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(MinutesTextBox.Text, out int value) && value > 0)
                MinutesTextBox.Text = (value - 1).ToString();
        }
       
            private void IncreaseValue1(object sender, RoutedEventArgs e)
            {
                var textBox = (System.Windows.Controls.TextBox)this.FindName("textBox1");
                if (textBox != null)
                {
                    if (int.TryParse(textBox.Text, out int value))
                    {
                        textBox.Text = (value + 1).ToString();
                    }
                }
            }

            private void DecreaseValue1(object sender, RoutedEventArgs e)
            {
                var textBox = (System.Windows.Controls.TextBox)this.FindName("textBox1");
                if (textBox != null)
                {
                    if (int.TryParse(textBox.Text, out int value) && value > 0)
                    {
                        textBox.Text = (value - 1).ToString();
                    }
                }
            }

        private void Customize_Click(object sender, RoutedEventArgs e)
        {
            EncoderSettingModal.Visibility = Visibility.Visible;
        }
        private void EncoderSettingOK_Click(object sender, RoutedEventArgs e)
        {
            EncoderSettingModal.Visibility = Visibility.Collapsed;
        }
        private void EncoderSettingCancel_Click(object sender, RoutedEventArgs e)
        {
            EncoderSettingModal.Visibility = Visibility.Collapsed;
        }
        private void EncoderSettings_CloseClicked(object sender, RoutedEventArgs e)
        {
            EncoderSettingModal.Visibility = Visibility.Collapsed;
        }
        //private void IncreaseValue2(object sender, RoutedEventArgs e)
        //{
        //    var textBox = (TextBox)this.FindName("textBox2");
        //    if (textBox != null)
        //    {
        //        if (int.TryParse(textBox.Text, out int value))
        //        {
        //            textBox.Text = (value + 1).ToString();
        //        }
        //    }
        //}

        //private void DecreaseValue2(object sender, RoutedEventArgs e)
        //{
        //    var textBox = (TextBox)this.FindName("textBox2");
        //    if (textBox != null)
        //    {
        //        if (int.TryParse(textBox.Text, out int value) && value > 0)
        //        {
        //            textBox.Text = (value - 1).ToString();
        //        }
        //    }
        //}
    }
}
