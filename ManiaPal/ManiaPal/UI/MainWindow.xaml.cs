using Cyotek.Windows.Forms;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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
using System.Windows.Threading;

namespace ManiaPal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static MainWindow Instance;

        public Palette[] Palettes = new Palette[8];
        public int CurrentPaletteSet = 0;
        public List<Extension> Extensions = new List<Extension>();
        public List<Extension.FileType> FileTypes = new List<Extension.FileType>();
        public Extension.FileType LoadedFileType = null;
        public ProcessMemory Memory = new ProcessMemory();
        public SpriteView SpriteView = new SpriteView();
        public ColorPickerDialog ColorPicker = null;
        public ManiaColor OldColour;
        public ManiaPalSettings SettingWindow = null;
        public int CurrentColourSlot = 0;
        public bool ColourPickerClosed = true;
        public bool Config_ShowActiveColours { get; set; }

        // Addresses to the palettes in Sonic Mania's memory
        public int[] PaletteAddresses = new int[8]
        {
            0x00945B58,
            0x00945D58,
            0x00945F58,
            0x00946158,
            0x00946358,
            0x00946558,
            0x00946758,
            0x00946958
        };


        public MainWindow()
        {
            Instance = this;
            for (int i = 0; i < 8; ++i)
                Palettes[i] = new Palette();

            InitializeComponent();
            
            SelectedSetButton = UI_PaletteSet_Def;
            Extension.OpenFileOptionsWindow = OpenFileOptionsWindow;
        }

        /// <summary>
        /// <para>Scans the main assembly for extensions.</para>
        /// <para>Don't forget to clear <see cref="Extensions"/> and <see cref="FileTypes"/> before calling.</para>
        /// </summary>
        public void LoadExtensions()
        {
            List<Type> loaders = new List<Type>();

            loaders.AddRange(Assembly.GetAssembly(typeof(MainWindow)).GetTypes().Where(t => t.BaseType == typeof(Extension)).ToList());
            if (Assembly.GetAssembly(typeof(MainWindow)) != Assembly.GetEntryAssembly())
                loaders.AddRange(Assembly.GetEntryAssembly().GetTypes().Where(t => t.BaseType == typeof(Extension)).ToList());
            foreach (var type in loaders)
                Extensions.Add((Extension)Activator.CreateInstance(type, this));
            foreach (var ext in Extensions)
                FileTypes.AddRange(ext.GetFileTypes());
        }

        /// <summary>
        /// <para>Builds a row of WPF colour slots.</para>
        /// <para>This should only be called once on startup.</para>
        /// </summary>
        /// <param name="row">The row on the palettes its assigned for</param>
        public void SetupUIPaletteRow(int row)
        {
            var stackpanel = new StackPanel();
            var checkbox = new CheckBox();
            checkbox.Margin = new Thickness(4, 9, 4, 4);
            stackpanel.Orientation = Orientation.Horizontal;
            stackpanel.Children.Add(checkbox);
            for (int i = 0; i < 16; ++i)
            {
                var border = new Border();
                border.BorderThickness = new Thickness(2);
                border.BorderBrush = new SolidColorBrush(Colors.Black);
                var border2 = new Border();
                border2.BorderThickness = new Thickness(1);
                border2.BorderBrush = new SolidColorBrush(Colors.Black);
                border.Child = border2;
                var grid = new Grid();
                grid.Tag = row * 16 + i;
                grid.Width = 30;
                grid.Height = 30;
                grid.Margin = new Thickness(2, 2, 2, 2);
                grid.Background = new SolidColorBrush(Colors.Black);
                grid.MouseLeftButtonDown += UI_PaletteSlot_LeftMouseDown;
                grid.Children.Add(border);
                stackpanel.Children.Add(grid);
            }
            UI_Palettes.Children.Add(stackpanel);
        }

        /// <summary>
        /// <para>Builds a 16x16 grid of WPF colour slots.</para>
        /// <para>This must be only called once on startup.</para>
        /// </summary>
        public void SetupUIPalette()
        {
            for (int i = 0; i < 16; ++i)
                SetupUIPaletteRow(i);
        }

        /// <summary>
        /// Refreshes the UI palette.
        /// </summary>
        /// <param name="set">The set to display. This should always be the current palette</param>
        public void RefreshPalette(int set)
        {
            var palette = Palettes[set];
            palette.ActiveRows = 0;
            for (int col = 0; col < 16; ++col)
            {
                var UIRow = UI_Palettes.Children[col] as StackPanel;
                bool isActive = false;
                for (int row = 0; row < 16; ++row)
                {
                    if (!palette.Colours[col * 16 + row].IsBlack())
                        isActive = true;

                    if (((UIRow.Children[row + 1] as Grid).Background as SolidColorBrush).Color != palette.Colours[col * 16 + row])
                        (UIRow.Children[row + 1] as Grid).Background = new SolidColorBrush(palette.Colours[col * 16 + row]);
                    else
                        continue;
                }
                if (isActive)
                    palette.ActiveRows |= (ushort)(1 << col);
                (UIRow.Children[0] as CheckBox).IsChecked = isActive;
            }
            if (SpriteView != null && SpriteView.GIF != null && SpriteView.LoadedFrame != null)
            {
                SpriteView.GIF.Palette = palette;
                UpdateSpriteViewImage();
                if (SpriteView.Bitmap != null)
                    UI_SpriteViewLabel.Visibility = Visibility.Collapsed;
            }

            for (int col = 0; col < 16; ++col)
            {
                var UIRow = UI_Palettes.Children[col] as StackPanel;
                for (int row = 0; row < 16; ++row)
                {
                    var border = (UIRow.Children[row + 1] as Grid).Children[0] as Border;
                    if (SpriteView != null && SpriteView.Bitmap != null && SpriteView.GIF.ActiveColours[col * 16 + row])
                    {
                        if (Config_ShowActiveColours)
                        {
                            if ((border.BorderBrush as SolidColorBrush).Color == Colors.Black)
                                border.BorderBrush = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            if ((border.BorderBrush as SolidColorBrush).Color != Colors.Black)
                                border.BorderBrush = new SolidColorBrush(Colors.Black);
                        }
                    }
                    else
                    {
                        if ((border.BorderBrush as SolidColorBrush).Color != Colors.Black)
                            border.BorderBrush = new SolidColorBrush(Colors.Black);
                    }
                }
            }

        }

        /// <summary>
        /// <para>Builds a list of filters containing file types that can load.</para>
        /// </summary>
        /// <returns>The filter string for the WPF file dialog</returns>
        public string BuildFilters()
        {
            string s = "All Supported Files|";

            foreach (var fileType in FileTypes)
            {
                bool first = true;
                foreach (var type in fileType.GetSupportedFileTypes())
                {
                    if (!first)
                        s += ';' + type;
                    else
                        s += type;
                    first = false;
                }
                s += ';';
            }
            foreach (var fileType in FileTypes)
            {
                s += '|' + fileType.GetFileTypeName() + '|';
                bool first = true;
                foreach (var type in fileType.GetSupportedFileTypes())
                {
                    if (!first)
                        s += ';' + type;
                    else
                        s += type;
                    first = false;
                }
            }
            return s;
        }

        /// <summary>
        /// <para>Reads the palette data from a current running instance of Sonic Mania.</para>
        /// <para>Make sure to call <see cref="RefreshPalette"/> for displaying the palette on the UI.</para>
        /// </summary>
        public void UpdatePaletteFromMemory()
        {
            if (Memory.ReadUInt16(0x400000) != 0x5A4D)
                Memory.Attach("SonicMania");
            byte[] bytes = Memory.Read(PaletteAddresses[CurrentPaletteSet], 256 * 2);
            Palettes[CurrentPaletteSet].ActiveRows = 0;
            for (int y = 0; y < 16; ++y)
            {
                bool active = false;
                for (int x = 0; x < 16; ++x)
                {
                    ushort colour565 = BitConverter.ToUInt16(bytes, (y * 16 + x) * 2);
                    Palettes[CurrentPaletteSet].Colours[(y * 16) + x].FromRGB565(colour565);
                    if (!Palettes[CurrentPaletteSet].Colours[(y * 16) + x].IsBlack())
                        active = true;
                }
                if (active)
                    Palettes[CurrentPaletteSet].ActiveRows |= (ushort)(1 << y);
            }
        }

        /// <summary>
        /// <para>The name of this function explains itself.</para>
        /// </summary>
        /// <param name="value">Value with a wildcard</param>
        /// <returns>A regex expression</returns>
        public static string WildCardToRegular(string value)
        {
            return "^" + Regex.Escape(value).Replace("\\*", ".*") + "$";
        }

        /// <summary>
        /// <para>Loads a file.</para>
        /// </summary>
        /// <param name="path">The path to the file to be loaded</param>
        public void LoadFile(string path)
        {
            foreach (var fileType in FileTypes)
            {
                foreach (var fileType2 in fileType.GetSupportedFileTypes())
                {
                    if (Regex.IsMatch(path.ToLower(CultureInfo.GetCultureInfo("en-us")), WildCardToRegular(fileType2.ToLower(CultureInfo.GetCultureInfo("en-us")))))
                    {
                        if (!fileType.Read(fileType.filePath = path, Palettes, CurrentPaletteSet))
                            continue;
                        RefreshPalette(CurrentPaletteSet);
                        LoadedFileType = fileType;
                        if (SettingWindow != null)
                            SettingWindow?.Reload();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// <para>Opens the File Options dialog for file types.</para>
        /// </summary>
        /// <param name="control">The <see cref="UserControl"/> to display containing the options</param>
        /// <returns>Is OK pressed?</returns>
        public bool OpenFileOptionsWindow(ContentControl control)
        {
            var window = new FileOptionsWindow(control);
            return window.ShowDialog() == true;
        }

        /// <summary>
        /// <para>Updates the sprite viewer if one is loaded.</para>
        /// </summary>
        public void UpdateSpriteViewImage()
        {
            if (SpriteView.Bitmap != null)
            {
                SpriteView.Bitmap.Dispose();
                SpriteView.Bitmap = null;
            }
            UI_SpriteView.Source = SpriteView.ConvertToSource();
        }

        private void UI_Window_Loaded(object sender, RoutedEventArgs e)
        {
            UI_MemOptions.Visibility = Visibility.Collapsed;
            Title = $"{App.ProgramTitle} v{App.ProgramVersion}";
            SetupUIPalette();
            LoadExtensions();
            Config_ShowActiveColours = true;

            // At this point extensions are loaded, loading files for debugging should work fine over here

            RefreshPalette(0);

            var timer = new DispatcherTimer();
            timer.Tick += UI_Timer_Tick;
            timer.Interval = new TimeSpan(0, 0, 0, 0, 32);
            timer.Start();
        }

        private void UI_Timer_Tick(object sender, EventArgs e)
        {
            if (UI_CheckBox_UpdateMem.IsChecked == true && UI_RadioButton_Mem.IsChecked == true)
            {
                UpdatePaletteFromMemory();
                RefreshPalette(CurrentPaletteSet);
            }
            if (ColorPicker != null)
            {
                var col = ColorPicker.Color;
                var result = ColorPicker.DialogResult;
                Palettes[CurrentPaletteSet].Colours[CurrentColourSlot].Red   = col.R;
                Palettes[CurrentPaletteSet].Colours[CurrentColourSlot].Green = col.G;
                Palettes[CurrentPaletteSet].Colours[CurrentColourSlot].Blue  = col.B;

                if (result == System.Windows.Forms.DialogResult.OK)
                {
                    ColorPicker = null;
                }
                else if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    ColorPicker = null;
                    Palettes[CurrentPaletteSet].Colours[CurrentColourSlot] = OldColour;
                }
                if (UI_RadioButton_Mem.IsChecked == true)
                {
                    Memory.Attach("SonicMania");
                    int addr = PaletteAddresses[CurrentPaletteSet] + 2 * CurrentColourSlot;
                    Memory.WriteUInt16(addr, Palettes[CurrentPaletteSet].Colours[CurrentColourSlot].ToRGB565());
                }
                RefreshPalette(CurrentPaletteSet);
            }
        }

        // What?
        bool ready = true;
        private void UI_PaletteSlot_LeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            Memory.Attach("SonicMania");
            int addr = PaletteAddresses[CurrentPaletteSet] + 2 * (int)(sender as Grid).Tag;
            if (!ready)
                return;
            ready = false;
            if (ColourPickerClosed)
            {
                ColorPicker = null;
            }
            else if (ColorPicker != null)
            {
                Palettes[CurrentPaletteSet].Colours[CurrentColourSlot] = OldColour;
                CurrentColourSlot = (int)(sender as Grid).Tag;
                OldColour = Palettes[CurrentPaletteSet].Colours[CurrentColourSlot];
                ColorPicker.Invoke(new Action(() => 
                {
                    ColorPicker.Color = System.Drawing.Color.FromArgb(
                        Palettes[CurrentPaletteSet].Colours[CurrentColourSlot].Red,
                        Palettes[CurrentPaletteSet].Colours[CurrentColourSlot].Green,
                        Palettes[CurrentPaletteSet].Colours[CurrentColourSlot].Blue
                        );
                    ColorPicker.Activate();
                }));
                ready = true;
                return;
            }
            ColourPickerClosed = false;
            CurrentColourSlot = (int)(sender as Grid).Tag;
            new Thread(() =>
            {
                OldColour = Palettes[CurrentPaletteSet].Colours[CurrentColourSlot];
                ColorPicker = new ColorPickerDialog();
                ColorPicker.BackColor = System.Drawing.Color.FromArgb(0x2D, 0x2D, 0x30);
                ColorPicker.ForeColor = System.Drawing.Color.FromArgb(0xEE, 0xEE, 0xEE);
                ColorPicker.Color = System.Drawing.Color.FromArgb(
                    Palettes[CurrentPaletteSet].Colours[CurrentColourSlot].Red,
                    Palettes[CurrentPaletteSet].Colours[CurrentColourSlot].Green,
                    Palettes[CurrentPaletteSet].Colours[CurrentColourSlot].Blue
                    );
                ColorPicker.FormClosed += ColorPicker_FormClosed;
                ColorPicker.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
                ready = true;
                ColorPicker.ShowDialog();
            }).Start();
        }

        private void ColorPicker_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            ColourPickerClosed = true;
        }

        private void UI_UpdatePalette_Click(object sender, RoutedEventArgs e)
        {
            UpdatePaletteFromMemory();
            RefreshPalette(CurrentPaletteSet);
        }

        private void UI_Options_Click(object sender, RoutedEventArgs e)
        {
            if (SettingWindow != null)
            {
                SettingWindow.Activate();
            }
            else
            {
                SettingWindow = new ManiaPalSettings(this);
                SettingWindow.Show();
            }
        }

        private void UI_IM_Click(object sender, RoutedEventArgs e)
        {
            new ImportExportWindow(this).ShowDialog();
        }

        private void UI_LoadFile_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = BuildFilters();
            if (ofd.ShowDialog() == true)
                LoadFile(ofd.FileName);
        }

        private void UI_SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (LoadedFileType != null)
            {
                LoadedFileType.Write(LoadedFileType.filePath, Palettes, CurrentPaletteSet);
                return;
            }

            var messagebox = new HedgeMessageBox("Save Warning!", "No File was loaded prior to saving!\n" +
                "ManiaPal is currently unable to save to any\nMania formats without it being loaded prior to saving.");
            messagebox.AddButton("Save Palette as Adobe Colour Table", () =>
            {
                var fileType = FileTypes.FirstOrDefault(t => t.GetFileTypeName() == "Adobe Colour Table");
                var sfd = new SaveFileDialog();
                sfd.Filter = fileType.GetSupportedFileFilters();
                if (sfd.ShowDialog() == true)
                    fileType.Write(sfd.FileName, Palettes, CurrentPaletteSet);
                messagebox.Close();
            });
            messagebox.AddButton("Don't Save", () => { messagebox.Close(); });
            messagebox.Show();
        }

        private void UI_IOType_File_Checked(object sender, RoutedEventArgs e)
        {
            // Ignore First Start
            if (UI_MemOptions == null)
                return;
            UI_MemOptions.Visibility = Visibility.Collapsed;
            UI_FileOptions.Visibility = Visibility.Visible;
        }

        private void UI_IOType_Mem_Checked(object sender, RoutedEventArgs e)
        {
            UI_MemOptions.Visibility = Visibility.Visible;
            UI_FileOptions.Visibility = Visibility.Collapsed;
        }

        public Button SelectedSetButton = null;
        private void UI_PaletteSet_Click(object sender, RoutedEventArgs e)
        {
            CurrentPaletteSet = int.Parse((sender as Button).Content as string);
            RefreshPalette(CurrentPaletteSet);
            if (SelectedSetButton != null)
                SelectedSetButton.Background = new SolidColorBrush(Color.FromRgb(0x3F, 0x3F, 0x46));
            SelectedSetButton = sender as Button;
            (sender as Button).Background = new SolidColorBrush(Color.FromRgb(0x00, 0x7A, 0xCC));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = App.HideInstead;
        }

        private void UI_SpriteView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = new AnimationSelectWindow(SpriteView);
            if (window.ShowDialog() == true)
                RefreshPalette(CurrentPaletteSet);
        }
    }
}
