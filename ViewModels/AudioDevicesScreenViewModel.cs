using EncoderApp.Models;
using EncoderApp.Services;
using EncoderApp.Views;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;

namespace EncoderApp.ViewModels
{
    public class AudioDevicesScreenViewModel : INotifyPropertyChanged
    {
        #region Fields
        private AsioOut _asioOut;
        private bool _isRestoringSettings;
        private int _selectedDeviceIndex;
        private bool _showUnsupportedSampleRates;
        private bool _enableAudio = true;
        private string _selectedAudioApi = "Windows WASAPI";
        private string _selectedLatency = "Default";
        private bool _latencyEnabled;
        private bool _enableInputAudio = true;
        private bool _showMonoInputs;
        private bool _enableSystemAudioCapture = true;
        private string _selectedPlaybackDevice;
        private string _selectedInputDevice;
        private string _selectedSystemAudioDevice;
        private string _selectedInputSampleRate;
        private string _selectedOutputSampleRate;
        #endregion

        #region Collections
        public ObservableCollection<string> PlaybackSampleRates { get; } = new();
        public ObservableCollection<string> PlaybackDevices { get; } = new();
        public ObservableCollection<string> InputDevices { get; } = new();
        public ObservableCollection<string> AudioApis { get; } = new()
            { "MME", "Windows DirectSound", "ASIO", "Windows WDM-KS", "Windows WASAPI" };
        public ObservableCollection<int> Latencies { get; } = new() { 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768 };
        public ObservableCollection<string> InputSampleRates { get; } = new();
        public ObservableCollection<string> OutputSampleRates { get; } = new();
        public ObservableCollection<string> SystemAudioDevices { get; } = new()
            { "System Default", "Speakers (Realtek High Definition Audio)", "Headphones (High Definition Audio Device)" };
        #endregion

        #region Properties
        public bool ShowUnsupportedSamplerates
        {
            get => _showUnsupportedSampleRates;
            set
            {
                if (_showUnsupportedSampleRates != value)
                {
                    _showUnsupportedSampleRates = value;
                    OnPropertyChanged();
                    UpdateInputSampleRates();
                    UpdateOutputSampleRates();
                    if (!_isRestoringSettings)
                        AppConfigurationManager.WriteValue("ShowUnsupportedSamplerates", value.ToString());
                }
            }
        }

        public bool EnableAudio
        {
            get => _enableAudio;
            set
            {
                _enableAudio = value;
                OnPropertyChanged();
                if (!_isRestoringSettings)
                {
                    AppConfigurationManager.WriteValue("EnableAudio", value.ToString());
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
                        AppConfigurationManager.WriteValue("SelectedAudioApi", value);
                        UpdateLatencyEnabled();
                        LoadPlaybackDevices();
                        _ = HandleAudioApiChangeAsync();
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
                        AppConfigurationManager.WriteValue("SelectedLatency", value);
                }
            }
        }

        public bool LatencyEnabled
        {
            get => _latencyEnabled;
            set
            {
                _latencyEnabled = value;
                OnPropertyChanged();
                if (!_isRestoringSettings)
                    AppConfigurationManager.WriteValue("LatencyEnabled", value.ToString());
            }
        }

        public bool EnableInputAudio
        {
            get => _enableInputAudio;
            set
            {
                _enableInputAudio = value;
                OnPropertyChanged();
                if (!_isRestoringSettings)
                {
                    AppConfigurationManager.WriteValue("EnableInputAudio", value.ToString());
                    _ = AudioInputCaptureAsync();
                }
                else
                {
                    AudioInputCapture();
                }
            }
        }

        public bool ShowMonoInputs
        {
            get => _showMonoInputs;
            set
            {
                _showMonoInputs = value;
                OnPropertyChanged();
                HandleMonoInputs();
                if (!_isRestoringSettings)
                    AppConfigurationManager.WriteValue("ShowMonoInputs", value.ToString());
            }
        }

        public bool EnableSystemAudioCapture
        {
            get => _enableSystemAudioCapture;
            set
            {
                _enableSystemAudioCapture = value;
                OnPropertyChanged();
                if (!_isRestoringSettings)
                {
                    AppConfigurationManager.WriteValue("EnableSystemAudioCapture", value.ToString());
                    _ = UpdateSystemAudioCaptureAsync();
                }
                else
                {
                    UpdateSystemAudioCapture();
                }
            }
        }

        public string SelectedPlaybackDevice
        {
            get => _selectedPlaybackDevice;
            set
            {
                if (_selectedPlaybackDevice != value)
                {
                    _selectedPlaybackDevice = value;
                    OnPropertyChanged();
                    if (!_isRestoringSettings && !string.IsNullOrEmpty(value))
                        AppConfigurationManager.WriteValue("SelectedPlaybackDevice", value);
                }
            }
        }

        public string SelectedInputDevice
        {
            get => _selectedInputDevice;
            set
            {
                if (_selectedInputDevice != value)
                {
                    _selectedInputDevice = value;
                    OnPropertyChanged();
                    if (!_isRestoringSettings && !string.IsNullOrEmpty(value))
                        AppConfigurationManager.WriteValue("SelectedInputDevice", value);
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
                    if (!_isRestoringSettings && !string.IsNullOrEmpty(value))
                        AppConfigurationManager.WriteValue("SelectedOutputSampleRate", value);
                }
            }
        }
        #endregion

        #region Constructor
        public AudioDevicesScreenViewModel()
        {
            _isRestoringSettings = true;

            RestoreSettings();
            LoadPlaybackDevices();
            LoadInputDevices();
            UpdateSampleRates();
            UpdateLatencyEnabled();

            _isRestoringSettings = false;
            AudioInputCapture();
        }
        #endregion

        #region Methods
        private void RestoreSettings()
        {
            _selectedAudioApi = AppConfigurationManager.ReadValue("SelectedAudioApi") ?? "Windows WASAPI";
            _selectedLatency = AppConfigurationManager.ReadValue("SelectedLatency") ?? "128";
            _selectedPlaybackDevice = AppConfigurationManager.ReadValue("SelectedPlaybackDevice") ?? "";
            _selectedInputDevice = AppConfigurationManager.ReadValue("SelectedInputDevice") ?? "";
            _selectedSystemAudioDevice = AppConfigurationManager.ReadValue("SelectedSystemAudioDevice") ?? "System Default";
            _selectedInputSampleRate = AppConfigurationManager.ReadValue("SelectedInputSampleRate") ?? "Device Default (Recommended)";
            _selectedOutputSampleRate = AppConfigurationManager.ReadValue("SelectedOutputSampleRate") ?? "Device Default (Recommended)";
            _showUnsupportedSampleRates = bool.TryParse(AppConfigurationManager.ReadValue("ShowUnsupportedSamplerates"), out bool showUnsupported) ? showUnsupported : false;
            _enableSystemAudioCapture = bool.TryParse(AppConfigurationManager.ReadValue("EnableSystemAudioCapture"), out bool enableSysAudio) ? enableSysAudio : true;
            _enableInputAudio = bool.TryParse(AppConfigurationManager.ReadValue("EnableInputAudio"), out bool enableInput) ? enableInput : true;
            _showMonoInputs = bool.TryParse(AppConfigurationManager.ReadValue("ShowMonoInputs"), out bool showMono) ? showMono : false;
            _latencyEnabled = bool.TryParse(AppConfigurationManager.ReadValue("LatencyEnabled"), out bool latencyEnabled) ? latencyEnabled : false;
        }

        public void LoadPlaybackDevices()
        {
            PlaybackDevices.Clear();

            if (SelectedAudioApi == "Windows WASAPI")
            {
                var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active); //output device and active device
                foreach (var dev in devices) PlaybackDevices.Add(dev.FriendlyName);
            }
            else
            {
                for (int i = 0; i < WaveOut.DeviceCount; i++)
                    PlaybackDevices.Add(WaveOut.GetCapabilities(i).ProductName);
            }
            SelectedPlaybackDevice = PlaybackDevices.Contains(_selectedPlaybackDevice) ? _selectedPlaybackDevice : PlaybackDevices.FirstOrDefault();
        }

        private void LoadInputDevices()
        {
            InputDevices.Clear();
            InputDevices.Add("Windows Default");
            var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active); //microphones, line-in, virtual audio cables,and filter working device

            foreach (var dev in devices) InputDevices.Add($"{dev.FriendlyName} - {dev.DataFlow}");
            SelectedInputDevice = InputDevices.Contains(_selectedInputDevice) ? _selectedInputDevice : InputDevices.FirstOrDefault();
        }

        private void UpdateInputSampleRates()
        {
            InputSampleRates.Clear();
            InputSampleRates.Add("Device Default (Recommended)");
            InputSampleRates.Add("44100");
            if (ShowUnsupportedSamplerates)
            {
                InputSampleRates.Add("48000");
                InputSampleRates.Add("96000");
            }
            SelectedInputSampleRate = InputSampleRates.Contains(_selectedInputSampleRate) ? _selectedInputSampleRate : InputSampleRates.FirstOrDefault() ?? "Device Default (Recommended)";
        }

        private void UpdateOutputSampleRates()
        {
            OutputSampleRates.Clear();
            OutputSampleRates.Add("Device Default (Recommended)");
            OutputSampleRates.Add("44100");
            if (ShowUnsupportedSamplerates)
            {
                OutputSampleRates.Add("48000");
                OutputSampleRates.Add("96000");
            }
            SelectedOutputSampleRate = OutputSampleRates.Contains(_selectedOutputSampleRate) ? _selectedOutputSampleRate : OutputSampleRates.FirstOrDefault() ?? "Device Default (Recommended)";
        }

        private void UpdateSampleRates()
        {
            UpdateInputSampleRates();
            UpdateOutputSampleRates();
        }

        public void UpdateLatencyEnabled() => LatencyEnabled = SelectedAudioApi == "ASIO" || SelectedAudioApi == "Windows WASAPI";

        private void HandleMonoInputs()
        {
            //  EnableInputAudio = !ShowMonoInputs;
            SelectedAudioApi = ShowMonoInputs ? "Windows WDM-KS" : "Windows WASAPI";
            UpdateSampleRates();
        }

        private async Task HandleAudioApiChangeAsync()
        {
            StopAsioPlayback();
            if (SelectedAudioApi == "ASIO")
            {
                await Task.Run(() =>
                {
                    try
                    {
                        StartAsioPlayback();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ASIO start error: {ex.Message}");
                        Logger.LogError(ex);
                        Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            CustomMessageBox.ShowInfo($"ASIO Playback Error: {ex.Message}", "Error")));
                    }
                });
            }
        }

        private void StartAsioPlayback()
        {
            if (string.IsNullOrEmpty(SelectedPlaybackDevice)) return;
            _asioOut = new AsioOut(SelectedPlaybackDevice);
            var silence = new SilenceProvider(WaveFormat.CreateIeeeFloatWaveFormat(44100, 2));
            var mixer = new MixingSampleProvider(silence.WaveFormat) { ReadFully = true };
            mixer.AddMixerInput(silence.ToSampleProvider());
            _asioOut.Init(mixer);
            _asioOut.Play();
        }

        private void StopAsioPlayback()
        {
            try
            {
                _asioOut?.Stop();
                _asioOut?.Dispose();
                _asioOut = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ASIO stop error: {ex.Message}");
            }
        }

        public async Task UpdateSystemAudioCaptureAsync()
        {
            AppConfigurationManager.WriteValue("OthersApplicationInput", EnableSystemAudioCapture ? "Yes" : "No");
            await Task.Run(async () =>
            {
                try
                {
                    if (EnableSystemAudioCapture)
                        await Task.Run(() => OtherAppAudioCaptureService.Instance.Start(_selectedDeviceIndex));
                    else
                        OtherAppAudioCaptureService.Instance.Stop();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    Debug.WriteLine($"Error in UpdateSystemAudioCapture: {ex.Message}");
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            CustomMessageBox.ShowInfo($"Error changing system audio capture: {ex.Message}", "Error")
                         ));
                }
            });
        }

        public void UpdateSystemAudioCapture()
        {
            AppConfigurationManager.WriteValue("OthersApplicationInput", EnableSystemAudioCapture ? "Yes" : "No");
            Task.Run(() =>
            {
                try
                {
                    if (EnableSystemAudioCapture)
                        OtherAppAudioCaptureService.Instance.Start(_selectedDeviceIndex);
                    else
                        OtherAppAudioCaptureService.Instance.Stop();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    Debug.WriteLine($"Error in UpdateSystemAudioCapture: {ex.Message}");
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                            CustomMessageBox.ShowInfo($"Error changing system audio capture: {ex.Message}", "Error")
                         ));
                }
            });
        }

        private void UpdateSelectedDeviceIndex()
        {
            if (SelectedAudioApi == "Windows WASAPI")
            {
                var devices = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active).ToList();
                var selected = InputDevices.IndexOf(SelectedInputDevice);
                _selectedDeviceIndex = selected >= 0 && selected < devices.Count ? selected : 0;
            }
            else
            {
                _selectedDeviceIndex = 0;
            }
        }

        public async Task AudioInputCaptureAsync()
        {
            AppConfigurationManager.WriteValue("AudioInput", EnableInputAudio ? "Yes" : "No");
            UpdateSelectedDeviceIndex();
            await Task.Run(() =>
            {
                try
                {
                    if (EnableInputAudio)
                        AudioInputCaptureService.Instance.Start(_selectedDeviceIndex, SelectedAudioApi);
                    else
                        AudioInputCaptureService.Instance.Stop();
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex);
                    Debug.WriteLine($"Error in AudioInputCapture: {ex.Message}");
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        CustomMessageBox.ShowInfo($"Error in audio input capture: {ex.Message}", "Error")));
                }
            });
        }

        public void AudioInputCapture()
        {
            AppConfigurationManager.WriteValue("AudioInput", EnableInputAudio ? "Yes" : "No");
            UpdateSelectedDeviceIndex();
            if (EnableInputAudio)
                AudioInputCaptureService.Instance.Start(_selectedDeviceIndex, SelectedAudioApi);
            else
                AudioInputCaptureService.Instance.Stop();
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}