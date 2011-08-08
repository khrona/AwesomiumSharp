using System;
using AwesomiumSharp;
using System.Drawing;
using System.Windows.Forms;
using AwesomiumSharp.Windows.Forms;
using System.Runtime.InteropServices;

namespace WinFormsSample
{
    public partial class WebForm : Form
    {
        #region Fields
        WebView webView;
        RenderBuffer rBuffer;
        Bitmap frameBuffer;
        bool needsResize;
        #endregion


        #region Ctors
        public WebForm()
        {
            // Notice that Control.DoubleBuffered has been set to true
            // in the designer, to prevent flickering.

            InitializeComponent();

            WebCoreConfig config = new WebCoreConfig { EnablePlugins = true };
            WebCore.Initialize( config );

            webView = WebCore.CreateWebView( this.ClientSize.Width, this.ClientSize.Height );
            webView.IsDirtyChanged += OnIsDirtyChanged;
            webView.SelectLocalFiles += OnSelectLocalFiles;
            webView.CursorChanged += OnCursorChanged;
            webView.LoadURL( "http://www.google.com" );
            webView.Focus();
        }
        #endregion


        #region Methods
        protected override void OnActivated( EventArgs e )
        {
            base.OnActivated( e );

            if ( !webView.IsEnabled )
                return;

            webView.Focus();
        }

        protected override void OnDeactivate( EventArgs e )
        {
            base.OnDeactivate( e );

            if ( !webView.IsEnabled )
                return;

            webView.Unfocus();
        }

        protected override void OnFormClosed( FormClosedEventArgs e )
        {
            if ( webView != null )
            {
                webView.IsDirtyChanged -= OnIsDirtyChanged;
                webView.SelectLocalFiles -= OnSelectLocalFiles;
                webView.Close();
                WebCore.Shutdown();
            }

            base.OnFormClosed( e );
        }

        protected override void OnPaint( PaintEventArgs e )
        {
            if ( ( webView != null ) && webView.IsEnabled && webView.IsDirty )
                rBuffer = webView.Render();

            if ( rBuffer != null )
                Utilities.DrawBuffer( rBuffer, e.Graphics, this.BackColor, ref frameBuffer );
            else
                base.OnPaint( e );
        }

        protected override void OnResize( EventArgs e )
        {
            base.OnResize( e );

            if ( ( webView == null ) || !webView.IsEnabled )
                return;

            if ( this.ClientSize.Width != 0 && this.ClientSize.Height != 0 )
                needsResize = true;
        }

        protected override void OnKeyPress( KeyPressEventArgs e )
        {
            base.OnKeyPress( e );

            if ( !webView.IsEnabled )
                return;

            webView.InjectKeyboardEvent( Utilities.GetKeyboardEvent( e ) );
        }

        protected override void OnKeyDown( KeyEventArgs e )
        {
            base.OnKeyDown( e );

            if ( !webView.IsEnabled )
                return;

            webView.InjectKeyboardEvent( Utilities.GetKeyboardEvent( WebKeyType.KeyDown, e ) );
        }

        protected override void OnKeyUp( KeyEventArgs e )
        {
            base.OnKeyUp( e );

            if ( !webView.IsEnabled )
                return;

            webView.InjectKeyboardEvent( Utilities.GetKeyboardEvent( WebKeyType.KeyUp, e ) );
        }

        protected override void OnMouseDown( MouseEventArgs e )
        {
            base.OnMouseDown( e );

            if ( !webView.IsEnabled )
                return;

            webView.InjectMouseDown( MouseButton.Left );
        }

        protected override void OnMouseUp( MouseEventArgs e )
        {
            base.OnMouseUp( e );

            if ( !webView.IsEnabled )
                return;

            webView.InjectMouseUp( MouseButton.Left );
        }

        protected override void OnMouseMove( MouseEventArgs e )
        {
            base.OnMouseMove( e );

            if ( !webView.IsEnabled )
                return;

            webView.InjectMouseMove( e.X, e.Y );
        }

        protected override void OnMouseWheel( MouseEventArgs e )
        {
            base.OnMouseWheel( e );

            if ( !webView.IsEnabled )
                return;

            webView.InjectMouseWheel( e.Delta );
        }
        #endregion

        #region Event Handlers
        private void OnIsDirtyChanged( object sender, EventArgs e )
        {
            if ( needsResize && !webView.IsDisposed )
            {
                if ( !webView.IsResizing )
                {
                    webView.Resize( this.ClientSize.Width, this.ClientSize.Height );
                    needsResize = false;
                }
            }

            if ( webView.IsDirty )
                this.Invalidate();
        }

        private void OnCursorChanged( object sender, ChangeCursorEventArgs e )
        {
            Cursor = Utilities.GetCursor( e.CursorType );
        }

        private void OnSelectLocalFiles( object sender, SelectLocalFilesEventArgs e )
        {
            using ( OpenFileDialog dialog = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ),
                CheckFileExists = true,
                Multiselect = e.SelectMultipleFiles
            } )
            {
                if ( dialog.ShowDialog( this ) == DialogResult.OK )
                {
                    if ( dialog.FileNames.Length > 0 )
                    {
                        e.SelectedFiles = dialog.FileNames;
                    }
                }
            }
        }
        #endregion
    }

    #region NativeMethods
    internal static class NativeMethods
    {

        [DllImport( "user32" )]
        internal static extern IntPtr GetDC( IntPtr hwnd );

        [DllImport( "user32.dll" )]
        internal static extern bool ReleaseDC( IntPtr hWnd, IntPtr hDC );

    }
    #endregion
}