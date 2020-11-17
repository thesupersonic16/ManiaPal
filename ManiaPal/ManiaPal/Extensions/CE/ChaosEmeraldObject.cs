using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaPal.Extensions.CE
{
    //ChaosEmerald.bin
    public class ChaosEmeraldObject : Extension
    {

        public MainWindow MainWindow;

        public ChaosEmeraldObject(MainWindow window) : base(window)
        {
            MainWindow = window;
        }

        public override string GetExtensionName()
        {
            return "ChaosEmerald Object (ChaosEmerald/CC54)";
        }

        public override List<FileType> GetFileTypes()
        {
            return new List<FileType>() { new FileTypeMenuObject(MainWindow) };
        }

        public override List<Credit> GetCredits()
        {
            return new List<Credit>()
            {
                new Credit("Rubberduckycooly", "ChaosEmerald.bin support")
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
            return "ChaosEmerald Object";
        }

        public override List<string> GetSupportedFileTypes()
        {
            return new List<string>() { "*CC54*", "*chaosemerald*" };
        }

        public override string GetSupportedFileFilters()
        {
            return "ChaosEmerald Object|*CC54*;*chaosemerald*";
        }

        public override bool Read(string filePath, Palette[] palettes, int currentPalette)
        {
            for (int i = 0; i < 8; ++i)
                palettes[i].Clear();

            Data = File.ReadAllBytes(filePath);
            using (var stream = new MemoryStream(Data))
            {
                stream.Position = 0x17;
                
                for (int y = 0; y < 7; ++y)
                {
                    for (int x = 0; x < 5; ++x)
                    {
                        palettes[0].Colours[y * 16 + x].Red = (byte)stream.ReadByte();
                        palettes[0].Colours[y * 16 + x].Green = (byte)stream.ReadByte();
                        palettes[0].Colours[y * 16 + x].Blue = (byte)stream.ReadByte();
                        ++stream.Position;
                    }
                }
            }
            return true;
        }

        public override void Write(string filePath, Palette[] palettes, int currentPalette)
        {
            using (var stream = new BinaryWriter(File.Create(filePath)))
            {
                stream.Write(Data);
                stream.Seek(0x17, SeekOrigin.Begin);

                for (int y = 0; y < 7; ++y)
                {
                    for (int x = 0; x < 5; ++x)
                    {
                        stream.Write(palettes[0].Colours[y * 16 + x].Red);
                        stream.Write(palettes[0].Colours[y * 16 + x].Green);
                        stream.Write(palettes[0].Colours[y * 16 + x].Blue);
                        stream.Seek(0x01, SeekOrigin.Current);
                    }
                }
            }
        }

    }
}
