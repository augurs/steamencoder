using EncoderApp.Models;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EncoderApp.Views
{
    public partial class MainWindow : Window
    {
        public List<StreamInfo> Streams { get; set; }

        private bool _isCustomMaximized = false;
        private Rect _restoreBounds;

        private const int NumBlocks = 10;
        private Rectangle[] audioInputBlocks;
        private Rectangle[] otherAppBlocks;
        private Rectangle[] masterBlocks;

        private WasapiLoopbackCapture captures;
        private double audioInputSliderValue = 50; 
        private double otherAppsSliderValue = 50; 
        private bool _isMicMuted = true;

        private double _previousAudioInputSliderValue = 50;
        private bool _isOtherAppsMuted = false; 
        private bool _isMasterMuted = false;
        private DateTime _lastUpdate = DateTime.MinValue;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            StartClock();

            ArtistTextBox.TextChanged += (s, e) => MetaDataLoad();
            TrackTitleTextBox.TextChanged += (s, e) => MetaDataLoad();
            MetaDataLoad();
            UpdateAudioDbText(audioInputSliderValue);
            UpdateOtherAppsDbText(otherAppsSliderValue);
            Streams = new List<StreamInfo>
            {
                new StreamInfo { Name = "Main Stream",   Mount = "Mount:/live",   IsConnected = true },
                new StreamInfo { Name = "Backup Stream", Mount = "Mount:/backup", IsConnected = false },
                new StreamInfo { Name = "Music Stream",  Mount = "Mount:/music",  IsConnected = true },
                new StreamInfo { Name = "News Stream",   Mount = "Mount:/news",   IsConnected = false }
            };

            CreateBlocks();
            InitAudio();
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
                OnAirPanel.Visibility = isConnected ? Visibility.Visible : Visibility.Collapsed;
                OffAirPanel.Visibility = isConnected ? Visibility.Collapsed : Visibility.Visible;
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

        private async void StartClock()
        {
            while (true)
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
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Preferences_Click(object sender, RoutedEventArgs e)
        {
            PreferencesModalOverlay.Visibility = Visibility.Visible;
        }
        private void ClosePreferences_Click(object sender, RoutedEventArgs e)
        {
            PreferencesModalOverlay.Visibility = Visibility.Collapsed;
        }
        private void PreferencesHeader_CloseClicked(object sender, EventArgs e)
        {
            PreferencesModalOverlay.Visibility = Visibility.Collapsed;
        }

        private void Streams_Click(object sender, RoutedEventArgs e)
        {
            StreamsModalOverlay.Visibility = Visibility.Visible;
        }

        private void StreamsHeader_CloseClicked(object sender, EventArgs e)
        {
            StreamsModalOverlay.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region VolumeMixer
        private void UpdateAudioDbText(double sliderValue)
        {
            if (AudioDbTextBlock != null)
            {
                string dbText = sliderValue <= 0
                    ? "-inf dB"
                    : $"{Math.Max(20 * Math.Log10(sliderValue / 100.0), -80):0.0} dB";
                AudioDbTextBlock.Text = dbText;
            }
        }
        private void UpdateOtherAppsDbText(double sliderValue)
        {
            if (OtherAppsDbTextBlock != null)
            {
                string dbText = sliderValue <= 0
                    ? "-inf dB"
                    : $"{Math.Max(20 * Math.Log10(sliderValue / 100.0), -80):0.0} dB";

                OtherAppsDbTextBlock.Text = dbText;

              
            }
        }

        private void AudioToggle_Click(object sender, MouseButtonEventArgs e)
        {
            _isMicMuted = !_isMicMuted;
            AudioToggleImage.Source = new BitmapImage(new Uri(
                _isMicMuted ? "/Images/muteSpeaker_Icon.png" : "/Images/EnableSpeaker.png",
                UriKind.Relative));
            if (_isMicMuted)
            {
                _previousAudioInputSliderValue = AudioSlider.Value;
                AudioSlider.Value = 0;
                captures?.StopRecording();
            }
            else
            {
                AudioSlider.Value = _previousAudioInputSliderValue;
                InitAudio();
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
                MessageBox.Show($"Failed to load speaker icon: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (_isOtherAppsMuted)
            {
                OtherAppsSlider.Value = 0; 
            }
            else
            {
                OtherAppsSlider.Value = 50; 
            }
        }
        private void MasterToggle_Click(object sender, MouseButtonEventArgs e)
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
                MessageBox.Show($"Failed to load speaker icon: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (_isMasterMuted)
            {
                AudioSlider.Value = 0;
                AudioSlider.IsEnabled = false;

                OtherAppsSlider.Value = 0;
                OtherAppsSlider.IsEnabled = false;
             
            }
            else
            {
                AudioSlider.Value = 50;
                AudioSlider.IsEnabled=true;
                OtherAppsSlider.Value = 50;
                OtherAppsSlider.IsEnabled = true;
            }
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
            }
        }
        private void OtherAppsSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sender is Slider slider && OtherAppsDbTextBlock != null)
            {
                otherAppsSliderValue = slider.Value;

                string dbText;
                if (slider.Value <= 0)
                {
                    dbText = "-inf dB";
                    OtherAppsToggleImage.Source = new BitmapImage(new Uri("/Images/muteSpeaker_Icon.png", UriKind.Relative));
                }
                else
                {
                    dbText = $"{Math.Max(20 * Math.Log10(slider.Value / 100.0), -80):0.0} dB";
                    OtherAppsToggleImage.Source = new BitmapImage(new Uri("/Images/EnableSpeaker.png", UriKind.Relative));
                }

                OtherAppsDbTextBlock.Text = dbText;

                double scale = slider.Value / 100.0;
                UpdateVUMeter(OtherApplicationsVU, (float)(scale * 0.7));
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
        private WasapiLoopbackCapture appCapture;

        private void InitAudio()
        {
            try
            {
                if (!_isMicMuted)
                {
                    micCapture = new WaveInEvent
                    {
                        WaveFormat = new WaveFormat(44100, 1)
                    };
                    micCapture.DataAvailable += Mic_DataAvailable;
                    micCapture.StartRecording();
                }

                if (!_isOtherAppsMuted)
                {
                    appCapture = new WasapiLoopbackCapture();
                    appCapture.WaveFormat = new WaveFormat(44100, 2);
                    appCapture.DataAvailable += App_DataAvailable;
                    appCapture.StartRecording();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error starting audio: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Mic_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_isMicMuted) return;

            if ((DateTime.Now - _lastUpdate).TotalMilliseconds < 50)
                return;

            float max = 0;
            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = BitConverter.ToInt16(e.Buffer, index);
                float sample32 = Math.Abs(sample / 32768f);
                if (sample32 > max)
                    max = sample32;
            }

            Dispatcher.Invoke(() =>
            {
                UpdateVUMeter(AudioInputVU, max * (float)(audioInputSliderValue / 100.0));
                if (!_isMasterMuted)
                    UpdateVUMeter(MasterVU, max * (float)(audioInputSliderValue / 100.0));
            });

            _lastUpdate = DateTime.Now;
        }
        private void App_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_isOtherAppsMuted) return;

            if ((DateTime.Now - _lastUpdate).TotalMilliseconds < 50)
                return;

            float max = 0;
            for (int index = 0; index < e.BytesRecorded; index += 4)
            {
                short sampleLeft = BitConverter.ToInt16(e.Buffer, index);
                short sampleRight = BitConverter.ToInt16(e.Buffer, index + 2);
                float sample32 = Math.Max(Math.Abs(sampleLeft / 32768f), Math.Abs(sampleRight / 32768f));
                if (sample32 > max)
                    max = sample32;
            }

            Dispatcher.Invoke(() =>
            {
                UpdateVUMeter(OtherApplicationsVU, max * (float)(otherAppsSliderValue / 100.0));
                if (!_isMasterMuted)
                    UpdateVUMeter(MasterVU, max * (float)(otherAppsSliderValue / 100.0));
            });

            _lastUpdate = DateTime.Now;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            micCapture?.StopRecording();
            micCapture?.Dispose();
            appCapture?.StopRecording();
            appCapture?.Dispose();
        }
        #endregion
    }
}
