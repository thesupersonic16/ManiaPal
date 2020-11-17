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
using System.Windows.Shapes;

namespace ManiaPal
{
    /// <summary>
    /// Interaction logic for ManiaPalSettings.xaml
    /// </summary>
    public partial class ManiaPalSettings : Window
    {

        public MainWindow MainWindow;

        public ManiaPalSettings(MainWindow window)
        {
            MainWindow = window;
            InitializeComponent();
        }

        public void Reload()
        {
            if (UI_Control.Children.Count > 0)
                UI_Control.Children.RemoveAt(0);
            if (MainWindow.LoadedFileType != null && MainWindow.LoadedFileType.OptionsControl != null)
                UI_Control.Children.Add(MainWindow.LoadedFileType.OptionsControl);
        }

        private void UI_OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = MainWindow;
            if (MainWindow.LoadedFileType != null && MainWindow.LoadedFileType.OptionsControl != null)
                UI_Control.Children.Add(MainWindow.LoadedFileType.OptionsControl);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (UI_Control.Children.Count > 0)
                UI_Control.Children.RemoveAt(0);
            MainWindow.SettingWindow = null;
        }

        private void UI_About_Click(object sender, RoutedEventArgs e)
        {
            new AboutWindow().ShowDialog();
        }
    }
}
