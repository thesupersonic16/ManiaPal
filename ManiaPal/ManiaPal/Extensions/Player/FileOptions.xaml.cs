using Microsoft.Win32;
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

namespace ManiaPal.Extensions.Player
{
    /// <summary>
    /// Interaction logic for FileOptions.xaml
    /// </summary>
    public partial class FileOptions : UserControl
    {
        public FileOptions()
        {
            InitializeComponent();
        }

        private void UI_SelectAnimationPath_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Sonic Mania Animation|*.bin";
            if (ofd.ShowDialog() == true)
                UI_FilePathTextBox.Text = ofd.FileName;
        }
    }
}
