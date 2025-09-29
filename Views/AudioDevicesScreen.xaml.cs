using EncoderApp.ViewModels;
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
    /// Interaction logic for AudioDevicesScreen.xaml
    /// </summary>
    public partial class AudioDevicesScreen : UserControl
    {
        private readonly AudioDevicesScreenViewModel viewModel;
        public AudioDevicesScreen()
        {
            InitializeComponent();
            viewModel = new AudioDevicesScreenViewModel();
            this.DataContext = viewModel;
        }
        private void ComboBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                var popup = comboBox.Template.FindName("Popup", comboBox) as System.Windows.Controls.Primitives.Popup;
                if (popup != null && popup.IsOpen)
                {
                    if (popup.IsMouseOver)
                        return;
                    e.Handled = true;
                    comboBox.IsDropDownOpen = false;
                    return;
                }

                if (!comboBox.IsDropDownOpen)
                {
                    e.Handled = true;
                    comboBox.Focus();
                    comboBox.IsDropDownOpen = true;
                }
            }
        }
    }
}
