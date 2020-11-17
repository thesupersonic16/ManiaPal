using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }


        private void UI_Window_Loaded(object sender, RoutedEventArgs e)
        {
            Title = $"About {App.ProgramTitle} v{App.ProgramVersion}";
            UI_Title.Content = $"{App.ProgramTitle} ({App.ProgramName}) v{App.ProgramVersion}";
            var mw = MainWindow.Instance;
            foreach (var ext in mw.Extensions)
            {
                UI_Plugins.Children.Add(new Label() { Content = ext.GetExtensionName() });
                foreach (var credit in ext.GetCredits())
                {
                    bool found = false;
                    foreach (var name in UI_Credits_Names.Children)
                    {
                        if ((name as Label).Content as string == credit.Name)
                        {
                            (UI_Credits_Comments.Children[UI_Credits_Names.Children.IndexOf(name as Label)] as Label).Content += $" & {credit.Description}";
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        UI_Credits_Names.Children.Add(new Label() { Content = credit.Name });
                        UI_Credits_Comments.Children.Add(new Label() { Content = credit.Description });
                    }
                }
            }
        }

        private void UI_JoinREMS_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://dc.railgun.works/retroengine");
        }

        private void UI_GitHub_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/thesupersonic16/ManiaPal");
        }

        private void UI_OK_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UI_FrameTitle_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        private void UI_FrameC_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((Grid)sender).Background = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
        }

        private void UI_FrameC_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Close();
        }

    }
}
