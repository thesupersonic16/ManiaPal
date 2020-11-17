using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaPal.Extensions.ManiaConfig
{
    public class ManiaConfig : Extension
    {
        public ManiaConfig(MainWindow window) : base(window)
        {
        }

        public override string GetExtensionName()
        {
            return "ManiaConfig (GameConfig.bin and StageConfig.bin)";
        }

        public override List<FileType> GetFileTypes()
        {
            return new List<FileType>() { new FileTypeManiaConfig() };
        }

        public override List<Credit> GetCredits()
        {
            return new List<Credit>()
            {
                new Credit("koolkdev", "Reverse-Engineering Config Parsers"),
                new Credit("OtherworldBob", "Updating GameConfig for Sonic Mania Plus")
            };
        }

    }

    public class FileTypeManiaConfig : Extension.FileType
    {
        public RSDKv5.CommonConfig Config;

        public enum ConfigType
        {
            GameConfig,
            StageConfig,
            OtherConfig
        }

        public FileTypeManiaConfig()
        {
            PaletteSets = 8;
        }

        public ConfigType GuessConfigType(string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            if (fileName.ToLower().Contains("game"))
                return ConfigType.GameConfig;
            if (fileName.ToLower().Contains("stage"))
                return ConfigType.StageConfig;
            return ConfigType.OtherConfig;
        }

        public override string GetFileTypeName()
        {
            return "ManiaConfig";
        }

        public override List<string> GetSupportedFileTypes()
        {
            return new List<string>() { "*GameConfig*", "*StageConfig*" };
        }

        public override string GetSupportedFileFilters()
        {
            return "ManiaConfig|*.bin";
        }

        public override bool Read(string filePath, Palette[] palettes, int palette)
        {
            var type = GuessConfigType(filePath);

            Config = null;

            try
            {
                if (type == ConfigType.GameConfig)
                    using (var stream = File.OpenRead(filePath))
                        Config = new RSDKv5.GameConfig(stream);
                if (type == ConfigType.StageConfig)
                    using (var stream = File.OpenRead(filePath))
                        Config = new RSDKv5.StageConfig(stream);
                if (type == ConfigType.OtherConfig)
                    return false;
            }
            catch (Exception ex)
            {
                new HedgeMessageBox("Failed to load ManiaConfig file!", "Failed to load ManiaConfig file! Aborting\nIf the file being read is clean,\n" +
                    "PLEASE REPORT THIS ISSUE WITH THE INFORMATION BELOW!\n\nError Message:\n" + ex).ShowDialog();
                return false;
            }

            for (int p = 0; p < 8; ++p)
            {
                palettes[p].Clear();
                for (int y = 0; y < 16; ++y)
                {
                    for (int x = 0; x < 16; ++x)
                    {
                        var col = Config.Palettes[p]?.Colors[y]?[x];
                        if (col == null)
                            continue;
                        palettes[p].Colours[y * 16 + x] = new ManiaColor(col.R, col.G, col.B);
                    }
                }
            }
            return true;
        }

        public override void Write(string filePath, Palette[] palettes, int palette)
        {
            if (Config != null)
            {
                for (int i = 0; i < 8; ++i)
                {
                    if (palettes[i].ActiveRows != 0)
                    {
                        Config.Palettes[i].Colors[i] = new RSDKv5.PaletteColor[16];
                        for (int y = 0; y < 16; ++y)
                        {
                            if ((palettes[i].ActiveRows & (1 << y)) != 0)
                            {
                                Config.Palettes[i].Colors[y] = new RSDKv5.PaletteColor[16];

                                for (int x = 0; x < 16; ++x)
                                {
                                    Config.Palettes[i].Colors[y][x] = new RSDKv5.PaletteColor();
                                    Config.Palettes[i].Colors[y][x].R = palettes[i].Colours[(y * 16) + x].Red;
                                    Config.Palettes[i].Colors[y][x].G = palettes[i].Colours[(y * 16) + x].Green;
                                    Config.Palettes[i].Colors[y][x].B = palettes[i].Colours[(y * 16) + x].Blue;
                                }
                            }
                            else
                                Config.Palettes[i].Colors[y] = null;
                        }
                    }
                }
                if (Config is RSDKv5.GameConfig gc)
                    using (var stream = File.Create(filePath))
                        gc.Write(stream);
                if (Config is RSDKv5.StageConfig sc)
                    using (var stream = File.Create(filePath))
                        sc.Write(stream);
            }
        }
    }
}
