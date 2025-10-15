using EncoderApp.Helpers; // Make sure RelayCommand is here
using EncoderApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace EncoderApp.ViewModels
{
    public class MetaDataViewModel : INotifyPropertyChanged
    {
        #region Properties

        private ObservableCollection<AppItem> _activeApps = new ObservableCollection<AppItem>();
        public ObservableCollection<AppItem> ActiveApps
        {
            get => _activeApps;
            set
            {
                _activeApps = value;
                OnPropertyChanged(nameof(ActiveApps));
            }
        }

        private string _selectedAppText;
        public string SelectedAppText
        {
            get => _selectedAppText;
            set
            {
                _selectedAppText = value;
                OnPropertyChanged(nameof(SelectedAppText));
            }
        }
        private AppItem _selectedApp;
        public AppItem SelectedApp
        {
            get => _selectedApp;
            set
            {
                _selectedApp = value;
                if (_selectedApp != null)
                {
                    SelectedAppText = _selectedApp.WindowTitle ?? _selectedApp.ProcessName ?? "No Selection";
                    SelectedIcon = _selectedApp.IconPath; 
                }
                else
                {
                    SelectedAppText = "No Selection";
                    SelectedIcon = null;
                }
                OnPropertyChanged(nameof(SelectedApp));
                OnPropertyChanged(nameof(SelectedAppText));
                OnPropertyChanged(nameof(SelectedIcon));
                UpdateMetadata();
            }
        }

        private string _selectedIcon;
        public string SelectedIcon
        {
            get => _selectedIcon;
            set
            {
                _selectedIcon = value;
                OnPropertyChanged(nameof(SelectedIcon));
            }
        }
        private bool _onlyListSoundingWindows; // Corrected from _onlyListSoundingWinodws
        public bool OnlyListSoundingWindows
        {
            get => _onlyListSoundingWindows;
            set
            {
                _onlyListSoundingWindows = value;
                OnPropertyChanged(nameof(OnlyListSoundingWindows));
                LoadActiveApps();
            }
        }
        private string _artist;
        public string Artist
        {
            get => _artist;
            set
            {
                _artist = value;
                OnPropertyChanged(nameof(Artist));
            }
        }
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
        private string _selectedFormat;
        public string SelectedFormat
        {
            get => _selectedFormat;
            set
            {
                _selectedFormat = value;
                OnPropertyChanged(nameof(SelectedFormat));
                UpdateMetadata();
            }
        }
        #endregion

        #region Constructor

        public MetaDataViewModel()
        {
            LoadActiveApps();
        }

        #endregion

        #region Methods
        public void UpdateMetadata()
        {
            if (string.IsNullOrWhiteSpace(SelectedApp?.WindowTitle))
            {
                Artist = string.Empty;
                Title = string.Empty;
                return;
            }

            string windowTitle = SelectedApp.WindowTitle.Trim();
            var parts = windowTitle.Split(new[] { " - ", "-", "–" }, StringSplitOptions.RemoveEmptyEntries);

            Artist = string.Empty;
            Title = string.Empty;

            switch (_selectedFormat)
            {
                case "rbArtistWindow":
                    if (parts.Length >= 2)
                    {
                        Artist = parts[0].Trim();
                        Title = parts[1].Trim();
                    }
                    else
                    {
                        Artist = windowTitle;
                    }
                    break;
                case "rbWindowArtist":
                    if (parts.Length >= 3)
                    {
                        Artist = parts[1].Trim();
                        Title = parts[2].Trim();
                    }
                    else if (parts.Length == 2)
                    {
                        Artist = parts[0].Trim();
                        Title = parts[1].Trim();
                    }
                    else
                    {
                        Artist = windowTitle;
                    }
                    break;
                case "rbArtistTitle":
                    if (parts.Length >= 2)
                    {
                        Artist = parts[0].Trim() + " - " + parts[1].Trim();
                        Title = parts[2].Trim();
                    }
                    else
                    {
                        Artist = windowTitle;
                    }
                    break;
                case "rbCustom":
                    Artist = parts[0].Trim();
                    Title = parts[1].Trim();
                    break;
                default:
                    Artist = windowTitle;
                    break;
            }
        }
        private bool IsProcessMakingSound(Process process)
        {
            try
            {
                if (process == null || process.HasExited || string.IsNullOrEmpty(process.ProcessName))
                    return false;

                string processName = process.ProcessName.ToLower();
                return processName.Contains("media") || processName.Contains("player") || processName.Contains("audio");
            }
            catch (Exception ex)
            {
                return false; 
            }
        }
        public void LoadActiveApps()
        {
            ActiveApps.Clear();

            var currentProcess = Process.GetCurrentProcess(); 

            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle) && p.MainWindowHandle != IntPtr.Zero)
                .OrderBy(p => p.ProcessName);

            try
            {
                if (OnlyListSoundingWindows)
                {
                    processes = (IOrderedEnumerable<Process>)processes
                        .Where(p => IsProcessMakingSound(p))
                        .ToList();
                }
                else
                {
                    var recentlyOpened = new List<Process>();
                    try
                    {
                        recentlyOpened = processes
                            .Where(p =>
                            {
                                try
                                {
                                    return DateTime.Now - Process.GetProcessById(p.Id).StartTime < TimeSpan.FromSeconds(5);
                                }
                                catch
                                {
                                    return false;
                                }
                            })
                            .ToList();
                    }
                    catch { }

                    foreach (var process in processes)
                    {
                        try
                        {
                            bool isExcluded =
                                process.Id == currentProcess.Id || 
                                process.ProcessName.Equals(currentProcess.ProcessName, StringComparison.OrdinalIgnoreCase) ||
                                process.ProcessName.Equals("explorer", StringComparison.OrdinalIgnoreCase) || 
                                process.MainWindowTitle.Contains("File Explorer", StringComparison.OrdinalIgnoreCase) ||
                                process.MainWindowTitle.Contains("This PC", StringComparison.OrdinalIgnoreCase) ||
                                process.MainWindowTitle.Contains("Documents", StringComparison.OrdinalIgnoreCase) ||
                                process.MainWindowTitle.Contains("Downloads", StringComparison.OrdinalIgnoreCase) ||
                                process.ProcessName.IndexOf("Database", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                process.MainWindowTitle.IndexOf("Database", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                process.ProcessName.IndexOf("Code", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                process.MainWindowTitle.IndexOf("Code", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                process.ProcessName.IndexOf("SQL", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                process.MainWindowTitle.IndexOf("SQL", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                process.ProcessName == "sqlservr" ||
                                process.ProcessName == "mysqld" ||
                                process.ProcessName == "postgres";

                            if (isExcluded)
                                continue;

                            string iconPath = null;
                            try
                            {
                                using (var icon = Icon.ExtractAssociatedIcon(process.MainModule.FileName))
                                {
                                    if (icon != null)
                                    {
                                        using (var bitmap = icon.ToBitmap())
                                        {
                                            var tempFile = Path.GetTempFileName() + ".png";
                                            bitmap.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);
                                            if (File.Exists(tempFile))
                                                iconPath = new Uri(tempFile, UriKind.Absolute).AbsoluteUri;
                                        }
                                    }
                                }
                            }
                            catch { }

                            ActiveApps.Add(new AppItem
                            {
                                ProcessName = process.ProcessName,
                                WindowTitle = process.MainWindowTitle,
                                ProcessId = process.Id,
                                IconPath = iconPath,
                                IsRecent = recentlyOpened.Any(r => r.Id == process.Id)
                            });
                        }
                        catch { }
                    }

                    ActiveApps = new ObservableCollection<AppItem>(ActiveApps.OrderByDescending(a => a.IsRecent));
                }
            }
            catch { }
        }


        public void Initialize() 
        {
            LoadActiveApps();
        }
        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}