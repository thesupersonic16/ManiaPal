using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ManiaPal
{
    public abstract class Extension
    {
        /// <summary>
        /// The friendly name of the extension.
        /// </summary>
        public abstract string GetExtensionName();
        /// <summary>
        /// The list of file types it can read or write.
        /// </summary>
        public abstract List<FileType> GetFileTypes();
        /// <summary>
        /// The list of authors and contributors for the extension.
        /// </summary>
        public abstract List<Credit> GetCredits();

        public Extension(MainWindow window) { }

        public static Func<ContentControl, bool> OpenFileOptionsWindow;

        public abstract class FileType
        {
            /// <summary>
            /// Path to the last loaded file.
            /// </summary>
            public string filePath = "";
            /// <summary>
            /// A WPF <see cref="UserControl"/> which will be used if any options are needed from the user.
            /// </summary>
            public Control OptionsControl;
            /// <summary>
            /// Does the file type support exporting?
            /// </summary>
            public bool AllowExport = false;
            /// <summary>
            /// The amount of palette sets the file type has. This is currently not used.
            /// </summary>
            public int PaletteSets = 1;
            
            // Methods
            /// <summary>
            /// The friendly name of the file type.
            /// </summary>
            public abstract string GetFileTypeName();
            /// <summary>
            /// List of strings for checking what file type to use. Use asterisks(*) for wildcards e.g. "*36D6*" or "*.bin"
            /// </summary>
            public abstract List<string> GetSupportedFileTypes();
            /// <summary>
            /// Filter string for the WPF file dialog.
            /// </summary>
            public abstract string GetSupportedFileFilters();
            /// <summary>
            /// Function to import a file to the main palette.
            /// </summary>
            /// <param name="filePath">Path to the file being loaded</param>
            /// <param name="Palettes">Reference to the palette to load the data into</param>
            /// <param name="currentPalette">The index to the palette that is currently active</param>
            /// <returns>Completed?</returns>
            public abstract bool Read(string filePath, Palette[] Palettes, int currentPalette);
            /// <summary>
            /// Function to export a palette to a file.
            /// </summary>
            /// <param name="filePath">Path to the file being written to</param>
            /// <param name="Palettes">Reference to the palette to read data from</param>
            /// <param name="currentPalette">The index to the palette that is currently active</param>
            public abstract void Write(string filePath, Palette[] Palettes, int currentPalette);

            // Overrides
            public override string ToString() { return GetFileTypeName(); }
        }

        public struct Credit
        {
            public string Name;
            public string Description;

            public Credit(string name, string description)
            {
                Name = name;
                Description = description;
            }

        }

    }
}
