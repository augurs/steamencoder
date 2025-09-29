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

        private bool _enableInputAudio = false;
        public bool EnableInputAudio
        {
            get => _enableInputAudio;
            set
            {
                if (_enableInputAudio != value)
                {
                    _enableInputAudio = value;
                    OnPropertyChanged();
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
        public AudioDevicesScreenViewModel()
        {
            UpdateSampleRates();
        }

        private void UpdateLatencyEnabled()
        {
            LatencyEnabled = SelectedAudioApi == "ASIO"|| SelectedAudioApi == "Windows WASAPI";
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
            if (InputSampleRates.Count > 0)
                SelectedInputSampleRate = InputSampleRates[0];

            if (OutputSampleRates.Count > 0)
                SelectedOutputSampleRate = OutputSampleRates[0];
        }
        private void checksShowMonoInputs()
        {
            if (ShowMonoInputs==true)
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
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
