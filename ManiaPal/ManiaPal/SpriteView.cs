using RSDKv5;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ManiaPal
{
    public class SpriteView
    {
        /// <summary>
        /// <para>Reference to the loaded animation.</para>
        /// <para>This should not be manually modified, see <see cref="Setup"/> for loading new animations.</para>
        /// </summary>
        public Animation LoadedAnimation = null;
        /// <summary>
        /// <para>Reference to the loaded frame within <see cref="LoadedAnimation"/>.</para>
        /// <para>This should not be manually modified, see <see cref="LoadFrame"/> for selecting frames.</para>
        /// </summary>
        public Animation.AnimationEntry.Frame LoadedFrame = null;
        /// <summary>
        /// <para>The buffered <see cref="System.Drawing.Bitmap"/> object containing the sprite.</para>
        /// <para>This field holds the <see cref="System.Drawing.Bitmap"/> object, see <see cref="GetBitmap()"/> for loading new maps.</para>
        /// </summary>
        public Bitmap Bitmap;
        /// <summary>
        /// Sprite indexing data. Holds the current palette and indices for the current sprite. 
        /// </summary>
        public GIF GIF = new GIF();
        /// <summary>
        /// <para>The path to the currently set animation file.</para>
        /// <para>This should not be manually modified, see <see cref="Setup"/> for loading new animations.</para>
        /// </summary>
        public string FilePath = "";


        /// <summary>
        /// Loads an animation for the sprite viewer.
        /// </summary>
        /// <param name="filePath">Path to the animation file.</param>
        public void Setup(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                LoadedAnimation = null;
                FilePath = null;
                return;
            }
            using (var stream = File.OpenRead(filePath))
                LoadedAnimation = new Animation(new Reader(stream));
            FilePath = Path.GetDirectoryName(Path.GetDirectoryName(filePath));
            GIF = new GIF();
        }

        /// <summary>
        /// <para>Loads an animation frame from the currently loaded animation.</para>
        /// <para>Make sure to set the animation using <see cref="Setup"/> and only selecting frames from the current loaded animation.</para>
        /// </summary>
        /// <param name="frame">Refence to the frame to load into the viewer</param>
        public void LoadFrame(Animation.AnimationEntry.Frame frame)
        {
            LoadedFrame = frame;
            var path = Path.Combine(FilePath, LoadedAnimation.SpriteSheets[frame.SpriteSheet]);
            if (File.Exists(path))
                GIF.Load(path);
            else
            {
                var messagebox = new HedgeMessageBox("Failed to load sprite sheet!",
                    "The sprite sheet for the loaded sprite\ncould not be found.");
                messagebox.AddButton("Select sprite sheet file", () =>
                {
                    var ofd = new OpenFileDialog();
                    ofd.Filter = "Graphics Interchange Format|*.gif";
                    if (ofd.ShowDialog() == true)
                        GIF.Load(ofd.FileName);
                    messagebox.Close();
                });
                messagebox.AddButton("Cancel", () => { messagebox.Close(); });
                messagebox.Show();
            }
        }

        /// <summary>
        /// <para>Creates a <see cref="System.Drawing.Bitmap"/> object.</para>
        /// <para>Calling this function will dispose the older instance of the <see cref="System.Drawing.Bitmap"/> object.</para>
        /// <para>See <see cref="ConvertToSource"/> for bindings.</para>
        /// </summary>
        /// <returns>A drawn <see cref="System.Drawing.Bitmap"/> object.</returns>
        public Bitmap GetBitmap()
        {
            if (Bitmap != null)
                Bitmap.Dispose();
            if (LoadedFrame == null)
                return null;
            return Bitmap = GIF.CreateImage(LoadedFrame.X, LoadedFrame.Y, LoadedFrame.Width, LoadedFrame.Height);
        }

        /// <summary>
        /// <para>Creates a <see cref="System.Windows.Media.Imaging.BitmapSource"/> object for bindings.</para>
        /// </summary>
        /// <returns>Refence to <see cref="Bitmap"/> using <see cref="System.Windows.Media.Imaging.BitmapSource"/></returns>
        public BitmapSource ConvertToSource()
        {
            if (LoadedFrame == null)
                return null;
            var bitmap = GetBitmap();
            if (bitmap == null)
                return null;
            IntPtr ip = bitmap.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = Imaging.CreateBitmapSourceFromHBitmap(ip, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                DeleteObject(ip);
            }

            return bs;
        }

        // Pinvokes
        [DllImport("gdi32")] public static extern int DeleteObject(IntPtr o);

    }
}
