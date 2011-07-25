#region Using
using System;
using System.IO;
using System.Net;
using AwesomiumSharp;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AwesomiumSharp.Windows.Controls;
#endregion

namespace TabbedWPFSample
{
    [TemplatePart( Name = TabView.ElementBrowser, Type = typeof( WebControl ) )]
    [TemplatePart( Name = TabView.ElementAddressBox, Type = typeof( WebControl ) )]
    class TabView : Control
    {
        #region Constants
        internal const String ElementBrowser = "PART_Browser";
        internal const String ElementAddressBox = "PART_AddressBox";

        private const String JS_FAVICON = "(function(){links = document.getElementsByTagName('link'); wHref=window.location.protocol + '//' + window.location.hostname + '/favicon.ico'; for(i=0; i<links.length; i++){s=links[i].rel; if(s.indexOf('icon') != -1){ wHref = links[i].href }; }; return wHref; })();";
        #endregion

        #region Fields
        private TextBox AddressBox;
        private String initialUrl;
        private MainWindow parentWindow;
        #endregion


        #region Ctors
        static TabView()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( TabView ), new FrameworkPropertyMetadata( typeof( TabView ) ) );
        }

        internal TabView( MainWindow parent, String url )
        {
            parentWindow = parent;
            initialUrl = url;

            this.Loaded += OnLoaded;
        }
        #endregion


        #region Overrides
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            AddressBox = GetTemplateChild( ElementAddressBox ) as TextBox;

            if ( AddressBox != null )
            {
                AddressBox.KeyDown += AddressBox_KeyDown;
                AddressBox.PreviewMouseLeftButtonDown += AddressBox_PreviewMouseLeftButtonDown;
                AddressBox.GotKeyboardFocus += AddressBox_GotKeyboardFocus;
            }

            this.SetValue( TabView.BrowserPropertyKey, GetTemplateChild( ElementBrowser ) );

            if ( Browser != null )
            {
                Browser.OpenExternalLink += OnOpenExternalLink;
                Browser.BeginLoading += OnBeginLoading;
                Browser.DomReady += OnDomReady;
                Browser.SelectLocalFiles += OnSelectLocalFiles;
                Browser.Download += OnDownload;
            }
        }
        #endregion

        #region Methods
        public void Close()
        {
            this.CommandBindings.Clear();

            if ( Browser != null )
                Browser.Close();
        }

        private void UpdateFavicon()
        {
            JSValue val = Browser.ExecuteJavascriptWithResult( JS_FAVICON );

            if ( ( val != null ) && ( val.Type == JSValueType.String ) )
            {
                Task.Factory.StartNew<BitmapImage>( GetFavicon, val.ToString() ).ContinueWith(
                    ( t ) =>
                    {
                        if ( t.Exception == null )
                            this.SetValue( TabView.FaviconPropertyKey, t.Result );
                    },
                    TaskScheduler.FromCurrentSynchronizationContext() );
            }
        }

        private static BitmapImage GetFavicon( Object href )
        {
            using ( WebClient client = new WebClient() )
            {
                Byte[] data = client.DownloadData( href.ToString() );

                if ( ( data != null ) && ( data.Length > 0 ) )
                {
                    MemoryStream stream = new MemoryStream( data );
                    BitmapImage bitmap = new BitmapImage();

                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    return bitmap;
                }
            }

            return null;
        }
        #endregion

        #region Properties

        #region ParentWindow
        public MainWindow ParentWindow
        {
            get
            {
                return parentWindow;
            }
        }
        #endregion

        #region Browser
        public WebControl Browser
        {
            get { return (WebControl)this.GetValue( TabView.BrowserProperty ); }
        }

        private static readonly DependencyPropertyKey BrowserPropertyKey =
            DependencyProperty.RegisterReadOnly( "Browser",
            typeof( WebControl ), typeof( TabView ),
            new FrameworkPropertyMetadata( null ) );

        public static readonly DependencyProperty BrowserProperty =
            BrowserPropertyKey.DependencyProperty;
        #endregion

        #region Favicon
        public ImageSource Favicon
        {
            get { return (ImageSource)this.GetValue( TabView.FaviconProperty ); }
        }

        private static readonly DependencyPropertyKey FaviconPropertyKey =
            DependencyProperty.RegisterReadOnly( "Favicon",
            typeof( ImageSource ), typeof( TabView ),
            new FrameworkPropertyMetadata( null ) );

        public static readonly DependencyProperty FaviconProperty =
            FaviconPropertyKey.DependencyProperty;
        #endregion

        #region DownloadProgress
        public int DownloadProgress
        {
            get { return (int)this.GetValue( TabView.DownloadProgressProperty ); }
            protected set { this.SetValue( TabView.DownloadProgressPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey DownloadProgressPropertyKey =
            DependencyProperty.RegisterReadOnly( "DownloadProgress",
            typeof( int ), typeof( TabView ),
            new FrameworkPropertyMetadata( 0 ) );

        public static readonly DependencyProperty DownloadProgressProperty =
            DownloadProgressPropertyKey.DependencyProperty;
        #endregion

        #region IsSelected
        public bool IsSelected
        {
            get { return (bool)this.GetValue( IsSelectedProperty ); }
            set { SetValue( IsSelectedProperty, value ); }
        }

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register( "IsSelected",
            typeof( bool ), typeof( TabView ),
            new FrameworkPropertyMetadata( false, IsSelectedChanged ) );

        private static void IsSelectedChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            TabView owner = (TabView)d;
            bool value = (bool)e.NewValue;

            // Add handling.
            if ( value )
                owner.ParentWindow.SelectedView = owner;
            else if ( owner.ParentWindow.SelectedView == owner )
                owner.ParentWindow.SelectedView = null;
        }
        #endregion        

        #endregion

        #region Event Handlers
        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            if ( ( Browser != null ) && ( initialUrl != null ) )
            {
                Browser.LoadURL( initialUrl );
                initialUrl = null;
            }
        }

        private void OnOpenExternalLink( object sender, OpenExternalLinkEventArgs e )
        {
            ParentWindow.OpenURL( e.Url );
        }

        private void OnBeginLoading( object sender, BeginLoadingEventArgs e )
        {
            this.ClearValue( TabView.FaviconPropertyKey );
        }

        private void OnDomReady( object sender, EventArgs e )
        {
            UpdateFavicon();
        }

        private void OnSelectLocalFiles( object sender, SelectLocalFilesEventArgs e )
        {
            OpenFileDialog dialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ),
                CheckFileExists = true,
                Multiselect = e.SelectMultipleFiles
            };

            if ( ( dialog.ShowDialog( ParentWindow ) ?? false ) && ( dialog.FileNames.Length > 0 ) )
            {
                e.SelectedFiles = dialog.FileNames;
            }
        }
        private void OnDownload( object sender, UrlEventArgs e )
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                FileName = Path.GetFileName( e.Url ),
                InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments )
            };

            if ( ( dialog.ShowDialog( ParentWindow ) ?? false ) && ( dialog.FileNames.Length == 1 ) )
            {
                ParentWindow.DownloadFile( e.Url, dialog.FileName );
            }
        }

        private void AddressBox_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key == Key.Return )
            {
                BindingExpression bind = BindingOperations.GetBindingExpression( AddressBox, TextBox.TextProperty );

                if ( bind != null )
                {
                    bind.UpdateSource();
                }
            }
        }

        private void AddressBox_PreviewMouseLeftButtonDown( object sender, MouseButtonEventArgs e )
        {
            if ( !AddressBox.IsKeyboardFocusWithin )
            {
                AddressBox.Focus();
                e.Handled = true;
            }
        }
        private void AddressBox_GotKeyboardFocus( object sender, KeyboardFocusChangedEventArgs e )
        {
            AddressBox.SelectAll();
        }
        #endregion
    }
}
