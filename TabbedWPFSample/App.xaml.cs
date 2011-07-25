using System;
using System.Windows;

namespace TabbedWPFSample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
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
            app.MainWindow = new MainWindow( args );
            app.MainWindow.Show();
            app.Run();
        }
    }
}
