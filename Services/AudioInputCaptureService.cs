using EncoderApp.Models;
using EncoderApp.Views;
using NAudio.Wave;
using System.Windows;

public class AudioInputCaptureService
{
    private static AudioInputCaptureService _instance;
    public static AudioInputCaptureService Instance => _instance ??= new AudioInputCaptureService();

    private WaveInEvent _waveIn;
    private readonly int sampleRate = 44100;
    private readonly int channels = 1;
    private string _selectedApi = "Windows WASAPI";
    public string SelectedApi
    {
        get => _selectedApi;
        set => _selectedApi = value; 
    }

    public event Action<float> OnVolumeChanged;

    /// <summary>
    /// Start capturing audio from the selected API and device index.
    /// </summary>
    public void Start(int deviceIndex = 0, string audioApi = null)
    {
        Stop(); 

        string apiToUse = audioApi ?? _selectedApi;

        switch (apiToUse)
        {
            case "ASIO":
                CustomMessageBox.ShowInfo("ASIO input is not supported. Using WASAPI.", "Info");
                goto case "Windows WASAPI";

            case "Windows WASAPI":

            case "0":
                StartWasapi(deviceIndex);
                break;
        }
    }
    public event Action<string> OnError;

    private void StartWasapi(int deviceIndex)
    {
        if (WaveInEvent.DeviceCount == 0)
        {
            OnError?.Invoke("No input devices found.");
            return;
        }
    }
    public void Stop()
    {
        if (_waveIn != null)
        {
            _waveIn.DataAvailable -= WaveIn_DataAvailable;
            _waveIn.RecordingStopped -= WaveIn_RecordingStopped;
            try { _waveIn.StopRecording(); } catch { }
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
        try
        {
            if (e.Exception != null)
            {
                CustomMessageBox.ShowInfo($"Error capturing audio: {e.Exception.Message}", "Error");
            }
            Stop();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
      
    }
}
