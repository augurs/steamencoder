using NAudio.Wave;
using System;
using System.Windows;

namespace EncoderApp.Services
{
    public class OtherAppAudioCaptureService
    {
        private static OtherAppAudioCaptureService _instance;
        public static OtherAppAudioCaptureService Instance => _instance ??= new OtherAppAudioCaptureService();

        private WasapiLoopbackCapture _loopbackCapture; 
        public event Action<float> OnVolumeChanged; 

        public void Start(int deviceIndex = 0)
        {
            if (_loopbackCapture != null) return;

            try
            {
                _loopbackCapture = new WasapiLoopbackCapture();
                _loopbackCapture.WaveFormat = new WaveFormat(44100, 2);

                _loopbackCapture.DataAvailable += (s, a) =>
                {
                    float max = 0;
                    for (int index = 0; index < a.BytesRecorded; index += 4)
                    {
                        short sampleLeft = BitConverter.ToInt16(a.Buffer, index);
                        short sampleRight = BitConverter.ToInt16(a.Buffer, index + 2);
                        float sample32 = Math.Max(Math.Abs(sampleLeft / 32768f), Math.Abs(sampleRight / 32768f));
                        if (sample32 > max)
                            max = sample32;
                    }

                    float volume = max * 100;
                    OnVolumeChanged?.Invoke(volume);
                };

                _loopbackCapture.StartRecording();
            }
            catch (Exception ex)
            {
              
            }
        }

        public void Stop()
        {
            if (_loopbackCapture != null)
            {
                _loopbackCapture.StopRecording();
                _loopbackCapture.Dispose();
                _loopbackCapture = null;
            }
        }
    }
}