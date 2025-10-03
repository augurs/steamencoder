using EncoderApp.Models;
using EncoderApp.Services;
using EncoderApp.Views;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace EncoderApp.ViewModels
{
    public class AudioDevicesScreenViewModel : INotifyPropertyChanged
    {
        #region Fields
        private bool _enableAudio = true;
        private string _selectedAudioApi = "Windows WASAPI";
        private bool _latencyEnabled;
        private bool _showUnsupportedSampleRates = false;
        private string _selectedInputSampleRate;
        private string _selectedOutputSampleRate;
        private string _selectedLatency = "Default";
        private bool _enableInputAudio = true;
        private bool _showMonoInputs;
        private bool _enableSystemAudioCapture = true;
        private string _selectedSystemAudioDevice;
        private bool _isRestoringSettings = false;
        private int _selectedDeviceIndex = 0;
        #endregion

        #region Collections
        public ObservableCollection<string> InputSampleRates { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> OutputSampleRates { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> SystemAudioDevices { get; set; } = new ObservableCollection<string>
        {
            "System Default",
            "Speakers (Realtek High Definition Audio)",
            "Headphones (High Definition Audio Device)"
        };
        #endregion

        #region Properties

        public bool EnableAudio
        {
            get => _enableAudio;
            set
            {
                if (_enableAudio != value)
                {
                    _enableAudio = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SelectedAudioApi
        {
            get => _selectedAudioApi;
            set
            {
                if (_selectedAudioApi != value)
                {
                    _selectedAudioApi = value;
                    OnPropertyChanged();
                    if (!_isRestoringSettings)
                    {
                        UpdateLatencyEnabled();
                        AppConfigurationManager.WriteValue("SelectedAudioApi", _selectedAudioApi);
                    }
                }
            }
        }

        public string SelectedLatency
        {
            get => _selectedLatency;
            set
            {
                if (_selectedLatency != value)
                {
                    _selectedLatency = value;
                    OnPropertyChanged();
                    if (!_isRestoringSettings)
                        AppConfigurationManager.WriteValue("SelectedLatency", _selectedLatency);
                }
            }
        }

        public bool LatencyEnabled
        {
            get => _latencyEnabled;
            set
            {
                if (_latencyEnabled != value)
                {
                    _latencyEnabled = value;
                    OnPropertyChanged();
                    AppConfigurationManager.WriteValue("LatencyEnabled", _latencyEnabled.ToString());
                }
            }
        }

        public bool ShowUnsupportedSamplerates
        {
            get => _showUnsupportedSampleRates;
            set
            {
                if (_showUnsupportedSampleRates != value)
                {
                    _showUnsupportedSampleRates = value;
                    OnPropertyChanged();
                    UpdateSampleRates();
                    if (!_isRestoringSettings)
                        AppConfigurationManager.WriteValue("ShowUnsupportedSamplerates", _showUnsupportedSampleRates.ToString());
                }
            }
        }

        public string SelectedInputSampleRate
        {
            get => _selectedInputSampleRate;
            set
            {
                if (_selectedInputSampleRate != value)
                {
                    _selectedInputSampleRate = value;
                    OnPropertyChanged();
                    if (!_isRestoringSettings && !string.IsNullOrEmpty(value))
                        AppConfigurationManager.WriteValue("SelectedInputSampleRate", value);
                }
            }
        }

        public string SelectedOutputSampleRate
        {
            get => _selectedOutputSampleRate;
            set
            {
                if (_selectedOutputSampleRate != value)
                {
                    _selectedOutputSampleRate = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool EnableInputAudio
        {
            get => _enableInputAudio;
            set
            {
                if (_enableInputAudio != value)
                {
                    _enableInputAudio = value;
                    OnPropertyChanged();
                    AudioInputCapture();
                }
            }
        }

        public bool ShowMonoInputs
        {
            get => _showMonoInputs;
            set
            {
                if (_showMonoInputs != value)
                {
                    _showMonoInputs = value;
                    OnPropertyChanged();
                    HandleMonoInputs();
                }
            }
        }

        public bool EnableSystemAudioCapture
        {
            get => _enableSystemAudioCapture;
            set
            {
                if (_enableSystemAudioCapture != value)
                {
                    _enableSystemAudioCapture = value;
                    OnPropertyChanged();
                    AppConfigurationManager.WriteValue("EnableSystemAudioCapture", value.ToString());
                    UpdateSystemAudioCapture();
                }
            }
        }

        public string SelectedSystemAudioDevice
        {
            get => _selectedSystemAudioDevice;
            set
            {
                if (_selectedSystemAudioDevice != value)
                {
                    _selectedSystemAudioDevice = value;
                    OnPropertyChanged();
                    if (!_isRestoringSettings && !string.IsNullOrEmpty(value))
                        AppConfigurationManager.WriteValue("SelectedSystemAudioDevice", value);
                }
            }
        }
        #endregion

        #region Constructor
        public AudioDevicesScreenViewModel()
        {
            _isRestoringSettings = true;

            RestoreSettings();
            UpdateSampleRates();
            SelectedInputSampleRate = InputSampleRates.Contains(_selectedInputSampleRate) ? _selectedInputSampleRate : InputSampleRates[0];
            SelectedLatency = new[] { "Default", "Low", "High" }.Contains(_selectedLatency) ? _selectedLatency : "Default";
            SelectedAudioApi = _selectedAudioApi;
            _isRestoringSettings = false;

            AudioInputCapture();
        }
        #endregion

        #region Methods
        private void RestoreSettings()
        {
            _selectedAudioApi = AppConfigurationManager.ReadValue("SelectedAudioApi") ?? _selectedAudioApi;
            _selectedInputSampleRate = AppConfigurationManager.ReadValue("SelectedInputSampleRate") ?? _selectedInputSampleRate;
            _selectedLatency = AppConfigurationManager.ReadValue("SelectedLatency") ?? _selectedLatency;

            if (bool.TryParse(AppConfigurationManager.ReadValue("LatencyEnabled"), out bool latency))
                _latencyEnabled = latency;

            if (bool.TryParse(AppConfigurationManager.ReadValue("ShowUnsupportedSamplerates"), out bool showUnsupported))
                _showUnsupportedSampleRates = showUnsupported;

            _selectedSystemAudioDevice = AppConfigurationManager.ReadValue("SelectedSystemAudioDevice") ?? "System Default";
            if (!SystemAudioDevices.Contains(_selectedSystemAudioDevice))
                _selectedSystemAudioDevice = "System Default";

            if (bool.TryParse(AppConfigurationManager.ReadValue("EnableSystemAudioCapture"), out bool sysAudio))
                _enableSystemAudioCapture = sysAudio;
        }

        private void UpdateLatencyEnabled()
        {
            if (_isRestoringSettings) return;
            LatencyEnabled = SelectedAudioApi == "ASIO" || SelectedAudioApi == "Windows WASAPI";
        }

        private void UpdateSampleRates()
        {
            InputSampleRates.Clear();
            OutputSampleRates.Clear();

            InputSampleRates.Add("Device Default (Recommended)");
            InputSampleRates.Add("44100");
            OutputSampleRates.Add("Device Default (Recommended)");
            OutputSampleRates.Add("44100");

            if (ShowUnsupportedSamplerates)
            {
                InputSampleRates.Add("48000");
                InputSampleRates.Add("96000");
                OutputSampleRates.Add("48000");
                OutputSampleRates.Add("96000");
            }

            if (string.IsNullOrEmpty(SelectedInputSampleRate) || !InputSampleRates.Contains(SelectedInputSampleRate))
                SelectedInputSampleRate = InputSampleRates[0];

            if (string.IsNullOrEmpty(SelectedOutputSampleRate) || !OutputSampleRates.Contains(SelectedOutputSampleRate))
                SelectedOutputSampleRate = OutputSampleRates[0];
        }

        private void HandleMonoInputs()
        {
            EnableInputAudio = !ShowMonoInputs;
            SelectedAudioApi = ShowMonoInputs ? "Windows WDM-KS" : "Windows WASAPI";
            UpdateSampleRates();
        }

        private async void UpdateSystemAudioCapture()
        {
            AppConfigurationManager.WriteValue("OthersApplicationInput", EnableSystemAudioCapture ? "Yes" : "No");
            await Task.Run(() =>
            {
                try
                {
                    if (EnableSystemAudioCapture)
                    { 
                        Debug.WriteLine($"Starting system audio capture with device index {_selectedDeviceIndex}");
                        OtherAppAudioCaptureService.Instance.Start(_selectedDeviceIndex);
                    }
                    else
                    {
                        Debug.WriteLine("Stopping system audio capture");
                        OtherAppAudioCaptureService.Instance.Stop();
                   
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error in UpdateSystemAudioCapture: {ex.Message}");
                    // Notify user on UI thread
                    Application.Current.Dispatcher.Invoke(() =>
                        CustomMessageBox.ShowInfo($"Error changing system audio capture: {ex.Message}", "Error")
                    );
                }
            });
        }

        private void AudioInputCapture()
        {
            AppConfigurationManager.WriteValue("AudioInput", EnableInputAudio ? "Yes" : "No");

            if (EnableInputAudio)
                AudioInputCaptureService.Instance.Start(_selectedDeviceIndex);
            else
                AudioInputCaptureService.Instance.Stop();
        }
        #endregion

        #region PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
