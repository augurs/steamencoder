using EncoderApp.Helpers; // Make sure RelayCommand is here
using EncoderApp.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
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
        public ICommand RefreshAppsCommand { get; }

        #endregion

        #region Constructor

        public MetaDataViewModel()
        {
            LoadActiveApps();
        }

        #endregion

        #region Methods

        public void LoadActiveApps()
        {
            ActiveApps.Clear();

            var processes = Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.MainWindowTitle) && p.MainWindowHandle != IntPtr.Zero)
                .OrderBy(p => p.ProcessName);

            foreach (var process in processes)
            {
                try
                {
                    // Debug output to see process details
                    Console.WriteLine($"Process: {process.ProcessName}, WindowTitle: {process.MainWindowTitle}");

                    // Enhanced filter to exclude "Database", "Code", and "SQL" related processes
                    bool isExcluded = (process.ProcessName?.IndexOf("Database", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      process.MainWindowTitle?.IndexOf("Database", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      process.ProcessName?.IndexOf("Code", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      process.MainWindowTitle?.IndexOf("Code", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      process.ProcessName?.IndexOf("SQL", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      process.MainWindowTitle?.IndexOf("SQL", StringComparison.OrdinalIgnoreCase) >= 0 ||
                                      process.ProcessName == "sqlservr" || // Common SQL Server process
                                      process.ProcessName == "mysqld" ||   // MySQL process
                                      process.ProcessName == "postgres");  // PostgreSQL process

                    if (isExcluded)
                    {
                        Console.WriteLine($"Excluded: {process.ProcessName} - {process.MainWindowTitle}");
                        continue; // Skip this process
                    }

                    string iconPath = null;
                    Icon icon = null;
                    try
                    {
                        icon = Icon.ExtractAssociatedIcon(process.MainModule.FileName);
                        if (icon != null)
                        {
                            using (var bitmap = icon.ToBitmap())
                            {
                                var tempFile = System.IO.Path.GetTempFileName() + ".png";
                                bitmap.Save(tempFile, System.Drawing.Imaging.ImageFormat.Png);
                                iconPath = new Uri(tempFile, UriKind.Absolute).AbsoluteUri;
                                Console.WriteLine($"Icon saved to: {iconPath}");
                            }
                        }
                    }
                    catch (Exception ex) { Console.WriteLine($"Icon error: {ex.Message}"); }

                    ActiveApps.Add(new AppItem
                    {
                        ProcessName = process.ProcessName,
                        WindowTitle = process.MainWindowTitle,
                        ProcessId = process.Id,
                        IconPath = iconPath
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Process error: {ex.Message}");
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }
}