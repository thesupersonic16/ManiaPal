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
    /// Interaction logic for ExtensionOptions.xaml
    /// </summary>
    public partial class ExtensionOptions : UserControl
    {

        public Palette[] Palettes;
        public Action<int> UpdatePalettes;
        public MainWindow Window;

        public int Set = 0;
        public int Offset = 0;

        public ExtensionOptions(MainWindow window)
        {
            Window = window;
            Palettes = window.Palettes;
            UpdatePalettes = window.RefreshPalette;
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            int id = int.Parse(((RadioButton)sender).Tag as string);
            switch (id)
            {
                case 0:
                    Offset = 0;
                    break;
                case 1:
                    Offset = 1;
                    break;
                case 2:
                    Offset = 2;
                    break;
                case 3:
                    Set = 0;
                    break;
                case 4:
                    Set = 1;
                    break;
                case 5:
                    Set = 2;
                    break;
            }
            for (int i = 0; i < 5; ++i)
                SetOffsets(i);
        }

        public void SetOffsets(int paletteID)
        {
            var palette = Palettes[paletteID];
            // Sonic
            palette.Offsets[4 * 16 + 00] = -((4 - (Offset + Set * 3)) * 16);
            palette.Offsets[4 * 16 + 01] = -((4 - (Offset + Set * 3)) * 16);
            palette.Offsets[4 * 16 + 02] = -((4 - (Offset + Set * 3)) * 16);
            palette.Offsets[4 * 16 + 03] = -((4 - (Offset + Set * 3)) * 16);
            palette.Offsets[4 * 16 + 04] = -((4 - (Offset + Set * 3)) * 16);
            palette.Offsets[4 * 16 + 05] = -((4 - (Offset + Set * 3)) * 16);

            // Tails
            palette.Offsets[4 * 16 + 06] = -((4 - (Offset + Set * 3)) * 16) - 6;
            palette.Offsets[4 * 16 + 07] = -((4 - (Offset + Set * 3)) * 16) - 6;
            palette.Offsets[4 * 16 + 08] = -((4 - (Offset + Set * 3)) * 16) - 6;
            palette.Offsets[4 * 16 + 09] = -((4 - (Offset + Set * 3)) * 16) - 6;
            palette.Offsets[4 * 16 + 10] = -((4 - (Offset + Set * 3)) * 16) - 6;
            palette.Offsets[4 * 16 + 11] = -((4 - (Offset + Set * 3)) * 16) - 6;

            // Knuckles
            palette.Offsets[5 * 16 + 00] = -((5 - (Offset + Set * 3)) * 16);
            palette.Offsets[5 * 16 + 01] = -((5 - (Offset + Set * 3)) * 16);
            palette.Offsets[5 * 16 + 02] = -((5 - (Offset + Set * 3)) * 16);
            palette.Offsets[5 * 16 + 03] = -((5 - (Offset + Set * 3)) * 16);
            palette.Offsets[5 * 16 + 04] = -((5 - (Offset + Set * 3)) * 16);
            palette.Offsets[5 * 16 + 05] = -((5 - (Offset + Set * 3)) * 16);

            // Mighty
            palette.Offsets[6 * 16 + 00] = -((6 - (Offset + Set * 3)) * 16);
            palette.Offsets[6 * 16 + 01] = -((6 - (Offset + Set * 3)) * 16);
            palette.Offsets[6 * 16 + 02] = -((6 - (Offset + Set * 3)) * 16);
            palette.Offsets[6 * 16 + 03] = -((6 - (Offset + Set * 3)) * 16);
            palette.Offsets[6 * 16 + 04] = -((6 - (Offset + Set * 3)) * 16);
            palette.Offsets[6 * 16 + 05] = -((6 - (Offset + Set * 3)) * 16);

            // Ray
            palette.Offsets[7 * 16 + 00] = -((7 - (Offset + Set * 3)) * 16);
            palette.Offsets[7 * 16 + 01] = -((7 - (Offset + Set * 3)) * 16);
            palette.Offsets[7 * 16 + 02] = -((7 - (Offset + Set * 3)) * 16);
            palette.Offsets[7 * 16 + 03] = -((7 - (Offset + Set * 3)) * 16);
            palette.Offsets[7 * 16 + 04] = -((7 - (Offset + Set * 3)) * 16);
            palette.Offsets[7 * 16 + 05] = -((7 - (Offset + Set * 3)) * 16);

            UpdatePalettes(Window.CurrentPaletteSet);
        }
    }
}
