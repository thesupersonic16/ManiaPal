using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaPal.Extensions.Player
{
    public class ClassicPlayerObject : Extension
    {

        public MainWindow MainWindow;

        public ClassicPlayerObject(MainWindow window) : base(window)
        {
            MainWindow = window;
        }

        public override string GetExtensionName()
        {
            return "Global Objects (GlobalCode)";
        }

        public override List<FileType> GetFileTypes()
        {
            return new List<FileType>() { new FileTypeClassicPlayerObject(MainWindow) };
        }

        public override List<Credit> GetCredits()
        {
            return new List<Credit>()
            {
                new Credit("Rubberduckycooly", "Added Globalcode.bin support")
            };
        }
    }

    public class FileTypeClassicPlayerObject : Extension.FileType
    {

        protected byte[] Data;

        public FileTypeClassicPlayerObject(MainWindow mainWindow)
        {
            OptionsControl = new ExtensionOptions(mainWindow);
        }

        public override string GetFileTypeName()
        {
            return "Player Object";
        }

        public override List<string> GetSupportedFileTypes()
        {
            return new List<string>() { "*GlobalCode*"};
        }

        public override string GetSupportedFileFilters()
        {
            return "Global Objects|*GlobalCode*;";
        }

        public override bool Read(string filePath, Palette[] palettes)
        {
            var options = new FileOptions();
            if (Extension.OpenFileOptionsWindow(options) == false)
                return false;
            try
            {
                if (!string.IsNullOrEmpty(options.UI_FilePathTextBox.Text))
                {
                    palettes[0].Clear();
                    for (int y = 0; y < 8; ++y)
                    {
                        for (int x = 0; x < 16; ++x)
                        {
                            palettes[0].Colours[(y + 16) * 16 + x] = new ManiaColor(0, 0, 0);
                            palettes[1].Colours[(y + 16) * 16 + x] = new ManiaColor(0, 0, 0);
                            palettes[2].Colours[(y + 16) * 16 + x] = new ManiaColor(0, 0, 0);
                            palettes[3].Colours[(y + 16) * 16 + x] = new ManiaColor(0, 0, 0);
                            palettes[4].Colours[(y + 16) * 16 + x] = new ManiaColor(0, 0, 0);
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
                stream.Position = 0x14;
                for (int p = 0; p < 3; ++p)
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
                for (int p = 0; p < 3; ++p)
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

                for (int p = 0; p < 3; ++p)
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
            }

            return true;
        }

        public override void Write(string filePath, Palette[] palettes)
        {
            using (var stream = new BinaryWriter(File.Create(filePath)))
            {
                stream.Write(Data);
                stream.Seek(0x14, SeekOrigin.Begin);
                for (int p = 0; p < 3; ++p) // Players
                {
                    for (int y = 0; y < 3; ++y) //Lines
                    {
                        for (int x = 0; x < 6; ++x) //Colours Per Line
                        {
                            stream.Seek(0x01, SeekOrigin.Current);
                            stream.Write(palettes[p].Colours[(0 * 3 + y) * 16 + x].Blue);
                            stream.Write(palettes[p].Colours[(0 * 3 + y) * 16 + x].Green);
                            stream.Write(palettes[p].Colours[(0 * 3 + y) * 16 + x].Red);
                        }
                    }
                    stream.Seek(0x09, SeekOrigin.Current);
                }

                for (int p = 0; p < 3; ++p)
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

                for (int p = 0; p < 3; ++p)
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
