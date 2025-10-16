using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EncoderApp.ViewModels;

namespace EncoderApp.Views
{
    public partial class Streams : UserControl
    {
        private readonly StreamsViewModel viewModel;

        public event RoutedEventHandler CloseClicked;

        public Streams()
        {
            InitializeComponent();
            viewModel = new StreamsViewModel();
            DataContext = viewModel;
            viewModel.CloseRequested += ViewModel_CloseRequested;
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
        private void ViewModel_CloseRequested(object sender, EventArgs e)
        {
            CloseClicked?.Invoke(this, new RoutedEventArgs());
            Visibility = Visibility.Collapsed;
        }

        private void Header_CloseClicked(object sender, RoutedEventArgs e)
        {
            viewModel.Close();
        }

        private void StreamsOK_Click(object sender, RoutedEventArgs e)
        {
            viewModel.SaveAndClose();
        }

        private void StreamsCancel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CancelNewStream();
            viewModel.Close();
        }

        private void Advanced_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ShowStreamsMetaDataModal();
        }

        private void Customize_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ShowEncoderSettingModal();
        }

        private void TestStream_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ShowStreamDiagnosticModal();
        }

        private void MetaDataOK_Click(object sender, RoutedEventArgs e)
        {
            viewModel.HideStreamsMetaDataModal();
        }

        private void MetaDataCancel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.HideStreamsMetaDataModal();
        }

        private void EncoderSettingOK_Click(object sender, RoutedEventArgs e)
        {
            viewModel.HideEncoderSettingModal();
        }

        private void EncoderSettingCancel_Click(object sender, RoutedEventArgs e)
        {
            viewModel.HideEncoderSettingModal();
        }

        private void MetaDataStream_CloseClicked(object sender, RoutedEventArgs e)
        {
            viewModel.HideStreamsMetaDataModal();
        }

        private void EncoderSettings_CloseClicked(object sender, RoutedEventArgs e)
        {
            viewModel.HideEncoderSettingModal();
        }

        private void StreamDiagnostic_CloseClicked(object sender, RoutedEventArgs e)
        {
            viewModel.HideStreamDiagnosticModal();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            viewModel.HideStreamDiagnosticModal();
        }

        private void Retry_Click(object sender, RoutedEventArgs e)
        {
            viewModel.ShowStreamDiagnosticModal();
        }

        private void AddNewStream_Click(object sender, RoutedEventArgs e)
        {
            viewModel.AddNewStream();
        }

        private void RemoveStream_Click(object sender, RoutedEventArgs e)
        {
            viewModel.RemoveStream();
        }

        private void CloneStream_Click(object sender, RoutedEventArgs e)
        {
            viewModel.CloneStream();
        }

        private void IncrementPort_Click(object sender, RoutedEventArgs e)
        {
            viewModel.IncrementPort();
        }

        private void DecrementPort_Click(object sender, RoutedEventArgs e)
        {
            viewModel.DecrementPort();
        }

        private void StreamNameTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            viewModel.StreamNameGotFocus();
        }

        private void StreamNameTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            viewModel.StreamNameLostFocus();
        }
        private void HostNameTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            viewModel.HostNameGotFocus();
        }

        private void HostNameTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            viewModel.HostNameLostFocus();
        }
        private void MountTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            viewModel.MountGotFocus();
        }

        private void MountTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            viewModel.MountLostFocus();
        }
        private void UserNameTxt_GotFocus(object sender, RoutedEventArgs e)
        {
            viewModel.UserNameGotFocus();
        }

        private void UserNameTxt_LostFocus(object sender, RoutedEventArgs e)
        {
            viewModel.UserNameLostFocus();
        }
        private void Pass_GotFoucs(object sender, RoutedEventArgs e)
        {
            viewModel.PassGotFoucs();
        }

        private void Pass_LostFocus(object sender, RoutedEventArgs e)
        {
            viewModel.PassLostFocus();
        }
        
      

        private void StreamsMetaDataModalHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.ModalHeaderMouseLeftButtonDown(sender, e, "StreamsMetaDataModal");
        }

        private void StreamsMetaDataModalHeader_MouseMove(object sender, MouseEventArgs e)
        {
            viewModel.ModalHeaderMouseMove(sender, e, "StreamsMetaDataModal");
        }

        private void EncoderSettingModalHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.ModalHeaderMouseLeftButtonDown(sender, e, "EncoderSettingModal");
        }

        private void EncoderSettingModalHeader_MouseMove(object sender, MouseEventArgs e)
        {
            viewModel.ModalHeaderMouseMove(sender, e, "EncoderSettingModal");
        }

        private void StreamDiagnosticModalHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.ModalHeaderMouseLeftButtonDown(sender, e, "StreamDiagnosticModal");
        }

        private void StreamDiagnosticModalHeader_MouseMove(object sender, MouseEventArgs e)
        {
            viewModel.ModalHeaderMouseMove(sender, e, "StreamDiagnosticModal");
        }

        private void ModalHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            viewModel.ModalHeaderMouseLeftButtonUp(sender, e);
        }

        private void StreamsMetaDataModal_MouseDown(object sender, MouseButtonEventArgs e)
        {
            viewModel.StreamsMetaDataModalMouseDown(sender, e);
        }

        private void StreamsMetaDataModal_KeyDown(object sender, KeyEventArgs e)
        {
            viewModel.StreamsMetaDataModalKeyDown(sender, e);
        }
    }
}