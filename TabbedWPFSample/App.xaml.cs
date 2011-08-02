using System;
using System.Windows;
using TabbedWPFSample.Properties;
using System.Diagnostics;
using System.Reflection;

namespace TabbedWPFSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    partial class App : Application
    {
        /// <summary>
        /// We need this because our main window no longer has a 
        /// XAML file to be set as a StartupUri.
        /// We achieve this by simply changing the "Build Action"
        /// of App.xaml, from ApplicationDefinition to Page.
        /// </summary>
        [STAThread()]
        static void Main( string[] args )
        {
            App app = new App();
            app.InitializeComponent();
            app.Run();

            // Changes to settings may require a restart.
            if ( My.Application.Restart )
                Process.Start( Assembly.GetEntryAssembly().CodeBase );
        }

        protected override void OnStartup( StartupEventArgs e )
        {
            // Force single instance application.
            WPFSingleInstance.Make( SecondInstance );

            this.MainWindow = new MainWindow( e.Args );
            this.MainWindow.Show();
        }

        private static void SecondInstance( object obj )
        {
            // When the user is trying to launch a new instance of the application,
            // trick him/her by simply opening a new window in this application.
            // It is important that no more that one WebCore is started per process.
            MainWindow win = new MainWindow( new string[] { Settings.Default.HomeURL } );
            win.Show();
        }
    }
}
