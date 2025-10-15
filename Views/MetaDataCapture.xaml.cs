using EncoderApp.Models;
using EncoderApp.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EncoderApp.Views
{
    /// <summary>
    /// Interaction logic for MetaDataCapture.xaml
    /// </summary>
    public partial class MetaDataCapture : UserControl
    {
        private readonly MetaDataViewModel viewModel;
        private ObservableCollection<RuleModel> _rules = new ObservableCollection<RuleModel>();

        public event RoutedEventHandler CloseClicked;
        public event RoutedEventHandler OkClicked;
        public event RoutedEventHandler CancelClicked;
        private Point _startMousePosition;
        private bool _isDragging = false;
        public MetaDataCapture()
        {
            InitializeComponent();
            viewModel = new MetaDataViewModel();
            this.DataContext = viewModel;
            this.Loaded += (s, e) => viewModel.Initialize();

            RuleList.ItemsSource = _rules;
            MetadataSettingsPanel.Opacity = 0.5;
        }
        #region Checkbox & RadioButton Handlers

        private void chkCustomizeFormatting_Checked(object sender, RoutedEventArgs e)
        {
            MetadataSettingsPanel.IsEnabled = true;
            MetadataSettingsPanel.Opacity = 1;
        }

        private void chkCustomizeFormatting_Unchecked(object sender, RoutedEventArgs e)
        {
            MetadataSettingsPanel.IsEnabled = false;
            MetadataSettingsPanel.Opacity = 0.5;
        }
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && DataContext is MetaDataViewModel viewModel)
            {
                viewModel.SelectedFormat = rb.Name;
                if (rbCustom == null || txtCustom == null) return;

                if (rbCustom.IsChecked == true)
                {
                    txtCustom.IsEnabled = true;
                    txtCustom.Opacity = 1;
                }
                else
                {
                    txtCustom.IsEnabled = false;
                    txtCustom.Opacity = 0.5;
                }

                viewModel.UpdateMetadata();
            }
        }
        private void RadioButton_Unchecked(object sender, RoutedEventArgs e)
        {
            
            if (rbCustom.IsChecked != true) { 
                txtCustom.IsEnabled = true;
                txtCustom.Opacity = 0.5;
            }
        }
        private void RBOuputFormat_Checked(object sender, RoutedEventArgs e)
        {
            if (CustomRBtn == null || CustomRBtn == null) return;

            if (CustomRBtn.IsChecked == true)
            {
                CustomTxtOutput.IsEnabled = true;
                CustomTxtOutput.Opacity = 1;
            }
            else
            {
                CustomTxtOutput.IsEnabled = false;
                CustomTxtOutput.Opacity = 0.5;
            }
        }
        private void RBOuputFormat_Unchecked(object sender, RoutedEventArgs e)
        {

            if (CustomRBtn.IsChecked != true)
            {
                CustomTxtOutput.IsEnabled = true;
                CustomTxtOutput.Opacity = 0.5;
            }
        }
        #endregion
        #region Rule Management
        private void AddNewRule_Click(object sender, RoutedEventArgs e)
        {
            _rules.Add(new RuleModel
            {
                IfValue = "MP3",
                IsValue = "",
                ThenValue = "Replace metadata"
            });
        }

        private void RemoveRule_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is RuleModel rule)
                _rules.Remove(rule);
        }
        #endregion
        #region ComboBox Handling
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
        #endregion
        #region Header
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
        #endregion
        #region Buttons
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

        private void Advanced_Click(object sender, RoutedEventArgs e)
        {
            MetadataRemappingModal.Visibility = Visibility.Visible;
        }
        #endregion

        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            viewModel.LoadActiveApps();
        }
    }

}
