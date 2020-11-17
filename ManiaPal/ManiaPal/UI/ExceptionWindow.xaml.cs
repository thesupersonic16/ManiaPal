using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ManiaPal
{
    /// <summary>
    /// Interaction logic for ExceptionWindow.xaml
    /// </summary>
    public partial class ExceptionWindow : Window
    {

        public Exception _Exception;
        public string ExtraInfo;

        public ExceptionWindow(Exception exception, string extraInfo = "") : this()
        {
            _Exception = exception;
            ExtraInfo = extraInfo;
        }

        public ExceptionWindow()
        {
            InitializeComponent();
        }

        public string GetReport(bool useMarkdown = false)
        {
            var body = new StringBuilder();

            body.AppendLine($"ManiaPal Info:");

            if (useMarkdown) body.AppendLine("```");
            body.AppendLine($"    Name: {App.ProgramName}");
            body.AppendLine($"    Version: {App.ProgramVersion}");
            body.AppendLine($"    Args: {string.Join(" ", App.Args)}");
            body.AppendLine($"    StartDir: {App.StartDirectory}");
            if (useMarkdown) body.AppendLine("```");

            body.AppendLine("");

            if (!string.IsNullOrEmpty(ExtraInfo))
            {
                body.AppendLine($"Extra Information: {ExtraInfo}");
            }

            if (_Exception != null)
            {
                body.AppendLine($"Exception:");
                if (useMarkdown) body.AppendLine("```");

                body.AppendLine($"    Type: {_Exception.GetType().Name}");
                body.AppendLine($"    Message: {_Exception.Message}");
                body.AppendLine($"    Source: {_Exception.Source}");
                body.AppendLine($"    Function: {_Exception.TargetSite}");
                if (_Exception.StackTrace != null)
                    body.AppendLine($"    StackTrace: \n    {_Exception.StackTrace.Replace("\n", "\n    ")}");

                body.AppendLine($"    InnerException: {_Exception.InnerException}");

                if (useMarkdown) body.AppendLine("```");
                body.AppendLine("");
            }

            return body.ToString();
        }

        public static void UnhandledExceptionEventHandler(Exception e, bool fatal = false)
        {
            var window = new ExceptionWindow(e);
            if (fatal)
                window.Header.Content = "ManiaPal has ran into a Fatal Error!";
            window.ShowDialog();
        }

        private void Button_CopyLog_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(GetReport(true));
        }

        private void Button_Ignore_Click(object sender, RoutedEventArgs e)
        {
            var messagebox = new HedgeMessageBox("Save Current Palette?", 
                "Do you want to try save your current palette\n" +
                "as a backup before contining/exiting?\n\n" +
                "This will not work if ManiaPal crashed while saving!");

            messagebox.AddButton("Save Palette", () =>
            {
                try
                {
                    var MP = MainWindow.Instance;
                    if (MP.LoadedFileType != null)
                    {
                        MP.LoadedFileType.filePath += ".bak";
                        MP.LoadedFileType.Write(MP.LoadedFileType.filePath, MP.Palettes, MP.CurrentPaletteSet);
                        return;
                    }
                }
                catch
                {
                    MessageBox.Show("Failed");
                }
                messagebox.Close();
            });
            messagebox.AddButton("Don't Save", () => { messagebox.Close(); });
            messagebox.ShowDialog();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox_ExceptionInfo.Text = GetReport();
        }
    }
}
