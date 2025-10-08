using EncoderApp.Views;
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
        private readonly object _lock = new object();
        public event Action<float> OnVolumeChanged;

        public void Start(int deviceIndex = 0)
        {
            lock (_lock)
            {
                if (_loopbackCapture != null) return;

                try
                {
                    _loopbackCapture = new WasapiLoopbackCapture();
                    _loopbackCapture.WaveFormat = new WaveFormat(44100, 2);
                    _loopbackCapture.DataAvailable += LoopbackCapture_DataAvailable;
                    _loopbackCapture.StartRecording();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to start loopback capture: {ex.Message}");
                    _loopbackCapture = null; // Ensure null on failure
                }
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                if (_loopbackCapture != null)
                {
                    try
                    {
                        _loopbackCapture.DataAvailable -= LoopbackCapture_DataAvailable;
                        _loopbackCapture.StopRecording();
                        _loopbackCapture.Dispose();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error stopping loopback: {ex.Message}");
                    }
                    finally
                    {
                        _loopbackCapture = null;
                    }
                }
            }
        }

        private void LoopbackCapture_DataAvailable(object s, WaveInEventArgs a)
        {
            lock (_lock)
            {
                if (_loopbackCapture == null) return; // Prevent processing if disposed

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
            }
        }
    }
}