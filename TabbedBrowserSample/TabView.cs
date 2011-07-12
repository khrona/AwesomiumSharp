using System;
using AwesomiumSharp;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

namespace TabbedBrowserSample
{
    public class OpenLinkEventArgs : EventArgs
    {
        public OpenLinkEventArgs( string url ) { this.url = url; }
        public string url;
    };

    public delegate void OpenLinkHandler( object sender, OpenLinkEventArgs e );

    public class TabView
    {
        WebView webView;
        TabItem tab;
        WriteableBitmap src;
        Int32Rect myRect;
        TabControl tabControl;
        Image img;
        string url, title;
        int zoomPercent = 100;
        ToolTip tip;
        bool hasFocus = true;
        bool isLoading;

        public event EventHandler OnUpdateURL;
        public event EventHandler OnUpdateTitle;
        public event OpenLinkHandler OnOpenExternalLink;

        public TabView( TabControl tabControl )
        {
            this.tabControl = tabControl;
            this.url = "";
            this.title = "New Tab";

            tab = new TabItem();
            img = new Image();

            img.Focusable = true;
            tab.Content = img;
            tab.Header = title;
            tabControl.Items.Add( tab );
            tab.Height = 25;
            img.Width = tabControl.Width;
            img.Height = tabControl.Height - (int)tab.Height - 2;
            tip = new ToolTip();
            tip.IsOpen = false;
            tab.ToolTip = tip;
            ToolTipService.SetIsEnabled( tab, false );

            src = new WriteableBitmap( (int)tabControl.Width, (int)tabControl.Height - (int)tab.Height - 2, 96, 96, PixelFormats.Bgra32, BitmapPalettes.WebPaletteTransparent );
            img.Source = src;
            tab.Content = img;
            myRect = new Int32Rect( 0, 0, (int)tabControl.Width, (int)tabControl.Height - (int)tab.Height - 2 );
            webView = WebCore.CreateWebview( (int)tabControl.Width, (int)tabControl.Height - (int)tab.Height - 2 );
            webView.IsDirtyChanged += OnIsDirtyChanged;
            webView.Focus();

            img.MouseWheel += mainWindowMouseWheel;
            img.MouseMove += browserImageMouseMove;
            img.MouseDown += browserImageMouseDown;
            img.MouseUp += browserImageMouseUp;

            // Setup Callbacks
            webView.BeginNavigation += onBeginNavigation;
            webView.BeginLoading += onBeginLoading;
            webView.CursorChanged += onChangeCursor;
            webView.ToolTipChanged += onChangeTooltip;
            webView.LoadCompleted += onFinishLoading;
            webView.OpenExternalLink += onOpenExternalLink;
            webView.TitleReceived += onReceiveTitle;
            webView.Crashed += onWebviewCrashed;
        }

        public void loadUrl( String url )
        {
            webView.LoadURL( url );
        }

        ~TabView()
        {
            webView.IsDirtyChanged -= OnIsDirtyChanged;
            webView.Close();
        }

        public void Close()
        {
            webView.Close();
        }

        public void focus()
        {
            hasFocus = true;
            webView.Focus();
            webView.ResumeRendering();
            ToolTipService.SetIsEnabled( tab, false );
        }

        public void unfocus()
        {
            hasFocus = false;
            webView.Unfocus();
            webView.PauseRendering();
            tip.IsOpen = false;
            tip.Content = title;
            ToolTipService.SetIsEnabled( tab, true );
        }

        private void OnIsDirtyChanged( object sender, EventArgs e )
        {
            render();
        }

        public void render()
        {
            if ( webView.IsDirty && hasFocus )
            {
                RenderBuffer rBuffer = webView.Render();
                rBuffer.CopyToBitmap( src );
                img.Source = src;
            }
        }

        public TabItem getTab()
        {
            return tab;
        }

        public void zoomOut()
        {
            if ( zoomPercent > 10 )
                webView.Zoom = ( zoomPercent -= 20 );
        }

        public void zoomIn()
        {
            if ( zoomPercent < 500 )
                webView.Zoom = ( zoomPercent += 20 );
        }

        void mainWindowMouseWheel( object sender, MouseWheelEventArgs e )
        {
            webView.InjectMouseWheel( e.Delta );
        }

        void browserImageMouseMove( object sender, MouseEventArgs e )
        {
            Point position = e.GetPosition( img );
            webView.InjectMouseMove( (int)position.X, (int)position.Y );
        }

        void browserImageMouseDown( object sender, MouseButtonEventArgs e )
        {
            img.Focus();
            webView.Focus();
            webView.InjectMouseDown( AwesomiumSharp.MouseButton.Left );
        }

        void browserImageMouseUp( object sender, MouseButtonEventArgs e )
        {
            webView.InjectMouseUp( AwesomiumSharp.MouseButton.Left );
        }

        public void handleKeyboardEvent( int msg, int wParam, int lParam )
        {
            webView.InjectKeyboardEventWin( msg, wParam, lParam );
        }

        public void goToHistoryOffset( int offset )
        {
            webView.GoToHistoryOffset( offset );
        }

        public void reload()
        {
            webView.Reload();
        }

        public void executeJavascript( string javascript )
        {
            webView.ExecuteJavascript( javascript );
        }

        public void loadURL( string URL )
        {
            webView.LoadURL( URL );
        }

        public string getURL()
        {
            return url;
        }

        public string getTitle()
        {
            return title;
        }

        public void print()
        {
            webView.Print();
        }

        private void updateTabHeader()
        {
            string tabTitle = title.Length > 25 ? String.Format( "{0}...", title.Remove( 25, title.Length - 25 ) ) : title;

            if ( isLoading )
            {
                StackPanel panel = new StackPanel { Height = 18, Orientation = Orientation.Horizontal };
                TextBlock titleBlock = new TextBlock { Text = tabTitle, Margin = new Thickness( 0, 0, 5, 0 ) };
                panel.Children.Add( titleBlock );
                GifImage indicator = new GifImage( new Uri( "throbber.gif", UriKind.Relative ) ) { Width = 16, Height = 16 };
                panel.Children.Add( indicator );

                tab.Header = panel;
            }
            else
            {
                tab.Header = tabTitle;
            }
        }

        private void onBeginNavigation( object sender, BeginNavigationEventArgs e )
        {
            isLoading = true;
            updateTabHeader();
        }

        private void onBeginLoading( object sender, BeginLoadingEventArgs e )
        {
            url = e.Url;
            isLoading = true;
            updateTabHeader();

            if ( OnUpdateURL != null )
                OnUpdateURL( this, null );
        }

        private void onChangeCursor( object sender, ChangeCursorEventArgs e )
        {
            if ( e.CursorType == AwesomiumSharp.CursorType.Hand )
                tab.Cursor = Cursors.Hand;
            else if ( e.CursorType == AwesomiumSharp.CursorType.Ibeam )
                tab.Cursor = Cursors.IBeam;
            else
                tab.Cursor = Cursors.Arrow;
        }

        private void onChangeTooltip( object sender, ChangeToolTipEventArgs e )
        {
            if ( e.ToolTip.Length > 0 )
            {
                tip.Content = e.ToolTip;
                tip.IsOpen = true;
                tip.IsEnabled = true;
            }
            else
            {
                tip.IsOpen = false;
                tip.IsEnabled = false;
            }
        }

        private void onFinishLoading( object sender, EventArgs e )
        {
            isLoading = false;
            updateTabHeader();
        }

        private void onOpenExternalLink( object sender, OpenExternalLinkEventArgs e )
        {
            if ( OnOpenExternalLink != null && e.Url.Length > 0 )
                OnOpenExternalLink( this, new OpenLinkEventArgs( e.Url ) );
        }

        private void onReceiveTitle( object sender, ReceiveTitleEventArgs e )
        {
            title = e.Title;
            updateTabHeader();

            if ( OnUpdateTitle != null )
                OnUpdateTitle( this, null );
        }

        private void onWebviewCrashed( object sender, EventArgs e )
        {
            tab.Content = "Oh no! Something broke-- this tab has crashed.";
        }
    }

    class GifImage : Image
    {
        private double fps = 14.2;

        public int FrameIndex
        {
            get { return (int)GetValue( FrameIndexProperty ); }
            set { SetValue( FrameIndexProperty, value ); }
        }

        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register( "FrameIndex", typeof( int ), typeof( GifImage ), new UIPropertyMetadata( 0, ChangingFrameIndex ) );

        static void ChangingFrameIndex( DependencyObject obj, DependencyPropertyChangedEventArgs ev )
        {
            GifImage ob = obj as GifImage;
            ob.Source = ob.gf.Frames[ (int)ev.NewValue ];
            ob.InvalidateVisual();
        }

        GifBitmapDecoder gf;
        Int32Animation anim;
        bool isRunning;

        public GifImage( Uri uri )
        {
            gf = new GifBitmapDecoder( uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default );
            anim = new Int32Animation( 0, gf.Frames.Count - 1, new Duration( new TimeSpan( 0, 0, 0, gf.Frames.Count / (int)fps,
                (int)( ( gf.Frames.Count / (double)fps - gf.Frames.Count / (int)fps ) * 1000 ) ) ) );
            anim.RepeatBehavior = RepeatBehavior.Forever;
            Source = gf.Frames[ 0 ];
        }

        protected override void OnRender( DrawingContext dc )
        {
            base.OnRender( dc );
            if ( !isRunning )
            {
                BeginAnimation( FrameIndexProperty, anim );
                isRunning = true;
            }
        }
    }
}