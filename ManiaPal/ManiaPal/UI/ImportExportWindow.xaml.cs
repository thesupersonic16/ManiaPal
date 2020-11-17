using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ManiaPal
{
    /// <summary>
    /// Interaction logic for ImportExportWindow.xaml
    /// </summary>
    public partial class ImportExportWindow : Window
    {

        public MainWindow Window;
        public Palette ImportPalette = new Palette();
        public Bitmap Map1 = new Bitmap(16, 16);
        public Bitmap Map2 = new Bitmap(16, 16);
        public Bitmap Map3 = new Bitmap(16, 16);
        public Bitmap Map4 = new Bitmap(16, 16);
        public bool Ready = false;
        public bool Mode = false;

        public ImportExportWindow(MainWindow window)
        {
            InitializeComponent();
            Window = window;
        }

        public Bitmap CreateBitmapFromPalette(Palette palette, Bitmap map)
        {
            for (int x = 0; x < 16; ++x)
            {
                for (int y = 0; y < 16; ++y)
                {
                    map.SetPixel(x, y, palette.Colours[y * 16 + x]);
                }
            }
            return map;
        }

        public BitmapSource ConvertToSource(Bitmap map)
        {
            IntPtr ip = map.GetHbitmap();
            BitmapSource bs;
            try
            {
                bs = Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                SpriteView.DeleteObject(ip);
            }

            return bs;
        }

        private void UI_OK_Click(object sender, RoutedEventArgs e)
        {
            int option = UI_ModeComboBox.SelectedIndex;
            
            var palettes = new Palette[8];
            switch (option)
            {
                case 0:
                    MergePalette(Window.Palettes[Window.CurrentPaletteSet], ImportPalette);
                    Window.RefreshPalette(Window.CurrentPaletteSet);
                    break;
                case 1:
                    if (UI_FormatComboBox.SelectedIndex >= 0)
                    {
                        if (UI_FormatComboBox.SelectedItem is Extension.FileType item && item.AllowExport)
                        {
                            var sfd = new SaveFileDialog
                            {
                                Filter = item.GetSupportedFileFilters()
                            };
                            if (sfd.ShowDialog() == true)
                            {
                                var type = (Extension.FileType)Activator.CreateInstance(item.GetType());
                                for (int i = 0; i < 8; ++i)
                                    palettes[i] = new Palette();
                                MergePalette_Export(palettes[0], Window.Palettes[Window.CurrentPaletteSet]);
                                item.Write(sfd.FileName, palettes, Window.CurrentPaletteSet);
                            }
                        }
                    }
                    break;
            }
            DialogResult = true;
            Close();
        }

        public void MergePalettes(Palette[] palettes1, Palette[] palettes2)
        {
            for (int p = 0; p < palettes2.Length; ++p)
            {
                MergePalette(palettes1[p], palettes2[p]);
            }
        }

        public void MergePalette(Palette palette1, Palette palette2)
        {

            for (int i = 0; i < Math.Min(palette2.Colours.Length, palette1.Colours.Length); ++i)
            {
                if (Checkbox_IgnoreBlack.IsChecked == true && palette2.Colours[i].IsBlack())
                    continue;
                if (Checkbox_Overlay.IsChecked == true)
                {
                    if (!palette2.Colours[i].Equals(palette2.Colours[0]))
                        palette1.Colours[i] = palette2.Colours[i];
                }
                else
                {
                    palette1.Colours[i] = palette2.Colours[i];
                }
            }
        }

        public void MergePalette_Export(Palette palette1, Palette palette2)
        {

            for (int i = 0; i < Math.Min(palette2.Colours.Length, palette1.Colours.Length); ++i)
            {
                if (CheckBox_Convert.IsChecked == true && palette2.Colours[i].IsBlack())
                    palette1.Colours[i] = palette2.Colours[0];
                else
                    palette1.Colours[i] = palette2.Colours[i];
            }
        }


        private void UI_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UI_FormatComboBox.Items.Clear();
            Window.FileTypes.ForEach(t =>
            {
                if (t.AllowExport)
                    UI_FormatComboBox.Items.Add(t);
            });

            var palettes = new Palette[8];
            for (int i = 0; i < 8; ++i)
                palettes[i] = new Palette();

            var palette = new Palette();
            MergePalette(palette, Window.Palettes[Window.CurrentPaletteSet]);
            MergePalette(palette, ImportPalette);

            CreateBitmapFromPalette(Window.Palettes[Window.CurrentPaletteSet], Map1);
            CreateBitmapFromPalette(palette, Map3);
            CreateBitmapFromPalette(Window.Palettes[Window.CurrentPaletteSet], Map4);


            Image_Current.Source = ConvertToSource(Map1);
            Image_Result.Source = ConvertToSource(Map3);
            Image_Result.Source = ConvertToSource(Map4);

            Ready = true;
        }

        private void UI_FormatComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UI_FormatComboBox.SelectedIndex >= 0)
            {
                if (UI_FormatComboBox.SelectedItem is Extension.FileType item)
                    UI_OK.IsEnabled = item.AllowExport;
            }
        }

        // TODO
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = Window.BuildFilters();
            if (ofd.ShowDialog() == true)
            {
                var palettes = new Palette[8];
                for (int i = 0; i < 8; ++i)
                    palettes[i] = new Palette();
                foreach (var fileType in Window.FileTypes)
                {
                    foreach (var fileType2 in fileType.GetSupportedFileTypes())
                    {
                        if (Regex.IsMatch(ofd.FileName.ToLower(CultureInfo.GetCultureInfo("en-us")), MainWindow.WildCardToRegular(fileType2.ToLower(CultureInfo.GetCultureInfo("en-us")))))
                        {
                            Extension.FileType type = Activator.CreateInstance(fileType.GetType()) as Extension.FileType;
                            if (!type.Read(type.filePath = ofd.FileName, palettes, Window.CurrentPaletteSet))
                                continue;
                            ImportPalette = palettes[0];
                            var palette = new Palette();
                            MergePalette(palette, Window.Palettes[Window.CurrentPaletteSet]);
                            MergePalette(palette, ImportPalette);

                            CreateBitmapFromPalette(ImportPalette, Map2);
                            CreateBitmapFromPalette(palette, Map3);

                            Image_Importing.Source = ConvertToSource(Map2);
                            Image_Result.Source = ConvertToSource(Map3);
                            return;
                        }
                    }
                }
            }
        }

        private void Checkbox_Overlay_Checked(object sender, RoutedEventArgs e)
        {
            if (!Ready)
                return;
            var palette = new Palette();
            MergePalette(palette, Window.Palettes[Window.CurrentPaletteSet]);
            MergePalette(palette, ImportPalette);

            CreateBitmapFromPalette(ImportPalette, Map2);
            CreateBitmapFromPalette(palette, Map3);

            Image_Importing.Source = ConvertToSource(Map2);
            Image_Result.Source = ConvertToSource(Map3);

        }

        private void Checkbox_IgnoreBlack_Checked(object sender, RoutedEventArgs e)
        {
            if (!Ready)
                return;
            var palette = new Palette();
            MergePalette(palette, Window.Palettes[Window.CurrentPaletteSet]);
            MergePalette(palette, ImportPalette);

            CreateBitmapFromPalette(ImportPalette, Map2);
            CreateBitmapFromPalette(palette, Map3);

            Image_Importing.Source = ConvertToSource(Map2);
            Image_Result.Source = ConvertToSource(Map3);

        }

        private void CheckBox_Convert_Checked(object sender, RoutedEventArgs e)
        {
            var palette = new Palette();
            MergePalette_Export(palette, Window.Palettes[Window.CurrentPaletteSet]);
            CreateBitmapFromPalette(palette, Map4);
            Image_Result.Source = ConvertToSource(Map4);

        }

        private void UI_ModeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!Ready)
                return;
            Mode = UI_ModeComboBox.SelectedIndex != 0;
            GroupBox_ImportPalette.Visibility = Mode ? Visibility.Collapsed : Visibility.Visible;
            UI_ImportOptions.Visibility  =  Mode ? Visibility.Collapsed : Visibility.Visible;
            UI_FormatLabel.Visibility    = !Mode ? Visibility.Collapsed : Visibility.Visible;
            UI_FormatComboBox.Visibility = !Mode ? Visibility.Collapsed : Visibility.Visible;
            UI_ExportOptions.Visibility  = !Mode ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
