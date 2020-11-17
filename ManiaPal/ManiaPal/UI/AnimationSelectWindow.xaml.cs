using Microsoft.Win32;
using RSDKv5;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ManiaPal
{
    /// <summary>
    /// Interaction logic for AnimationSelectWindow.xaml
    /// </summary>
    public partial class AnimationSelectWindow : Window
    {

        public SpriteView SpriteView;
        public int FrameID;
        public int FrameIDMax;
        
        public AnimationSelectWindow(SpriteView spriteView)
        {
            SpriteView = spriteView;
            InitializeComponent();
        }

        /// <summary>
        /// Updates all the information on the window.
        /// </summary>
        public void Update()
        {
            if (UI_Animation.SelectedItem == null || (UI_Animation.SelectedItem as Animation.AnimationEntry).Frames.Count == 0)
            {
                UI_Preview.Source = null;
                return;
            }
            UI_FrameIDLabel.Content = $"{FrameID} / {FrameIDMax}";
            SpriteView.LoadFrame((UI_Animation.SelectedItem as Animation.AnimationEntry).Frames[FrameID]);
            UpdatePreview(UI_Preview);
            if (SpriteView.GIF.CheckForStageReference())
                UI_StagePaletteWarning.Visibility = Visibility.Visible;
            else
                UI_StagePaletteWarning.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Sets the preview image.
        /// </summary>
        /// <param name="image">The image to set the preview to.</param>
        public void UpdatePreview(Image image)
        {
            if (SpriteView.Bitmap != null)
            {
                SpriteView.Bitmap.Dispose();
                SpriteView.Bitmap = null;
            }
            image.Source = SpriteView.ConvertToSource();
        }

        private void UI_OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void UI_Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void UI_SelectAnimationPath_Click(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "Sonic Mania Animation|*.bin";
            if (ofd.ShowDialog() == true)
            {
                UI_FilePathTextBox.Text = ofd.FileName;
                try
                {
                    // Prepare the viewer
                    SpriteView.Setup(ofd.FileName);
                }
                catch
                {
                    // Clear viewer if failed
                    SpriteView.Setup(null);
                    // Display error
                    var messageBox = new HedgeMessageBox("Failed to load animation!",
                        "ManiaPal failed to parse the selected file.\nMake sure you have selected the correct file.");
                    messageBox.AddButton("OK", () => { messageBox.Close(); });
                    messageBox.ShowDialog();
                    return;
                }
                // Clean and fill the animation list
                UI_Animation.Items.Clear();
                foreach (var animation in SpriteView.LoadedAnimation.Animations)
                    UI_Animation.Items.Add(animation);
                
                FrameID = 0;
                FrameIDMax = 0;
                Update();
                // Load the first animation
                if (UI_Animation.Items.Count != 0)
                    UI_Animation.SelectedIndex = 0;
            }
        }

        private void UI_Animation_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = UI_Animation.SelectedItem as Animation.AnimationEntry;
            FrameID = 0;
            if (item != null)
                FrameIDMax = item.Frames.Count - 1;
            Update();
        }

        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            do
            {
                if (FrameID > 0)
                {
                    --FrameID;
                    Update();
                }
                else
                    return;
            } while (!SpriteView.GIF.IsFrameValid());
        }

        private void RepeatButton_Click_1(object sender, RoutedEventArgs e)
        {
            do
            {
                if (FrameID < FrameIDMax)
                {
                    ++FrameID;
                    Update();
                }
                else
                    return;
            } while (!SpriteView.GIF.IsFrameValid());
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            UI_StagePaletteWarning.Visibility = Visibility.Collapsed;
        }
    }
}
