using EncoderApp.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EncoderApp.Views
{
    /// <summary>
    /// Interaction logic for MetadataRemapping.xaml
    /// </summary>
    public partial class MetadataRemapping : UserControl
    {
        public event RoutedEventHandler CloseClicked;

        public MetadataRemapping()
        {
            InitializeComponent();
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
        private Point _startMousePosition;
        private bool _isDragging = false;
        private void ModalHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _startMousePosition = e.GetPosition(null);
                (sender as UIElement).CaptureMouse();
            }
        }
        private void ModalHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentMousePosition = e.GetPosition(null);
                Vector delta = currentMousePosition - _startMousePosition;

                RootTransform.X += delta.X;
                RootTransform.Y += delta.Y;

                _startMousePosition = currentMousePosition;
            }
        }
        private void ModalHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement).ReleaseMouseCapture();
        }


        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CloseClicked?.Invoke(this, e);
                this.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                CloseClicked?.Invoke(this, e);
                this.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }

        }
    }
}
