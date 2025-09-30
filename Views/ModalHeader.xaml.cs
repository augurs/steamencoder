
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EncoderApp.Views
{
    /// <summary>
    /// Interaction logic for HeaderControl.xaml
    /// </summary>
    public partial class ModalHeader : UserControl
    {
        public event RoutedEventHandler CloseClicked;

        public ModalHeader()
        {
            InitializeComponent();

        }
        //private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        Window parentWindow = Window.GetWindow(this);
        //        if (parentWindow != null)
        //        {
        //            parentWindow.DragMove();
        //        }
        //    }
        //}

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            CloseClicked?.Invoke(this, e); 
        }

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(ModalHeader), new PropertyMetadata(""));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }
    }
}
