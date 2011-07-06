/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : WebControl.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/03/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    This is a WPF control wrapping Awesomium. The major differences 
 *    and changes to the old WebViewControl are:
 *    
 *    - Instead of wrapping the wrapper (meaning, wrapping WebView), we
 *      become the initial wrapper ourselves and directly implement all
 *      C API just as WebView does. This improves performance, allows more
 *      control and allows the developer to access the full API directly 
 *      from this control.
 *    
 *    - Just like in the other files edited, here too, many changes 
 *      with respect to standard .NET guidelines and naming convension 
 *      were made.
 *    
 *    - Properties are available as dependency properties. This allows
 *      the developer to easily bind to properties and monitor their 
 *      status in triggers etc.
 *    
 *    - WebControl is a now a FrameworkElement as it should be. This means
 *      styling is possible but templating is not. No useless elements in 
 *      the visual tree and no children such as images. We render directly 
 *      on the element's surface and override our own events' triggers.
 *    
 *    - WebControl includes an internal auto updater just as the one added 
 *      in WebCore. It monitors WebCore's auto updater and starts or stops
 *      as needed.
 *    
 *    - Part of the Find logic is now handled by this class making it
 *      more straightforward. FindNext is added.
 *    
 *    - Extensive pro-exception verification of validity before every API
 *      call was added here too.
 *    
 *    - Most of the control's methods can be called using routed commands.
 *      We bind to many of the already available Application and Navigation
 *      commands, and we cover more functionality by providing our own 
 *      commands through WebControlCommands. This makes it easy for the UI
 *      developer to manipulate the control directly from XAML. See 
 *      InitializeCommandBindings() to get an idea of what we support.
 *    
 * 
 *    07/06/2011:
 *    
 *    - Changes in respect to the changes of the WebCore's auto-update
 *      logic.
 *      
 *    - Minor fixes and improvements.
 * 
 ********************************************************************************/

#region Using
using System;
using System.Text;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
#endregion

namespace AwesomiumSharp.Windows.Controls
{
    public class WebControl : FrameworkElement, IWebView
    {
        #region Fields
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_CHAR = 0x0102;

        private static KeyGesture browseBackGesture;

        private Matrix deviceTransform;
        private WriteableBitmap bitmap;
        private ToolTip toolTip;
        private Random findRequestRandomizer;

        internal Dictionary<string, JSCallback> jsObjectCallbackMap;

        private CallbackBeginLoadingCallback beginLoadingCallback;
        private CallbackBeginNavigationCallback beginNavigationCallback;
        private CallbackChangeCursorCallback changeCursorCallback;
        private CallbackChangeKeyboardFocusCallback changeKeyboardFocusCallback;
        private CallbackChangeTargetURLCallback changeTargetURLCallback;
        private CallbackChangeTooltipCallback changeTooltipCallback;
        private CallbackDomReadyCallback domReadyCallback;
        private CallbackFinishLoadingCallback finishLoadingCallback;
        private CallbackGetPageContentsCallback getPageContentsCallback;
        private CallbackJsCallback jsCallback;
        private CallbackOpenExternalLinkCallback openExternalLinkCallback;
        private CallbackPluginCrashedCallback pluginCrashedCallback;
        private CallbackReceiveTitleCallback receiveTitleCallback;
        private CallbackRequestDownloadCallback requestDownloadCallback;
        private CallbackRequestFileChooserCallback requestFileChooserCallback;
        private CallbackRequestMoveCallback requestMoveCallback;
        private CallbackWebviewCrashedCallback webviewCrashedCallback;
        private CallbackGetScrollDataCallback getScrollDataCallback;
        private CallbackJSConsoleMessageCallback jsConsoleMessageCallback;
        private CallbackGetFindResultsCallback getFindResultsCallback;
        private CallbackUpdateIMECallback updateIMECallback;

        private CallbackResourceRequestCallback resourceRequestCallback;
        private CallbackResourceResponseCallback resourceResponseCallback;
        #endregion

        #region Events
        internal event JSCallbackCalledEventHandler JSCallbackCalled;

        /// <summary>
        /// Occurs when this WebView needs to be rendered again.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is fired continuously while <see cref="IsDirty"/> is true and until a call 
        /// to <see cref="Render"/> is made that will render the updated view into an offscreen
        /// pixel buffer and clear the dirty state.
        /// </para>
        /// <para>
        /// This event is not automatically fired if you are running Awesomium from a non-UI
        /// thread. Please read the Remarks section of <see cref="WebCore.Update"/> for
        /// details.
        /// </para>
        /// </remarks>
        public event EventHandler IsDirtyChanged;

        /// <summary>
        /// Raises the <see cref="IsDirtyChanged"/> event.
        /// </summary>
        protected virtual void OnIsDirtyChanged( object sender, EventArgs e )
        {
            if ( IsDirtyChanged != null )
                IsDirtyChanged( sender, e );
        }

        /// <summary>
        /// This event occurs when a WebView begins loading a new page (first bits of data received from server).
        /// </summary>
        public event BeginLoadingEventHandler BeginLoading;

        /// <summary>
        /// Raises the <see cref="BeginLoading"/> event.
        /// </summary>
        protected virtual void OnBeginLoading( object sender, BeginLoadingEventArgs e )
        {
            if ( BeginLoading != null )
                BeginLoading( sender, e );
        }

        /// <summary>
        /// This event occurs when a WebView begins navigating to a new URL.
        /// </summary>
        public event BeginNavigationEventHandler BeginNavigation;

        /// <summary>
        /// Raises the <see cref="BeginNavigation"/> event.
        /// </summary>
        protected virtual void OnBeginNavigation( object sender, BeginNavigationEventArgs e )
        {
            if ( BeginNavigation != null )
                BeginNavigation( sender, e );
        }

        /// <summary>
        /// This event occurs when the mouse cursor type changes.
        /// </summary>
        public event CursorChangedEventHandler CursorChanged;

        /// <summary>
        /// Raises the <see cref="CursorChanged"/> event.
        /// </summary>
        protected virtual void OnCursorChanged( object sender, ChangeCursorEventArgs e )
        {
            if ( CursorChanged != null )
                CursorChanged( sender, e );
        }

        /// <summary>
        /// This event occurs when keyboard focus changes (usually as a result of a textbox being focused).
        /// </summary>
        public event KeyboardFocusChangedEventHandler KeyboardFocusChanged;

        /// <summary>
        /// Raises the <see cref="KeyboardFocusChanged"/> event.
        /// </summary>
        protected virtual void OnKeyboardFocusChanged( object sender, ChangeKeyboardFocusEventArgs e )
        {
            if ( KeyboardFocusChanged != null )
                KeyboardFocusChanged( sender, e );
        }

        /// <summary>
        /// This event occurs when the target URL changes (usually the result of hovering over a link).
        /// </summary>
        public event UrlEventHandler TargetUrlChanged;

        /// <summary>
        /// Raises the <see cref="TargetUrlChanged"/> event.
        /// </summary>
        protected virtual void OnTargetUrlChanged( object sender, UrlEventArgs e )
        {
            if ( TargetUrlChanged != null )
                TargetUrlChanged( sender, e );
        }

        /// <summary>
        /// This event occurs when the tooltip text changes.
        /// </summary>
        public event TooltipChangedEventHandler TooltipChanged;

        /// <summary>
        /// Raises the <see cref="TooltipChanged"/> event.
        /// </summary>
        protected virtual void OnTooltipChanged( object sender, ChangeTooltipEventArgs e )
        {
            if ( TooltipChanged != null )
                TooltipChanged( sender, e );
        }

        /// <summary>
        /// This event occurs once the document has been parsed for a page but before all resources (images, etc.)
        /// have been loaded. This is your first chance to execute Javascript on a page (useful for initialization purposes).
        /// </summary>
        public event EventHandler DomReady;

        /// <summary>
        /// Raises the <see cref="DomReady"/> event.
        /// </summary>
        protected virtual void OnDomReady( object sender, EventArgs e )
        {
            if ( DomReady != null )
                DomReady( sender, e );
        }

        /// <summary>
        /// This event occurs once a page (and all of its sub-frames) has completely finished loading.
        /// </summary>
        public event EventHandler LoadCompleted;

        /// <summary>
        /// Raises the <see cref="LoadCompleted"/> event.
        /// </summary>
        protected virtual void OnLoadCompleted( object sender, EventArgs e )
        {
            if ( LoadCompleted != null )
                LoadCompleted( sender, e );
        }

        /// <summary>
        /// This event occurs once the page contents (as text) have been retrieved (usually after the end
        /// of each page load). This plain text is useful for indexing/search purposes.
        /// </summary>
        public event PageContentsReceivedEventHandler PageContentsReceived;

        /// <summary>
        /// Raises the <see cref="PageContentsReceived"/> event.
        /// </summary>
        protected virtual void OnPageContentsReceived( object sender, GetPageContentsEventArgs e )
        {
            if ( PageContentsReceived != null )
                PageContentsReceived( sender, e );
        }

        /// <summary>
        /// This event occurs when an external link is attempted to be opened. An external link
        /// is any link that normally opens in a new window (for example, links with target="_blank", calls
        /// to window.open(), and URL open events from Flash plugins). You are responsible for
        /// creating a new WebView to handle these URLs yourself.
        /// </summary>
        public event OpenExternalLinkEventHandler OpenExternalLink;

        /// <summary>
        /// Raises the <see cref="OpenExternalLink"/> event.
        /// </summary>
        protected virtual void OnOpenExternalLink( object sender, OpenExternalLinkEventArgs e )
        {
            if ( OpenExternalLink != null )
                OpenExternalLink( sender, e );
        }

        /// <summary>
        /// This event occurs whenever a plugin crashes on a page (usually Flash).
        /// </summary>
        public event PluginCrashedEventHandler PluginCrashed;

        /// <summary>
        /// Raises the <see cref="PluginCrashed"/> event.
        /// </summary>
        protected virtual void OnPluginCrashed( object sender, PluginCrashedEventArgs e )
        {
            if ( PluginCrashed != null )
                PluginCrashed( sender, e );
        }

        /// <summary>
        /// This event occurs once we receive the page title.
        /// </summary>
        public event TitleReceivedEventHandler TitleReceived;

        /// <summary>
        /// Raises the <see cref="TitleReceived"/> event.
        /// </summary>
        protected virtual void OnTitleReceived( object sender, ReceiveTitleEventArgs e )
        {
            if ( TitleReceived != null )
                TitleReceived( sender, e );
        }

        /// <summary>
        /// This event occurs whenever the window is requested to be moved (via Javascript).
        /// </summary>
        public event MoveEventHandler Move;

        /// <summary>
        /// Raises the <see cref="Move"/> event.
        /// </summary>
        protected virtual void OnMove( object sender, MoveEventArgs e )
        {
            if ( Move != null )
                Move( sender, e );
        }

        /// <summary>
        /// This event occurs whenever a URL is requested to be downloaded (you must handle this yourself).
        /// </summary>
        public event UrlEventHandler Download;

        /// <summary>
        /// Raises the <see cref="Download"/> event.
        /// </summary>
        protected virtual void OnDownload( object sender, UrlEventArgs e )
        {
            if ( Download != null )
                Download( sender, e );
        }

        /// <summary>
        /// This event occurs when the renderer (which is isolated in a separate process) crashes unexpectedly.
        /// </summary>
        public event EventHandler Crashed;

        /// <summary>
        /// Raises the <see cref="Crashed"/> event.
        /// </summary>
        protected virtual void OnCrashed( object sender, EventArgs e )
        {
            if ( Crashed != null )
                Crashed( sender, e );
        }

        /// <summary>
        /// This event occurs whenever a page requests a file chooser dialog to be displayed (usually due
        /// to an upload form being clicked by a user). You will need to display your own dialog.
        /// Assign the selected local file(s) to <see cref="SelectLocalFilesEventArgs.SelectedFiles"/>
        /// </summary>
        /// <remarks>
        /// The dialog does not have to be modal, this request is non-blocking. Once a file has been chosen by the user,
        /// you can manually report this back to the view by calling <see cref="WebView.ChooseFile"/>.
        /// </remarks>
        public event SelectLocalFilesEventHandler SelectLocalFiles;

        /// <summary>
        /// Raises the <see cref="FileChooserRequest"/> event.
        /// </summary>
        protected virtual void OnSelectLocalFiles( object sender, SelectLocalFilesEventArgs e )
        {
            if ( SelectLocalFiles != null )
                SelectLocalFiles( sender, e );
        }

        /// <summary>
        /// This event occurs as a response to WebView.RequestScrollData
        /// </summary>
        public event ScrollDataReceivedEventHandler ScrollDataReceived;

        /// <summary>
        /// Raises the <see cref="ScrollDataReceived"/> event.
        /// </summary>
        protected virtual void OnScrollDataReceived( object sender, ScrollDataEventArgs e )
        {
            if ( ScrollDataReceived != null )
                ScrollDataReceived( sender, e );
        }

        /// <summary>
        /// This event occurs whenever a new message is added to the Javascript Console (usually
        /// the result of a Javascript error).
        /// </summary>
        public event JSConsoleMessageAddedEventHandler JSConsoleMessageAdded;

        /// <summary>
        /// Raises the <see cref="JSConsoleMessageAdded"/> event.
        /// </summary>
        protected virtual void OnJSConsoleMessageAdded( object sender, JSConsoleMessageEventArgs e )
        {
            if ( JSConsoleMessageAdded != null )
                JSConsoleMessageAdded( sender, e );
        }

        /// <summary>
        /// This event occurs whenever we receive results back from an in-page find operation (WebView.Find).
        /// </summary>
        public event FindResultsReceivedEventHandler FindResultsReceived;

        /// <summary>
        /// Raises the <see cref="FindResultsReceived"/> event.
        /// </summary>
        protected virtual void OnFindResultsReceived( object sender, GetFindResultsEventArgs e )
        {
            if ( FindResultsReceived != null )
                FindResultsReceived( sender, e );
        }

        /// <summary>
        /// This event occurs whenever the user does something that changes the 
        /// position or visiblity of the IME Widget. This event is only active when 
        /// IME is activated (please see WebView.ActivateIME).
        /// </summary>
        public event ImeUpdatedEventHandler ImeUpdated;

        /// <summary>
        /// Raises the <see cref="ImeUpdated"/> event.
        /// </summary>
        protected virtual void OnImeUpdated( object sender, UpdateImeEventArgs e )
        {
            if ( ImeUpdated != null )
                ImeUpdated( sender, e );
        }

        /// <summary>
        /// This event occurs whenever there is a request for a certain resource (URL). You can either modify the request
        /// before it is sent or immediately return your own custom response. This is useful for implementing your own
        /// custom resource-loading back-end or for tracking of resource loads.
        /// </summary>
        public event ResourceRequestEventHandler ResourceRequest;

        /// <summary>
        /// Raises the <see cref="ResourceRequest"/> event.
        /// </summary>
        protected virtual ResourceResponse OnResourceRequest( object sender, ResourceRequestEventArgs e )
        {
            if ( ResourceRequest != null )
                return ResourceRequest( sender, e );

            return null;
        }

        /// <summary>
        /// This event occurs whenever a response has been received from a server. This is useful for statistics
        /// and resource tracking purposes.
        /// </summary>
        public event ResourceResponseEventHandler ResourceResponse;

        /// <summary>
        /// Raises the <see cref="ResourceResponse"/> event.
        /// </summary>
        protected virtual void OnResourceResponse( object sender, ResourceResponseEventArgs e )
        {
            if ( ResourceResponse != null )
                ResourceResponse( sender, e );
        }
        #endregion


        #region Ctors
        static WebControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( WebControl ), new FrameworkPropertyMetadata( typeof( WebControl ) ) );
            FocusableProperty.OverrideMetadata( typeof( WebControl ), new FrameworkPropertyMetadata( true ) );

            // We need to remove and restore this when the view gets/loses keyboard focus.
            browseBackGesture = NavigationCommands.BrowseBack.InputGestures.OfType<KeyGesture>().SingleOrDefault( ( kg ) =>
                ( kg.Modifiers == ModifierKeys.None ) && ( kg.Key == Key.Back ) );
        }

        public WebControl()
        {
            if ( DesignerProperties.GetIsInDesignMode( this ) )
                return;

            toolTip = new ToolTip();

            InitializeCore();
            InitializeDelegates( this.Instance );
            InitializeCommandBindings();

            findRequestRandomizer = new Random();

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }
        #endregion

        #region Dtor
        /// <summary>
        /// Destroys and removes this web view control. Any call to members of this control
        /// after calling this method, will cause a <see cref="InvalidOperationException"/>.
        /// </summary>
        public void Close()
        {
            this.Destroy();
        }

        ~WebControl()
        {
            this.Destroy();
        }

        private void Destroy( bool shuttingDown = false )
        {
            if ( Instance != IntPtr.Zero )
            {
                // If there are other controls created, they will take care of this.
                // If this is the only control created or left, the WebCore will
                // automatically shutdown after removing it.
                if ( Application.Current.MainWindow != null )
                    Application.Current.MainWindow.Closing -= ShutdownCore;

                if ( hookAdded )
                {
                    HwndSource source = (HwndSource)PresentationSource.FromVisual( this );
                    if ( source != null )
                    {
                        source.RemoveHook( HandleMessages );
                        hookAdded = false;
                    }
                }

                WebCore.DestroyView( this );
                Instance = IntPtr.Zero;
            }
        }
        #endregion


        #region Overrides

        #region Mouse
        /// <inheritdoc />
        protected override void OnPreviewMouseMove( MouseEventArgs e )
        {
            if ( !IsLive )
                return;

            var pos = e.GetPosition( this );
            var x = (int)( pos.X * deviceTransform.M11 );
            var y = (int)( pos.Y * deviceTransform.M22 );

            awe_webview_inject_mouse_move( Instance, x, y );
            base.OnPreviewMouseMove( e );
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_down( Instance, MouseButton.Left );
            base.OnPreviewMouseLeftButtonDown( e );
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseLeftButtonUp( MouseButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_up( Instance, MouseButton.Left );
            base.OnPreviewMouseLeftButtonUp( e );
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseRightButtonDown( MouseButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_down( Instance, MouseButton.Right );
            base.OnPreviewMouseRightButtonDown( e );
        }

        /// <inheritdoc />
        protected override void OnMouseRightButtonUp( MouseButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_up( Instance, MouseButton.Right );
            base.OnMouseRightButtonUp( e );
        }

        /// <inheritdoc />
        protected override void OnPreviewMouseWheel( MouseWheelEventArgs e )
        {
            if ( !IsLive )
                return;

            awe_webview_inject_mouse_wheel( Instance, e.Delta, 0 );
            base.OnPreviewMouseWheel( e );
        }

        /// <inheritdoc />
        protected override void OnMouseLeave( MouseEventArgs e )
        {
            if ( !IsLive )
                return;

            awe_webview_inject_mouse_move( Instance, -1, -1 );
            base.OnMouseLeave( e );
        }
        #endregion

        #region Stylus
        /// <inheritdoc />
        protected override void OnPreviewStylusMove( StylusEventArgs e )
        {
            if ( !IsLive )
                return;

            var pos = e.GetPosition( this );
            var x = (int)( pos.X * deviceTransform.M11 );
            var y = (int)( pos.Y * deviceTransform.M22 );

            awe_webview_inject_mouse_move( Instance, x, y );
            base.OnPreviewStylusMove( e );
        }

        /// <inheritdoc />
        protected override void OnPreviewStylusButtonDown( StylusButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_down( Instance, MouseButton.Left );
            base.OnPreviewStylusButtonDown( e );
        }

        /// <inheritdoc />
        protected override void OnPreviewStylusButtonUp( StylusButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_up( Instance, MouseButton.Left );
            base.OnPreviewStylusButtonUp( e );
        }
        #endregion

        #region Touch
        /// <inheritdoc />
        protected override void OnPreviewTouchMove( TouchEventArgs e )
        {
            if ( !IsLive )
                return;

            var pos = e.GetTouchPoint( this );
            var x = (int)( pos.Position.X * deviceTransform.M11 );
            var y = (int)( pos.Position.Y * deviceTransform.M22 );

            awe_webview_inject_mouse_move( Instance, x, y );
            base.OnPreviewTouchMove( e );
        }

        /// <inheritdoc />
        protected override void OnPreviewTouchDown( TouchEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_down( Instance, MouseButton.Left );
            base.OnPreviewTouchDown( e );
        }

        /// <inheritdoc />
        protected override void OnPreviewTouchUp( TouchEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_up( Instance, MouseButton.Left );
            base.OnPreviewTouchUp( e );
        }
        #endregion

        #region Focus
        /// <inheritdoc />
        protected override void OnGotFocus( RoutedEventArgs e )
        {
            if ( !IsLive )
                return;

            FocusView();

            base.OnGotFocus( e );
        }

        /// <inheritdoc />
        protected override void OnLostFocus( RoutedEventArgs e )
        {
            if ( !IsLive )
                return;

            UnfocusView();
            toolTip.IsOpen = false;

            base.OnLostFocus( e );
        }
        #endregion

        #region HitTest
        /// <inheritdoc />
        protected override HitTestResult HitTestCore( PointHitTestParameters hitTestParameters )
        {
            // Eliminates the need to paint a background to capture input.
            return new PointHitTestResult( this, Mouse.PrimaryDevice.GetPosition( this ) );
        }
        #endregion

        #region Measure/Arrange
        /// <inheritdoc />
        protected override Size MeasureOverride( Size availableSize )
        {
            var size = base.MeasureOverride( availableSize );

            if ( IsLive )
            {
                try
                {
                    deviceTransform = PresentationSource.FromVisual( this ).CompositionTarget.TransformToDevice;

                    var width = (int)( availableSize.Width * deviceTransform.M11 );
                    var height = (int)( availableSize.Height * deviceTransform.M22 );

                    awe_webview_resize( Instance, width, height, true, 300 );
                    bitmap = new WriteableBitmap( width, height, 96, 96, PixelFormats.Bgra32, BitmapPalettes.WebPaletteTransparent );
                }
                catch { /* */ }
                finally
                {
                    GC.Collect();
                }

                Update();
            }

            return size;
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride( Size arrangeBounds )
        {
            if ( IsLive )
                Update();

            return base.ArrangeOverride( arrangeBounds );
        }
        #endregion

        #region Render
        /// <inheritdoc />
        protected override void OnRender( DrawingContext drawingContext )
        {
            if ( IsLive && ( bitmap != null ) && ( this.ActualWidth > 0 ) && ( this.ActualHeight > 0 ) )
                drawingContext.DrawImage( bitmap, new Rect( new Point(), base.RenderSize ) );
            else
                base.OnRender( drawingContext );
        }
        #endregion

        #endregion

        #region Methods

        #region Internal

        #region VerifyLive
        private void VerifyLive()
        {
            if ( !IsLive )
                throw new InvalidOperationException( "The WebControl is not initialized." );
        }
        #endregion

        #region InitializeCore
        private void InitializeCore()
        {
            this.Instance = WebCore.CreateWebviewInstance( (int)this.ActualWidth, (int)this.ActualHeight, this );
            this.Focus();

            if ( Application.Current.MainWindow != null )
                Application.Current.MainWindow.Closing += ShutdownCore;
        }

        private void ShutdownCore( object sender, CancelEventArgs e )
        {
            WebCore.Shutdown();
        }
        #endregion

        #region InitializeDelegates
        private void InitializeDelegates( IntPtr webview )
        {
            beginNavigationCallback = internalBeginNavigationCallback;
            awe_webview_set_callback_begin_navigation( webview, beginNavigationCallback );

            beginLoadingCallback = internalBeginLoadingCallback;
            awe_webview_set_callback_begin_loading( webview, beginLoadingCallback );

            finishLoadingCallback = internalFinishLoadingCallback;
            awe_webview_set_callback_finish_loading( webview, finishLoadingCallback );

            jsCallback = internalJsCallback;
            awe_webview_set_callback_js_callback( webview, jsCallback );

            receiveTitleCallback = internalReceiveTitleCallback;
            awe_webview_set_callback_receive_title( webview, receiveTitleCallback );

            changeTooltipCallback = internalChangeTooltipCallback;
            awe_webview_set_callback_change_tooltip( webview, changeTooltipCallback );

            changeCursorCallback = internalChangeCursorCallback;
            awe_webview_set_callback_change_cursor( webview, changeCursorCallback );

            changeKeyboardFocusCallback = internalChangeKeyboardFocusCallback;
            awe_webview_set_callback_change_keyboard_focus( webview, changeKeyboardFocusCallback );

            changeTargetURLCallback = internalChangeTargetURLCallback;
            awe_webview_set_callback_change_target_url( webview, changeTargetURLCallback );

            openExternalLinkCallback = internalOpenExternalLinkCallback;
            awe_webview_set_callback_open_external_link( webview, openExternalLinkCallback );

            requestDownloadCallback = internalRequestDownloadCallback;
            awe_webview_set_callback_request_download( webview, requestDownloadCallback );

            webviewCrashedCallback = internalWebviewCrashedCallback;
            awe_webview_set_callback_web_view_crashed( webview, webviewCrashedCallback );

            pluginCrashedCallback = internalPluginCrashedCallback;
            awe_webview_set_callback_plugin_crashed( webview, pluginCrashedCallback );

            requestMoveCallback = internalRequestMoveCallback;
            awe_webview_set_callback_request_move( webview, requestMoveCallback );

            getPageContentsCallback = internalGetPageContentsCallback;
            awe_webview_set_callback_get_page_contents( webview, getPageContentsCallback );

            domReadyCallback = internalDomReadyCallback;
            awe_webview_set_callback_dom_ready( webview, domReadyCallback );

            requestFileChooserCallback = internalRequestFileChooser;
            awe_webview_set_callback_request_file_chooser( webview, requestFileChooserCallback );

            getScrollDataCallback = internalGetScrollData;
            awe_webview_set_callback_get_scroll_data( webview, getScrollDataCallback );

            jsConsoleMessageCallback = internalJSConsoleMessage;
            awe_webview_set_callback_js_console_message( webview, jsConsoleMessageCallback );

            getFindResultsCallback = internalGetFindResults;
            awe_webview_set_callback_get_find_results( webview, getFindResultsCallback );

            updateIMECallback = internalUpdateIME;
            awe_webview_set_callback_update_ime( webview, updateIMECallback );

            resourceRequestCallback = internalResourceRequestCallback;
            awe_webview_set_callback_resource_request( webview, resourceRequestCallback );

            resourceResponseCallback = internalResourceResponseCallback;
            awe_webview_set_callback_resource_response( webview, resourceResponseCallback );

            jsObjectCallbackMap = new Dictionary<string, JSCallback>();
            this.JSCallbackCalled += handleJSCallback;
        }
        #endregion

        #region InitializeCommandBindings
        private void InitializeCommandBindings()
        {
            this.CommandBindings.Add( new CommandBinding( ApplicationCommands.Copy, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( ApplicationCommands.Cut, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( ApplicationCommands.Paste, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( ApplicationCommands.SelectAll, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( ApplicationCommands.Find, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( NavigationCommands.BrowseBack, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( NavigationCommands.BrowseForward, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( NavigationCommands.BrowseStop, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( NavigationCommands.Refresh, OnCommandExecuted, CanExecuteCommand ) );
            // NavigationCommands.Search appears to be using the F3 KeyGesture so it's good to represent our FindNext.
            this.CommandBindings.Add( new CommandBinding( NavigationCommands.Search, OnCommandExecuted, CanExecuteCommand ) );

            this.CommandBindings.Add( new CommandBinding( WebControlCommands.ActivateIME, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.AddURLFilter, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.CancelIMEComposition, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.ChooseFile, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.ClearAllURLFilters, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.ConfirmIMEComposition, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.CreateObject, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.DestroyObject, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.LoadFile, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.LoadURL, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.ResetZoom, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.StopFind, OnCommandExecuted, CanExecuteCommand ) );
        }
        #endregion


        #region Update
        private void Update()
        {
            if ( bitmap != null )
            {
                RenderBuffer buffer = Render();
                if ( buffer != null )
                {
                    buffer.CopyToBitmap( bitmap );
                }
            }
        }
        #endregion

        #region Render
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_webview_render( IntPtr webview );

        protected RenderBuffer Render()
        {
            return new RenderBuffer( awe_webview_render( Instance ) );
        }
        #endregion

        #region PauseRendering
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_pause_rendering( IntPtr webview );

        /// <summary>
        /// All rendering is actually done asynchronously in a separate process
        /// and so the page is usually continuously rendering even if you never call
        /// WebView.Render. Call this to temporarily pause rendering.
        /// </summary>
        protected void PauseRendering()
        {
            awe_webview_pause_rendering( Instance );
        }
        #endregion

        #region ResumeRendering
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_resume_rendering( IntPtr webview );

        /// <summary>
        /// Resume rendering after a call to WebView.PauseRendering
        /// </summary>
        protected void ResumeRendering()
        {
            awe_webview_resume_rendering( Instance );
        }
        #endregion

        #region InjectMouseMove
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_move( IntPtr webview, int x, int y );

        protected void InjectMouseMove( int x, int y )
        {
            awe_webview_inject_mouse_move( Instance, x, y );
        }
        #endregion

        #region InjectMouseDown
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_down( IntPtr webview, MouseButton mouseButton );

        protected void InjectMouseDown( MouseButton mouseButton )
        {
            awe_webview_inject_mouse_down( Instance, mouseButton );
        }
        #endregion

        #region InjectMouseUp
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_up( IntPtr webview, MouseButton mouseButton );

        protected void InjectMouseUp( MouseButton mouseButton )
        {
            awe_webview_inject_mouse_up( Instance, mouseButton );
        }
        #endregion

        #region InjectMouseWheel
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_wheel( IntPtr webview, int scroll_amount_vert, int scroll_amount_horz );

        protected void InjectMouseWheel( int scrollAmountVert, int scrollAmountHorz = 0 )
        {
            awe_webview_inject_mouse_wheel( Instance, scrollAmountVert, scrollAmountHorz );
        }
        #endregion

        #region InjectKeyboardEvent
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_keyboard_event( IntPtr webview, WebKeyboardEvent key_event );

        protected void InjectKeyboardEvent( WebKeyboardEvent keyEvent )
        {
            awe_webview_inject_keyboard_event( Instance, keyEvent );
        }
        #endregion

        #region InjectKeyboardEventWin
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_keyboard_event_win( IntPtr webview, int msg, int wparam, int lparam );

        protected void InjectKeyboardEventWin( int msg, int wparam, int lparam )
        {
            awe_webview_inject_keyboard_event_win( Instance, msg, wparam, lparam );
        }
        #endregion

        #region Resize
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static bool awe_webview_resize( IntPtr webview,
            int width,
            int height,
            bool wait_for_repaint,
            int repaint_timeout_ms );

        /// <summary>
        /// Resizes this WebView to certain dimensions. This operation can fail
        /// if another resize is already pending (see WebView.isResizing) or if
        /// the repaint timeout was exceeded.
        /// </summary>
        /// <param name="width">The width in pixels to resize to</param>
        /// <param name="height">The height in pixels to resize to</param>
        /// <param name="waitForRepaint">Whether or not to wait for the WebView
        /// to finish repainting to avoid flicker (default is true).</param>
        /// <param name="repaintTimeoutMs">The max amount of time to wait for
        /// a repaint, in milliseconds</param>
        /// <returns>Returns true if the resize was successful.</returns>
        protected bool Resize( int width, int height, bool waitForRepaint = true, int repaintTimeoutMs = 300 )
        {
            return awe_webview_resize( Instance, width, height, waitForRepaint, repaintTimeoutMs );
        }
        #endregion

        #region UnfocusView
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_unfocus( IntPtr webview );

        protected void UnfocusView()
        {
            awe_webview_unfocus( Instance );
        }
        #endregion

        #region FocusView
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_focus( IntPtr webview );

        /// <summary>
        /// Notifies the current page that it has gained focus. You will need to
        /// call this to gain textbox focus, among other things. (If you fail to
        /// ever see a blinking caret when typing text, this is why.)
        /// </summary>
        protected void FocusView()
        {
            awe_webview_focus( Instance );
        }
        #endregion

        #endregion

        #region Public

        #region LoadURL
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_resource_interceptor(/*To do?*/);

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_load_url( IntPtr webview, IntPtr url, IntPtr frame_name, IntPtr username, IntPtr password );

        public void LoadURL( string url, string frameName = "", string username = "", string password = "" )
        {
            VerifyLive();

            StringHelper urlStr = new StringHelper( url );
            StringHelper frameNameStr = new StringHelper( frameName );
            StringHelper usernameStr = new StringHelper( username );
            StringHelper passwordStr = new StringHelper( password );

            awe_webview_load_url( Instance, urlStr.Value, frameNameStr.Value, usernameStr.Value, passwordStr.Value );
        }
        #endregion

        #region LoadHTML
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_load_html( IntPtr webview, IntPtr html, IntPtr frame_name );

        public void LoadHTML( string html, string frameName = "" )
        {
            VerifyLive();

            StringHelper htmlStr = new StringHelper( html );
            StringHelper frameNameStr = new StringHelper( frameName );

            awe_webview_load_html( Instance, htmlStr.Value, frameNameStr.Value );
        }
        #endregion

        #region LoadFile
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_load_file( IntPtr webview, IntPtr file, IntPtr frame_name );

        public void LoadFile( string file, string frameName = "" )
        {
            VerifyLive();

            StringHelper fileStr = new StringHelper( file );
            StringHelper frameNameStr = new StringHelper( frameName );

            awe_webview_load_file( Instance, fileStr.Value, frameNameStr.Value );
        }
        #endregion

        #region GoToHistoryOffset
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_go_to_history_offset( IntPtr webview, int offset );

        public void GoToHistoryOffset( int offset )
        {
            VerifyLive();
            awe_webview_go_to_history_offset( Instance, offset );
        }
        #endregion

        #region GoBack
        public void GoBack()
        {
            GoToHistoryOffset( -1 );
        }
        #endregion

        #region GoForward
        public void GoForward()
        {
            GoToHistoryOffset( 1 );
        }
        #endregion

        #region Stop
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_stop( IntPtr webview );

        public void Stop()
        {
            VerifyLive();
            awe_webview_stop( Instance );
        }
        #endregion

        #region Reload
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_reload( IntPtr webview );

        public void Reload()
        {
            VerifyLive();
            awe_webview_reload( Instance );
        }
        #endregion

        #region ExecuteJavascript
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_execute_javascript( IntPtr webview, IntPtr javascript, IntPtr frame_name );

        /// <summary>
        /// Executes a string of Javascript in the context of the current page
        /// asynchronously.
        /// </summary>
        /// <param name="javascript">The string of Javascript to execute</param>
        /// <param name="frameName">Optional; the name of the frame to execute in,
        /// leave this blank to execute in the main frame.</param>
        public void ExecuteJavascript( string javascript, string frameName = "" )
        {
            VerifyLive();

            StringHelper javascriptStr = new StringHelper( javascript );
            StringHelper frameNameStr = new StringHelper( frameName );

            awe_webview_execute_javascript( Instance, javascriptStr.Value, frameNameStr.Value );
        }
        #endregion

        #region ExecuteJavascriptWithResult
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static IntPtr awe_webview_execute_javascript_with_result( IntPtr webview, IntPtr javascript, IntPtr frame_name, int timeout_ms );

        /// <summary>
        /// Executes a string of Javascript in the context of the current page
        /// synchronously with a result.
        /// </summary>
        /// <param name="javascript">The string of Javascript to execute</param>
        /// <param name="frameName">Optional; the name of the frame to execute in,
        /// leave this blank to execute in the main frame.</param>
        /// <param name="timeoutMs">Optional; the maximum time to wait for the result
        /// to be computed. Leave this 0 to wait forever (may hang if Javascript is 
        /// invalid!)</param>
        /// <returns>Returns the result as a JSValue. Please note that the returned
        /// result is only a shallow, read-only copy of the original object. This
        /// method does not return system-defined Javascript objects (such as "window",
        /// "document", or any DOM elements).</returns>
        public JSValue ExecuteJavascriptWithResult( string javascript, string frameName = "", int timeoutMs = 0 )
        {
            VerifyLive();

            StringHelper javascriptStr = new StringHelper( javascript );
            StringHelper frameNameStr = new StringHelper( frameName );

            IntPtr temp = awe_webview_execute_javascript_with_result( Instance, javascriptStr.Value, frameNameStr.Value, timeoutMs );

            JSValue result = new JSValue( temp ) { ownsInstance = true };

            return result;
        }
        #endregion

        #region CallJavascriptFunction
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_call_javascript_function( IntPtr webview,
            IntPtr objectName,
            IntPtr function,
            IntPtr arguments,
            IntPtr frame_name );

        public void CallJavascriptFunction( string objectName, string function, params JSValue[] arguments )
        {
            VerifyLive();

            IntPtr jsarray = JSArrayHelper.CreateArray( arguments );

            StringHelper objectNameStr = new StringHelper( objectName );
            StringHelper functionStr = new StringHelper( function );
            StringHelper frameNameStr = new StringHelper( "" );

            awe_webview_call_javascript_function( Instance, objectNameStr.Value, functionStr.Value, jsarray, frameNameStr.Value );

            JSArrayHelper.DestroyArray( jsarray );
        }
        #endregion

        #region CreateObject
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_create_object( IntPtr webview, IntPtr object_name );

        /// <summary>
        /// Creates a new global Javascript object that will persist throughout
        /// the lifetime of this WebView. This is useful for exposing your application's
        /// data and events to Javascript. This object is managed directly by Awesomium
        /// so you can modify its properties and bind callback functions via
        /// WebView.SetObjectProperty and WebView.SetObjectCallback, respectively.
        /// </summary>
        /// <param name="objectName"></param>
        public void CreateObject( string objectName )
        {
            VerifyLive();

            StringHelper objectNameStr = new StringHelper( objectName );
            awe_webview_create_object( Instance, objectNameStr.Value );
        }
        #endregion

        #region DestroyObject
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_destroy_object( IntPtr webview, IntPtr object_name );

        public void DestroyObject( string objectName )
        {
            VerifyLive();

            StringHelper objectNameStr = new StringHelper( objectName );
            awe_webview_destroy_object( Instance, objectNameStr.Value );
        }
        #endregion

        #region SetObjectProperty
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_object_property( IntPtr webview, IntPtr object_name, IntPtr property_name, IntPtr val );

        /// <summary>
        /// Sets a property of a Javascript object previously created by WebView.CreateObject.
        /// An example of usage:
        /// <pre>
        /// webView.CreateObject("MyObject");
        /// webView.SetObjectProperty("MyObject", "color", "blue");
        /// 
        /// // You can now access this object's property via Javascript on any 
        /// // page loaded into this WebView:
        /// var myColor = MyObject.color; // value would be "blue"
        /// </pre>
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="propertyName"></param>
        /// <param name="val"></param>
        public void SetObjectProperty( string objectName, string propertyName, JSValue val )
        {
            VerifyLive();

            StringHelper objectNameStr = new StringHelper( objectName );
            StringHelper propertyNameStr = new StringHelper( propertyName );

            awe_webview_set_object_property( Instance, objectNameStr.Value, propertyNameStr.Value, val.Instance );
        }
        #endregion

        #region SetObjectCallback
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_object_callback( IntPtr webview, IntPtr object_name, IntPtr callback_name );

        /// <summary>
        /// Binds a callback function to a Javascript object previously created by WebView.CreateObject.
        /// An example of usage:
        /// <pre>
        /// public void OnSelectItem(object sender, JSCallbackEventArgs e)
        /// {
        ///     System.Console.WriteLine("Player selected item: " + e.args[0].ToString());
        /// }
        /// 
        /// public void initWebView()
        /// {
        ///     webView.CreateObject("MyObject");
        ///     webView.SetObjectCallback("MyObject", "selectItem", OnSelectItem);
        /// }
        /// 
        /// // You can now call the function "OnSelectItem" from Javascript:
        /// MyObject.selectItem("shotgun");
        /// </pre>
        /// </summary>
        /// <param name="objectName"></param>
        /// <param name="callbackName"></param>
        /// <param name="callback"></param>
        public void SetObjectCallback( string objectName, string callbackName, JSCallback callback )
        {
            VerifyLive();

            StringHelper objectNameStr = new StringHelper( objectName );
            StringHelper callbackNameStr = new StringHelper( callbackName );

            awe_webview_set_object_callback( Instance, objectNameStr.Value, callbackNameStr.Value );

            string key = String.Format( "{0}.{1}", objectName, callbackName );

            if ( jsObjectCallbackMap.ContainsKey( key ) )
                jsObjectCallbackMap.Remove( key );

            if ( callback != null )
                jsObjectCallbackMap.Add( key, callback );
        }
        #endregion

        #region GetDirtyBounds
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern AweRect awe_webview_get_dirty_bounds( IntPtr webview );

        public AweRect GetDirtyBounds()
        {
            VerifyLive();

            AweRect bounds = awe_webview_get_dirty_bounds( Instance );
            AweRect result = new AweRect
            {
                X = bounds.X,
                Y = bounds.Y,
                Width = bounds.Width,
                Height = bounds.Height
            };

            return result;
        }
        #endregion

        #region Cut
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_cut( IntPtr webview );

        public void Cut()
        {
            VerifyLive();
            awe_webview_cut( Instance );
        }
        #endregion

        #region Copy
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_copy( IntPtr webview );

        public void Copy()
        {
            VerifyLive();
            awe_webview_copy( Instance );
        }
        #endregion

        #region Paste
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_paste( IntPtr webview );

        public void Paste()
        {
            VerifyLive();
            awe_webview_paste( Instance );
        }
        #endregion

        #region SelectAll
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_select_all( IntPtr webview );

        public void SelectAll()
        {
            VerifyLive();
            awe_webview_select_all( Instance );
        }
        #endregion

        #region ResetZoom
        public void ResetZoom()
        {
            VerifyLive();
            this.ClearValue( WebControl.ZoomProperty );
        }
        #endregion

        #region SetTransparent
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_transparent( IntPtr webview, bool is_transparent );

        /// <summary>
        /// Sets whether or not pages should be rendered with transparency
        /// preserved (for ex, for pages with style="background-color:transparent")
        /// </summary>
        /// <param name="isTransparent"></param>
        public void SetTransparent( bool isTransparent )
        {
            VerifyLive();
            awe_webview_set_transparent( Instance, isTransparent );
        }
        #endregion

        #region SetURLFilteringMode
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_url_filtering_mode( IntPtr webview, URLFilteringMode filteringMode );

        public void SetURLFilteringMode( URLFilteringMode filteringMode )
        {
            VerifyLive();
            awe_webview_set_url_filtering_mode( Instance, filteringMode );
        }
        #endregion

        #region AddURLFilter
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_add_url_filter( IntPtr webview, IntPtr filter );

        public void AddURLFilter( string filter )
        {
            VerifyLive();
            StringHelper filterStr = new StringHelper( filter );
            awe_webview_add_url_filter( Instance, filterStr.Value );
        }
        #endregion

        #region ClearAllURLFilters
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_clear_all_url_filters( IntPtr webview );

        public void ClearAllURLFilters()
        {
            VerifyLive();
            awe_webview_clear_all_url_filters( Instance );
        }
        #endregion

        #region SetHeaderDefinition
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_header_definition( IntPtr webview,
            IntPtr name,
            uint num_fields,
            IntPtr[] field_names,
            IntPtr[] field_values );

        public void SetHeaderDefinition( string name, NameValueCollection fields )
        {
            VerifyLive();

            StringHelper nameStr = new StringHelper( name );

            int count = fields.Count;
            IntPtr[] keys = new IntPtr[ count ];
            IntPtr[] values = new IntPtr[ count ];

            for ( int i = 0; i < count; i++ )
            {
                byte[] utf16string;

                utf16string = Encoding.Unicode.GetBytes( fields.GetKey( i ) );
                keys[ i ] = StringHelper.awe_string_create_from_utf16( utf16string, (uint)fields.GetKey( i ).Length );

                utf16string = Encoding.Unicode.GetBytes( fields.Get( i ) );
                values[ i ] = StringHelper.awe_string_create_from_utf16( utf16string, (uint)fields.Get( i ).Length );
            }

            awe_webview_set_header_definition( Instance, nameStr.Value, (uint)count, keys, values );

            for ( uint i = 0; i < count; i++ )
            {
                StringHelper.awe_string_destroy( keys[ i ] );
                StringHelper.awe_string_destroy( values[ i ] );
            }
        }
        #endregion

        #region AddHeaderRewriteRule
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_add_header_rewrite_rule( IntPtr webview, IntPtr rule, IntPtr name );

        public void AddHeaderRewriteRule( string rule, string name )
        {
            VerifyLive();

            StringHelper ruleStr = new StringHelper( rule );
            StringHelper nameStr = new StringHelper( name );

            awe_webview_add_header_rewrite_rule( Instance, ruleStr.Value, nameStr.Value );
        }
        #endregion

        #region RemoveHeaderRewriteRule
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_remove_header_rewrite_rule( IntPtr webview, IntPtr rule );

        public void RemoveHeaderRewriteRule( string rule )
        {
            VerifyLive();

            StringHelper ruleStr = new StringHelper( rule );
            awe_webview_remove_header_rewrite_rule( Instance, ruleStr.Value );
        }
        #endregion

        #region RemoveHeaderRewriteRulesByDefinition
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_remove_header_rewrite_rules_by_definition_name( IntPtr webview, IntPtr name );

        public void RemoveHeaderRewriteRulesByDefinition( string name )
        {
            VerifyLive();

            StringHelper nameStr = new StringHelper( name );
            awe_webview_remove_header_rewrite_rules_by_definition_name( Instance, nameStr.Value );
        }
        #endregion

        #region ChooseFile
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_choose_file( IntPtr webview, IntPtr file_path );

        public void ChooseFile( string filePath )
        {
            VerifyLive();
            StringHelper filePathStr = new StringHelper( filePath );
            awe_webview_choose_file( Instance, filePathStr.Value );
        }
        #endregion

        #region Print
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_print( IntPtr webview );

        /// <summary>
        /// Print the current page. To suppress the printer selection dialog
        /// and print immediately using OS defaults, see WebCore.setSuppressPrinterDialog
        /// </summary>
        public void Print()
        {
            VerifyLive();
            awe_webview_print( Instance );
        }
        #endregion

        #region RequestScrollData
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_request_scroll_data( IntPtr webview, IntPtr frame_name );

        public void RequestScrollData( string frameName = "" )
        {
            VerifyLive();
            StringHelper frameNameStr = new StringHelper( frameName );
            awe_webview_request_scroll_data( Instance, frameNameStr.Value );
        }
        #endregion

        #region Find
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_find( IntPtr webview,
            int request_id,
            IntPtr search_string,
            bool forward,
            bool case_sensitive,
            bool find_next );

        private FindData findRequest;

        /// <summary>
        /// Start finding a certain string on the current web-page. All matches
        /// of the string will be highlighted on the page and you can jump to different 
        /// instances of the string by using the <see cref="FindNext"/> method.
        /// To get actual stats about a certain query, please see <see cref="FindResultsReceived"/>.
        /// </summary>
        /// <param name="searchStr">The string to search for.</param>
        /// <param name="forward">Whether or not we should search forward, down the page.</param>
        /// <param name="caseSensitive">Whether or not this search is case-sensitive.</param>
        public void Find( string searchStr, bool forward = true, bool caseSensitive = false )
        {
            VerifyLive();

            if ( findRequest != null )
            {
                StopFind( true );
            }

            findRequest = new FindData( findRequestRandomizer.Next(), searchStr, caseSensitive );
            StringHelper searchCStr = new StringHelper( searchStr );
            awe_webview_find( Instance, findRequest.RequestID, searchCStr.Value, forward, caseSensitive, false );
        }

        /// <summary>
        /// Jump to the next match of a previously successful search.
        /// </summary>
        /// <param name="forward">
        /// Whether or not we should search forward, down the page.
        /// </param>
        public void FindNext( bool forward = true )
        {
            VerifyLive();

            if ( findRequest == null )
                return;

            StringHelper searchCStr = new StringHelper( findRequest.SearchText );
            awe_webview_find( Instance, findRequest.RequestID, searchCStr.Value, forward, findRequest.CaseSensitive, true );
        }
        #endregion

        #region StopFind
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_stop_find( IntPtr webview, bool clear_selection );
        /// <summary>
        /// Stop finding. This will un-highlight all matches of a previous call to WebView.Find
        /// </summary>
        /// <param name="clearSelection">Whether or not we should also deselect the 
        /// currently-selected string instance</param>
        public void StopFind( bool clearSelection )
        {
            VerifyLive();
            awe_webview_stop_find( Instance, clearSelection );
            findRequest = null;
        }
        #endregion

        #region TranslatePage
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_translate_page( IntPtr webview, IntPtr source_language, IntPtr target_language );
        /// <summary>
        /// Attempt automatic translation of the current page via Google
        /// Translate. All language codes are ISO 639-2.
        /// </summary>
        /// <param name="sourceLanguage">The language to translate from (for ex. "en" for English)</param>
        /// <param name="targetLanguage">The language to translate to (for ex. "fr" for French)</param>
        public void TranslatePage( string sourceLanguage, string targetLanguage )
        {
            VerifyLive();

            StringHelper sourceLanguageStr = new StringHelper( sourceLanguage );
            StringHelper targetLanguageStr = new StringHelper( targetLanguage );

            awe_webview_translate_page( Instance, sourceLanguageStr.Value, targetLanguageStr.Value );
        }
        #endregion

        #region ActivateIME
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_activate_ime( IntPtr webview, bool activate );

        /// <summary>
        /// Call this method to the let the WebView know you will be passing
        /// text input via IME and will need to be notified of any IME-related
        /// events (such as caret position, user unfocusing textbox, etc.).
        /// Please see WebView.OnUpdateIME
        /// </summary>
        /// <param name="activate"></param>
        public void ActivateIME( bool activate )
        {
            VerifyLive();
            awe_webview_activate_ime( Instance, activate );
        }
        #endregion

        #region SetIMEComposition
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_ime_composition( IntPtr webview,
            IntPtr input_string,
            int cursor_pos,
            int target_start,
            int target_end );

        /// <summary>
        /// Create or Update the current IME text composition.
        /// </summary>
        /// <param name="inputStr">The string generated by your IME</param>
        /// <param name="cursorPos">The current cursor position in your IME composition.</param>
        /// <param name="targetStart">The position of the beginning of the selection.</param>
        /// <param name="targetEnd">The position of the end of the selection.</param>
        public void SetIMEComposition( string inputStr, int cursorPos, int targetStart, int targetEnd )
        {
            VerifyLive();

            StringHelper inputCStr = new StringHelper( inputStr );
            awe_webview_set_ime_composition( Instance, inputCStr.Value, cursorPos, targetStart, targetEnd );
        }
        #endregion

        #region ConfirmIMEComposition
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_confirm_ime_composition( IntPtr webview, IntPtr input_string );

        public void ConfirmIMEComposition( string inputStr )
        {
            VerifyLive();

            StringHelper inputCStr = new StringHelper( inputStr );
            awe_webview_confirm_ime_composition( Instance, inputCStr.Value );
        }
        #endregion

        #region CancelIMEComposition
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_cancel_ime_composition( IntPtr webview );

        public void CancelIMEComposition()
        {
            VerifyLive();
            awe_webview_cancel_ime_composition( Instance );
        }
        #endregion

        #endregion

        #endregion

        #region Properties

        #region IsLive
        /// <summary>
        /// Gets if the control is live and the view is instantiated.
        /// </summary>
        protected bool IsLive
        {
            get
            {
                return ( Instance != IntPtr.Zero ) &&
                    !DesignerProperties.GetIsInDesignMode( this ) &&
                    !IsCrashed;
            }
        }
        #endregion

        #region Instance
        internal IntPtr Instance { get; private set; }
        #endregion

        #region IsDirty
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_webview_is_dirty( IntPtr webview );

        /// <summary>
        /// Gets whether or not this WebControl needs to be rendered again.
        /// </summary>
        public bool IsDirty
        {
            get { return (bool)this.GetValue( WebControl.IsDirtyProperty ); }
            internal set
            {
                VerifyLive();
                SetValue( WebControl.IsDirtyPropertyKey, value );

                // Insist on firing while True.
                if ( value )
                {
                    OnPropertyChanged( new DependencyPropertyChangedEventArgs( WebControl.IsDirtyProperty, false, true ) );
                    Update();
                    OnIsDirtyChanged( this, EventArgs.Empty );
                }
            }
        }

        private static readonly DependencyPropertyKey IsDirtyPropertyKey =
                                DependencyProperty.RegisterReadOnly( "IsDirty",
                                typeof( bool ), typeof( WebControl ),
                                new FrameworkPropertyMetadata( false, IsDirtyPropChanged ) );

        public static readonly DependencyProperty IsDirtyProperty =
            IsDirtyPropertyKey.DependencyProperty;

        private static void IsDirtyPropChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            WebControl owner = (WebControl)d;
            bool value = (bool)e.NewValue;

            if ( owner.IsLive && !value )
            {
                owner.Update();
                owner.OnIsDirtyChanged( owner, EventArgs.Empty );
            }
        }
        #endregion

        #region IsResizing
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_webview_is_resizing( IntPtr webview );

        /// <summary>
        /// Checks whether or not there is a resize operation pending.
        /// </summary>
        /// <returns>Returns true if we are waiting for the WebView process to
        /// return acknowledgment of a pending resize operation.</returns>
        public bool IsResizing
        {
            get { return (bool)this.GetValue( WebControl.IsResizingProperty ); }
        }

        private static readonly DependencyPropertyKey IsResizingPropertyKey =
                                DependencyProperty.RegisterReadOnly( "IsResizing",
                                typeof( bool ), typeof( WebControl ),
                                new FrameworkPropertyMetadata( false, null, CoerceIsResizing ) );

        public static readonly DependencyProperty IsResizingProperty =
            IsResizingPropertyKey.DependencyProperty;

        private static object CoerceIsResizing( DependencyObject d, object baseValue )
        {
            WebControl owner = (WebControl)d;

            if ( !owner.IsLive )
                return baseValue;

            return awe_webview_is_dirty( owner.Instance );
        }
        #endregion

        #region IsLoadingPage
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_webview_is_loading_page( IntPtr webview );

        public bool IsLoadingPage
        {
            get { return (bool)this.GetValue( WebControl.IsLoadingPageProperty ); }
        }

        private static readonly DependencyPropertyKey IsLoadingPagePropertyKey =
                                DependencyProperty.RegisterReadOnly( "IsLoadingPage",
                                typeof( bool ), typeof( WebControl ),
                                new FrameworkPropertyMetadata( false, null, CoerceIsLoadingPage ) );

        public static readonly DependencyProperty IsLoadingPageProperty =
            IsLoadingPagePropertyKey.DependencyProperty;

        private static object CoerceIsLoadingPage( DependencyObject d, object baseValue )
        {
            WebControl owner = (WebControl)d;

            if ( !owner.IsLive )
                return false;

            return awe_webview_is_loading_page( owner.Instance );
        }
        #endregion

        #region Title
        public string Title
        {
            get { return (string)this.GetValue( WebControl.TitleProperty ); }
            protected set { SetValue( WebControl.TitlePropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey TitlePropertyKey =
            DependencyProperty.RegisterReadOnly( "Title",
            typeof( string ), typeof( WebControl ),
            new FrameworkPropertyMetadata( null ) );

        public static readonly DependencyProperty TitleProperty =
            TitlePropertyKey.DependencyProperty;
        #endregion

        #region HasKeyboardFocus
        public bool HasKeyboardFocus
        {
            get { return (bool)this.GetValue( WebControl.HasKeyboardFocusProperty ); }
            protected set { SetValue( WebControl.HasKeyboardFocusPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey HasKeyboardFocusPropertyKey =
            DependencyProperty.RegisterReadOnly( "HasKeyboardFocus",
            typeof( bool ), typeof( WebControl ),
            new FrameworkPropertyMetadata( false ) );

        public static readonly DependencyProperty HasKeyboardFocusProperty =
            HasKeyboardFocusPropertyKey.DependencyProperty;
        #endregion

        #region TargetURL
        public string TargetURL
        {
            get { return (string)this.GetValue( WebControl.TargetURLProperty ); }
            protected set { SetValue( WebControl.TargetURLPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey TargetURLPropertyKey =
            DependencyProperty.RegisterReadOnly( "TargetURL",
            typeof( string ), typeof( WebControl ),
            new FrameworkPropertyMetadata( null ) );

        public static readonly DependencyProperty TargetURLProperty =
            TargetURLPropertyKey.DependencyProperty;
        #endregion

        #region IsCrashed
        public bool IsCrashed
        {
            get { return (bool)this.GetValue( WebControl.IsCrashedProperty ); }
            protected set { SetValue( WebControl.IsCrashedPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey IsCrashedPropertyKey =
            DependencyProperty.RegisterReadOnly( "IsCrashed",
            typeof( bool ), typeof( WebControl ),
            new FrameworkPropertyMetadata( false ) );

        public static readonly DependencyProperty IsCrashedProperty =
            IsCrashedPropertyKey.DependencyProperty;
        #endregion

        #region PageContents
        public string PageContents
        {
            get { return (string)this.GetValue( WebControl.PageContentsProperty ); }
            protected set { SetValue( WebControl.PageContentsPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey PageContentsPropertyKey =
            DependencyProperty.RegisterReadOnly( "PageContents",
            typeof( string ), typeof( WebControl ),
            new FrameworkPropertyMetadata( null ) );

        public static readonly DependencyProperty PageContentsProperty =
            PageContentsPropertyKey.DependencyProperty;
        #endregion

        #region IsDomReady
        public bool IsDomReady
        {
            get { return (bool)this.GetValue( WebControl.IsDomReadyProperty ); }
            protected set { SetValue( WebControl.IsDomReadyPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey IsDomReadyPropertyKey =
            DependencyProperty.RegisterReadOnly( "IsDomReady",
            typeof( bool ), typeof( WebControl ),
            new FrameworkPropertyMetadata( null ) );

        public static readonly DependencyProperty IsDomReadyProperty =
            IsDomReadyPropertyKey.DependencyProperty;
        #endregion

        #region Zoom
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_zoom( IntPtr webview, int zoom_percent );
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_reset_zoom( IntPtr webview );

        /// <summary>
        /// Gets or sets the zoom percentage. The default is 100.
        /// Valid range is from 10 to 500.
        /// </summary>
        public int Zoom
        {
            get { return (int)this.GetValue( ZoomProperty ); }
            set { SetValue( ZoomProperty, value ); }
        }

        public static readonly DependencyProperty ZoomProperty =
            DependencyProperty.Register( "Zoom",
            typeof( int ), typeof( WebControl ),
            new FrameworkPropertyMetadata( 100, ZoomChanged ), ValidateZoom );

        private static bool ValidateZoom( object value )
        {
            return ( (int)value >= 10 ) && ( (int)value <= 500 );
        }

        private static void ZoomChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            WebControl owner = (WebControl)d;
            int value = (int)e.NewValue;

            if ( !owner.IsLive )
                return;

            if ( value == 100 )
                awe_webview_reset_zoom( owner.Instance );
            else
                awe_webview_set_zoom( owner.Instance, value );
        }
        #endregion

        #region Source
        private string actualSource;

        public Uri Source
        {
            get { return (Uri)this.GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( "Source",
            typeof( Uri ), typeof( WebControl ),
            new FrameworkPropertyMetadata( null, SourceChanged ) );

        private static void SourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            WebControl owner = (WebControl)d;
            Uri value = (Uri)e.NewValue;

            if ( !owner.IsLive )
                return;

            if ( String.Compare( owner.actualSource, value.AbsoluteUri, true ) != 0 )
            {
                owner.actualSource = value.AbsoluteUri;
                owner.LoadURL( owner.actualSource );
            }
        }
        #endregion

        #endregion

        #region Internal Event Handlers

        #region Command Handlers
        private void OnCommandExecuted( object sender, ExecutedRoutedEventArgs e )
        {
            if ( e.Command is RoutedCommand )
            {
                RoutedCommand command = (RoutedCommand)e.Command;

                switch ( command.Name )
                {
                    case "Copy":
                        this.Copy();
                        break;

                    case "Cut":
                        this.Cut();
                        break;

                    case "Paste":
                        this.Paste();
                        break;

                    case "SelectAll":
                        this.SelectAll();
                        break;

                    case "Find":
                        if ( e.Parameter != null )
                            this.Find( e.Parameter.ToString() );
                        break;

                    case "BrowseForward":
                        this.GoForward();
                        break;

                    case "BrowseBack":
                        this.GoBack();
                        break;

                    case "BrowseStop":
                        this.Stop();
                        break;

                    case "Refresh":
                        this.Reload();
                        break;

                    case "Zoom":
                        if ( e.Parameter != null )
                            this.Zoom = (int)e.Parameter;
                        break;

                    case "Search":
                        this.FindNext();
                        break;

                    case "LoadURL":
                        if ( e.Parameter != null )
                        {
                            this.LoadURL( e.Parameter is Uri ? ( (Uri)e.Parameter ).AbsoluteUri : e.Parameter.ToString() );
                        }
                        break;

                    case "LoadFile":
                        if ( e.Parameter != null )
                        {
                            this.LoadFile( e.Parameter is Uri ? ( (Uri)e.Parameter ).AbsoluteUri : e.Parameter.ToString() );
                        }
                        break;

                    case "ActivateIME":
                        if ( e.Parameter != null )
                            this.ActivateIME( (bool)e.Parameter );
                        break;

                    case "AddURLFilter":
                        if ( e.Parameter != null )
                            this.AddURLFilter( e.Parameter.ToString() );
                        break;

                    case "CancelIMEComposition":
                        this.CancelIMEComposition();
                        break;

                    case "ChooseFile":
                        if ( e.Parameter != null )
                            this.ChooseFile( e.Parameter.ToString() );
                        break;

                    case "ClearAllURLFilters":
                        this.ClearAllURLFilters();
                        break;

                    case "ConfirmIMEComposition":
                        if ( e.Parameter != null )
                            this.ConfirmIMEComposition( e.Parameter.ToString() );
                        break;

                    case "CreateObject":
                        if ( e.Parameter != null )
                            this.CreateObject( e.Parameter.ToString() );
                        break;

                    case "DestroyObject":
                        if ( e.Parameter != null )
                            this.DestroyObject( e.Parameter.ToString() );
                        break;

                    case "ResetZoom":
                        this.ResetZoom();
                        break;

                    case "StopFind":
                        this.StopFind( e.Parameter != null ? (bool)e.Parameter : true );
                        break;

                }
            }
        }

        private void CanExecuteCommand( object sender, CanExecuteRoutedEventArgs e )
        {
            e.CanExecute = this.IsLive;
        }
        #endregion


        #region Loaded
        // In WPF, the Loaded/Unloaded events may be fired more than once
        // in the lifetime of a control. Such as when the control is hidden/shown
        // or when the control is completely covered by another control and
        // then appears again (through a change in Panel.ZIndex for example).

        private bool hookAdded;

        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            if ( hookAdded )
                return;

            HwndSource source = (HwndSource)PresentationSource.FromVisual( this );
            if ( source != null )
            {
                source.AddHook( HandleMessages );
                hookAdded = true;
            }

            ResumeRendering();
        }

        private void OnUnloaded( object sender, RoutedEventArgs e )
        {
            PauseRendering();
        }
        #endregion

        #region HandleMessages
        private IntPtr HandleMessages( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            int message = ( msg & 65535 );

            if ( ( message == WM_KEYDOWN || message == WM_KEYUP || message == WM_CHAR ) && IsFocused )
            {
                awe_webview_inject_keyboard_event_win( Instance, msg, (int)wParam, (int)lParam );
                handled = true;
            }
            else
            {
                handled = false;
            }

            return IntPtr.Zero;
        }
        #endregion


        #region Web View Events

        #region BeginNavigation
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackBeginNavigationCallback( IntPtr caller, IntPtr url, IntPtr frame_name );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_begin_navigation( IntPtr webview, CallbackBeginNavigationCallback callback );

        private void internalBeginNavigationCallback( IntPtr caller, IntPtr url, IntPtr frame_name )
        {
            BeginNavigationEventArgs e = new BeginNavigationEventArgs( StringHelper.ConvertAweString( url ),
                StringHelper.ConvertAweString( frame_name ) );

            actualSource = e.Url;

            this.IsDomReady = false;
            this.Source = new Uri( e.Url );
            this.CoerceValue( WebControl.IsLoadingPageProperty );
            this.OnBeginNavigation( this, e );
        }
        #endregion

        #region BeginLoading
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackBeginLoadingCallback( IntPtr caller, IntPtr url, IntPtr frame_name, int status_code, IntPtr mime_type );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_begin_loading( IntPtr webview, CallbackBeginLoadingCallback callback );

        private void internalBeginLoadingCallback( IntPtr caller, IntPtr url, IntPtr frame_name, int status_code, IntPtr mime_type )
        {
            BeginLoadingEventArgs e = new BeginLoadingEventArgs( StringHelper.ConvertAweString( url ),
                StringHelper.ConvertAweString( frame_name ),
                status_code,
                StringHelper.ConvertAweString( mime_type ) );

            actualSource = e.Url;

            this.Source = new Uri( e.Url );
            this.CoerceValue( WebControl.IsLoadingPageProperty );
            this.OnBeginLoading( this, e );
        }
        #endregion

        #region FinishLoading
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackFinishLoadingCallback( IntPtr caller );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_finish_loading(
                            IntPtr webview,
                            CallbackFinishLoadingCallback callback );

        private void internalFinishLoadingCallback( IntPtr caller )
        {
            this.CoerceValue( WebControl.IsLoadingPageProperty );
            this.OnLoadCompleted( this, EventArgs.Empty );
        }
        #endregion

        #region JsCallback
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackJsCallback( IntPtr caller, IntPtr object_name, IntPtr callback_name, IntPtr arguments );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_js_callback( IntPtr webview, CallbackJsCallback callback );


        private void internalJsCallback( IntPtr caller, IntPtr object_name, IntPtr callback_name, IntPtr arguments )
        {
            JSValue[] args = JSArrayHelper.getArray( arguments );

            JSCallbackEventArgs e = new JSCallbackEventArgs( StringHelper.ConvertAweString( object_name ), StringHelper.ConvertAweString( callback_name ), args );

            if ( JSCallbackCalled != null )
                JSCallbackCalled( this, e );
        }
        #endregion

        #region ReceiveTitle
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackReceiveTitleCallback( IntPtr caller, IntPtr title, IntPtr frame_name );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_receive_title( IntPtr webview, CallbackReceiveTitleCallback callback );

        private void internalReceiveTitleCallback( IntPtr caller, IntPtr title, IntPtr frame_name )
        {
            ReceiveTitleEventArgs e = new ReceiveTitleEventArgs(
                StringHelper.ConvertAweString( title ),
                StringHelper.ConvertAweString( frame_name ) );

            this.Title = e.Title;
            this.OnTitleReceived( this, e );
        }
        #endregion

        #region ChangeTooltip
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackChangeTooltipCallback( IntPtr caller, IntPtr tooltip );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_change_tooltip( IntPtr webview, CallbackChangeTooltipCallback callback );

        private void internalChangeTooltipCallback( IntPtr caller, IntPtr tooltip )
        {
            ChangeTooltipEventArgs e = new ChangeTooltipEventArgs( StringHelper.ConvertAweString( tooltip ) );

            if ( String.IsNullOrEmpty( e.Tooltip ) )
            {
                toolTip.IsOpen = false;
            }
            else
            {
                toolTip.Content = e.Tooltip;
                toolTip.IsOpen = true;
            }

            this.OnTooltipChanged( this, e );
        }
        #endregion

        #region ChangeCursor
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackChangeCursorCallback( IntPtr caller, int cursor );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_change_cursor( IntPtr webview, CallbackChangeCursorCallback callback );

        private void internalChangeCursorCallback( IntPtr caller, int cursor )
        {
            ChangeCursorEventArgs e = new ChangeCursorEventArgs( (CursorType)cursor );

            this.Cursor = Utilities.GetCursor( e.CursorType );
            this.OnCursorChanged( this, e );
        }
        #endregion

        #region ChangeKeyboardFocus
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackChangeKeyboardFocusCallback( IntPtr caller, bool is_focused );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_change_keyboard_focus( IntPtr webview, CallbackChangeKeyboardFocusCallback callback );

        private void internalChangeKeyboardFocusCallback( IntPtr caller, bool is_focused )
        {
            ChangeKeyboardFocusEventArgs e = new ChangeKeyboardFocusEventArgs( is_focused );

            this.HasKeyboardFocus = e.IsFocused;
            if ( e.IsFocused )
            {
                this.Focus();

                // The BrowseBack command reserves the backspace key. 
                // Remove this gesture and restore when view loses keyboard focus.
                if ( ( browseBackGesture != null ) && NavigationCommands.BrowseBack.InputGestures.Contains( browseBackGesture ) )
                    NavigationCommands.BrowseBack.InputGestures.Remove( browseBackGesture );
            }
            else if ( ( browseBackGesture != null ) && !NavigationCommands.BrowseBack.InputGestures.Contains( browseBackGesture ) )
                NavigationCommands.BrowseBack.InputGestures.Add( browseBackGesture );

            this.OnKeyboardFocusChanged( this, e );
        }
        #endregion

        #region ChangeTargetURL
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackChangeTargetURLCallback( IntPtr caller, IntPtr url );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_change_target_url( IntPtr webview, CallbackChangeTargetURLCallback callback );

        private void internalChangeTargetURLCallback( IntPtr caller, IntPtr url )
        {
            UrlEventArgs e = new UrlEventArgs( StringHelper.ConvertAweString( url ) );

            this.TargetURL = e.Url;
            this.OnTargetUrlChanged( this, e );
        }
        #endregion

        #region OpenExternalLink
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackOpenExternalLinkCallback( IntPtr caller, IntPtr url, IntPtr source );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_open_external_link( IntPtr webview, CallbackOpenExternalLinkCallback callback );

        private void internalOpenExternalLinkCallback( IntPtr caller, IntPtr url, IntPtr source )
        {
            OpenExternalLinkEventArgs e = new OpenExternalLinkEventArgs(
                StringHelper.ConvertAweString( url ),
                StringHelper.ConvertAweString( source ) );

            this.OnOpenExternalLink( this, e );
        }
        #endregion

        #region RequestDownload
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackRequestDownloadCallback( IntPtr caller, IntPtr download );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_request_download( IntPtr webview, CallbackRequestDownloadCallback callback );

        private void internalRequestDownloadCallback( IntPtr caller, IntPtr download )
        {
            UrlEventArgs e = new UrlEventArgs( StringHelper.ConvertAweString( download ) );
            this.OnDownload( this, e );
        }
        #endregion

        #region WebviewCrashed
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackWebviewCrashedCallback( IntPtr caller );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_web_view_crashed( IntPtr webview, CallbackWebviewCrashedCallback callback );

        private void internalWebviewCrashedCallback( IntPtr caller )
        {
            this.IsCrashed = true;
            this.OnCrashed( this, EventArgs.Empty );
        }
        #endregion

        #region PluginCrashed
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackPluginCrashedCallback( IntPtr caller, IntPtr plugin_name );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_plugin_crashed( IntPtr webview, CallbackPluginCrashedCallback callback );

        private void internalPluginCrashedCallback( IntPtr caller, IntPtr plugin_name )
        {
            PluginCrashedEventArgs e = new PluginCrashedEventArgs( StringHelper.ConvertAweString( plugin_name ) );
            this.OnPluginCrashed( this, e );
        }
        #endregion

        #region RequestMove
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackRequestMoveCallback( IntPtr caller, int x, int y );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_request_move( IntPtr webview, CallbackRequestMoveCallback callback );

        private void internalRequestMoveCallback( IntPtr caller, int x, int y )
        {
            MoveEventArgs e = new MoveEventArgs( x, y );
            this.OnMove( this, e );
        }
        #endregion

        #region GetPageContents
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackGetPageContentsCallback( IntPtr caller, IntPtr url, IntPtr contents );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_get_page_contents( IntPtr webview, CallbackGetPageContentsCallback callback );

        private void internalGetPageContentsCallback( IntPtr caller, IntPtr url, IntPtr contents )
        {
            GetPageContentsEventArgs e = new GetPageContentsEventArgs(
                StringHelper.ConvertAweString( url ),
                StringHelper.ConvertAweString( contents ) );

            actualSource = e.Url;

            this.Source = new Uri( e.Url );
            this.CoerceValue( WebControl.IsLoadingPageProperty );
            this.PageContents = e.Contents;
            this.OnPageContentsReceived( this, e );
        }
        #endregion

        #region DomReady
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackDomReadyCallback( IntPtr caller );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_dom_ready( IntPtr webview, CallbackDomReadyCallback callback );

        private void internalDomReadyCallback( IntPtr caller )
        {
            this.IsDomReady = true;
            this.OnDomReady( this, EventArgs.Empty );
        }
        #endregion

        #region RequestFileChooser
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackRequestFileChooserCallback( IntPtr caller, bool select_multiple_files, IntPtr title, IntPtr default_path );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_request_file_chooser( IntPtr webview, CallbackRequestFileChooserCallback callback );

        private void internalRequestFileChooser( IntPtr caller, bool select_multiple_files, IntPtr title, IntPtr default_paths )
        {
            SelectLocalFilesEventArgs e = new SelectLocalFilesEventArgs(
                select_multiple_files,
                StringHelper.ConvertAweString( title ),
                StringHelper.ConvertAweString( default_paths ) );

            this.OnSelectLocalFiles( this, e );

            if ( ( e.SelectedFiles != null ) && ( e.SelectedFiles.Length > 0 ) )
            {
                foreach ( string f in e.SelectedFiles )
                    this.ChooseFile( f );
            }
        }
        #endregion

        #region GetScrollData
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackGetScrollDataCallback( IntPtr caller, int contentWidth, int contentHeight, int preferredWidth, int scrollX, int scrollY );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_get_scroll_data( IntPtr webview, CallbackGetScrollDataCallback callback );

        private void internalGetScrollData( IntPtr caller, int contentWidth, int contentHeight, int preferredWidth, int scrollX, int scrollY )
        {
            ScrollDataEventArgs e = new ScrollDataEventArgs( new ScrollData( contentWidth, contentHeight, preferredWidth, scrollX, scrollY ) );
            this.OnScrollDataReceived( this, e );
        }
        #endregion

        #region JSConsoleMessage
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackJSConsoleMessageCallback( IntPtr caller, IntPtr message, int lineNumber, IntPtr source );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_js_console_message( IntPtr webview, CallbackJSConsoleMessageCallback callback );

        private void internalJSConsoleMessage( IntPtr caller, IntPtr message, int lineNumber, IntPtr source )
        {
            JSConsoleMessageEventArgs e = new JSConsoleMessageEventArgs(
                StringHelper.ConvertAweString( message ),
                lineNumber,
                StringHelper.ConvertAweString( source ) );

            this.OnJSConsoleMessageAdded( this, e );
        }
        #endregion

        #region GetFindResults
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackGetFindResultsCallback( IntPtr caller, int request_id, int num_matches, AweRect selection, int cur_match, bool finalUpdate );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_get_find_results( IntPtr webview, CallbackGetFindResultsCallback callback );

        private void internalGetFindResults( IntPtr caller, int request_id, int num_matches, AweRect selection, int cur_match, bool finalUpdate )
        {
            GetFindResultsEventArgs e = new GetFindResultsEventArgs( request_id,
                num_matches,
                selection,
                cur_match,
                finalUpdate );

            this.OnFindResultsReceived( this, e );
        }
        #endregion

        #region UpdateIME
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackUpdateIMECallback( IntPtr caller, int state, AweRect caret_rect );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_update_ime( IntPtr webview, CallbackUpdateIMECallback callback );

        private void internalUpdateIME( IntPtr caller, int state, AweRect caret_rect )
        {
            UpdateImeEventArgs e = new UpdateImeEventArgs( (IMEState)state, caret_rect );
            this.OnImeUpdated( this, e );
        }
        #endregion

        #region ResourceRequest
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate IntPtr CallbackResourceRequestCallback( IntPtr caller, IntPtr request );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_resource_request( IntPtr webview, CallbackResourceRequestCallback callback );

        private IntPtr internalResourceRequestCallback( IntPtr caller, IntPtr request )
        {
            ResourceRequest requestWrap = new ResourceRequest( request );
            ResourceRequestEventArgs e = new ResourceRequestEventArgs( requestWrap );

            if ( ResourceRequest != null )
            {
                ResourceResponse response = this.OnResourceRequest( this, e );

                if ( response != null )
                    return response.getInstance();
            }

            return IntPtr.Zero;
        }
        #endregion

        #region ResourceResponse
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackResourceResponseCallback( IntPtr caller, IntPtr url, int statusCode, bool wasCached, long requestTimeMs,
            long responseTimeMs, long expectedContentSize, IntPtr mimeType );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_resource_response( IntPtr webview, CallbackResourceResponseCallback callback );

        private void internalResourceResponseCallback( IntPtr caller, IntPtr url, int statusCode, bool wasCached, long requestTimeMs,
            long responseTimeMs, long expectedContentSize, IntPtr mimeType )
        {
            ResourceResponseEventArgs e = new ResourceResponseEventArgs(
                StringHelper.ConvertAweString( url ),
                statusCode, wasCached,
                requestTimeMs,
                responseTimeMs,
                expectedContentSize,
                StringHelper.ConvertAweString( mimeType ) );

            this.OnResourceResponse( this, e );
        }
        #endregion


        #region JSCallback
        internal void handleJSCallback( object sender, JSCallbackEventArgs e )
        {
            string key = String.Format( "{0}.{1}", e.ObjectName, e.CallbackName );

            if ( jsObjectCallbackMap.ContainsKey( key ) )
                jsObjectCallbackMap[ key ]( sender, e );
        }
        #endregion

        #endregion

        #endregion


        #region IWebView Members
        void IWebView.PrepareForShutdown()
        {
            if ( Instance != IntPtr.Zero )
            {
                resourceRequestCallback = null;
                awe_webview_set_callback_resource_request( Instance, null );

                resourceResponseCallback = null;
                awe_webview_set_callback_resource_response( Instance, null );

                this.Destroy( true );
            }
        }

        IntPtr IWebView.Instance
        {
            get
            {
                return this.Instance;
            }
            set
            {
                this.Instance = value;
            }
        }

        bool IWebView.IsDirty
        {
            get
            {
                return this.IsDirty;
            }
            set
            {
                this.IsDirty = value;
            }
        }
        #endregion
    }
}
