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
        }

        public MainWindow( string[] args )
        {
            initialUrls = args;

            tabViews = new ObservableCollection<TabView>();
            this.SetValue( MainWindow.ViewsPropertyKey, tabViews );
            downloads = new ObservableCollection<Download>();
            this.SetValue( MainWindow.DownloadsPropertyKey, downloads );

            this.Loaded += OnLoaded;
            this.CommandBindings.Add( new CommandBinding( MainWindow.OpenInTab, OnOpenTab, CanOpenTab ) );
            this.CommandBindings.Add( new CommandBinding( MainWindow.OpenInWindow, OnOpenWindow, CanOpenWindow ) );
            this.CommandBindings.Add( new CommandBinding( MainWindow.CloseTab, OnCloseTab ) );
            this.CommandBindings.Add( new CommandBinding( MainWindow.NewTab, OnNewTab ) );
            this.CommandBindings.Add( new CommandBinding( ApplicationCommands.Close, OnClose ) );
        }
        #endregion


        #region Overrides
        protected override void OnSourceInitialized( EventArgs e )
        {
            base.OnSourceInitialized( e );
        }

        protected override void OnClosing( CancelEventArgs e )
        {
            this.Hide();

            foreach ( TabView view in tabViews )
            {
                if ( view.Browser != null )
                    view.Browser.Close();
            }

            tabViews.Clear();

            WebCore.Shutdown();
            base.OnClosing( e );
        }
        #endregion

        #region Methods
        public void OpenURL( String url )
        {
            if ( WebCore.IsRunning )
            {
                tabViews.Add( new TabView( this, url ) );
            }
        }

        public void DownloadFile( string url, string file )
        {
            Download download = new Download( url, file );
            Download existingDownload = downloads.SingleOrDefault( ( d ) => d == download );

            this.DownloadsVisible = true;

            if ( existingDownload != null )
                download = existingDownload;
            else
                downloads.Add( download );

            download.Start();
        }
        #endregion

        #region Properties

        #region Static
        public static RoutedUICommand OpenInTab { get; private set; }
        public static RoutedUICommand OpenInWindow { get; private set; }
        public static RoutedUICommand NewTab { get; private set; }
        public static RoutedUICommand CloseTab { get; private set; }
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
            // Setup WebCore with plugins enabled.            
            WebCoreConfig config = new WebCoreConfig { EnablePlugins = true, HomeURL = Settings.Default.HomeURL };
            // Caution! Do not initialize the WebCore in window's constructor.
            // This is a startup window and a synchronization context
            // (necessary for auto-update), is not yet available during
            // construction; the Dispatcher is not running yet (see App.xaml.cs).
            WebCore.Initialize( config );

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
                Process.Start( Assembly.GetExecutingAssembly().Location, String.Format( "\"{0}\"", target ) );
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
                    // Close the application.
                    this.Close();
                else
                {
                    TabView view = (TabView)e.Parameter;
                    // Remove the tab.
                    tabViews.Remove( view );
                    // Close the view and the WebControl.
                    view.Close();
                }
            }
        }

        private void OnClose( object sender, ExecutedRoutedEventArgs e )
        {
            if ( ( e.Parameter != null ) && ( String.Compare( e.Parameter.ToString(), "Downloads", true ) == 0 ) )
                this.DownloadsVisible = false;
        }
        #endregion
    }
}
