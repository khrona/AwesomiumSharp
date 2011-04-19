using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Collections;
using AwesomiumSharp;

namespace AwesomiumSharp
{
    public class WebViewControl : Border
    {
#region Fields

        const int WM_KEYDOWN = 0x0100;
        const int WM_KEYUP = 0x0101;
        const int WM_CHAR = 0x0102;

        private WebView _webview;
        private Image _image;
        private WriteableBitmap _bitmap;
        private DispatcherTimer _updateTimer;
        private Matrix _deviceTransform;

#endregion

#region Construction

        static WebViewControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WebViewControl), new FrameworkPropertyMetadata(typeof(WebViewControl)));
        }

        public WebViewControl()
        {
            if (!CoreInitialized)
                InitializeCore();

            Loaded += ControlLoaded;
            Unloaded += ControlUnloaded;
            LostFocus += ControlLostFocus;
            GotFocus += ControlGotFocus;
        }

        ~WebViewControl()
        {
            if (CoreInitialized && _webview != null)
                _webview.Dispose();
        }

#endregion 
        
#region WebCore Management

        private void InitializeCore()
        {
            WebCore.Config config = new WebCore.Config { enablePlugins = true, saveCacheAndCookies = true };
            WebCore.Initialize(config);

            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Closing += ShutdownCore;

            CoreInitialized = true;
        }

        private void ShutdownCore(Object sender, EventArgs e)
        {
            WebCore.Shutdown();

            CoreInitialized = false;
        }

#endregion

#region Events

        public class OpenLinkEventArgs : EventArgs
        {
            public OpenLinkEventArgs(string url) { this.url = url; }
            public string url;
        };

        public event OpenLinkHandler OnOpenExternalLink;
        public delegate void OpenLinkHandler(object sender, OpenLinkEventArgs e);

#endregion

#region Properties

        public static bool CoreInitialized { get; set; }

#endregion

#region Public Methods

        public void LoadURL(string URL)
        {
            _webview.LoadURL(URL);
        }

#endregion

#region Private Implementation

        private void LoadWebView(int width, int height)
        {
            _webview = WebCore.CreateWebview(width, height);
            _webview.LoadURL(Source);
            _webview.Focus();
            _webview.OnFinishLoading += OnFinishLoading;
            _webview.OnOpenExternalLink += OpenExternalLink;
            _webview.OnWebViewCrashed += OnWebviewCrashed;
            _webview.OnChangeCursor += OnChangeCursor;
        }

        private void Update(object sender, EventArgs e)
        {
            if (!CoreInitialized)
                return;

            WebCore.Update();
            Render();
        }

        private void Render()
        {
            if (_webview == null)
                return;

            if (_webview.IsDirty() && _bitmap != null)
            {
                RenderBuffer buffer = _webview.Render();
                if (buffer != null)
                {
                    buffer.CopyToBitmap(_bitmap);
                    _image.Source = _bitmap;
                }
            }
        }

#region Event Handlers / Delegates

        private void ControlLoaded(object sender, EventArgs e)
        {
            HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
            if (source != null)
            {
                source.AddHook(HandleMessages);
            }
            this.MouseWheel += HandleMouseWheel;
            this.MouseMove += HandleMouseMove;
            this.MouseDown += HandleMouseDown;
            this.MouseUp += HandleMouseUp;

            _updateTimer = new DispatcherTimer();
            _updateTimer.Tick += Update;
            _updateTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);
            _updateTimer.Start();

            Child = _image;
        }

        private void ControlUnloaded(object sender, EventArgs e)
        {
            _updateTimer.Stop();
        }

        private void ControlGotFocus(object sender, EventArgs e)
        {
            _webview.Focus();
        }

        private void ControlLostFocus(object sender, EventArgs e)
        {
            _webview.Unfocus();
        }

        private IntPtr HandleMessages(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            int message = (msg & 65535);

            if ((message == WM_KEYDOWN || message == WM_KEYUP || message == WM_CHAR) && IsFocused)
            {
                HandleKeyboardEvent(msg, (int)wParam, (int)lParam);
                handled = true;
            }
            else
            {
                handled = false;
            }

            return IntPtr.Zero;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            var size = base.ArrangeOverride(arrangeBounds);

            _deviceTransform = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;

            var width = (int)(size.Width * _deviceTransform.M11);
            var height = (int)(size.Height * _deviceTransform.M22);

            width -= (int)(base.BorderThickness.Left + base.BorderThickness.Right);
            height -= (int)(base.BorderThickness.Top + base.BorderThickness.Bottom);

            if (width != Width || height != Height || _bitmap == null)
            {
                if (_webview == null)
                    LoadWebView(width, height);
                else
                    _webview.Resize(width, height);

                if (_image == null)
                    _image = new Image();

                _bitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.WebPaletteTransparent);
                _image.Source = _bitmap;
                Child = _image;
            }

            return size;
        }

        private void HandleMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (!CoreInitialized && _webview != null)
                return;

            _webview.InjectMouseWheel(e.Delta);
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (!CoreInitialized && _webview != null)
                return;

            var pos = e.GetPosition(this);
            var x = (pos.X - base.BorderThickness.Left) * _deviceTransform.M11;
            var y = (pos.Y - base.BorderThickness.Top) * _deviceTransform.M22;

            _webview.InjectMouseMove((int)x, (int)y);
        }

        private void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!CoreInitialized && _webview != null)
                return;

            Focus();
            _webview.Focus();
            _webview.InjectMouseDown(AwesomiumSharp.MouseButton.Left);
        }

        private void HandleMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!CoreInitialized && _webview != null)
                return;

            _webview.InjectMouseUp(AwesomiumSharp.MouseButton.Left);
        }

        private void HandleKeyboardEvent(int msg, int wParam, int lParam)
        {
            if (!CoreInitialized && _webview != null)
                return;

            _webview.InjectKeyboardEventWin(msg, wParam, lParam);
        }

        private void OnChangeCursor(object sender, WebView.ChangeCursorEventArgs e)
        {
            if (e.cursorType == AwesomiumSharp.CursorType.Hand)
                Cursor = Cursors.Hand;
            else if (e.cursorType == AwesomiumSharp.CursorType.Ibeam)
                Cursor = Cursors.IBeam;
            else
                Cursor = Cursors.Arrow;
        }

        private void OnFinishLoading(object sender, WebView.FinishLoadingEventArgs e)
        {
            IsLoading = false;
        }

        private void OpenExternalLink(object sender, WebView.OpenExternalLinkEventArgs e)
        {
            if (OnOpenExternalLink != null && e.url.Length > 0)
            {
                OnOpenExternalLink(this, new OpenLinkEventArgs(e.url));
            }
        }

        private void OnWebviewCrashed(object sender, WebView.WebViewCrashedEventArgs e)
        {
            IsLoading = false;
            Child = new TextBlock { Text = "Error: This WebView has crashed." };
            _webview = null;
        }

#endregion

#endregion

#region Dependency Properties

#region Source (Dependency Property)

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(string), typeof(WebViewControl),
                    new UIPropertyMetadata(String.Empty, OnSourceChanged));

        public string Source
        {
            [DebuggerStepThrough]
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((WebViewControl)d).OnSourceChanged(e);
        }

        private void OnSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (_webview != null && e.NewValue != null)
            {
                _webview.LoadURL(e.NewValue.ToString());
                IsLoading = true;
            }
        }

#endregion

#region IsLoading (Dependency Property)

        public static readonly DependencyProperty IsLoadingProperty =
        DependencyProperty.Register("IsLoading", typeof(bool), typeof(WebViewControl), new UIPropertyMetadata(false));

        public bool IsLoading
        {
            [DebuggerStepThrough]
            get { return (bool)GetValue(IsLoadingProperty); }
            set { SetValue(IsLoadingProperty, value); }
        }

#endregion

#endregion
    }
}