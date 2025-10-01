using EncoderApp.Models;
using EncoderApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EncoderApp.ViewModels
{
    public class AudioDevicesScreenViewModel: INotifyPropertyChanged
    {
        #region Audio Playback
        private bool _enableAudio = true;
        private string _selectedAudioApi = "Windows WASAPI";
        private bool _latencyEnabled;
        private bool _showUnsupportedSamplerates=false;
        private string _selectedInputSampleRate;
        public string SelectedInputSampleRate
        {
            get => _selectedInputSampleRate;
            set
            {
                if (_selectedInputSampleRate != value)
                {
                    _selectedInputSampleRate = value;

                    OnPropertyChanged();
                    AppConfigurationManager.WriteValue("SelectedInputSampleRate", _selectedInputSampleRate);

                }
            }
        }

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
                    UpdateLatencyEnabled();
                    AppConfigurationManager.WriteValue("SelectedAudioApi", _selectedAudioApi);
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
            get => _showUnsupportedSamplerates;
            set
            {
                if (_showUnsupportedSamplerates != value)
                {
                    _showUnsupportedSamplerates = value;
                    OnPropertyChanged();
                    UpdateSampleRates();
                }
            }
        }
        public ObservableCollection<string> InputSampleRates { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> OutputSampleRates { get; } = new ObservableCollection<string>();

        #endregion
        #region Audio Input
        private string _selectedOutputSampleRate;
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

        private bool _enableInputAudio = true;
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
        private bool _ShowMonoInputs;
        public bool ShowMonoInputs
        {
            get => _ShowMonoInputs;
            set
            {
                if (_ShowMonoInputs != value)
                {
                    _ShowMonoInputs = value;
                    OnPropertyChanged();
                    checksShowMonoInputs();
                }
            }
        }
        #endregion
        #region OtherApplication
        private bool _enableSystemAudioCapture = true;
        public bool EnableSystemAudioCapture
        {
            get => _enableSystemAudioCapture;
            set
            {
                if (_enableSystemAudioCapture != value)
                {
                    _enableSystemAudioCapture = value;
                    OnPropertyChanged();
                    // Update audio capture state if needed
                    UpdateSystemAudioCapture();
                }
            }
        }

        #endregion
        private bool _isRestoringSettings = false;

        public AudioDevicesScreenViewModel()
        {
            _isRestoringSettings = true;
            string savedApi = AppConfigurationManager.ReadValue("SelectedAudioApi");
            if (!string.IsNullOrEmpty(savedApi))
                _selectedAudioApi = savedApi;

            string savedInputRate = AppConfigurationManager.ReadValue("SelectedInputSampleRate");
            if (!string.IsNullOrEmpty(savedInputRate))
                _selectedInputSampleRate = savedInputRate;

            string savedLatency = AppConfigurationManager.ReadValue("LatencyEnabled");
            if (!string.IsNullOrEmpty(savedLatency) && bool.TryParse(savedLatency, out bool latency))
                _latencyEnabled = latency;

            UpdateSampleRates();

            if (InputSampleRates.Contains(_selectedInputSampleRate))
                SelectedInputSampleRate = _selectedInputSampleRate;
            else
                SelectedInputSampleRate = InputSampleRates[0];

            if (!string.IsNullOrEmpty(_selectedAudioApi))
                SelectedAudioApi = _selectedAudioApi;

            _isRestoringSettings = false;

            AudioInputCapture();
        }

        private void UpdateLatencyEnabled()
        {
            if (_isRestoringSettings)
                return;

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

            // Only set SelectedInputSampleRate if it's null or invalid
            if (string.IsNullOrEmpty(SelectedInputSampleRate) || !InputSampleRates.Contains(SelectedInputSampleRate))
                SelectedInputSampleRate = InputSampleRates[0];

            if (string.IsNullOrEmpty(SelectedOutputSampleRate) || !OutputSampleRates.Contains(SelectedOutputSampleRate))
                SelectedOutputSampleRate = OutputSampleRates[0];
        }

        private void checksShowMonoInputs()
        {
            if (ShowMonoInputs == true)
            {
                EnableInputAudio = false;
                SelectedAudioApi = "Windows WDM-KS";
            }
            else
            {
                EnableInputAudio = true;
                SelectedAudioApi = "Windows WASAPI";
            }
            UpdateSampleRates();
        }
        private void UpdateSystemAudioCapture()
        {
            if (EnableSystemAudioCapture)
            {
                OtherAppAudioCaptureService.Instance.Start(selectedDeviceIndex); 
            }
            else
            {
                OtherAppAudioCaptureService.Instance.Stop();
            }
        }
        int selectedDeviceIndex = 0;
       
        private void AudioInputCapture()
        {
            if (EnableInputAudio)
            {
               AppConfigurationManager.WriteValue("AudioInput", "Yes");
               AudioInputCaptureService.Instance.Start(selectedDeviceIndex);
            }  
            else
            {
                AppConfigurationManager.WriteValue("AudioInput", "No");
                AudioInputCaptureService.Instance.Stop();
            }

        }
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
