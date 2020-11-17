using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaPal.Extensions.Player
{
    public class PlayerObject : Extension
    {

        public MainWindow MainWindow;

        public PlayerObject(MainWindow window) : base(window)
        {
            MainWindow = window;
        }

        public override string GetExtensionName()
        {
            return "Player Object (Player/36D6)";
        }

        public override List<FileType> GetFileTypes()
        {
            return new List<FileType>() { new FileTypePlayerObject(MainWindow) };
        }

        public override List<Credit> GetCredits()
        {
            return new List<Credit>()
            {
                new Credit("RMGRich", "Helped with adding Player.bin support")
            };
        }
    }

    public class FileTypePlayerObject : Extension.FileType
    {

        protected byte[] Data;

        public FileTypePlayerObject(MainWindow mainWindow)
        {
            OptionsControl = new ExtensionOptions(mainWindow);
        }

        public override string GetFileTypeName()
        {
            return "Player Object";
        }

        public override List<string> GetSupportedFileTypes()
        {
            return new List<string>() { "*36D6*", "*player*" };
        }

        public override string GetSupportedFileFilters()
        {
            return "PlayerObject|*36D6*;*player*";
        }

        public override bool Read(string filePath, Palette[] palettes, int palette)
        {
            var options = new FileOptions();
            if (Extension.OpenFileOptionsWindow(options) == false)
                return false;
            try
            {
                if (!string.IsNullOrEmpty(options.UI_FilePathTextBox.Text))
                {
                    RSDKv5.GameConfig config;
                    string path = options.UI_FilePathTextBox.Text;
                    if (path.StartsWith("."))
                        path = Path.GetFullPath(Path.GetDirectoryName(filePath) + '\\' + path);
                    using (var stream = File.OpenRead(path))
                        config = new RSDKv5.GameConfig(stream);

                    palettes[0].Clear(); // Sonic
                    palettes[1].Clear(); // Tails
                    palettes[2].Clear(); // Knuckles
                    palettes[3].Clear(); // Mighty
                    palettes[4].Clear(); // Ray
                    for (int y = 0; y < 8; ++y)
                    {
                        for (int x = 0; x < 16; ++x)
                        {
                            var col = config.Palettes[0]?.Colors[y]?[x];
                            if (col == null)
                                continue;
                            palettes[0].Colours[(y + 16) * 16 + x] = new ManiaColor(col.R, col.G, col.B); // Sonic
                            palettes[1].Colours[(y + 16) * 16 + x] = new ManiaColor(col.R, col.G, col.B); // Tails
                            palettes[2].Colours[(y + 16) * 16 + x] = new ManiaColor(col.R, col.G, col.B); // Knuckles
                            palettes[3].Colours[(y + 16) * 16 + x] = new ManiaColor(col.R, col.G, col.B); // Mighty
                            palettes[4].Colours[(y + 16) * 16 + x] = new ManiaColor(col.R, col.G, col.B); // Ray
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExceptionWindow.UnhandledExceptionEventHandler(ex);
            }


            Data = File.ReadAllBytes(filePath);
            using (var stream = new MemoryStream(Data))
            {
                stream.Position = 0x539;
                for (int p = 0; p < 5; ++p)
                {
                    for (int y = 0; y < 3; ++y)
                    {
                        for (int x = 0; x < 6; ++x)
                        {
                            ++stream.Position;
                            palettes[p].Colours[y * 16 + x].Blue = (byte)stream.ReadByte();
                            palettes[p].Colours[y * 16 + x].Green = (byte)stream.ReadByte();
                            palettes[p].Colours[y * 16 + x].Red = (byte)stream.ReadByte();
                        }
                    }
                    stream.Position += 9;
                }
                for (int p = 0; p < 5; ++p)
                {
                    for (int y = 0; y < 3; ++y)
                    {
                        for (int x = 0; x < 6; ++x)
                        {
                            ++stream.Position;
                            palettes[p].Colours[(1 * 3 + y) * 16 + x].Blue = (byte)stream.ReadByte();
                            palettes[p].Colours[(1 * 3 + y) * 16 + x].Green = (byte)stream.ReadByte();
                            palettes[p].Colours[(1 * 3 + y) * 16 + x].Red = (byte)stream.ReadByte();
                        }
                    }
                    stream.Position += 9;
                }

                for (int p = 0; p < 5; ++p)
                {
                    for (int y = 0; y < 3; ++y)
                    {
                        for (int x = 0; x < 6; ++x)
                        {
                            ++stream.Position;
                            palettes[p].Colours[(2 * 3 + y) * 16 + x].Blue = (byte)stream.ReadByte();
                            palettes[p].Colours[(2 * 3 + y) * 16 + x].Green = (byte)stream.ReadByte();
                            palettes[p].Colours[(2 * 3 + y) * 16 + x].Red = (byte)stream.ReadByte();
                        }
                    }
                    stream.Position += 9;
                }

            }

            for (int ii = 0; ii < 5; ++ii)
            {
                for (int i = 0; i < 16 * 8; ++i)
                {
                    palettes[ii].Offsets[i] = (16 * 16);
                }

                // Sonic
                palettes[ii].Offsets[4 * 16 + 00] = -(4 * 16);
                palettes[ii].Offsets[4 * 16 + 01] = -(4 * 16);
                palettes[ii].Offsets[4 * 16 + 02] = -(4 * 16);
                palettes[ii].Offsets[4 * 16 + 03] = -(4 * 16);
                palettes[ii].Offsets[4 * 16 + 04] = -(4 * 16);
                palettes[ii].Offsets[4 * 16 + 05] = -(4 * 16);

                // Tails
                palettes[ii].Offsets[4 * 16 + 06] = -(1 * 16) - 6;
                palettes[ii].Offsets[4 * 16 + 07] = -(1 * 16) - 6;
                palettes[ii].Offsets[4 * 16 + 08] = -(1 * 16) - 6;
                palettes[ii].Offsets[4 * 16 + 09] = -(1 * 16) - 6;
                palettes[ii].Offsets[4 * 16 + 10] = -(1 * 16) - 6;
                palettes[ii].Offsets[4 * 16 + 11] = -(1 * 16) - 6;

                // Knuckles
                palettes[ii].Offsets[5 * 16 + 00] = (1 * 16);
                palettes[ii].Offsets[5 * 16 + 01] = (1 * 16);
                palettes[ii].Offsets[5 * 16 + 02] = (1 * 16);
                palettes[ii].Offsets[5 * 16 + 03] = (1 * 16);
                palettes[ii].Offsets[5 * 16 + 04] = (1 * 16);
                palettes[ii].Offsets[5 * 16 + 05] = (1 * 16);

                // Mighty
                palettes[ii].Offsets[6 * 16 + 00] = (3 * 16);
                palettes[ii].Offsets[6 * 16 + 01] = (3 * 16);
                palettes[ii].Offsets[6 * 16 + 02] = (3 * 16);
                palettes[ii].Offsets[6 * 16 + 03] = (3 * 16);
                palettes[ii].Offsets[6 * 16 + 04] = (3 * 16);
                palettes[ii].Offsets[6 * 16 + 05] = (3 * 16);

                // Ray
                palettes[ii].Offsets[7 * 16 + 00] = (5 * 16);
                palettes[ii].Offsets[7 * 16 + 01] = (5 * 16);
                palettes[ii].Offsets[7 * 16 + 02] = (5 * 16);
                palettes[ii].Offsets[7 * 16 + 03] = (5 * 16);
                palettes[ii].Offsets[7 * 16 + 04] = (5 * 16);
                palettes[ii].Offsets[7 * 16 + 05] = (5 * 16);
            }

            return true;
        }

        public override void Write(string filePath, Palette[] palettes, int palette)
        {
            using (var stream = new BinaryWriter(File.Create(filePath)))
            {
                stream.Write(Data);
                stream.Seek(0x539, SeekOrigin.Begin);
                for (int p = 0; p < 5; ++p)
                {
                    for (int y = 0; y < 3; ++y)
                    {
                        for (int x = 0; x < 6; ++x)
                        {
                            stream.Seek(0x01, SeekOrigin.Current);
                            stream.Write(palettes[p].Colours[(0 * 3 + y) * 16 + x].Blue);
                            stream.Write(palettes[p].Colours[(0 * 3 + y) * 16 + x].Green);
                            stream.Write(palettes[p].Colours[(0 * 3 + y) * 16 + x].Red);
                        }
                    }
                    stream.Seek(0x09, SeekOrigin.Current);
                }

                for (int p = 0; p < 5; ++p)
                {
                    for (int y = 0; y < 3; ++y)
                    {
                        for (int x = 0; x < 6; ++x)
                        {
                            stream.Seek(0x01, SeekOrigin.Current);
                            stream.Write(palettes[p].Colours[(1 * 3 + y) * 16 + x].Blue);
                            stream.Write(palettes[p].Colours[(1 * 3 + y) * 16 + x].Green);
                            stream.Write(palettes[p].Colours[(1 * 3 + y) * 16 + x].Red);
                        }
                    }
                    stream.Seek(0x09, SeekOrigin.Current);
                }

                for (int p = 0; p < 5; ++p)
                {
                    for (int y = 0; y < 3; ++y)
                    {
                        for (int x = 0; x < 6; ++x)
                        {
                            stream.Seek(0x01, SeekOrigin.Current);
                            stream.Write(palettes[p].Colours[(2 * 3 + y) * 16 + x].Blue);
                            stream.Write(palettes[p].Colours[(2 * 3 + y) * 16 + x].Green);
                            stream.Write(palettes[p].Colours[(2 * 3 + y) * 16 + x].Red);
                        }
                    }
                    stream.Seek(0x09, SeekOrigin.Current);
                }

            }
        }

    }
}
