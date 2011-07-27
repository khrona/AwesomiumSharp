/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    MainWindow.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 *__________________________________________________________________________
 *
 *  Notes:
 *
 *  Application window. This does not act as a main-parent window. 
 *  It's reusable. The application will exit when all windows are closed.
 *  Completely styled with a custom WindowChrome. Check the XAML file.
 *   
 ***************************************************************************/

#region Using
using System;
using System.Linq;
using AwesomiumSharp;
using System.Windows;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using TabbedWPFSample.Properties;
using System.Collections.ObjectModel;
#endregion

namespace TabbedWPFSample
{
    class MainWindow : Window
    {
        #region Fields
        private string[] initialUrls;
        private ObservableCollection<TabView> tabViews;
        private ObservableCollection<Download> downloads;
        #endregion


        #region Ctors
        static MainWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( MainWindow ), new FrameworkPropertyMetadata( typeof( MainWindow ) ) );

            // We implement some elementary commands.
            // The shortcuts specified, are the same as in Chrome.
            OpenInTab = new RoutedUICommand(
                Properties.Resources.OpenInNewTab,
                "OpenInTab",
                typeof( MainWindow ),
                new InputGestureCollection( new KeyGesture[] { new KeyGesture( Key.Enter, ModifierKeys.Control ) } ) );
            OpenInWindow = new RoutedUICommand(
                Properties.Resources.OpenInNewWindow,
                "OpenInWindow",
                typeof( MainWindow ),
                new InputGestureCollection( new KeyGesture[] { new KeyGesture( Key.Enter, ModifierKeys.Shift ) } ) );
            NewTab = new RoutedUICommand(
                Properties.Resources.NewTab,
                "NewTab",
                typeof( MainWindow ),
                new InputGestureCollection( new KeyGesture[] { new KeyGesture( Key.T, ModifierKeys.Control ) } ) );
            CloseTab = new RoutedUICommand(
                Properties.Resources.CloseTab,
                "CloseTab",
                typeof( MainWindow ),
                new InputGestureCollection( new KeyGesture[] { new KeyGesture( Key.W, ModifierKeys.Control ) } ) );
            ShowDownloads = new RoutedUICommand(
                Properties.Resources.Downloads,
                "ShowDownloads",
                typeof( MainWindow ) );
            ShowSettings = new RoutedUICommand(
                Properties.Resources.Settings,
                "ShowSettings",
                typeof( MainWindow ) );
        }

        public MainWindow( string[] args )
        {
            // Keep this. We will use it when we load.
            initialUrls = args;

            // Initialize collections.
            tabViews = new ObservableCollection<TabView>();
            this.SetValue( MainWindow.ViewsPropertyKey, tabViews );
            downloads = new ObservableCollection<Download>();
            this.SetValue( MainWindow.DownloadsPropertyKey, downloads );

            // Assign event handlers.
            this.Loaded += OnLoaded;

            // Assign command handlers.
            this.CommandBindings.Add( new CommandBinding( MainWindow.OpenInTab, OnOpenTab, CanOpenTab ) );
            this.CommandBindings.Add( new CommandBinding( MainWindow.OpenInWindow, OnOpenWindow, CanOpenWindow ) );
            this.CommandBindings.Add( new CommandBinding( MainWindow.CloseTab, OnCloseTab ) );
            this.CommandBindings.Add( new CommandBinding( MainWindow.NewTab, OnNewTab ) );
            this.CommandBindings.Add( new CommandBinding( MainWindow.ShowDownloads, OnShowDownloads ) );
            this.CommandBindings.Add( new CommandBinding( MainWindow.ShowSettings, OnShowSettings ) );
            this.CommandBindings.Add( new CommandBinding( ApplicationCommands.Close, OnClose ) );

            // Initialize (but DO NOT Start) the WebCore.
            MainWindow.InitializeCore();
        }
        #endregion


        #region Overrides
        protected override void OnClosing( CancelEventArgs e )
        {
            // Hide during cleanup.
            this.Hide();

            // Close the views and perform cleanup
            // for every tab.
            foreach ( TabView view in tabViews )
                view.Close();

            tabViews.Clear();

            // We may not be the last window open.
            if ( Application.Current.Windows.Count == 1 )
                WebCore.Shutdown();

            base.OnClosing( e );
        }
        #endregion

        #region Methods

        #region InitializeCore (static)
        private static void InitializeCore()
        {
            // We may be a new window in the same process.
            if ( !WebCore.IsRunning )
            {
                // Setup WebCore with plugins enabled.            
                WebCoreConfig config = new WebCoreConfig
                {
                    // !THERE CAN ONLY BE A SINGLE WebCore RUNNING PER PROCESS!
                    // We have insured that our application is single instance,
                    // with the use of the WPFSingleInstance utility.
                    // We can now safely enable cache and cookies.
                    SaveCacheAndCookies = true,
                    // In case our application is installed in ProgramFiles,
                    // we wouldn't want the WebCore to attempt to create folders
                    // and files in there. We do not have the required privileges.
                    // Furthermore, it is important to allow each user account
                    // have its own cache and cookies. So, there's no better place
                    // than the Application User Data Path.
                    UserDataPath = My.Application.UserAppDataPath,
                    EnablePlugins = true,
                    HomeURL = Settings.Default.HomeURL,
                    // For the time being, we have to disable logging.
                    // WebCore will attempt to create the log file in the
                    // folder where the application resides. We do not want this
                    // for the same reasons mentioned above for UserDataPath.
                    LogLevel = LogLevel.None
                };

                // Caution! Do not start the WebCore in window's constructor.
                // This may be a startup window and a synchronization context
                // (necessary for auto-update), is may not be available during
                // construction; the Dispatcher may not be running yet (see App.xaml.cs).
                //
                // Setting the start parameter to false, let's us define
                // configuration settings early enough to be secure, but
                // actually delay the starting of the WebCore until
                // the first WebControl or WebView is created.
                WebCore.Initialize( config, false );
            }
        }
        #endregion


        #region OpenURL
        public void OpenURL( String url )
        {
            tabViews.Add( new TabView( this, url ) );
        }
        #endregion

        #region DownloadFile
        public void DownloadFile( string url, string file )
        {
            // Create a download item.
            Download download = new Download( url, file );
            // If the same file had previously been downloaded,
            // let the old one assume the identity of the new.
            Download existingDownload = downloads.SingleOrDefault( ( d ) => d == download );

            // Show the downloads bar.
            this.DownloadsVisible = true;

            if ( existingDownload != null )
                download = existingDownload;
            else
                downloads.Add( download );

            // Start downloading.
            download.Start();
        }
        #endregion

        #endregion

        #region Properties

        #region Static
        public static RoutedUICommand OpenInTab { get; private set; }
        public static RoutedUICommand OpenInWindow { get; private set; }
        public static RoutedUICommand NewTab { get; private set; }
        public static RoutedUICommand CloseTab { get; private set; }
        public static RoutedUICommand ShowDownloads { get; private set; }
        public static RoutedUICommand ShowSettings { get; private set; }
        #endregion


        #region Views
        public IEnumerable<TabView> Views
        {
            get { return (IEnumerable<TabView>)this.GetValue( MainWindow.ViewsProperty ); }
        }

        private static readonly DependencyPropertyKey ViewsPropertyKey =
            DependencyProperty.RegisterReadOnly( "Views",
            typeof( IEnumerable<TabView> ), typeof( MainWindow ),
            new FrameworkPropertyMetadata( null ) );

        public static readonly DependencyProperty ViewsProperty =
            ViewsPropertyKey.DependencyProperty;
        #endregion

        #region Downloads
        public IEnumerable<Download> Downloads
        {
            get { return (IEnumerable<Download>)this.GetValue( MainWindow.DownloadsProperty ); }
        }

        private static readonly DependencyPropertyKey DownloadsPropertyKey =
            DependencyProperty.RegisterReadOnly( "Downloads",
            typeof( IEnumerable<Download> ), typeof( MainWindow ),
            new FrameworkPropertyMetadata( null ) );

        public static readonly DependencyProperty DownloadsProperty =
            DownloadsPropertyKey.DependencyProperty;
        #endregion

        #region DownloadsVisible
        public bool DownloadsVisible
        {
            get { return (bool)this.GetValue( MainWindow.DownloadsVisibleProperty ); }
            protected set { this.SetValue( MainWindow.DownloadsVisiblePropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey DownloadsVisiblePropertyKey =
            DependencyProperty.RegisterReadOnly( "DownloadsVisible",
            typeof( bool ), typeof( MainWindow ),
            new FrameworkPropertyMetadata( false ) );

        public static readonly DependencyProperty DownloadsVisibleProperty =
            DownloadsVisiblePropertyKey.DependencyProperty;
        #endregion

        #region SelectedView
        public TabView SelectedView { get; internal set; }
        #endregion

        #endregion

        #region Event Handlers
        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            // Just like any respectable browser, we are ready to respond
            // to command line arguments passed if our browser is set as
            // the default browser or when a user attempts to open an html
            // file or shortcut with our application.
            bool openUrl = false;
            if ( ( initialUrls != null ) && ( initialUrls.Length > 0 ) )
            {
                foreach ( String url in initialUrls )
                {
                    Uri absoluteUri;
                    Uri.TryCreate( url, UriKind.Absolute, out absoluteUri );

                    if ( absoluteUri != null )
                    {
                        this.OpenURL( absoluteUri.AbsoluteUri );
                        openUrl = true;
                    }
                }

                initialUrls = null;
            }

            if ( !openUrl )
                this.OpenURL( Settings.Default.HomeURL );
        }

        private void OnOpenTab( object sender, ExecutedRoutedEventArgs e )
        {
            // If this is called from a menu item, the target URL is specified as a parameter.
            // If the user simply hit the shortcut, we need to get the target URL (if any) from the currently selected tab.
            // The same applies for the rest of link related commands below.
            string target = (string)e.Parameter ?? ( SelectedView != null ? SelectedView.Browser.TargetURL : String.Empty );

            if ( !String.IsNullOrWhiteSpace( target ) )
                this.OpenURL( target );
        }

        private void CanOpenTab( object sender, CanExecuteRoutedEventArgs e )
        {
            string target = (string)e.Parameter ?? ( SelectedView != null ? SelectedView.Browser.TargetURL : String.Empty );
            e.CanExecute = !String.IsNullOrWhiteSpace( target );
        }

        private void OnOpenWindow( object sender, ExecutedRoutedEventArgs e )
        {
            string target = (string)e.Parameter ?? ( SelectedView != null ? SelectedView.Browser.TargetURL : String.Empty );

            if ( !String.IsNullOrWhiteSpace( target ) )
            {
                MainWindow win = new MainWindow( new string[] { target } );
                win.Show();

                // Or we can launch a separate process. Not appropriate in Awesomium,
                // though safe for this sample since we are not using cache and logging.
                //Process.Start( Assembly.GetExecutingAssembly().Location, String.Format( "\"{0}\"", target ) );
            }
        }

        private void CanOpenWindow( object sender, CanExecuteRoutedEventArgs e )
        {
            string target = (string)e.Parameter ?? ( SelectedView != null ? SelectedView.Browser.TargetURL : String.Empty );
            e.CanExecute = !String.IsNullOrWhiteSpace( target );
        }

        private void OnNewTab( object sender, ExecutedRoutedEventArgs e )
        {
            this.OpenURL( Settings.Default.HomeURL );
        }

        private void OnCloseTab( object sender, ExecutedRoutedEventArgs e )
        {
            if ( ( e.Parameter != null ) && ( e.Parameter is TabView ) )
            {
                if ( tabViews.Count == 1 )
                    // This is the last tab we are attempting to close.
                    // Close the window. If this is the last window, the application exits.
                    this.Close();
                else
                {
                    TabView view = (TabView)e.Parameter;
                    // Remove the tab.
                    tabViews.Remove( view );
                    // Close the view and the WebControl.
                    view.Close();

                    GC.Collect();
                }
            }
        }

        private void OnShowDownloads( object sender, ExecutedRoutedEventArgs e )
        {
            this.DownloadsVisible = true;
        }

        private void OnShowSettings( object sender, ExecutedRoutedEventArgs e )
        {
            // Show here a settings dialog.
        }

        private void OnClose( object sender, ExecutedRoutedEventArgs e )
        {
            // The command we handle here is ApplicationCommands.Close. We need to maintain
            // the re-usability of this command, so we define a parameter for the downloads bar.
            if ( ( e.Parameter != null ) && ( String.Compare( e.Parameter.ToString(), "Downloads", true ) == 0 ) )
                this.DownloadsVisible = false;
        }
        #endregion
    }
}
