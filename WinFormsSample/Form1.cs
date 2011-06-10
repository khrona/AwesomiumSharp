using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using AwesomiumSharp;

namespace WinFormsSample
{
    public partial class WebForm : Form
    {
        WebView webView;
        Timer timer;
        Bitmap frameBuffer = null;
        bool needsResize = false;

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

            WebCore.Config config = new WebCore.Config();
            config.enablePlugins = true;
            WebCore.Initialize(config);

            webView = WebCore.CreateWebview(webViewBitmap.Width, webViewBitmap.Height);
            webView.LoadURL("http://www.google.com");
            webView.Focus();

            timer = new Timer();
            timer.Interval = 20;
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        void WebForm_Activated(object sender, EventArgs e)
        {
            webView.Focus();
        }

        void WebForm_Deactivate(object sender, EventArgs e)
        {
            webView.Unfocus();
        }

        void WebForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            timer.Dispose();
            webView.Dispose();
            WebCore.Shutdown();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            if (needsResize)
            {
                if (!webView.IsResizing())
                {
                    webView.Resize(webViewBitmap.Width, webViewBitmap.Height);
                    needsResize = false;
                }
            }

            WebCore.Update();
            if (webView.IsDirty())
                Render();
        }

        void Render()
        {
            RenderBuffer rBuffer = webView.Render();

            if (frameBuffer == null)
            {
                frameBuffer = new Bitmap(rBuffer.GetWidth(), rBuffer.GetHeight(), PixelFormat.Format32bppArgb);
            }
            else if (frameBuffer.Width != rBuffer.GetWidth() || frameBuffer.Height != rBuffer.GetHeight())
            {
                frameBuffer.Dispose();
                frameBuffer = new Bitmap(rBuffer.GetWidth(), rBuffer.GetHeight(), PixelFormat.Format32bppArgb);
            }

            BitmapData bits = frameBuffer.LockBits(new Rectangle(0, 0, rBuffer.GetWidth(), rBuffer.GetHeight()),
                                ImageLockMode.ReadWrite, frameBuffer.PixelFormat);


            unsafe
            {
                UInt64* ptrBase = (UInt64*)((byte*)bits.Scan0);
                UInt64* datBase = (UInt64*)rBuffer.GetBuffer();
                UInt32 lOffset = 0;
                UInt32 lEnd = (UInt32)webViewBitmap.Height * (UInt32)(webViewBitmap.Width / 8);

                // copy 64 bits at a time, 4 times (since we divided by 8)
                for (lOffset = 0; lOffset < lEnd; lOffset++)
                {
                    *ptrBase++ = *datBase++;
                    *ptrBase++ = *datBase++;
                    *ptrBase++ = *datBase++;
                    *ptrBase++ = *datBase++;
                }
            }

            frameBuffer.UnlockBits(bits);

            webViewBitmap.Image = frameBuffer;
        }

        void WebForm_Resize(object sender, EventArgs e)
        {
            if (webViewBitmap.Width != 0 && webViewBitmap.Height != 0)
                needsResize = true;
        }

        void WebForm_KeyPress(object sender, KeyPressEventArgs e)
        {
            WebKeyboardEvent keyEvent = new WebKeyboardEvent();
            keyEvent.type = WebKeyType.Char;
            keyEvent.text = new ushort[] { e.KeyChar, 0, 0, 0 };
            webView.InjectKeyboardEvent(keyEvent);
        }

        void WebForm_KeyDown(object sender, KeyEventArgs e)
        {
            WebKeyboardEvent keyEvent = new WebKeyboardEvent();
            keyEvent.type = WebKeyType.KeyDown;
            keyEvent.virtualKeyCode = (int)e.KeyCode;
            webView.InjectKeyboardEvent(keyEvent);
        }

        void WebForm_KeyUp(object sender, KeyEventArgs e)
        {
            WebKeyboardEvent keyEvent = new WebKeyboardEvent();
            keyEvent.type = WebKeyType.KeyUp;
            keyEvent.virtualKeyCode = (int)e.KeyCode;
            webView.InjectKeyboardEvent(keyEvent);
        }

        void WebForm_MouseUp(object sender, MouseEventArgs e)
        {
            webView.InjectMouseUp(MouseButton.Left);
        }

        void WebForm_MouseDown(object sender, MouseEventArgs e)
        {
            webView.InjectMouseDown(MouseButton.Left);
        }

        void WebForm_MouseMove(object sender, MouseEventArgs e)
        {
            webView.InjectMouseMove(e.X, e.Y);
        }

        void WebForm_MouseWheel(object sender, MouseEventArgs e)
        {
            webView.InjectMouseWheel(e.Delta);
        }
    }
}