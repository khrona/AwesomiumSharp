using System;
using AwesomiumSharp;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace WinFormsSample
{
    public partial class WebForm : Form
    {
        WebView webView;
        Bitmap frameBuffer;
        bool needsResize;

        public WebForm()
        {
            InitializeComponent();

            Resize += WebForm_Resize;
            webViewBitmap.MouseMove += WebForm_MouseMove;
            webViewBitmap.MouseDown += WebForm_MouseDown;
            webViewBitmap.MouseUp += WebForm_MouseUp;
            MouseWheel += WebForm_MouseWheel;
            KeyDown += WebForm_KeyDown;
            KeyUp += WebForm_KeyUp;
            KeyPress += WebForm_KeyPress;
            FormClosed += WebForm_FormClosed;
            Activated += WebForm_Activated;
            Deactivate += WebForm_Deactivate;

            WebCoreConfig config = new WebCoreConfig { EnablePlugins = true };
            WebCore.Initialize( config );

            webView = WebCore.CreateWebview( webViewBitmap.Width, webViewBitmap.Height );
            webView.IsDirtyChanged += OnIsDirtyChanged;
            webView.LoadURL( "http://www.google.com" );
            webView.Focus();
        }

        void WebForm_Activated( object sender, EventArgs e )
        {
            if ( !webView.IsDisposed )
                webView.Focus();
        }

        void WebForm_Deactivate( object sender, EventArgs e )
        {
            if ( !webView.IsDisposed )
                webView.Unfocus();
        }

        void WebForm_FormClosed( object sender, FormClosedEventArgs e )
        {
            webView.IsDirtyChanged -= OnIsDirtyChanged;
            webView.Close();
            WebCore.Shutdown();
        }

        private void OnIsDirtyChanged( object sender, EventArgs e )
        {
            if ( needsResize && !webView.IsDisposed )
            {
                if ( !webView.IsResizing )
                {
                    webView.Resize( webViewBitmap.Width, webViewBitmap.Height );
                    needsResize = false;
                }
            }

            if ( webView.IsDirty )
                Render();
        }

        void Render()
        {
            if ( webView.IsDisposed )
                return;

            RenderBuffer rBuffer = webView.Render();

            if ( frameBuffer == null )
            {
                frameBuffer = new Bitmap( rBuffer.Width, rBuffer.Height, PixelFormat.Format32bppArgb );
            }
            else if ( frameBuffer.Width != rBuffer.Width || frameBuffer.Height != rBuffer.Height )
            {
                frameBuffer.Dispose();
                frameBuffer = new Bitmap( rBuffer.Width, rBuffer.Height, PixelFormat.Format32bppArgb );
            }

            BitmapData bits = frameBuffer.LockBits( new Rectangle( 0, 0, rBuffer.Width, rBuffer.Height ),
                                ImageLockMode.ReadWrite, frameBuffer.PixelFormat );


            unsafe
            {
                UInt64* ptrBase = (UInt64*)( (byte*)bits.Scan0 );
                UInt64* datBase = (UInt64*)rBuffer.Buffer;
                UInt32 lOffset = 0;
                UInt32 lEnd = (UInt32)webViewBitmap.Height * (UInt32)( webViewBitmap.Width / 8 );

                // copy 64 bits at a time, 4 times (since we divided by 8)
                for ( lOffset = 0; lOffset < lEnd; lOffset++ )
                {
                    *ptrBase++ = *datBase++;
                    *ptrBase++ = *datBase++;
                    *ptrBase++ = *datBase++;
                    *ptrBase++ = *datBase++;
                }
            }

            frameBuffer.UnlockBits( bits );

            webViewBitmap.Image = frameBuffer;
        }

        void WebForm_Resize( object sender, EventArgs e )
        {
            if ( webViewBitmap.Width != 0 && webViewBitmap.Height != 0 )
                needsResize = true;
        }

        void WebForm_KeyPress( object sender, KeyPressEventArgs e )
        {
            WebKeyboardEvent keyEvent = new WebKeyboardEvent { Type = WebKeyType.Char, Text = new ushort[] { e.KeyChar, 0, 0, 0 } };

            if ( !webView.IsDisposed )
                webView.InjectKeyboardEvent( keyEvent );
        }

        void WebForm_KeyDown( object sender, KeyEventArgs e )
        {
            WebKeyboardEvent keyEvent = new WebKeyboardEvent { Type = WebKeyType.KeyDown, VirtualKeyCode = (VirtualKey)e.KeyCode };

            if ( !webView.IsDisposed )
                webView.InjectKeyboardEvent( keyEvent );
        }

        void WebForm_KeyUp( object sender, KeyEventArgs e )
        {
            WebKeyboardEvent keyEvent = new WebKeyboardEvent { Type = WebKeyType.KeyUp, VirtualKeyCode = (VirtualKey)e.KeyCode };

            if ( !webView.IsDisposed )
                webView.InjectKeyboardEvent( keyEvent );
        }

        void WebForm_MouseUp( object sender, MouseEventArgs e )
        {
            if ( !webView.IsDisposed )
                webView.InjectMouseUp( MouseButton.Left );
        }

        void WebForm_MouseDown( object sender, MouseEventArgs e )
        {
            if ( !webView.IsDisposed )
                webView.InjectMouseDown( MouseButton.Left );
        }

        void WebForm_MouseMove( object sender, MouseEventArgs e )
        {
            if ( !webView.IsDisposed )
                webView.InjectMouseMove( e.X, e.Y );
        }

        void WebForm_MouseWheel( object sender, MouseEventArgs e )
        {
            if ( !webView.IsDisposed )
                webView.InjectMouseWheel( e.Delta );
        }
    }
}