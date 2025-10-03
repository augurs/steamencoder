using EncoderApp.Views;
using NAudio.Wave;
using System;
using System.Windows;

namespace EncoderApp.Services
{
    public class AudioInputCaptureService
    {
        private static AudioInputCaptureService _instance;
        public static AudioInputCaptureService Instance => _instance ??= new AudioInputCaptureService();

        private WaveInEvent _waveIn;
        private readonly int sampleRate = 44100;
        private readonly int channels = 1;

        public event Action<float> OnVolumeChanged;

        /// <summary>
        /// Start capturing audio from the specified input device.
        /// </summary>
        /// 


        public void Start(int deviceIndex = 0)
        {
            Stop();

            if (deviceIndex < 0 || deviceIndex >= WaveInEvent.DeviceCount)
            {
                CustomMessageBox.ShowInfo(
                    $"Invalid input device index: {deviceIndex}. Available devices: {WaveInEvent.DeviceCount}. Please select a valid microphone.",
                    "Warning"
                );
                return;
            }

            _waveIn = new WaveInEvent
            {
                DeviceNumber = deviceIndex,
                WaveFormat = new WaveFormat(sampleRate, channels),
                BufferMilliseconds = 50 
            };

            _waveIn.DataAvailable += WaveIn_DataAvailable;
            _waveIn.RecordingStopped += WaveIn_RecordingStopped;

            try
            {
                _waveIn.StartRecording();
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowInfo(
                    $"Failed to start microphone recording:\n{ex.Message}",
                    "Error"
                );
            }


        }


        /// <summary>
        /// Stop audio capture
        /// </summary>
        public void Stop()
        {
            if (_waveIn != null)
            {
                _waveIn.DataAvailable -= WaveIn_DataAvailable;
                _waveIn.RecordingStopped -= WaveIn_RecordingStopped;

                _waveIn.StopRecording();
                _waveIn.Dispose();
                _waveIn = null;
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            int samples = e.BytesRecorded / 2;
            if (samples == 0) return;

            double sumSquares = 0;
            for (int i = 0; i < e.BytesRecorded; i += 2)
            {
                short sample = (short)((e.Buffer[i + 1] << 8) | e.Buffer[i]);
                float sample32 = sample / 32768f;
                sumSquares += sample32 * sample32;
            }

            float rms = (float)Math.Sqrt(sumSquares / samples);
            float level = rms * 100; 
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnVolumeChanged?.Invoke(level);
            });
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                CustomMessageBox.ShowInfo(
     "Error capturing audio: " + e.Exception.Message,
     "Error"
 );

            }
            Stop();
        }
    }
}
