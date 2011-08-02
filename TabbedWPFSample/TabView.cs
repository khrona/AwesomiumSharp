/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    TabView.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 *__________________________________________________________________________
 *
 *  Notes:
 *
 *  Represents the contents of a tab in an application window. This control
 *  contains the WebControl and an independent bar with the address-box,
 *  navigation buttons etc. It is lookless and the only two needed parts
 *  (the WebControl and the address-box) are exposed as template parts
 *  so that the control can easily be styled. The default style is defined
 *  in Themes/TabView.xaml.
 *   
 ***************************************************************************/

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
        public const String ElementBrowser = "PART_Browser";
        public const String ElementAddressBox = "PART_AddressBox";

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
        /// <summary>
        /// Closes the view and performs cleanup.
        /// </summary>
        public void Close()
        {
            this.CommandBindings.Clear();

            if ( Browser != null )
                Browser.Close();
        }

        private void UpdateFavicon()
        {
            // Execute some simple javascript that will search for a favicon.
            using ( JSValue val = Browser.ExecuteJavascriptWithResult( JS_FAVICON ) )
            {
                // Check if we got a valid answer.
                if ( ( val != null ) && ( val.Type == JSValueType.String ) )
                {
                    // We do not need to perform the download of the favicon synchronously.
                    // May be a full icon set (thus big).
                    Task.Factory.StartNew<BitmapImage>( GetFavicon, val.ToString() ).ContinueWith(
                        ( t ) =>
                        {
                            // If the download completed successfully, set the new favicon.
                            // This post-completion procedure is executed synchronously.
                            if ( t.Exception == null )
                                this.SetValue( TabView.FaviconPropertyKey, t.Result );
                        },
                        TaskScheduler.FromCurrentSynchronizationContext() );
                }
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
                    bitmap.Freeze(); // Needed to safely pass the bitmap across threads.

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
        // This property is bound to the IsSelected property of the
        // WebTabItem that hosts us. It allows us to inform the parent
        // window of the currently selected tab, at any given time.
        // As the binding is a TwoWay binding, this also allows us
        // to update the selected tab from code.
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

            if ( value )
                owner.ParentWindow.SelectedView = owner;
        }
        #endregion

        #region IsSourceView
        public bool IsSourceView
        {
            get { return (bool)this.GetValue( IsSourceViewProperty ); }
            protected set { SetValue( IsSourceViewPropertyKey, value ); }
        }

        public static readonly DependencyPropertyKey IsSourceViewPropertyKey =
            DependencyProperty.RegisterReadOnly( "IsSourceView",
            typeof( bool ), typeof( TabView ),
            new FrameworkPropertyMetadata( false ) );

        public static readonly DependencyProperty IsSourceViewProperty =
            IsSourceViewPropertyKey.DependencyProperty;
        #endregion

        #endregion

        #region Event Handlers
        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            if ( ( Browser != null ) && ( initialUrl != null ) )
            {
                // The WebControl has been instantiated by now (and the core
                // has started if this was the first view created). Load
                // the URL that was given to us during construction of the tab.
                Browser.LoadURL( initialUrl );
                initialUrl = null;
            }
        }

        private void OnOpenExternalLink( object sender, OpenExternalLinkEventArgs e )
        {
            // Inform the window that the web control is asking for a new window or tab.
            // Currently, the event does not provide information of whether this request
            // is the result of a user clicking on a link with target="_blank" or javascript
            // calling window.open() etc. We assume target="_blank" and open a new tab.
            // When we get this info, we will be able to support floating windows.
            ParentWindow.OpenURL( e.Url );
        }

        private void OnBeginLoading( object sender, BeginLoadingEventArgs e )
        {
            // By now we have already navigated to the address.
            // Clear the old favicon. The default style, will assign
            // a default (globe) icon to the tab when null is set for
            // FaviconProperty.
            this.ClearValue( TabView.FaviconPropertyKey );
        }

        private void OnDomReady( object sender, EventArgs e )
        {
            // DOM is ready. We can start looking for a favicon.
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

            // Unlike WindowsForms, dialogs in WPF return a nullable boolean value.
            if ( ( dialog.ShowDialog( ParentWindow ) ?? false ) && ( dialog.FileNames.Length > 0 ) )
            {
                e.SelectedFiles = dialog.FileNames;
            }
        }
        private void OnDownload( object sender, UrlEventArgs e )
        {
            SaveFileDialog dialog = new SaveFileDialog()
            {
                // This should give us the file name part of the URL.
                FileName = Path.GetFileName( e.Url ),
                // We set MyDocuments as the default. You can change this as you wish
                // but make sure the specified folder actually exists.
                InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments )
            };

            // Unlike WindowsForms, dialogs in WPF return a nullable boolean value.
            if ( ( dialog.ShowDialog( ParentWindow ) ?? false ) && ( dialog.FileNames.Length == 1 ) )
            {
                ParentWindow.DownloadFile( e.Url, dialog.FileName );
            }
        }

        private void AddressBox_KeyDown( object sender, KeyEventArgs e )
        {
            if ( e.Key == Key.Return )
            {
                // The correct specification of the Binding on the Text property of the address-box,
                // should specify UpdateSourceTrigger=Explicit. This allows us to only update
                // the source of the Binding when the user hits Return. This assumes that the Text
                // property of the address-box is binded to the Source property of the WebControl.
                BindingExpression bind = BindingOperations.GetBindingExpression( AddressBox, TextBox.TextProperty );

                // If the parent binding has a different UpdateSourceTrigger set, we assume the designer
                // has something else in his/her mind. Do not perform an explicit update of the source.
                if ( ( bind != null ) && ( bind.ParentBinding.UpdateSourceTrigger == UpdateSourceTrigger.Explicit ) )
                {
                    bind.UpdateSource();
                }

                // Move focus to the view.
                if ( Browser != null )
                    Browser.Focus();
            }
        }

        // The following two handlers, allow us to handle the focusing of the address-box ourselves.
        // In WPF, this is the only way to perform a SelectAll when the address-box is focused.

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
