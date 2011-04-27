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
            timer.Interval = 30;
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
            WebCore.Update();
            if (webView.IsDirty())
                Render();
        }

        void Render()
        {
            RenderBuffer rBuffer = webView.Render();

            int[] data = new int[webViewBitmap.Width * webViewBitmap.Height];
            Marshal.Copy(rBuffer.GetBuffer(), data, 0, webViewBitmap.Width * webViewBitmap.Height);

            Bitmap bmp = new Bitmap(webViewBitmap.Width, webViewBitmap.Height, PixelFormat.Format32bppArgb);
            BitmapData bits = bmp.LockBits(new Rectangle(0, 0, webViewBitmap.Width, webViewBitmap.Height),
                              ImageLockMode.ReadWrite, bmp.PixelFormat);
            unsafe
            {
                for (int y = 0; y < webViewBitmap.Height; y++)
                {
                    int* row = (int*)((byte*)bits.Scan0 + (y * bits.Stride));
                    for (int x = 0; x < webViewBitmap.Width; x++)
                    {
                        row[x] = data[y * webViewBitmap.Width + x];
                    }
                }
            }
            bmp.UnlockBits(bits);

            webViewBitmap.Image = bmp;
        }

        void WebForm_Resize(object sender, EventArgs e)
        {
            if (webViewBitmap.Width != 0 && webViewBitmap.Height != 0)
            {
                webView.Resize(webViewBitmap.Width, webViewBitmap.Height);
                WebCore.Update();
            }
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