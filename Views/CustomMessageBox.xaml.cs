using System.Windows;
using System.Windows.Controls;

namespace EncoderApp.Views
{
    public partial class CustomMessageBox : UserControl
    {
        public event RoutedEventHandler OkClicked;
        public event RoutedEventHandler CancelClicked;

        public CustomMessageBox(string message, string titleType, bool showCancel = true)
        {
            InitializeComponent();

            txtMessage.Text = message;
            txtTitle.Text = titleType;
            ApplyIcon(titleType);

            CancelButton.Visibility = showCancel ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ApplyIcon(string titleType)
        {
            switch (titleType)
            {
                case "Warning":
                    txtIcon.Text = "⚠";
                    txtIcon.Foreground = System.Windows.Media.Brushes.Orange;
                    break;
                case "Error":
                    txtIcon.Text = "❌";
                    txtIcon.Foreground = System.Windows.Media.Brushes.Red;
                    break;
                case "Success":
                    txtIcon.Text = "✅";
                    txtIcon.Foreground = System.Windows.Media.Brushes.LimeGreen;
                    break;
                default:
                    txtIcon.Text = "";
                    break;
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            OkClicked?.Invoke(this, e);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelClicked?.Invoke(this, e);
        }

        private void Header_CloseClicked(object sender, RoutedEventArgs e)
        {
            OkClicked?.Invoke(this, e);
        }

        public static void ShowInfo(string message, string titleType)
        {
            Window window = new Window
            {
                Content = new CustomMessageBox(message, titleType, showCancel: false),
                Width = 330,
                Height = 190,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Background = null,
                AllowsTransparency = true
            };

            if (window.Content is CustomMessageBox msgBox)
            {
                msgBox.OkClicked += (s, e) => window.Close();
            }

            window.ShowDialog();
        }
      
        public static bool ShowConfirmation(string message, string titleType)
        {
            bool result = false;

            Window window = new Window
            {
                Content = new CustomMessageBox(message, titleType, showCancel: true),
                Width = 330,
                Height = 190,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Background = null,
                AllowsTransparency = true
            };

            if (window.Content is CustomMessageBox msgBox)
            {
                msgBox.OkClicked += (s, e) =>
                {
                    result = true;
                    window.Close();
                };
                msgBox.CancelClicked += (s, e) =>
                {
                    result = false;
                    window.Close();
                };
            }

            window.ShowDialog();
            return result;
        }

    }
}
