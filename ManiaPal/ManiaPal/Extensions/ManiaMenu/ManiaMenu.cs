using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaPal.Extensions.ManiaMenu
{
    public class ManiaMenu : Extension
    {

        public MainWindow MainWindow;

        public ManiaMenu(MainWindow window) : base(window)
        {
            MainWindow = window;
        }

        public override string GetExtensionName()
        {
            return "UIBackground Object (UIBackground/BD3D)";
        }

        public override List<FileType> GetFileTypes()
        {
            return new List<FileType>() { new FileTypeMenuObject(MainWindow) };
        }

        public override List<Credit> GetCredits()
        {
            return new List<Credit>()
            {
                new Credit("Rubberduckycooly", "Helped with adding UIBackground.bin support")
            };
        }
    }

    public class FileTypeMenuObject : Extension.FileType
    {

        protected byte[] Data;

        public FileTypeMenuObject(MainWindow mainWindow)
        {
        }

        public override string GetFileTypeName()
        {
            return "UIBackground Object";
        }

        public override List<string> GetSupportedFileTypes()
        {
            return new List<string>() { "*BD3D*", "*uibackground*" };
        }

        public override string GetSupportedFileFilters()
        {
            return "UIBackground Object|*BD3D*;*uibackground*";
        }

        public override bool Read(string filePath, Palette[] palettes, int palette)
        {
            for (int i = 0; i < 8; ++i)
                palettes[i].Clear();
            Data = File.ReadAllBytes(filePath);
            using (var stream = new MemoryStream(Data))
            {
                stream.Position = 0x0C;

                for (int y = 0; y < 5; ++y)
                {
                    for (int x = 0; x < 3; ++x)
                    {
                        ++stream.Position;
                        palettes[0].Colours[y * 16 + x].Blue = (byte)stream.ReadByte();
                        palettes[0].Colours[y * 16 + x].Green = (byte)stream.ReadByte();
                        palettes[0].Colours[y * 16 + x].Red = (byte)stream.ReadByte();
                    }
                }
            }
            return true;
        }

        public override void Write(string filePath, Palette[] palettes, int palette)
        {
            using (var stream = new BinaryWriter(File.Create(filePath)))
            {
                stream.Write(Data);
                stream.Seek(0x0C, SeekOrigin.Begin);

                for (int y = 0; y < 5; ++y)
                {
                    for (int x = 0; x < 3; ++x)
                    {
                        stream.Seek(0x01, SeekOrigin.Current);
                        stream.Write(palettes[0].Colours[(0 * 3 + y) * 16 + x].Blue);
                        stream.Write(palettes[0].Colours[(0 * 3 + y) * 16 + x].Green);
                        stream.Write(palettes[0].Colours[(0 * 3 + y) * 16 + x].Red);
                    }
                }
            }
        }

    }
}
