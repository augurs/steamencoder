using EncoderApp.Models;
using EncoderApp.Services;
using EncoderApp.ViewModels;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;  

namespace EncoderApp.Views
{
    public partial class MainWindow : Window
    {
        private AudioDevicesScreenViewModel _audioDevicesViewModel;
        public ObservableCollection<StreamInfo> Streams { get; set; } = new ObservableCollection<StreamInfo>();

        private bool _isCustomMaximized = false;
        private Rect _restoreBounds;

        private const int NumBlocks = 10;
        private Rectangle[] audioInputBlocks;
        private Rectangle[] otherAppBlocks;
        private Rectangle[] masterBlocks;

        private double audioInputSliderValue = 50;
        private double otherAppsSliderValue = 50;
        private bool _isMicMuted = false;

        private double _previousAudioInputSliderValue = 50;
        private bool _isOtherAppsMuted = false;
        private bool _isMasterMuted = false;
        private DateTime _lastMicUpdate = DateTime.MinValue;
        private DateTime _lastAppUpdate = DateTime.MinValue;
        private DateTime _lastSystemAudioUpdate = DateTime.MinValue;

        public MainWindow()
        {
            try
            {
                InitializeComponent();
                DataContext = this;

                _audioDevicesViewModel = (Preferences.DataContext as AudioDevicesScreenViewModel) ?? new AudioDevicesScreenViewModel();
                _audioDevicesViewModel.PropertyChanged += AudioDevicesViewModel_PropertyChanged;
                StartClock();
            
                ArtistTextBox.TextChanged += (s, e) => MetaDataLoad();
                TrackTitleTextBox.TextChanged += (s, e) => MetaDataLoad();
                MetaDataLoad();
                UpdateAudioDbText(audioInputSliderValue);
                UpdateOtherAppsDbText(otherAppsSliderValue);
                Streams.Clear();
                AppConfigurationManager.LoadStreamsIntoCollection(Streams);
                CreateBlocks();
                InitAudio();
                OtherAppAudioCaptureService.Instance.OnVolumeChanged += OnSystemVolumeChanged;
                AudioInputCaptureService.Instance.OnError += OnAudioInputError;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        private void OnSystemVolumeChanged(float level)
        {
            if ((DateTime.Now - _lastSystemAudioUpdate).TotalMilliseconds < 50) return;
            _lastSystemAudioUpdate = DateTime.Now;

            if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) return;

            float normalizedLevel = Math.Clamp(level / 100f, 0f, 1f);
            float scaledLevel = normalizedLevel * (float)(otherAppsSliderValue / 100.0);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (!_isOtherAppsMuted && _audioDevicesViewModel.EnableSystemAudioCapture)
                {
                    UpdateVUMeter(OtherApplicationsVU, scaledLevel);
                    if (!_isMasterMuted)
                        UpdateVUMeter(MasterVU, scaledLevel);
                }
            }), DispatcherPriority.Background);  
        }

        private void OnAudioInputError(string msg)
        {
            if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) return;
            Dispatcher.BeginInvoke(new Action(() => CustomMessageBox.ShowInfo(msg, "Error")), DispatcherPriority.Normal);
        }

        private async void AudioDevicesViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_audioDevicesViewModel.EnableSystemAudioCapture))
            {
                Debug.WriteLine($"EnableSystemAudioCapture changed to {_audioDevicesViewModel.EnableSystemAudioCapture}");
                await Task.Run(() =>
                {
                    try
                    {
                        if (!_isOtherAppsMuted && _audioDevicesViewModel.EnableSystemAudioCapture && !_isMasterMuted)
                        {
                            Debug.WriteLine("Starting OtherAppAudioCaptureService");
                            OtherAppAudioCaptureService.Instance.Start();
                        }
                        else
                        {
                            Debug.WriteLine("Stopping OtherAppAudioCaptureService");
                            OtherAppAudioCaptureService.Instance.Stop();
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex);
                        Debug.WriteLine($"Error in AudioDevicesViewModel_PropertyChanged: {ex.Message}");
                        if (!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished)
                            Dispatcher.BeginInvoke(new Action(() => CustomMessageBox.ShowInfo($"Error changing system audio capture: {ex.Message}", "Error")));
                    }
                });
            }
        }

        #region Header
        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isCustomMaximized)
            {
                _restoreBounds = new Rect(this.Left, this.Top, this.Width, this.Height);
                var workArea = SystemParameters.WorkArea;
                this.Top = workArea.Top;
                this.Left = workArea.Left;
                this.Width = workArea.Width;
                this.Height = workArea.Height;

                _isCustomMaximized = true;
            }
            else
            {
                this.Left = _restoreBounds.Left;
                this.Top = _restoreBounds.Top;
                this.Width = _restoreBounds.Width;
                this.Height = _restoreBounds.Height;

                _isCustomMaximized = false;
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        #region Streams
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is StreamInfo stream)
            {
                bool isConnected = stream.IsConnected;
                streamONText.Text = stream.Name;
                OnAirPanel.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;

                streamOFFText.Text = stream.Name;
                OffAirPanel.Visibility = isConnected ? Visibility.Collapsed : Visibility.Visible;
                Errorpanel.Visibility = Visibility.Collapsed;
                if (stream.IsError == true)
                {
                    OnAirPanel.Visibility = Visibility.Collapsed;
                    OffAirPanel.Visibility = Visibility.Collapsed;
                    Errorpanel.Visibility = Visibility.Visible;
                }
            }
        }
        private void StreamsCancel_Click(object sender, RoutedEventArgs e)
        {
            StreamsModalOverlay.Visibility = Visibility.Collapsed;
        }
        private void StreamsOK_Click(object sender, RoutedEventArgs e)
        {
            StreamsModalOverlay.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Preferences
        private void PreferencesCancel_Click(object sender, RoutedEventArgs e)
        {
            PreferencesModalOverlay.Visibility = Visibility.Collapsed;
        }
        private void PreferencesOK_Click(object sender, RoutedEventArgs e)
        {
            PreferencesModalOverlay.Visibility = Visibility.Collapsed;
        }
        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            PreferencesModalOverlay.Visibility = Visibility.Collapsed;
        }
        private void ok_click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            PreferencesModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            PreferencesModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void Apply_click(object sender, RoutedEventArgs e)
        {
            ApplySettings();
            applyPrefencecsbtn.IsEnabled = false;
            applyPrefencecsbtn.Opacity = 0.5;
        }

        private void ApplySettings()
        {
            var vm = Preferences.DataContext as AudioDevicesScreenViewModel;
            if (vm != null)
            {
                AppConfigurationManager.WriteValue("EnableAudio", vm.EnableAudio.ToString());
                AppConfigurationManager.WriteValue("SelectedPlaybackDevice", vm.SelectedPlaybackDevice ?? "");
                AppConfigurationManager.WriteValue("SelectedAudioApi", vm.SelectedAudioApi);
                AppConfigurationManager.WriteValue("SelectedLatency", vm.SelectedLatency);
                AppConfigurationManager.WriteValue("SelectedInputDevice", vm.SelectedInputDevice ?? "");
                AppConfigurationManager.WriteValue("SelectedInputSampleRate", vm.SelectedInputSampleRate);
                AppConfigurationManager.WriteValue("EnableSystemAudioCapture", vm.EnableSystemAudioCapture.ToString());
                vm.UpdateLatencyEnabled();
                vm.LoadPlaybackDevices();
                vm.AudioInputCapture();
                vm.UpdateSystemAudioCapture();
            }
        }
        #endregion

        #region BroadcastButtons
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StartButton.IsEnabled = false;
            StartButton.Opacity = 0.5;

            StopButton.IsEnabled = true;
            StopButton.Opacity = 1;
        }
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopButton.IsEnabled = false;
            StopButton.Opacity = 0.5;

            StartButton.IsEnabled = true;
            StartButton.Opacity = 1;
        }
        #endregion

        #region About
        private void AboutOK_Click(object sender, RoutedEventArgs e)
        {
            AboutModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void AboutHeader_CloseClicked(object sender, RoutedEventArgs e)
        {
            AboutModalOverlay.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region ModalHeaders
        private Point _startPoint;
        private bool _isDragging = false;

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(null);
            _isDragging = true;
            (sender as UIElement).CaptureMouse();
        }

        private void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging)
            {
                Point currentPoint = e.GetPosition(null);
                double offsetX = currentPoint.X - _startPoint.X;
                double offsetY = currentPoint.Y - _startPoint.Y;

                OverlayTransform.X += offsetX;
                OverlayTransform.Y += offsetY;

                _startPoint = currentPoint;
            }
        }

        private void Header_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            (sender as UIElement).ReleaseMouseCapture();
        }
        private Point _streamsStartPoint;
        private bool _isStreamsDragging = false;

        private Point _aboutStartPoint;
        private bool _isAboutDragging = false;

        private void StreamsHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _streamsStartPoint = e.GetPosition(null);
            _isStreamsDragging = true;
            (sender as UIElement).CaptureMouse();
        }

        private void StreamsHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isStreamsDragging)
            {
                Point currentPoint = e.GetPosition(null);
                double offsetX = currentPoint.X - _streamsStartPoint.X;
                double offsetY = currentPoint.Y - _streamsStartPoint.Y;

                StreamsOverlayTransform.X += offsetX;
                StreamsOverlayTransform.Y += offsetY;

                _streamsStartPoint = currentPoint;
            }
        }

        private void StreamsHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isStreamsDragging = false;
            (sender as UIElement).ReleaseMouseCapture();
        }
        private void AboutHeader_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _aboutStartPoint = e.GetPosition(null);
            _isAboutDragging = true;
            (sender as UIElement).CaptureMouse();
        }

        private void AboutHeader_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isAboutDragging)
            {
                Point currentPoint = e.GetPosition(null);
                double offsetX = currentPoint.X - _aboutStartPoint.X;
                double offsetY = currentPoint.Y - _aboutStartPoint.Y;

                AboutOverlayTransform.X += offsetX;
                AboutOverlayTransform.Y += offsetY;

                _aboutStartPoint = currentPoint;
            }
        }

        private void AboutHeader_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isAboutDragging = false;
            (sender as UIElement).ReleaseMouseCapture();
        }
        #endregion

        private async void StartClock()
        {
            while (!Dispatcher.HasShutdownStarted)
            {
                TimeText.Text = DateTime.Now.ToString("HH:mm:ss");
                await Task.Delay(1000);
            }
        }

        #region Metadata
        private void ArtistTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ArtistTextBox.Text == "Artist")
            {
                ArtistTextBox.Text = "";
                ArtistTextBox.Foreground = Brushes.Gray;
            }
        }
        private void ArtistTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ArtistTextBox.Text))
            {
                ArtistTextBox.Text = "Artist";
                ArtistTextBox.Foreground = Brushes.Gray;
            }
        }
        private void TrackTitleTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TrackTitleTextBox.Text == "Track title")
            {
                TrackTitleTextBox.Text = "";
                TrackTitleTextBox.Foreground = Brushes.Gray;
            }
        }
        private void TrackTitleTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TrackTitleTextBox.Text))
            {
                TrackTitleTextBox.Text = "Track title";
                TrackTitleTextBox.Foreground = Brushes.Gray;
            }
        }
        public void MetaDataLoad()
        {
            if (!string.IsNullOrWhiteSpace(ArtistTextBox.Text)
                && !string.IsNullOrWhiteSpace(TrackTitleTextBox.Text)
                && ArtistTextBox.Text != "Artist"
                && TrackTitleTextBox.Text != "Track title")
            {
                UpdateBtn.IsEnabled = true; UpdateBtn.Opacity = 1;
            }
            else
            {
                UpdateBtn.IsEnabled = false; UpdateBtn.Opacity = 0.5;
            }
        }

        private void UpdateMetdata_Click(object sender, RoutedEventArgs e) { }
        #endregion

        #region MenuButtons
        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            PreferencesModalOverlay.Visibility = Visibility.Visible;
        }
        private void ClosePreferences_Click(object sender, RoutedEventArgs e)
        {
            PreferencesModalOverlay.Visibility = Visibility.Collapsed;
        }
        private void PreferencesHeader_CloseClicked(object sender, RoutedEventArgs e)
        {
            PreferencesModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void Streams_Click(object sender, RoutedEventArgs e)
        {
            StreamsModalOverlay.Visibility = Visibility.Visible;
        }

        private void StreamsHeader_CloseClicked(object sender, RoutedEventArgs e)
        {
            StreamsModalOverlay.Visibility = Visibility.Collapsed;
        }
        private void About_Click(object sender, RoutedEventArgs e)
        {
            AboutModalOverlay.Visibility = Visibility.Visible;
        }
        private void MetadataCapture_Click(object sender, RoutedEventArgs e)
        {
            if (MetaDataContent.Content == null)
            {
                var dataCaptureControl = new MetaDataCapture();
                dataCaptureControl.CloseClicked += (s, args) =>
                {
                    MetaDataContent.Content = null;
                    //  MetaDataFadeOverlay.Visibility = Visibility.Collapsed;
                };
                MetaDataContent.Content = dataCaptureControl;
                //  MetaDataFadeOverlay.Visibility = Visibility.Visible;
            }
            else
            {
                MetaDataContent.Content = null;
            }
        }
        #endregion

        #region VolumeMixer
        private void UpdateAudioDbText(double sliderValue)
        {
            try
            {
                if (AudioDbTextBlock != null)
                {
                    string dbText = sliderValue <= 0
                        ? "-inf dB"
                        : $"{Math.Max(20 * Math.Log10(sliderValue / 100.0), -80):0.0} dB";
                    AudioDbTextBlock.Text = dbText;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }
        private void UpdateOtherAppsDbText(double sliderValue)
        {
            try
            {
                if (OtherAppsDbTextBlock != null)
                {
                    string dbText = sliderValue <= 0
                        ? "-inf dB"
                        : $"{Math.Max(20 * Math.Log10(sliderValue / 100.0), -80):0.0} dB";
                    OtherAppsDbTextBlock.Text = dbText;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }
        private void AudioToggle_Click(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _isMicMuted = !_isMicMuted;
                AudioToggleImage.Source = new BitmapImage(new Uri(
                    _isMicMuted ? "/Images/muteSpeaker_Icon.png" : "/Images/EnableSpeaker.png",
                    UriKind.Relative));
                if (_isMicMuted)
                {
                    AudioInputCaptureService.Instance.Stop();
                    _previousAudioInputSliderValue = AudioSlider.Value;
                    AudioSlider.Value = 0;
                }
                else
                {
                    AudioSlider.Value = _previousAudioInputSliderValue;
                    if (_audioDevicesViewModel.EnableInputAudio)
                    {
                        AudioInputCaptureService.Instance.Start();
                    }
                    InitAudio();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }
        private void OtherAppsToggle_Click(object sender, MouseButtonEventArgs e)
        {
            _isOtherAppsMuted = !_isOtherAppsMuted;

            try
            {
                OtherAppsToggleImage.Source = new BitmapImage(new Uri(
                    _isOtherAppsMuted ? "/Images/muteSpeaker_Icon.png" : "/Images/EnableSpeaker.png",
                    UriKind.Relative));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                CustomMessageBox.ShowInfo($"Failed to load speaker icon: {ex.Message}", "Error");
            }

            if (_isOtherAppsMuted)
            {
                OtherAppsSlider.Value = 0;

                OtherAppAudioCaptureService.Instance.Stop();
                UpdateVUMeter(OtherApplicationsVU, 0);
                if (!_isMasterMuted)
                    UpdateVUMeter(MasterVU, 0);
            }
            else
            {
                OtherAppsSlider.Value = 50;
                if (_audioDevicesViewModel.EnableSystemAudioCapture)
                {
                    OtherAppAudioCaptureService.Instance.Start();
                }
            }
        }
        private async void MasterToggle_Click(object sender, MouseButtonEventArgs e)
        {
            _isMasterMuted = !_isMasterMuted;

            try
            {
                MasterToggleImage.Source = new BitmapImage(new Uri(
                    _isMasterMuted ? "/Images/muteSpeaker_Icon.png" : "/Images/EnableSpeaker.png",
                    UriKind.Relative));
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                CustomMessageBox.ShowInfo($"Failed to load speaker icon: {ex.Message}", "Error");
            }

            // UI updates on main thread
            if (_isMasterMuted)
            {
                AudioSlider.Value = 0;
                AudioSlider.IsEnabled = false;
                OtherAppsSlider.Value = 0;
                OtherAppsSlider.IsEnabled = false;
                UpdateVUMeter(AudioInputVU, 0);
                UpdateVUMeter(OtherApplicationsVU, 0);
                UpdateVUMeter(MasterVU, 0);
            }
            else
            {
                AudioSlider.Value = 50;
                AudioSlider.IsEnabled = true;
                OtherAppsSlider.Value = 50;
                OtherAppsSlider.IsEnabled = true;
            }

            // Background for heavy ops
            _ = Task.Run(() =>
            {
                try
                {
                    if (_isMasterMuted)
                    {
                        OtherAppAudioCaptureService.Instance.Stop();
                        AudioInputCaptureService.Instance.Stop();
                    }
                    else
                    {
                        if (!_isMicMuted && _audioDevicesViewModel.EnableInputAudio)
                            AudioInputCaptureService.Instance.Start();
                        if (!_isOtherAppsMuted && _audioDevicesViewModel.EnableSystemAudioCapture)
                            OtherAppAudioCaptureService.Instance.Start();
                        if (!_isMicMuted && _audioDevicesViewModel.EnableInputAudio)
                            InitAudio();
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    if (!Dispatcher.HasShutdownStarted && !Dispatcher.HasShutdownFinished)
                        Dispatcher.BeginInvoke(new Action(() => CustomMessageBox.ShowInfo($"Error in MasterToggle_Click: {ex.Message}", "Error")));
                }
            });
        }
        private void AudioSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider != null && AudioDbTextBlock != null)
            {
                audioInputSliderValue = slider.Value;

                string dbText = slider.Value <= 0
                    ? "-inf dB"
                    : $"{Math.Max(20 * Math.Log10(slider.Value / 100.0), -80):0.0} dB";

                AudioDbTextBlock.Text = dbText;

                float scale = (float)(slider.Value / 100.0);
                UpdateVUMeter(AudioInputVU, scale);
                if (AudioDbTextBlock.Text == "-inf dB")
                {
                    AudioToggleImage.Source = new BitmapImage(new Uri(
                        "/Images/muteSpeaker_Icon.png",
                        UriKind.Relative));
                }
                else
                {
                    AudioToggleImage.Source = new BitmapImage(new Uri(
                        "/Images/EnableSpeaker.png",
                        UriKind.Relative));
                }
            }
        }
        private void OtherAppsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            var slider = sender as Slider;
            if (slider != null && OtherAppsDbTextBlock != null)
            {
                otherAppsSliderValue = slider.Value;

                string dbText = slider.Value <= 0
                    ? "-inf dB"
                    : $"{Math.Max(20 * Math.Log10(slider.Value / 100.0), -80):0.0} dB";

                OtherAppsDbTextBlock.Text = dbText;

                float scale = (float)(slider.Value / 100.0);
                if (!_isOtherAppsMuted && _audioDevicesViewModel.EnableSystemAudioCapture)
                {
                    UpdateVUMeter(OtherApplicationsVU, scale * 0.7f);
                }
                if (OtherAppsDbTextBlock.Text == "-inf dB")
                {
                    OtherAppsToggleImage.Source = new BitmapImage(new Uri("/Images/muteSpeaker_Icon.png", UriKind.Relative));
                }
                else
                {
                    OtherAppsToggleImage.Source = new BitmapImage(new Uri("/Images/EnableSpeaker.png", UriKind.Relative));
                }
            }
        }
        private void CreateBlocks()
        {
            audioInputBlocks = new Rectangle[NumBlocks];
            otherAppBlocks = new Rectangle[NumBlocks];
            masterBlocks = new Rectangle[NumBlocks];

            for (int i = 0; i < NumBlocks; i++)
            {
                audioInputBlocks[i] = new Rectangle
                {
                    Width = 40,
                    Height = 20,
                    Margin = new Thickness(0, 0, 0, 0),
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 0,
                    RenderTransform = new ScaleTransform(1, 1),
                    RenderTransformOrigin = new Point(0.5, 1)
                };
                AudioInputVU.Children.Insert(0, audioInputBlocks[i]);

                otherAppBlocks[i] = new Rectangle
                {
                    Width = 40,
                    Height = 20,
                    Margin = new Thickness(0, 0, 0, 0),
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 0,
                    RenderTransform = new ScaleTransform(1, 1),
                    RenderTransformOrigin = new Point(0.5, 1)
                };
                OtherApplicationsVU.Children.Insert(0, otherAppBlocks[i]);

                masterBlocks[i] = new Rectangle
                {
                    Width = 40,
                    Height = 20,
                    Margin = new Thickness(0, 0, 0, 0),
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Transparent,
                    StrokeThickness = 0,
                    RenderTransform = new ScaleTransform(1, 1),
                    RenderTransformOrigin = new Point(0.5, 1)
                };
                MasterVU.Children.Insert(0, masterBlocks[i]);
            }
        }
        private void UpdateVUMeter(StackPanel vuPanel, float level)
        {
            if (vuPanel == null)
            {
                return;
            }

            float normalizedLevel = Math.Clamp(level, 0, 1);
            int activeBlocks = (int)(normalizedLevel * NumBlocks);

            for (int i = 0; i < vuPanel.Children.Count; i++)
            {
                if (vuPanel.Children[i] is not Rectangle rectangle) continue;

                bool isActive = i >= (NumBlocks - activeBlocks);

                if (rectangle.RenderTransform as ScaleTransform == null)
                {
                    rectangle.RenderTransform = new ScaleTransform(1, 1);
                    rectangle.RenderTransformOrigin = new Point(0.5, 1);
                }

                var scaleTransform = (ScaleTransform)rectangle.RenderTransform;

                if (isActive)
                {
                    scaleTransform.ScaleY = 1;
                    rectangle.Fill = Brushes.LimeGreen;
                }
                else
                {
                    scaleTransform.ScaleY = 1;
                    rectangle.Fill = Brushes.Transparent;
                }
            }
        }
        private WaveInEvent micCapture;

        private void InitAudio()
        {
            try
            {
                if (!_isMicMuted && _audioDevicesViewModel.EnableInputAudio)
                {
                    micCapture = new WaveInEvent
                    {
                        WaveFormat = new WaveFormat(44100, 1)
                    };
                    micCapture.DataAvailable += Mic_DataAvailable;
                    micCapture.StartRecording();
                    AudioInputCaptureService.Instance.Start();
                }

                if (!_isOtherAppsMuted && _audioDevicesViewModel.EnableSystemAudioCapture)
                {
                    OtherAppAudioCaptureService.Instance.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }

        private void Mic_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (Dispatcher.HasShutdownStarted || Dispatcher.HasShutdownFinished) return;
            var audioInputOkay = AppConfigurationManager.ReadValue("AudioInput");

            if (_isMicMuted || audioInputOkay != "Yes") return;

            if ((DateTime.Now - _lastMicUpdate).TotalMilliseconds < 50) return;
            _lastMicUpdate = DateTime.Now;

            float max = 0;
           
            int step = 4; 
            for (int index = 0; index < e.BytesRecorded; index += 2 * step)
            {
                if (index >= e.BytesRecorded - 1) break;
                short sample = BitConverter.ToInt16(e.Buffer, index);
                float sample32 = Math.Abs(sample / 32768f);
                if (sample32 > max)
                    max = sample32;
            }

            float scaledMax = max * (float)(audioInputSliderValue / 100.0);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateVUMeter(AudioInputVU, scaledMax);
                if (!_isMasterMuted)
                    UpdateVUMeter(MasterVU, scaledMax);
            }), DispatcherPriority.Background);
        }

        protected override async void OnClosed(EventArgs e)
        {
            _audioDevicesViewModel.PropertyChanged -= AudioDevicesViewModel_PropertyChanged;

            try { OtherAppAudioCaptureService.Instance.Stop(); }
            catch (Exception ex) { Logger.LogError(ex); }

            try { AudioInputCaptureService.Instance.Stop(); }
            catch (Exception ex) { Logger.LogError(ex); }

            try
            {
                micCapture?.StopRecording();
                await Task.Delay(100);  
                micCapture?.Dispose();
                micCapture = null;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }

            base.OnClosed(e);
        }
        #endregion
    }
}