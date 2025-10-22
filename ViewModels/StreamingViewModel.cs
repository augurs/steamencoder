using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using NAudio.CoreAudioApi;
using NAudio.MediaFoundation;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using NAudio.Dsp;
using Concentus.Enums;
using Concentus.Oggfile;
using Concentus.Structs;
using System.Windows.Threading;
using EncoderApp.Views;


namespace EncoderApp.ViewModels
{
    public class StreamingViewModel
    {
        private bool isStreaming;
        private HttpClient? httpClient;
        private CancellationTokenSource? cts;
        private BlockingCollection<byte[]>? sendQueue;
        private WasapiLoopbackCapture? systemCapture;
        private BufferedWaveProvider? systemBuffer;
        private bool systemEnabled = true;
        private WasapiCapture? micCapture;
        private BufferedWaveProvider? micBuffer;
        private VolumeSampleProvider? sysVol;
        private VolumeSampleProvider? micVol;
        private bool micEnabled = true;
        private readonly WaveFormat outputFormat = WaveFormat.CreateIeeeFloatWaveFormat(48000, 2);
       
        private sealed class SmoothLimiter : ISampleProvider
        {
            private readonly ISampleProvider source;
            private readonly float threshold;
            private readonly float releaseRate;
            private float gain = 1f;

            public SmoothLimiter(ISampleProvider src, float thresholdDb = -3f, float releaseRate = 0.0005f)
            {
                source = src;
                threshold = (float)Math.Pow(10.0, thresholdDb / 20.0);
                this.releaseRate = releaseRate;
                WaveFormat = src.WaveFormat;
            }

            public WaveFormat WaveFormat { get; }

            public int Read(float[] buffer, int offset, int count)
            {
                int read = source.Read(buffer, offset, count);
                for (int i = offset; i < offset + read; i++)
                {
                    float x = buffer[i] * gain;
                    float abs = Math.Abs(x);

                    if (abs > threshold)
                        gain *= 0.98f;
                    else
                        gain += (1f - gain) * releaseRate;

                    buffer[i] = Math.Max(-1f, Math.Min(1f, x));
                }
                return read;
            }
        }
        public void StartStreaming(string iceCastUrlive="")
        {
            if (isStreaming)
            {
                //SetStatus("Already streaming...", "Orange");
                CustomMessageBox.ShowInfo("Already streaming...", "Warning");
                return;
            }

            try
            {
                string icecastUrl = "";
                icecastUrl = iceCastUrlive;
                string sourcePassword = "Aa_3o2$8E";
                CustomMessageBox.ShowInfo("Connecting to Icecast...", "Success");
                //SetStatus("Connecting to Icecast...", "Yellow");

                httpClient = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                    "Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($"source:{sourcePassword}")));
                httpClient.DefaultRequestHeaders.Add("ice-name", "Augurs System + Mic Streamer");
                httpClient.DefaultRequestHeaders.Add("ice-description", "Live system + mic stream");
                httpClient.DefaultRequestHeaders.Add("ice-genre", "Live");
                httpClient.DefaultRequestHeaders.Add("ice-public", "1");
                httpClient.DefaultRequestHeaders.Add("User-Agent", "NAudio-Icecast-Streamer");

                cts = new CancellationTokenSource();
                sendQueue = new BlockingCollection<byte[]>(new ConcurrentQueue<byte[]>(), 800);

                // --- System capture ---
                systemCapture = new WasapiLoopbackCapture();
                systemBuffer = new BufferedWaveProvider(systemCapture.WaveFormat)
                {
                    BufferLength = systemCapture.WaveFormat.AverageBytesPerSecond * 6,
                    DiscardOnBufferOverflow = true
                };
                systemCapture.DataAvailable += (s, a) =>
                {
                    try
                    {
                        if (isStreaming && systemEnabled)
                            systemBuffer!.AddSamples(a.Buffer, 0, a.BytesRecorded);
                    }
                    catch { Debug.WriteLine("⚠️ System buffer overflow"); }
                };

                // --- Microphone capture ---
                micCapture = new WasapiCapture();
                micBuffer = new BufferedWaveProvider(micCapture.WaveFormat)
                {
                    BufferLength = micCapture.WaveFormat.AverageBytesPerSecond * 6,
                    DiscardOnBufferOverflow = true
                };
                micCapture.DataAvailable += (s, a) =>
                {
                    try
                    {
                        if (isStreaming && micEnabled)
                            micBuffer!.AddSamples(a.Buffer, 0, a.BytesRecorded);
                    }
                    catch { Debug.WriteLine("⚠️ Mic buffer overflow"); }
                };

                // --- Resample to 48 kHz stereo float ---
                var sysResampler = new MediaFoundationResampler(systemBuffer, outputFormat) { ResamplerQuality = 60 };
                var micResampler = new MediaFoundationResampler(micBuffer, WaveFormat.CreateIeeeFloatWaveFormat(48000, 1))
                { ResamplerQuality = 60 };

                var sysProvider = sysResampler.ToSampleProvider();
                var micProvider = new MonoToStereoSampleProvider(micResampler.ToSampleProvider());

                sysVol = new VolumeSampleProvider(sysProvider) { Volume = systemEnabled ? 0.85f : 0f };
                micVol = new VolumeSampleProvider(micProvider) { Volume = micEnabled ? 0.95f : 0f };

                var mixer = new MixingSampleProvider(new[] { sysVol, micVol }) { ReadFully = true };
                var limited = new SmoothLimiter(mixer, -3f, 0.0007f);

                // --- Encoder thread ---
                var encoderThread = new Thread(() =>
                {
                    try
                    {
                        Thread.CurrentThread.Priority = ThreadPriority.Highest;
                        const int frameDurationMs = 40;
                        int samplesPerFrame = outputFormat.SampleRate * outputFormat.Channels * frameDurationMs / 1000;

                        var floatBuffer = new float[samplesPerFrame];
                        var pcmShort = new short[samplesPerFrame];

                        using (var ms = new MemoryStream())
                        {
                            var encoder = new OpusEncoder(48000, 2, OpusApplication.OPUS_APPLICATION_AUDIO)
                            {
                                Bitrate = 192000,
                                Complexity = 10,
                                SignalType = OpusSignal.OPUS_SIGNAL_AUTO,
                                UseVBR = true
                            };

                            var oggOut = new OpusOggWriteStream(encoder, ms);
                            long streamPos = 0;

                            long frameTicks = frameDurationMs * Stopwatch.Frequency / 1000;
                            long nextTick = Stopwatch.GetTimestamp() + frameTicks;

                            while (!cts!.IsCancellationRequested)
                            {
                                int samplesRead = limited.Read(floatBuffer, 0, floatBuffer.Length);
                                if (samplesRead == 0)
                                {
                                    Array.Clear(floatBuffer, 0, floatBuffer.Length);
                                    samplesRead = floatBuffer.Length;
                                }

                                for (int i = 0; i < samplesRead; i++)
                                    pcmShort[i] = (short)(Math.Clamp(floatBuffer[i], -1f, 1f) * short.MaxValue);

                                oggOut.WriteSamples(pcmShort, 0, samplesRead);

                                int newBytes = (int)(ms.Length - streamPos);
                                if (newBytes > 0)
                                {
                                    var data = new byte[newBytes];
                                    ms.Position = streamPos;
                                    ms.Read(data, 0, newBytes);
                                    streamPos = ms.Position;
                                    sendQueue!.Add(data, cts.Token);
                                }

                                if (sendQueue.Count > 700)
                                    Thread.Sleep(5);

                                while (Stopwatch.GetTimestamp() < nextTick)
                                    Thread.SpinWait(100);
                                nextTick += frameTicks;
                            }
                            oggOut.Finish();
                        }
                    }
                    catch (OperationCanceledException) { }
                    finally { try { sendQueue!.CompleteAdding(); } catch { } }
                });
                encoderThread.IsBackground = true;
                encoderThread.Start();

                // --- Stream to Icecast ---
                var streamContent = new PushStreamContent((outputStream, _, _) =>
                {
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            if (sendQueue!.TryTake(out var first, 2000, cts!.Token))
                            {
                                await outputStream.WriteAsync(first, 0, first.Length, cts.Token);
                                await outputStream.FlushAsync(cts.Token);
                            }

                            int flushCounter = 0;
                            while (!cts.Token.IsCancellationRequested)
                            {
                                if (sendQueue.TryTake(out var chunk, 300, cts.Token))
                                {
                                    await outputStream.WriteAsync(chunk, 0, chunk.Length, cts.Token);
                                    flushCounter += chunk.Length;

                                    if (flushCounter > 24000)
                                    {
                                        await outputStream.FlushAsync(cts.Token);
                                        flushCounter = 0;
                                    }

                                    await Task.Delay(2, cts.Token);
                                }
                                else
                                    await Task.Delay(5, cts.Token);

                                if (sendQueue.IsCompleted && sendQueue.Count == 0)
                                    break;
                            }
                        }
                        catch (OperationCanceledException) { }
                        finally
                        {
                            try { await outputStream.FlushAsync(); outputStream.Close(); } catch { }
                        }
                    });
                }, "application/ogg");

                systemCapture.StartRecording();
                micCapture.StartRecording();

                var responseTask = httpClient.PutAsync(icecastUrl, streamContent, cts.Token);
                isStreaming = true;
               // SetStatus("🎶 Streaming (Natural + Stable, Drift-Free)", "LimeGreen");
                CustomMessageBox.ShowInfo("🎶 Streaming (Natural + Stable, Drift-Free)", "Success");
                _ = Task.Run(async () =>
                {
                    var response = await responseTask;
                    if (!response.IsSuccessStatusCode)
                        CustomMessageBox.ShowInfo($"❌ Icecast failed: {response.StatusCode}", "Error");
                        //Dispatcher.Invoke(() => SetStatus($"❌ Icecast failed: {response.StatusCode}", "Red"));
                });
            }
            catch (Exception ex)
            {
               // SetStatus($"Error: {ex.Message}", "Red");
                CustomMessageBox.ShowInfo($"Error: {ex.Message}", "Error");
                StopStreaming();
            }
        }
        public void StopStreaming()
        {
            if (!isStreaming) return;
            CustomMessageBox.ShowInfo("Stopping...", "Warning");
           // SetStatus("Stopping...", "Orange");
            isStreaming = false;

            try
            {
                cts?.Cancel();
                systemCapture?.StopRecording();
                micCapture?.StopRecording();
                systemCapture?.Dispose();
                micCapture?.Dispose();
                sendQueue?.CompleteAdding();
                httpClient?.Dispose();
                CustomMessageBox.ShowInfo("Stopped ❌", "Success");
                //SetStatus("Stopped ❌", "Red");
            }
            catch (Exception ex)
            {
                CustomMessageBox.ShowInfo($"Stop error: {ex.Message}", "Error");
               // SetStatus($"Stop error: {ex.Message}", "Red");
            }
        }

    }
}
