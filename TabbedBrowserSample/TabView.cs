using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AwesomiumSharp;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;
using System.Collections;
using System.Timers;

namespace TabbedBrowserSample
{
    public class OpenLinkEventArgs : EventArgs
    {
        public OpenLinkEventArgs(string url) { this.url = url; }
        public string url;
    };

    public delegate void OpenLinkHandler(object sender, OpenLinkEventArgs e);

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
        bool isLoading = false;

        public event EventHandler OnUpdateURL;
        public event EventHandler OnUpdateTitle;
        public event OpenLinkHandler OnOpenExternalLink;

        public TabView(TabControl tabControl)
        {
            this.tabControl = tabControl;
            this.url = "";
            this.title = "New Tab";

            tab = new TabItem();
            img = new Image();

            img.Focusable = true;
            tab.Content = img;
            tab.Header = title;
            tabControl.Items.Add(tab);
            tab.Height = 25;
            img.Width = tabControl.Width;
            img.Height = tabControl.Height - (int)tab.Height - 2;
            tip = new ToolTip();
            tip.IsOpen = false;
            tab.ToolTip = tip;
            ToolTipService.SetIsEnabled(tab, false);

            src = new WriteableBitmap((int)tabControl.Width, (int)tabControl.Height - (int)tab.Height - 2, 96, 96, PixelFormats.Bgra32, BitmapPalettes.WebPaletteTransparent);
            img.Source = src;
            tab.Content = img;
            myRect = new Int32Rect(0, 0, (int)tabControl.Width, (int)tabControl.Height - (int)tab.Height - 2);
            webView = WebCore.CreateWebview((int)tabControl.Width, (int)tabControl.Height - (int)tab.Height - 2);
            webView.Focus();

            img.MouseWheel += mainWindowMouseWheel;
            img.MouseMove += browserImageMouseMove;
            img.MouseDown += browserImageMouseDown;
            img.MouseUp += browserImageMouseUp;

            // Setup Callbacks
            webView.OnBeginNavigation += onBeginNavigation;
            webView.OnBeginLoading += onBeginLoading;
            webView.OnChangeCursor += onChangeCursor;
            webView.OnChangeTooltip += onChangeTooltip;
            webView.OnFinishLoading += onFinishLoading;
            webView.OnOpenExternalLink += onOpenExternalLink;
            webView.OnReceiveTitle += onReceiveTitle;
            webView.OnWebViewCrashed += onWebviewCrashed;
        }

        public void loadUrl(String url)
        {
            webView.LoadURL(url);
        }

        ~TabView()
        {
            webView.Dispose();
        }

        public void focus()
        {
            hasFocus = true;
            webView.Focus();
            webView.ResumeRendering();
            ToolTipService.SetIsEnabled(tab, false);
        }

        public void unfocus()
        {
            hasFocus = false;
            webView.Unfocus();
            webView.PauseRendering();
            tip.IsOpen = false;
            tip.Content = title;
            ToolTipService.SetIsEnabled(tab, true);
        }

        public void render()
        {
            if (webView.IsDirty() && hasFocus)
            {
                RenderBuffer rBuffer = webView.Render();
                rBuffer.CopyToBitmap(src);
                img.Source = src;
            }
        }

        public TabItem getTab()
        {
            return tab;
        }

        public void zoomOut()
        {
            if (zoomPercent > 10)
                webView.SetZoom(zoomPercent -= 20);
        }

        public void zoomIn()
        {
            if (zoomPercent < 500)
                webView.SetZoom(zoomPercent += 20);
        }

        void mainWindowMouseWheel(object sender, MouseWheelEventArgs e)
        {
            webView.InjectMouseWheel(e.Delta);
        }

        void browserImageMouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(img);
            webView.InjectMouseMove((int)position.X, (int)position.Y);
        }

        void browserImageMouseDown(object sender, MouseButtonEventArgs e)
        {
            img.Focus();
            webView.Focus();
            webView.InjectMouseDown(AwesomiumSharp.MouseButton.Left);
        }

        void browserImageMouseUp(object sender, MouseButtonEventArgs e)
        {
            webView.InjectMouseUp(AwesomiumSharp.MouseButton.Left);
        }

        public void handleKeyboardEvent(int msg, int wParam, int lParam)
        {
            webView.InjectKeyboardEventWin(msg, wParam, lParam);
        }

        public void goToHistoryOffset(int offset)
        {
            webView.GoToHistoryOffset(offset);
        }

        public void reload()
        {
            webView.Reload();
        }

        public void executeJavascript(string javascript)
        {
            webView.ExecuteJavascript(javascript);
        }

        public void loadURL(string URL)
        {
            webView.LoadURL(URL);
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
            string tabTitle;

            if (title.Length > 25)
                tabTitle = title.Remove(25, title.Length - 25) + "...";
            else
                tabTitle = title;

            if (isLoading)
            {
                StackPanel panel = new StackPanel();
                panel.Height = 18;
                panel.Orientation = Orientation.Horizontal;

                TextBlock titleBlock = new TextBlock();
                titleBlock.Text = tabTitle;
                titleBlock.Margin = new Thickness(0, 0, 5, 0);
                panel.Children.Add(titleBlock);

                GifImage indicator = new GifImage(new Uri("throbber.gif", UriKind.Relative));
                indicator.Width = 16;
                indicator.Height = 16;
                panel.Children.Add(indicator);

                tab.Header = panel;
            }
            else
            {
                tab.Header = tabTitle;
            }
        }

        private void onBeginNavigation(object sender, WebView.BeginNavigationEventArgs e)
        {
            isLoading = true;
            updateTabHeader();
        }

        private void onBeginLoading(object sender, WebView.BeginLoadingEventArgs e)
        {
            url = e.url;
            isLoading = true;
            updateTabHeader();

            if (OnUpdateURL != null)
                OnUpdateURL(this, null);
        }

        private void onChangeCursor(object sender, WebView.ChangeCursorEventArgs e)
        {
            if (e.cursorType == AwesomiumSharp.CursorType.Hand)
                tab.Cursor = Cursors.Hand;
            else if (e.cursorType == AwesomiumSharp.CursorType.Ibeam)
                tab.Cursor = Cursors.IBeam;
            else
                tab.Cursor = Cursors.Arrow;
        }

        private void onChangeTooltip(object sender, WebView.ChangeTooltipEventArgs e)
        {
            if (e.tooltip.Length > 0)
            {
                tip.Content = e.tooltip;
                tip.IsOpen = true;
                tip.IsEnabled = true;
            }
            else
            {
                tip.IsOpen = false;
                tip.IsEnabled = false;
            }
        }

        private void onFinishLoading(object sender, WebView.FinishLoadingEventArgs e)
        {
            isLoading = false;
            updateTabHeader();
        }

        private void onOpenExternalLink(object sender, WebView.OpenExternalLinkEventArgs e)
        {
            if (OnOpenExternalLink != null && e.url.Length > 0)
                OnOpenExternalLink(this, new OpenLinkEventArgs(e.url));
        }

        private void onReceiveTitle(object sender, WebView.ReceiveTitleEventArgs e)
        {
            title = e.title;
            updateTabHeader();

            if (OnUpdateTitle != null)
                OnUpdateTitle(this, null);
        }

        private void onWebviewCrashed(object sender, WebView.WebViewCrashedEventArgs e)
        {
            tab.Content = "Oh no! Something broke-- this tab has crashed.";
        }
    }

    class GifImage : Image
    {
        private double fps = 14.2;

        public int FrameIndex
        {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }

        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));

        static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            GifImage ob = obj as GifImage;
            ob.Source = ob.gf.Frames[(int)ev.NewValue];
            ob.InvalidateVisual();
        }

        GifBitmapDecoder gf;
        Int32Animation anim;
        bool isRunning = false;

        public GifImage(Uri uri)
        {
            gf = new GifBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            anim = new Int32Animation(0, gf.Frames.Count - 1, new Duration(new TimeSpan(0, 0, 0, gf.Frames.Count / (int)fps, 
                (int)((gf.Frames.Count / (double)fps - gf.Frames.Count / (int)fps) * 1000))));
            anim.RepeatBehavior = RepeatBehavior.Forever;
            Source = gf.Frames[0];
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (!isRunning)
            {
                BeginAnimation(FrameIndexProperty, anim);
                isRunning = true;
            }
        }
    }
}