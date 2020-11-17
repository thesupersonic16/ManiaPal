using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManiaPal
{
    public class GIF
    {
        /// <summary>
        /// <para>The palette containing all the colours from the GIF file.</para>
        /// </summary>
        public Palette Palette = new Palette();
        /// <summary>
        /// <para>An array of indices within the GIF. These point to the colours in <see cref="Palette"/>.</para>
        /// </summary>
        public byte[] Pixels;
        /// <summary>
        /// <para>An array of colours in <see cref="Palette"/> that are in use.</para>
        /// <para>This isn't really used besides for highlighting slots in the GUI.</para>
        /// </summary>
        public bool[] ActiveColours;
        /// <summary>
        /// <para>The width of the GIF file.</para>
        /// </summary>
        public int Width;
        /// <summary>
        /// <para>The width of the GIF file.</para>
        /// </summary>
        public int Height;
        /// <summary>
        /// <para>The path to the currently loaded GIF file.</para>
        /// <para>This should not be manually modified, see <see cref="Load"/> for loading GIF files.</para>
        /// </summary>
        public string LastPath;
        /// <summary>
        /// <para>The last regions</para>
        /// </summary>
        int lx, ly, lw, lh;


        /// <summary>
        /// <para>Loads and parses a GIF file.</para>
        /// </summary>
        /// <param name="filePath">Path to the GIF file.</param>
        /// <param name="reload">Should the file be reloaded if it is currently loaded?</param>
        public void Load(string filePath, bool reload = false)
        {
            if (LastPath == filePath && !reload)
                return;
            LastPath = filePath;
            Bitmap map = Image.FromFile(filePath) as Bitmap;
            for (int i = 0; i < 256; ++i)
            {
                if (i == map.Palette.Entries.Length)
                    break;
                Palette.Colours[i].Red = map.Palette.Entries[i].R;
                Palette.Colours[i].Green = map.Palette.Entries[i].G;
                Palette.Colours[i].Blue = map.Palette.Entries[i].B;
            }
            ActiveColours = new bool[16 * 24];

            Pixels = GetIndices(map);
            Width = map.Width;
            Height = map.Height;
        }

        /// <summary>
        /// <para>Converts the GIF into a <see cref="System.Drawing.Bitmap"/> with the RGBA format.</para>
        /// <para>This also accepts a region.</para>
        /// </summary>
        /// <param name="x">The left position to start at.</param>
        /// <param name="y">The top position to start at.</param>
        /// <param name="width">The amount of pixels from the left to convert.</param>
        /// <param name="height">The amount of pixels from the top to convert.</param>
        /// <returns>A drawn <see cref="System.Drawing.Bitmap"/> object in RGBA format. This will return null if it fails.</returns>
        public Bitmap CreateImage(int x, int y, int width, int height)
        {
            lx = x;
            ly = y;
            lw = width;
            lh = height;

            // Check regions
            if (width == 0 || height == 0 || x + width > Width || y + height > Height)
                return null;

            Bitmap map = new Bitmap(width, height);
            
            // Clear list of colours
            for (int i = 0; i < 16 * 24; ++i)
                ActiveColours[i] = false;

            // Prepare Bitmap for writing
            BitmapData rawData = map.LockBits
            (
                new Rectangle(0, 0, map.Width, map.Height),
                ImageLockMode.ReadWrite,
                map.PixelFormat
            );

            // Write pixels
            unsafe
            {
                byte* p = (byte*)rawData.Scan0;
                for (int i = 0; i < (rawData.Width * rawData.Height) * 4; i+=4)
                {
                    var pi = Pixels[(y + (i / 4) / width) * Width + x + (i / 4) % width];
                    ActiveColours[pi + Palette.Offsets[pi]] = true;
                    var col = Palette.Colours[pi + Palette.Offsets[pi]];
                    p[i + 0] = col.Blue;
                    p[i + 1] = col.Green;
                    p[i + 2] = col.Red;
                    p[i + 3] = (byte)(pi == 0 ? 0x00 : 0xFF);
                }
            }
            
            // Unlock the bitmap so it won't stay locked in memory
            map.UnlockBits(rawData);

            return map;
        }

        /// <summary>
        /// <para>Gets the indices of a indexed <see cref="System.Drawing.Bitmap"/>. Array must be indexed by [y * 16 + x].</para>
        /// </summary>
        /// <param name="bitmap">The indexed <see cref="System.Drawing.Bitmap"/> used to read the indices from.</param>
        /// <returns>An array of indices.</returns>
        public static byte[] GetIndices(Bitmap bitmap)
        {
            // Prepare Bitmap for reading
            BitmapData rawData = bitmap.LockBits
            (
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly,
                bitmap.PixelFormat
            );

            byte[] indices = new byte[rawData.Height * rawData.Width];

            // Copy the indices
            unsafe
            {
                byte* p = (byte*)rawData.Scan0;
                Parallel.For(0, rawData.Height, y =>
                {
                    for (int x = 0; x < rawData.Width; x++)
                    {
                        int offset = y * rawData.Stride + x;
                        indices[x + y * rawData.Width] = (p[offset]);
                    }
                });
            }

            // Unlock the bitmap so it won't stay locked in memory
            bitmap.UnlockBits(rawData);

            return indices;
        }

        /// <summary>
        /// <para>Searches for colour in the palette and returns the index of the colour.</para>
        /// </summary>
        /// <param name="col">The colour to search for.</param>
        /// <returns>The index of where the colour is located.</returns>
        public byte GetIndex(ManiaColor col)
        {
            for (byte i = 0; i < Palette.Colours.Length; ++i)
                if (Palette.Colours[i].Red == col.Red &&
                    Palette.Colours[i].Green == col.Green &&
                    Palette.Colours[i].Blue == col.Blue)
                    return i;
            return 0;
        }

        // TODO: Make this function static
        /// <summary>
        /// <para>Searches for colour in a given palette and returns the index of the colour.</para>
        /// </summary>
        /// <param name="col">The colour to search for.</param>
        /// <param name="pal">The palette to search in.</param>
        /// <returns>The index of where the colour is located.</returns>
        public byte GetIndex(ManiaColor col, ManiaColor[] pal)
        {
            for (byte i = 0; i < pal.Length; ++i)
                if (pal[i].Red == col.Red &&
                    pal[i].Green == col.Green &&
                    pal[i].Blue == col.Blue)
                    return i;
            return 0;
        }

        /// <summary>
        /// <para>Checks if the given region is valid for the current GIF.</para>
        /// </summary>
        /// <param name="x">The left position to start at.</param>
        /// <param name="y">The top position to start at.</param>
        /// <param name="width">The amount of pixels from the left to convert.</param>
        /// <param name="height">The amount of pixels from the top to convert.</param>
        /// <returns>True if the region given is valid.</returns>
        public bool IsFrameValid(int x, int y, int width, int height)
        {
            lx = x;
            ly = y;
            lw = width;
            lh = height;
            if (width == 0 || height == 0 || x + width > Width || y + height > Height)
                return false;
            return true;
        }

        /// <summary>
        /// <para>Checks if the last given region is valid for the current GIF.</para>
        /// </summary>
        /// <returns>True if the region given is valid.</returns>
        public bool IsFrameValid()
        {
            if (lw == 0 || lh == 0 || lx + lw > Width || ly + lh > Height)
                return false;
            return true;
        }

        /// <summary>
        /// <para>Checks if the last given region overlaps with the stage palette. This is just a indice check if its 128 or over.</para>
        /// </summary>
        /// <returns>If a colour overlaps with the stage palette.</returns>
        public bool CheckForStageReference()
        {
            if (lw == 0 || lh == 0 || lx + lw > Width || ly + lh > Height)
                return false;
            for (int yy = 0; yy < lh; ++yy)
            {
                for (int xx = 0; xx < lw; ++xx)
                {
                    var i = Pixels[(ly + yy) * Width + (lx + xx)];
                    if (i >= 128)
                        return true;
                }
            }
            return false;
        }
    }
}
