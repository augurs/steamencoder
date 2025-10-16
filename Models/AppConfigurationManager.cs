using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Xml.Linq;

namespace EncoderApp.Models
{
    public static class AppConfigurationManager
    {
        private static readonly string _filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EncoderApp", "ApplicationSetting.xml");
        private static readonly string _streamsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StreamsData.xml");

        private static readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        static AppConfigurationManager()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_filePath));
        }

        public static void WriteValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            try
            {
                _lock.EnterWriteLock();
                try
                {
                    XDocument doc;
                    if (File.Exists(_filePath))
                    {
                        doc = XDocument.Load(_filePath);
                    }
                    else
                    {
                        doc = new XDocument(new XElement("Settings"));
                    }

                    XElement root = doc.Element("Settings");
                    if (root == null)
                    {
                        root = new XElement("Settings");
                        doc.Add(root);
                    }

                    XElement setting = root.Element(key);
                    if (setting == null)
                    {
                        setting = new XElement(key);
                        root.Add(setting);
                    }
                    setting.Value = value;

                    doc.Save(_filePath);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error writing setting '{key}': {ex.Message}");
            }
        }

        public static string ReadValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                _lock.EnterReadLock();
                try
                {
                    if (!File.Exists(_filePath))
                    {
                        return null;
                    }

                    XDocument doc = XDocument.Load(_filePath);
                    XElement setting = doc.Element("Settings")?.Element(key);
                    return setting?.Value;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading setting '{key}': {ex.Message}");
                return null;
            }
        }

        public static void LoadStreamsIntoCollection(ObservableCollection<StreamInfo> streams)
        {
            if (streams == null)
                throw new ArgumentNullException(nameof(streams));

            try
            {
                _lock.EnterReadLock();
                try
                {
                    if (!File.Exists(_streamsFilePath))
                    {
                        streams.Clear();
                        streams.Add(new StreamInfo { Name = "Test Stream", Mount = "Mount:/test", IsConnected = true });
                        return;
                    }

                    XDocument doc = XDocument.Load(_streamsFilePath);
                    var rootElement = doc.Element("Data");
                    if (rootElement == null)
                    {
                        streams.Clear();
                        return;
                    }

                    var streamElements = rootElement.Elements("Stream");
                    if (!streamElements.Any())
                    {
                        streams.Clear();
                        return;
                    }

                    streams.Clear();
                    foreach (var streamElement in streamElements)
                    {
                        var nameElement = streamElement.Element("Name");
                        var mountElement = streamElement.Element("Mount");
                        var isConnectedElement = streamElement.Element("IsConnected");
                        var isErrorElement = streamElement.Element("IsError");

                        var stream = new StreamInfo
                        {
                            Name = nameElement?.Value ?? "Unknown",
                            Mount = mountElement?.Value ?? "Unknown",
                            IsConnected = bool.TryParse(isConnectedElement?.Value, out bool isConnected) ? isConnected : false,
                            IsError = bool.TryParse(isErrorElement?.Value, out bool isError) ? isError : false
                        };

                        streams.Add(stream);
                    }
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading streams: {ex.Message}");
            }
        }
        public static void LoadStreamsIntoCollection(ObservableCollection<StreamModel> streams)
        {
            if (streams == null) throw new ArgumentNullException(nameof(streams));

            try
            {
                _lock.EnterReadLock();
                if (!File.Exists(_streamsFilePath))
                {
                    streams.Clear();
                    streams.Add(new StreamModel { Name = "Test Stream", Mount = "Mount:/test", IsConnected = true });
                    SaveStreamsToFile(streams);
                    return;
                }

                XDocument doc = XDocument.Load(_streamsFilePath);
                var rootElement = doc.Element("Data");
                if (rootElement == null)
                {
                    streams.Clear();
                    return;
                }

                var streamElements = rootElement.Elements("Stream");
                if (!streamElements.Any())
                {
                    streams.Clear();
                    return;
                }

                streams.Clear();
                foreach (var streamElement in streamElements)
                {
                    var stream = new StreamModel
                    {
                        Name = streamElement.Element("Name")?.Value ?? "Unknown",
                        ServerType = streamElement.Element("ServerType")?.Value ?? "IceCast 2",
                        HostNameOrIP = streamElement.Element("HostNameOrIP")?.Value ?? "",
                        Port = int.TryParse(streamElement.Element("Port")?.Value, out int port) ? port : 8000,
                        Mount = streamElement.Element("Mount")?.Value ?? "",
                        UserName = streamElement.Element("UserName")?.Value ?? "",
                        Password = streamElement.Element("Password")?.Value ?? "",
                        AudioCodec = streamElement.Element("AudioCodec")?.Value ?? "MP3 (128kbps)",
                        SelectNWInterface = streamElement.Element("SelectNWInterface")?.Value ?? "Default"
              
                    };
                    streams.Add(stream);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public static void SaveStreamsToFile(ObservableCollection<StreamModel> streams)
        {
            if (streams == null) throw new ArgumentNullException(nameof(streams));

            try
            {
                _lock.EnterWriteLock();
                XDocument doc = new XDocument(new XElement("Data"));
                var root = doc.Root;
                foreach (var stream in streams)
                {
                    root?.Add(new XElement("Stream",
                        new XElement("Name", stream.Name),
                        new XElement("ServerType", stream.ServerType),
                        new XElement("HostNameOrIP", stream.HostNameOrIP),
                        new XElement("Port", stream.Port),
                        new XElement("Mount", stream.Mount),
                        new XElement("UserName", stream.UserName),
                        new XElement("Password", stream.Password),
                        new XElement("AudioCodec", stream.AudioCodec),
                        new XElement("SelectNWInterface", stream.SelectNWInterface)
                    ));
                }
                doc.Save(_streamsFilePath);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}