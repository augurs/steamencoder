using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using EncoderApp.Models;

namespace EncoderApp.ViewModels
{
    public class StreamsViewModel : INotifyPropertyChanged
    {
        private StreamModel _selectedStream;
        private StreamModel _temporaryStream; 
        private string _streamName;
        private string _selectedServerType;
        private string _hostNameOrIP;
        private int _port ;
        private string _mount;
        private string _userName;
        private string _password;
        private string _selectNWInterface;
        private string _selectedAudioCodec;
        private Visibility _streamsMetaDataModalVisibility;
        private Visibility _encoderSettingModalVisibility;
        private Visibility _streamDiagnosticModalVisibility;
        private bool _isDragging;
        private Point _startMousePosition;
        private Point _streamsMetaDataModalTransform;
        private Point _encoderSettingModalTransform;
        private Point _streamDiagnosticModalTransform;

        public event EventHandler StreamAdded;
        public event EventHandler CloseRequested;

        public ObservableCollection<StreamModel> AllStream { get; } = new ObservableCollection<StreamModel>();

        public StreamModel SelectedStream
        {
            get => _selectedStream;
            set
            {
                _selectedStream = value;
                OnPropertyChanged();
                if (_selectedStream != null)
                {
                    StreamName = _selectedStream.Name;
                    SelectedServerType = _selectedStream.ServerType;
                    HostNameOrIP = _selectedStream.HostNameOrIP;
                    Port = _selectedStream.Port;
                    Mount = _selectedStream.Mount;
                    UserName = _selectedStream.UserName;
                    Password = _selectedStream.Password;
                    SelectNWInterface = _selectedStream.SelectNWInterface;
                    SelectedAudioCodec = _selectedStream.AudioCodec;
                }
            }
        }

        public string StreamName
        {
            get => _streamName;
            set
            {
                _streamName = value;
                OnPropertyChanged();
            }
        }

        public string SelectedServerType
        {
            get => _selectedServerType;
            set
            {
                _selectedServerType = value;
                OnPropertyChanged();
            }
        }

        public string HostNameOrIP
        {
            get => _hostNameOrIP;
            set
            {
                _hostNameOrIP = value;
                OnPropertyChanged();
            }
        }

        public int Port
        {
            get => _port;
            set
            {
                if (value >= 1 && value <= 65535)
                {
                    _port = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Mount
        {
            get => _mount;
            set
            {
                _mount = value;
                OnPropertyChanged();
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged();
                AppConfigurationManager.WriteValue("UserName", value);
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged();
            }
        }

        public string SelectNWInterface
        {
            get => _selectNWInterface;
            set
            {
                _selectNWInterface = value;
                OnPropertyChanged();
            }
        }

        public string SelectedAudioCodec
        {
            get => _selectedAudioCodec;
            set
            {
                _selectedAudioCodec = value;
                OnPropertyChanged();
            }
        }

        public Visibility StreamsMetaDataModalVisibility
        {
            get => _streamsMetaDataModalVisibility;
            set
            {
                _streamsMetaDataModalVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility EncoderSettingModalVisibility
        {
            get => _encoderSettingModalVisibility;
            set
            {
                _encoderSettingModalVisibility = value;
                OnPropertyChanged();
            }
        }

        public Visibility StreamDiagnosticModalVisibility
        {
            get => _streamDiagnosticModalVisibility;
            set
            {
                _streamDiagnosticModalVisibility = value;
                OnPropertyChanged();
            }
        }

        public Point StreamsMetaDataModalTransform
        {
            get => _streamsMetaDataModalTransform;
            set
            {
                _streamsMetaDataModalTransform = value;
                OnPropertyChanged();
            }
        }

        public Point EncoderSettingModalTransform
        {
            get => _encoderSettingModalTransform;
            set
            {
                _encoderSettingModalTransform = value;
                OnPropertyChanged();
            }
        }

        public Point StreamDiagnosticModalTransform
        {
            get => _streamDiagnosticModalTransform;
            set
            {
                _streamDiagnosticModalTransform = value;
                OnPropertyChanged();
            }
        }

        public StreamsViewModel()
        {
            StreamsMetaDataModalVisibility = Visibility.Collapsed;
            EncoderSettingModalVisibility = Visibility.Collapsed;
            StreamDiagnosticModalVisibility = Visibility.Collapsed;
            StreamsMetaDataModalTransform = new Point(0, 0);
            EncoderSettingModalTransform = new Point(0, 0);
            StreamDiagnosticModalTransform = new Point(0, 0);
            AppConfigurationManager.LoadStreamsIntoCollection(AllStream);
            InititializeStreams();
        }
        public void InititializeStreams()
        {

            StreamName = "My Radio Station";
            SelectedServerType = "IceCast 2";
            HostNameOrIP = "Enter Host/IP";
            Port = 8000;
            Mount = "/Mount";
            UserName = "Enter UserName";
            Password = "Enter Password";
            SelectNWInterface = "Default";
            SelectedAudioCodec = "MP3 (128kbps)";
        }
     
        public void ShowStreamsMetaDataModal()
        {
            StreamsMetaDataModalVisibility = Visibility.Visible;
            EncoderSettingModalVisibility = Visibility.Collapsed;
            StreamDiagnosticModalVisibility = Visibility.Collapsed;
        }

        public void HideStreamsMetaDataModal()
        {
            StreamsMetaDataModalVisibility = Visibility.Collapsed;
        }

        public void ShowEncoderSettingModal()
        {
            EncoderSettingModalVisibility = Visibility.Visible;
            StreamsMetaDataModalVisibility = Visibility.Collapsed;
            StreamDiagnosticModalVisibility = Visibility.Collapsed;
        }

        public void HideEncoderSettingModal()
        {
            EncoderSettingModalVisibility = Visibility.Collapsed;
        }

        public void ShowStreamDiagnosticModal()
        {
            StreamDiagnosticModalVisibility = Visibility.Visible;
            StreamsMetaDataModalVisibility = Visibility.Collapsed;
            EncoderSettingModalVisibility = Visibility.Collapsed;
        }

        public void HideStreamDiagnosticModal()
        {
            StreamDiagnosticModalVisibility = Visibility.Collapsed;
        }

        public void Close()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }

        public void SaveAndClose()
        {
            if (_temporaryStream != null && ValidateStream(_temporaryStream))
            {
                _temporaryStream.Name = StreamName;
                _temporaryStream.ServerType = SelectedServerType;
                _temporaryStream.HostNameOrIP = HostNameOrIP;
                _temporaryStream.Port = Port;
                _temporaryStream.Mount = Mount;
                _temporaryStream.UserName = UserName;
                _temporaryStream.Password = Password;
                _temporaryStream.SelectNWInterface = SelectNWInterface;
                _temporaryStream.AudioCodec = SelectedAudioCodec;

                AllStream.Add(_temporaryStream);
                SelectedStream = _temporaryStream;
                _temporaryStream = null; 
                AppConfigurationManager.SaveStreamsToFile(AllStream);
                StreamAdded?.Invoke(this, EventArgs.Empty);
            }
            else if (SelectedStream != null && ValidateStream(SelectedStream))
            {
                SelectedStream.Name = StreamName;
                SelectedStream.ServerType = SelectedServerType;
                SelectedStream.HostNameOrIP = HostNameOrIP;
                SelectedStream.Port = Port;
                SelectedStream.Mount = Mount;
                SelectedStream.UserName = UserName;
                SelectedStream.Password = Password;
                SelectedStream.AudioCodec = SelectedAudioCodec;
                SelectedStream.SelectNWInterface = SelectNWInterface;
                AppConfigurationManager.SaveStreamsToFile(AllStream);
                StreamAdded?.Invoke(this, EventArgs.Empty);
            }
            Close();
        }

        public void AddNewStream()
        {
            _temporaryStream = new StreamModel
            {
                Name = "New Stream",
                ServerType = "IceCast 2",
                HostNameOrIP = "mystation.in.airtime.pro",
                Port = 8000,
                Mount = "",
                UserName = "source",
                Password = "",
                SelectNWInterface = "Default",
                AudioCodec = "MP3 (128kbps)",
                IsConnected = true
            };
            SelectedStream = _temporaryStream; 
        }

        public void CancelNewStream()
        {
            _temporaryStream = null; 
            SelectedStream = AllStream.FirstOrDefault(); 
        }

        public void RemoveStream()
        {
            if (_temporaryStream != null && SelectedStream == _temporaryStream)
            {
                _temporaryStream = null;
                SelectedStream = AllStream.FirstOrDefault();
            }
            else if (SelectedStream != null)
            {
                AllStream.Remove(SelectedStream);
                SelectedStream = AllStream.FirstOrDefault();
                AppConfigurationManager.SaveStreamsToFile(AllStream);
                StreamAdded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void CloneStream()
        {
            if (SelectedStream != null && SelectedStream != _temporaryStream)
            {
                var clone = new StreamModel
                {
                    Name = SelectedStream.Name + " Copy",
                    ServerType = SelectedServerType,
                    HostNameOrIP = HostNameOrIP,
                    Port = Port,
                    Mount = Mount,
                    UserName = UserName,
                    Password = Password,
                    AudioCodec = SelectedAudioCodec,
                    SelectNWInterface = SelectNWInterface,
                    IsConnected = true
                };
                AllStream.Add(clone);
                SelectedStream = clone;
                AppConfigurationManager.SaveStreamsToFile(AllStream);
                StreamAdded?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool ValidateStream(StreamModel stream)
        {
            if (string.IsNullOrWhiteSpace(stream.Name))
                return false;
            if (string.IsNullOrWhiteSpace(stream.HostNameOrIP))
                return false;
            if (stream.Port < 1 || stream.Port > 65535)
                return false;
            return true;
        }

        public void StreamNameGotFocus()
        {
            if (StreamName == "My Radio Station")
            {
                StreamName = "";
            }
        }

        public void StreamNameLostFocus()
        {
            if (string.IsNullOrWhiteSpace(StreamName))
            {
                StreamName = "My Radio Station";
            }
        }
        public void HostNameGotFocus()
        {
            if (HostNameOrIP == "Enter Host/IP")
            {
                HostNameOrIP = "";
            }
        }

        public void HostNameLostFocus()
        {
            if (string.IsNullOrWhiteSpace(HostNameOrIP))
            {
                HostNameOrIP = "Enter Host/IP";
            }
        }
        public void MountGotFocus()
        {
            if (Mount == "/Mount")
            {
                Mount = "";
            }
        }

        public void MountLostFocus()
        {
           
            if (string.IsNullOrWhiteSpace(Mount))
            {
                Mount = "/Mount";
            }
        }
        public void UserNameGotFocus()
        {
            if (UserName == "Enter UserName")
            {
                UserName = "";
            }
        }

        public void UserNameLostFocus()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                UserName = "Enter UserName";
            }
        }
        public void PassGotFoucs()
        {
            if (Password == "Enter Password")
            {
                Password = "";
            }
        }

        public void PassLostFocus()
        {
            if (string.IsNullOrWhiteSpace(Password))
            {
                Password = "Enter Password";
            }
        }
        public void ComboBoxPreviewMouseLeftButtonDown(object sender)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.Focus();
                comboBox.IsDropDownOpen = true;
            }
        }

        public void ModalHeaderMouseLeftButtonDown(object sender, MouseButtonEventArgs e, string modalName)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                _isDragging = true;
                _startMousePosition = e.GetPosition(null);
                (sender as UIElement)?.CaptureMouse();
            }
        }

        public void ModalHeaderMouseMove(object sender, MouseEventArgs e, string modalName)
        {
            if (_isDragging)
            {
                Point currentMousePosition = e.GetPosition(null);
                Vector delta = currentMousePosition - _startMousePosition;
                if (modalName == "StreamsMetaDataModal")
                {
                    StreamsMetaDataModalTransform = new Point(StreamsMetaDataModalTransform.X + delta.X, StreamsMetaDataModalTransform.Y + delta.Y);
                }
                else if (modalName == "EncoderSettingModal")
                {
                    EncoderSettingModalTransform = new Point(EncoderSettingModalTransform.X + delta.X, EncoderSettingModalTransform.Y + delta.Y);
                }
                else if (modalName == "StreamDiagnosticModal")
                {
                    StreamDiagnosticModalTransform = new Point(StreamDiagnosticModalTransform.X + delta.X, StreamDiagnosticModalTransform.Y + delta.Y);
                }
                _startMousePosition = currentMousePosition;
            }
        }

        public void ModalHeaderMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                (sender as UIElement)?.ReleaseMouseCapture();
            }
        }

        public void StreamsMetaDataModalMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Grid)
            {
                StreamsMetaDataModalVisibility = Visibility.Collapsed;
            }
        }

        public void StreamsMetaDataModalKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                StreamsMetaDataModalVisibility = Visibility.Collapsed;
            }
        }

        public void IncrementPort()
        {
            Port = Math.Min(Port + 1, 65535);
        }

        public void DecrementPort()
        {
            Port = Math.Max(Port - 1, 1);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}