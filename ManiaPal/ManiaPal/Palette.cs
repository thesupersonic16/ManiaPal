using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ManiaPal
{
    public class Palette
    {
        /// <summary>
        /// <para>Colours in the palette. first 16 rows are visible and saved, other 8 rows are for other use.</para>
        /// </summary>
        public ManiaColor[] Colours = new ManiaColor[16 * 24];
        public int[] Offsets = new int[16 * 16];
        public ushort ActiveRows = 0;

        /// <summary>
        /// <para>Clears the palette.</para>
        /// </summary>
        public void Clear()
        {
            Colours = new ManiaColor[16 * 24];
            Offsets = new int[16 * 16];
        }

        /// <summary>
        /// <para>Merges two palettes together. Main palette goes behind.</para>
        /// </summary>
        /// <param name="palette">The palette to overlay</param>
        /// <param name="size">The amount of colours to merge. You shouldn't need to change this from the default 256</param>
        public void Merge(Palette palette, int size = 256)
        {
            var mainColour = palette.Colours[0];
            for (int i = 0; i < size; ++i)
                if (!palette.Colours[i].Equals(mainColour))
                    Colours[i] = palette.Colours[i];
        }
    }


    public struct ManiaColor
    {
        public byte Red;
        public byte Green;
        public byte Blue;

        public ManiaColor(byte red, byte green, byte blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public ushort ToRGB565()
        {
            return (ushort)(((Red & 0b11111000) << 8) | ((Green & 0b11111100) << 3) | (Blue >> 3));
        }

        public void FromRGB565(ushort rbg565)
        {
            Red = (byte)((rbg565 & 0b1111100000000000) >> 8);
            Green = (byte)((rbg565 & 0b0000011111100000) >> 3);
            Blue = (byte)((rbg565 & 0b0000000000011111) << 3);
        }

        public bool IsBlack()
        {
            return Red == 0 && Green == 0 && Blue == 0;
        }

        public override bool Equals(object obj)
        {
            if (obj is Color color)
            {
                bool match = true;
                if (color.R != Red)
                    match = false;
                if (color.G != Green)
                    match = false;
                if (color.B != Blue)
                    match = false;
                return match;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            var hashCode = -1058441243;
            hashCode = hashCode * -1521134295 + Red.GetHashCode();
            hashCode = hashCode * -1521134295 + Green.GetHashCode();
            hashCode = hashCode * -1521134295 + Blue.GetHashCode();
            return hashCode;
        }

        public static implicit operator Color(ManiaColor color)
        {
            return Color.FromRgb(color.Red, color.Green, color.Blue);
        }

        public static implicit operator System.Drawing.Color(ManiaColor color)
        {
            return System.Drawing.Color.FromArgb(0xFF, color.Red, color.Green, color.Blue);
        }
    }
}
