using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ManiaPal
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Assembly
        public static Version Version = Assembly.GetExecutingAssembly().GetName().Version;
        public static string StartDirectory = AppDomain.CurrentDomain.BaseDirectory;
        
        // Program Information 
        public static string ProgramName = "ManiaPal";
        public static string ProgramTitle = "Sonic Mania Palette Editor";
        public static string ProgramVersion = $"{Version.Major}.{Version.Minor:D2}";
        
        public static bool HideInstead = false;
        public static string[] Args = new string[0];

        [STAThread]
        public static void Main(string[] args)
        {
            // Use TLSv1.2
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            // Force US English
            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("en-US");

#if !DEBUG
            // Enable our crash handler if compiled in Release
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                ExceptionWindow.UnhandledExceptionEventHandler(e.ExceptionObject as Exception, e.IsTerminating);
            };
#endif

            Args = args;
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }
    }
}
