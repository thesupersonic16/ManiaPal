using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaPal.Extensions.GGPAL
{

    public class GGPAL : Extension
    {

        public MainWindow MainWindow;

        public GGPAL(MainWindow window) : base(window)
        {
            MainWindow = window;
        }

        public override string GetExtensionName()
        {
            return "Palette (.pal)";
        }

        public override List<FileType> GetFileTypes()
        {
            return new List<FileType>() { new FileTypeGGPAL() };
        }

        public override List<Credit> GetCredits()
        {
            return new List<Credit>()
            {
            };
        }
    }

    public class FileTypeGGPAL : Extension.FileType
    {

        public FileTypeGGPAL()
        {
            AllowExport = true;
        }

        public override string GetFileTypeName()
        {
            return "Palette";
        }

        public override List<string> GetSupportedFileTypes()
        {
            return new List<string>() { "*.pal" };
        }

        public override string GetSupportedFileFilters()
        {
            return "Palette|*.pal";
        }

        public override bool Read(string filePath, Palette[] palettes, int currentPalette)
        {
            var file = File.ReadAllBytes(filePath);
            if (file[0] == 'R')
                using (var stream = new MemoryStream(file))
                    ReadCPPALBinary(stream, palettes, currentPalette);
            if (file[0] == 'J')
                using (var stream = new MemoryStream(file))
                    palettes[currentPalette] = ReadJASC(stream);
            return true;
        }

        public override void Write(string filePath, Palette[] palettes, int currentPalette)
        {
            var options = new FileOptionsE();
            if (Extension.OpenFileOptionsWindow(options) == false)
                return;

            if (options.UI_FormatComboBox.SelectedIndex == 0)
            {
                using (var stream = File.Create(filePath))
                    WriteJASC(stream, palettes[currentPalette]);
            }
            else if (options.UI_FormatComboBox.SelectedIndex == 1)
            {
                using (var stream = File.Create(filePath))
                    WriteMSPAL(stream, palettes, currentPalette);
            }
        }

        // Graphics Gale
        public Palette ReadJASC(Stream stream)
        {
            Palette newPalette = new Palette();
            var reader = new StreamReader(stream, Encoding.ASCII);
            if (reader.ReadLine() != "JASC-PAL")
                throw new Exception("File is not a JASC Palette!");
            if (reader.ReadLine() != "0100")
                throw new Exception("Palette File is not valid!");
            int count = int.Parse(reader.ReadLine());
            if (count > 256)
                throw new Exception("Palette is too large for ManiaPal to load! Only 256 colour palettes are supported");
            for (int i = 0; i < count; ++i)
            {
                string[] split = reader.ReadLine().Split(' ');
                newPalette.Colours[i].Red = byte.Parse(split[0]);
                newPalette.Colours[i].Green = byte.Parse(split[1]);
                newPalette.Colours[i].Blue = byte.Parse(split[2]);
            }
            return newPalette;
        }

        public void WriteJASC(Stream stream, Palette palette)
        {
            var writer = new StreamWriter(stream, Encoding.ASCII);
            writer.WriteLine("JASC-PAL");
            writer.WriteLine("0100");
            writer.WriteLine("256");
            for (int i = 0; i < 256; ++i)
                writer.WriteLine("{0} {1} {2}", palette.Colours[i].Red, palette.Colours[i].Green, palette.Colours[i].Blue);
            writer.Flush();
        }


        // Corel Paintshop
        public void ReadCPPALBinary(Stream stream, Palette[] palettes, int palette)
        {
            Palette newPalette = new Palette();
            var reader = new BinaryReader(stream);
            uint sig = reader.ReadUInt32();
            uint fileSize = reader.ReadUInt32();
            {
                string paletteSig = new string(reader.ReadChars(0x8));
                uint paletteSize = reader.ReadUInt32() - 4;
                if (paletteSize < 0x400)
                    throw new Exception("Palette does not contain 256 colours");
                short unknown = reader.ReadInt16();
                short unknown2 = reader.ReadInt16();

                for (int y = 0; y < 16; ++y)
                    for (int x = 0; x < 16; ++x)
                    {
                        newPalette.Colours[(y * 16) + x].Red = (byte)stream.ReadByte();
                        newPalette.Colours[(y * 16) + x].Green = (byte)stream.ReadByte();
                        newPalette.Colours[(y * 16) + x].Blue = (byte)stream.ReadByte();
                        stream.ReadByte();
                    }
            }
            reader.Close();
            palettes[palette] = (newPalette);
        }

        public void WriteMSPAL(Stream stream, Palette[] palettes, int palette)
        {
            var pal = palettes[palette];
            var writer = new BinaryWriter(stream);
            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(0x0410);
            writer.Write(Encoding.ASCII.GetBytes("PAL data"));
            writer.Write(0x0404);
            writer.Write((short)0x0300);
            writer.Write((short)0x0100);
                for (int y = 0; y < 16; ++y)
                    for (int x = 0; x < 16; ++x)
                    {
                        writer.Write(pal.Colours[(y * 16) + x].Red);
                        writer.Write(pal.Colours[(y * 16) + x].Green);
                        writer.Write(pal.Colours[(y * 16) + x].Blue);
                        writer.Write((byte)0);
                    }
            writer.Close();
        }
    }

}
