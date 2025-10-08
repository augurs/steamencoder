using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EncoderApp.Views
{
    /// <summary>
    /// Interaction logic for MetaDataCapture.xaml
    /// </summary>
    public partial class MetaDataCapture : UserControl
    {
        public event RoutedEventHandler CloseClicked;
        public event RoutedEventHandler OkClicked;
        public MetaDataCapture()
        {
            InitializeComponent();
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
        private void Header_CloseClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                CloseClicked?.Invoke(this, e);
                this.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        //private void ModalHeader_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    // Only respond to left mouse button
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        // Move the parent window
        //        Window parentWindow = Window.GetWindow(this);
        //        parentWindow?.DragMove();
        //    }
        //}

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OkClicked?.Invoke(this, e);
        }


    }
}
