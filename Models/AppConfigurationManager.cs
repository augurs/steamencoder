using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace EncoderApp.Models
{
    public static class AppConfigurationManager
    {
        private static readonly string _filePath = "ApplicationSetting.xml";
        private static readonly string _streamsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "StreamsData.xml");
        public static void LoadStreamsIntoCollection(ObservableCollection<StreamInfo> streams)
        {
            if (streams == null)
            {
                throw new ArgumentNullException(nameof(streams));
            }
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
                    return;
                }
                if (rootElement == null)
                {
                    return;
                }
                var streamElements = rootElement.Elements("Stream");
                if (!streamElements.Any())
                {
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
            catch (Exception ex)
            {
               
            }
        }
        public static void WriteValue(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));
            if (value == null)
                throw new ArgumentNullException(nameof(value));
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
            catch (Exception ex)
            {
            }
        }

        public static string ReadValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key));

            try
            {
                if (!File.Exists(_filePath))
                {
                    return null;
                }

                XDocument doc = XDocument.Load(_filePath);
                XElement setting = doc.Element("Settings")?.Element(key);
                if (setting == null)
                {
                    return null;
                }

                return setting.Value;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
