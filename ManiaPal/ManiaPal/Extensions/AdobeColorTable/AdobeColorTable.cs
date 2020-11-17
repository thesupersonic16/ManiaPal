using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaPal.Extensions.AdobeColorTable
{
    public class AdobeColorTable : Extension
    {
        public AdobeColorTable(MainWindow window) : base(window)
        {
        }

        public override string GetExtensionName()
        {
            return "Adobe Color Table (.act)";
        }

        public override List<FileType> GetFileTypes()
        {
            return new List<FileType>() { new FileTypeAdobeColorTable() };
        }

        public override List<Credit> GetCredits()
        {
            return new List<Credit>()
            {
            };
        }
    }

    public class FileTypeAdobeColorTable : Extension.FileType
    {

        public FileTypeAdobeColorTable()
        {
            AllowExport = true;
        }

        public override string GetFileTypeName()
        {
            return "Adobe Color Table";
        }

        public override List<string> GetSupportedFileTypes()
        {
            return new List<string>() { "*.act" };
        }

        public override string GetSupportedFileFilters()
        {
            return "Adobe Color Table|*.act";
        }

        public override bool Read(string filePath, Palette[] palettes, int currentPalette)
        {
            palettes[currentPalette].Clear();
            using (var stream = File.OpenRead(filePath))
            {
                for (int y = 0; y < 16; ++y)
                    for (int x = 0; x < 16; ++x)
                    {
                        palettes[currentPalette].Colours[(y * 16) + x].Red   = (byte)stream.ReadByte();
                        palettes[currentPalette].Colours[(y * 16) + x].Green = (byte)stream.ReadByte();
                        palettes[currentPalette].Colours[(y * 16) + x].Blue  = (byte)stream.ReadByte();
                    }
            }
            return true;
        }

        public override void Write(string filePath, Palette[] palettes, int currentPalette)
        {
            using (var stream = new BinaryWriter(File.Create(filePath)))
            {
                for (int y = 0; y < 16; ++y)
                    for (int x = 0; x < 16; ++x)
                    {
                        stream.Write(palettes[currentPalette].Colours[(y * 16) + x].Red);
                        stream.Write(palettes[currentPalette].Colours[(y * 16) + x].Green);
                        stream.Write(palettes[currentPalette].Colours[(y * 16) + x].Blue);
                    }
            }
        }
    }
}
