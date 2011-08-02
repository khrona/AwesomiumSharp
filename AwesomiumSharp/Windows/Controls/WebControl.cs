/********************************************************************************
 *    Project  : AwesomiumSharp
 *    File     : WebControl.cs
 *    Version  : 1.6.2.0 
 *    Date     : 08/02/2011
 *    Author   : Perikles C. Stephanidis (AmaDeuS)
 *    Contact  : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes    :
 *
 *    Represents a WPF control that wraps an Awesomium web view. 
 *    You can use it to embed Awesomium directly in your WPF application 
 *    without any additional work.
 *    
 *    Changelog: 
 *    
 *    https://github.com/khrona/AwesomiumSharp/commits/master.atom
 *    
 ********************************************************************************/

#region Using
using System;
using System.Linq;
using System.Windows;
using System.Collections;
using System.Windows.Input;
using System.Windows.Media;
using System.ComponentModel;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Collections.Specialized;
using System.IO;
#endregion

namespace AwesomiumSharp.Windows.Controls
{
    #region Documentation
    /// <summary>
    /// Represents a WPF control that wraps an Awesomium web view.
    /// You can use it to embed Awesomium directly in your WPF application without any additional work.
    /// </summary>
    /// <remarks>
    /// You can create an instance of this class by directly invoking the
    /// default constructor (either by dropping it in your designer surface, through XAML or from code). 
    /// You do not need to explicitly create an instance of a web view through <see cref="WebCore"/>.
    /// WebControl takes care of this internally.
    /// <p/>
    /// <note>
    /// Note that it is safe to use this control in a design environment for layout and configuration
    /// purposes. <see cref="WebCore"/> and the underlying web view are only instantiated during runtime.
    /// </note>
    /// <p/>
    /// <h4>The Role of the <see cref="UIElement.IsEnabled"/> Property</h4>
    /// In addition to its regular meaning, the <see cref="UIElement.IsEnabled"/> property has a special
    /// meaning in <see cref="WebControl"/>: it also indicates if the underlying view is valid and enabled.
    /// <p/>
    /// A <see cref="WebControl"/> is considered invalid when it has been destroyed 
    /// (by either calling <see cref="WebControl.Close"/> or <see cref="WebCore.Shutdown"/>)
    /// or was never properly instantiated.
    /// <p/>
    /// Manually setting the <see cref="UIElement.IsEnabled"/> property to true, will temporarily render 
    /// the control disabled.
    /// <p/>
    /// <note type="inherit">
    /// Inheritors should rely on the <see cref="IsLive"/> property. Accessing <see cref="IsLive"/> also 
    /// updates the value of <see cref="UIElement.IsEnabled"/>.
    /// </note>
    /// <p/>
    /// <note>
    /// When crashed, this control will attempt to recreate its underlying view.
    /// For details, see: <see cref="IsCrashed"/>.
    /// </note>
    /// <p/>
    /// <note type="caution">
    /// While disabled (either because the view is destroyed or because you manually set this property)
    /// attempting to access members of this control, may cause a <see cref="InvalidOperationException"/>
    /// (see the documentation of each member).
    /// </note>
    /// </remarks>
    /// <threadsafety static="true" instance="false" />
    /// <seealso cref="IsLive"/>
    /// <seealso cref="IsCrashed"/>
    #endregion
    [Description( "Represents a WPF control that wraps an Awesomium web view. You can use it to embed Awesomium directly in your WPF application without any additional work." )]
    [System.Drawing.ToolboxBitmap( typeof( WebControl ) )]
    public class WebControl : FrameworkElement, IWebView
    {
        #region Fields
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_CHAR = 0x0102;

        private const int VK_TAB = 0x09;
        private const int VK_END = 0x23;
        private const int VK_HOME = 0x24;

        private static KeyGesture browseBackGesture;

        private HwndSource hwndSource;
        private Matrix deviceTransform;
        private WriteableBitmap bitmap;
        private ToolTip toolTip;
        private Random findRequestRandomizer;
        private WebControlInvalidLayer controlLayer;
        private Boolean needsResize, flushAplha;
        private int resizeWidth, resizeHeight;
        private string lastTitle;
        private bool hookAdded;

        private Dictionary<string, JSCallback> jsObjectCallbackMap;

        private SelectionHelper selectionHelper;

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

        #region JSCallbackCalled
        internal event JSCallback JSCallbackCalled;
        #endregion


        #region IsDirtyChanged
        /// <summary>
        /// Occurs when this <see cref="WebControl"/> needs to be rendered again.
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
        #endregion

        #region BeginLoading
        /// <summary>
        /// This event occurs when a <see cref="WebControl"/> begins loading a new page (first bits of data received from server).
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
        #endregion

        #region BeginNavigation
        /// <summary>
        /// This event occurs when a <see cref="WebControl"/> begins navigating to a new URL.
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
        #endregion

        #region CursorChanged
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
        #endregion

        #region KeyboardFocusChanged
        /// <summary>
        /// This event occurs when keyboard focus changes (usually as a result of a text-box being focused).
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
        #endregion

        #region TargetUrlChanged
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
        #endregion

        #region ToolTipChanged
        /// <summary>
        /// This event occurs when the tooltip text changes.
        /// </summary>
        public event ToolTipChangedEventHandler ToolTipChanged;

        /// <summary>
        /// Raises the <see cref="ToolTipChanged"/> event.
        /// </summary>
        protected virtual void OnToolTipChanged( object sender, ChangeToolTipEventArgs e )
        {
            if ( ToolTipChanged != null )
                ToolTipChanged( sender, e );
        }
        #endregion

        #region DomReady
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
        #endregion

        #region LoadCompleted
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
        #endregion

        #region PageContentsReceived
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
        #endregion

        #region OpenExternalLink
        /// <summary>
        /// This event occurs when an external link is attempted to be opened. An external link
        /// is any link that normally opens in a new window (for example, links with target="_blank", calls
        /// to window.open(), and URL open events from Flash plugins). You are responsible for
        /// creating a new <see cref="WebView"/> or <see cref="WebControl"/> to handle these URLs yourself.
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
        #endregion

        #region PluginCrashed
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
        #endregion

        #region TitleReceived
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
        #endregion

        #region Move
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
        #endregion

        #region Download
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
        #endregion

        #region Crashed
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
        #endregion

        #region SelectLocalFiles
        /// <summary>
        /// This event occurs whenever a page requests a file chooser dialog to be displayed (usually due
        /// to an upload form being clicked by a user). You will need to display your own dialog.
        /// Assign the selected local file(s) to <see cref="SelectLocalFilesEventArgs.SelectedFiles"/>
        /// </summary>
        /// <remarks>
        /// The dialog does not have to be modal; this request is non-blocking. Once a file has been chosen by the user,
        /// you can manually report this back to the view by calling <see cref="WebControl.ChooseFile"/>.
        /// </remarks>
        public event SelectLocalFilesEventHandler SelectLocalFiles;

        /// <summary>
        /// Raises the <see cref="SelectLocalFiles"/> event.
        /// </summary>
        protected virtual void OnSelectLocalFiles( object sender, SelectLocalFilesEventArgs e )
        {
            if ( SelectLocalFiles != null )
                SelectLocalFiles( sender, e );
        }
        #endregion

        #region ScrollDataReceived
        /// <summary>
        /// This event fires in response to <see cref="WebControl.RequestScrollData"/>.
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
        #endregion

        #region JSConsoleMessageAdded
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
        #endregion

        #region FindResultsReceived
        /// <summary>
        /// This event occurs whenever we receive results back from an in-page find operation
        /// (<see cref="WebControl.Find"/>).
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
        #endregion

        #region SelectionChanged
        /// <summary>
        /// This event occurs when the selection in the current page, changes.
        /// </summary>
        public event WebSelectionChangedHandler SelectionChanged;

        /// <summary>
        /// Raises the <see cref="SelectionChanged"/> event.
        /// </summary>
        protected virtual void OnSelectionChanged( object sender, WebSelectionEventArgs e )
        {
            if ( SelectionChanged != null )
                SelectionChanged( sender, e );
        }
        #endregion

        #region ImeUpdated
        /// <summary>
        /// This event occurs whenever the user does something that changes the 
        /// position or visibility of the IME Widget. This event is only active when 
        /// IME is activated (please see <see cref="WebControl.ActivateIME"/>).
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
        #endregion

        #region ResourceRequest
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
        #endregion

        #region ResourceResponse
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

        #endregion


        #region Ctors
        static WebControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( WebControl ), new FrameworkPropertyMetadata( typeof( WebControl ) ) );
            FocusableProperty.OverrideMetadata( typeof( WebControl ), new FrameworkPropertyMetadata( true ) );
            IsEnabledProperty.OverrideMetadata( typeof( WebControl ), new FrameworkPropertyMetadata( null, CoerceIsEnabled ) );

            // We need to remove and restore this when the view gets/loses keyboard focus.
            browseBackGesture = NavigationCommands.BrowseBack.InputGestures.OfType<KeyGesture>().SingleOrDefault( ( kg ) =>
                ( kg.Modifiers == ModifierKeys.None ) && ( kg.Key == Key.Back ) );
        }

        /// <summary>
        /// Creates and initializes an instance of <see cref="WebControl"/> and its underlying web view.
        /// </summary>
        public WebControl()
        {
            if ( this.IsSourceControl )
                this.SetValue( WebControl.IsSourceControlPropertyKey, true );

            controlLayer = new WebControlInvalidLayer( this );
            this.IsEnabledChanged += OnIsEnabledChanged;

            if ( DesignerProperties.GetIsInDesignMode( this ) )
                return;

            toolTip = new ToolTip();
            selectionHelper = new SelectionHelper( this, OnWebSelectionChanged );

            InitializeCore();
            InitializeDelegates( this.Instance );
            InitializeCommandBindings();

            findRequestRandomizer = new Random();

            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;

            needsResize = false;
            flushAplha = true;
        }
        #endregion

        #region Dtor
        /// <summary>
        /// Destroys and removes this web view control. Any call to members of this control
        /// after calling this method, will cause a <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <remarks>
        /// To avoid exceptions, do not call this method when the hosting UI of the control (if any)
        /// is still alive and visible.
        /// </remarks>
        public void Close()
        {
            if ( Instance != IntPtr.Zero )
            {
                if ( hookAdded )
                {
                    if ( hwndSource != null )
                    {
                        hwndSource.RemoveHook( HandleMessages );
                        hookAdded = false;
                    }

                    ComponentDispatcher.ThreadFilterMessage -= OnThreadFilterMessage;
                }

                this.ClearDelegates();

                controlLayer = null;
                toolTip = null;

                this.Loaded -= OnLoaded;
                this.Unloaded -= OnUnloaded;
                this.IsEnabledChanged -= OnIsEnabledChanged;

                WebCore.DestroyView( this );
                Instance = IntPtr.Zero;

                GC.SuppressFinalize( this );
            }
        }

        ~WebControl()
        {
            this.Close();
        }
        #endregion


        #region Overrides

        #region Layer
        /** @name Visual Tree Overrides
         * Visual tree related event triggers overriden from <see cref="FrameworkElement"/> and <see cref="Visual"/>.
         */
        /** @{ */

        /// @internal
        /// <inheritdoc />
        protected override IEnumerator LogicalChildren
        {
            get
            {
                if ( !IsLive && ( controlLayer != null ) )
                    yield return controlLayer;

                yield return null;
            }
        }

        /// @internal
        /// <inheritdoc />
        protected override Visual GetVisualChild( int index )
        {
            return !IsLive && ( controlLayer != null ) ? controlLayer : null;
        }

        /// @internal
        /// <inheritdoc />
        protected override int VisualChildrenCount
        {
            get
            {
                return !IsLive && ( controlLayer != null ) ? 1 : 0;
            }
        }

        /** @} */
        #endregion

        #region Mouse
        /** @name Mouse Overrides
         * Mouse event triggers overriden from <see cref="UIElement"/>.
         */
        /** @{ */

        /// @internal
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

        /// @internal
        /// <inheritdoc />
        protected override void OnPreviewMouseLeftButtonDown( MouseButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();

            // Clear internal selection info before injecting.
            selectionHelper.ClearSelection();

            awe_webview_inject_mouse_down( Instance, MouseButton.Left );
            base.OnPreviewMouseLeftButtonDown( e );
        }

        /// @internal
        /// <inheritdoc />
        protected override void OnPreviewMouseLeftButtonUp( MouseButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_up( Instance, MouseButton.Left );
            base.OnPreviewMouseLeftButtonUp( e );
        }

        /// @internal
        /// <inheritdoc />
        protected override void OnPreviewMouseRightButtonDown( MouseButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_down( Instance, MouseButton.Right );
            base.OnPreviewMouseRightButtonDown( e );
        }

        /// @internal
        /// <inheritdoc />
        protected override void OnMouseRightButtonUp( MouseButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_up( Instance, MouseButton.Right );
            base.OnMouseRightButtonUp( e );
        }

        /// @internal
        /// <inheritdoc />
        protected override void OnPreviewMouseWheel( MouseWheelEventArgs e )
        {
            if ( !IsLive )
                return;

            awe_webview_inject_mouse_wheel( Instance, e.Delta, 0 );
            base.OnPreviewMouseWheel( e );
        }

        /// @internal
        /// <inheritdoc />
        protected override void OnMouseLeave( MouseEventArgs e )
        {
            if ( !IsLive )
                return;

            // Moving mouse capture to child elements such as the context menu,
            // fires a MouseLeave event. The following logic ensures we will
            // inject a mouse leave, only when the mouse really leaves the 
            // surface of the view.
            Point pt = Mouse.GetPosition( this );
            Rect r = new Rect( this.RenderSize );

            if ( !r.Contains( pt ) )
                awe_webview_inject_mouse_move( Instance, -1, -1 );

            base.OnMouseLeave( e );
        }

        /** @} */
        #endregion

        #region Stylus
        /** @name Stylus Overrides
         * Stylus event triggers overriden from <see cref="UIElement"/>.
         */
        /** @{ */

        /// @internal
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

        /// @internal
        /// <inheritdoc />
        protected override void OnPreviewStylusButtonDown( StylusButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_down( Instance, MouseButton.Left );
            base.OnPreviewStylusButtonDown( e );
        }

        /// @internal
        /// <inheritdoc />
        protected override void OnPreviewStylusButtonUp( StylusButtonEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_up( Instance, MouseButton.Left );
            base.OnPreviewStylusButtonUp( e );
        }

        /** @} */
        #endregion

        #region Touch
        /** @name Touch Overrides
         * Touch event triggers overriden from <see cref="UIElement"/>.
         */
        /** @{ */

        /// @internal
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

        /// @internal
        /// <inheritdoc />
        protected override void OnPreviewTouchDown( TouchEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_down( Instance, MouseButton.Left );
            base.OnPreviewTouchDown( e );
        }

        /// @internal
        /// <inheritdoc />
        protected override void OnPreviewTouchUp( TouchEventArgs e )
        {
            if ( !IsLive )
                return;

            this.Focus();
            awe_webview_inject_mouse_up( Instance, MouseButton.Left );
            base.OnPreviewTouchUp( e );
        }

        /** @} */
        #endregion

        #region Focus
        /** @name Focus Overrides
         * Focus event triggers overriden from <see cref="UIElement"/>.
         */
        /** @{ */

        /// @internal
        /// <inheritdoc />
        protected override void OnGotFocus( RoutedEventArgs e )
        {
            if ( !IsLive )
                return;

            FocusView();

            base.OnGotFocus( e );
        }

        /// @internal
        /// <inheritdoc />
        protected override void OnLostFocus( RoutedEventArgs e )
        {
            if ( !IsLive )
                return;

            UnfocusView();
            toolTip.IsOpen = false;

            base.OnLostFocus( e );
        }

        /** @} */
        #endregion

        #region HitTest
        /** @name HitTest Overrides
         * HitTest event triggers overriden from <see cref="UIElement"/>.
         */
        /** @{ */

        /// @internal
        /// <inheritdoc />
        protected override HitTestResult HitTestCore( PointHitTestParameters hitTestParameters )
        {
            // Eliminates the need to paint a background to capture input.
            return new PointHitTestResult( this, Mouse.PrimaryDevice.GetPosition( this ) );
        }

        /** @} */
        #endregion

        #region Measure/Arrange
        /** @name Measure/Arrange Overrides
         * Measure/Arrange event triggers overriden from <see cref="FrameworkElement"/>.
         */
        /** @{ */

        /// @internal
        /// <inheritdoc />
        protected override Size MeasureOverride( Size availableSize )
        {
            if ( controlLayer != null )
                controlLayer.Measure( availableSize );

            var size = base.MeasureOverride( availableSize );

            if ( IsLive )
            {
                deviceTransform = PresentationSource.FromVisual( this ).CompositionTarget.TransformToDevice;

                resizeWidth = (int)( availableSize.Width * deviceTransform.M11 );
                resizeHeight = (int)( availableSize.Height * deviceTransform.M22 );

                needsResize = true;
            }

            return size;
        }

        /// @internal
        /// <inheritdoc />
        protected override Size ArrangeOverride( Size arrangeBounds )
        {
            if ( controlLayer != null )
                controlLayer.Arrange( new Rect( new Point(), arrangeBounds ) );

            if ( IsLive )
                Update();

            return base.ArrangeOverride( arrangeBounds );
        }

        /** @} */
        #endregion

        #region Render
        /** @name Rendering Overrides
         * Rendering event triggers overriden from <see cref="UIElement"/>.
         */
        /** @{ */

        /// @internal
        /// <inheritdoc />
        protected override void OnRender( DrawingContext drawingContext )
        {
            if ( IsLive && ( bitmap != null ) && ( this.ActualWidth > 0 ) && ( this.ActualHeight > 0 ) )
                drawingContext.DrawImage( bitmap, new Rect( new Point(), base.RenderSize ) );
            else
                base.OnRender( drawingContext );
        }

        /** @} */
        #endregion

        #endregion

        #region Methods

        #region Internal

        #region VerifyLive
        private void VerifyLive()
        {
            if ( !IsLive )
                throw new InvalidOperationException( AwesomiumSharp.Resources.ERR_WebControlDisabled );
        }
        #endregion

        #region EnsureView
        private bool EnsureView()
        {
            if ( IsLive )
                return true;

            if ( CanRecreateView )
            {
                try
                {
                    WebCore.DestroyView( this );
                }
                catch { }
                finally
                {
                    Instance = IntPtr.Zero;
                }

                InitializeCore();

                if ( this.Instance != null )
                {
                    this.IsCrashed = false;
                    InitializeDelegates( Instance );
                    return true;
                }
            }

            return false;
        }
        #endregion

        #region InitializeCore
        private void InitializeCore()
        {
            this.Instance = WebCore.CreateWebViewInstance( (int)this.ActualWidth, (int)this.ActualHeight, this, IsSourceControl );
            this.Focus();
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

            if ( jsObjectCallbackMap == null )
                jsObjectCallbackMap = new Dictionary<string, JSCallback>();

            if ( this.JSCallbackCalled == null )
                this.JSCallbackCalled += handleJSCallback;

            selectionHelper.RegisterSelectionHelper();
        }
        #endregion

        #region ClearDelegates
        private void ClearDelegates()
        {
            resourceRequestCallback = null;
            awe_webview_set_callback_resource_request( Instance, null );

            resourceResponseCallback = null;
            awe_webview_set_callback_resource_response( Instance, null );

            beginLoadingCallback = null;
            awe_webview_set_callback_begin_loading( Instance, null );

            beginNavigationCallback = null;
            awe_webview_set_callback_begin_navigation( Instance, null );

            changeCursorCallback = null;
            awe_webview_set_callback_change_cursor( Instance, null );

            changeKeyboardFocusCallback = null;
            awe_webview_set_callback_change_keyboard_focus( Instance, null );

            changeTargetURLCallback = null;
            awe_webview_set_callback_change_target_url( Instance, null );

            changeTooltipCallback = null;
            awe_webview_set_callback_change_tooltip( Instance, null );

            domReadyCallback = null;
            awe_webview_set_callback_dom_ready( Instance, null );

            finishLoadingCallback = null;
            awe_webview_set_callback_finish_loading( Instance, null );

            getFindResultsCallback = null;
            awe_webview_set_callback_get_find_results( Instance, null );

            getPageContentsCallback = null;
            awe_webview_set_callback_get_page_contents( Instance, null );

            getScrollDataCallback = null;
            awe_webview_set_callback_get_scroll_data( Instance, null );

            jsCallback = null;
            awe_webview_set_callback_js_callback( Instance, null );

            jsConsoleMessageCallback = null;
            awe_webview_set_callback_js_console_message( Instance, null );

            openExternalLinkCallback = null;
            awe_webview_set_callback_open_external_link( Instance, null );

            pluginCrashedCallback = null;
            awe_webview_set_callback_plugin_crashed( Instance, null );

            receiveTitleCallback = null;
            awe_webview_set_callback_receive_title( Instance, null );

            requestFileChooserCallback = null;
            awe_webview_set_callback_request_file_chooser( Instance, null );

            requestDownloadCallback = null;
            awe_webview_set_callback_request_download( Instance, null );

            requestMoveCallback = null;
            awe_webview_set_callback_request_move( Instance, null );

            updateIMECallback = null;
            awe_webview_set_callback_update_ime( Instance, null );

            webviewCrashedCallback = null;
            awe_webview_set_callback_web_view_crashed( Instance, null );

            selectionHelper.Dispose();
            selectionHelper = null;

            this.JSCallbackCalled -= handleJSCallback;

            if ( jsObjectCallbackMap != null )
            {
                jsObjectCallbackMap.Clear();
                jsObjectCallbackMap = null;
            }
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
            this.CommandBindings.Add( new CommandBinding( ApplicationCommands.Print, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( NavigationCommands.BrowseHome, OnCommandExecuted, CanExecuteCommand ) );
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
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.CopyHTML, OnCommandExecuted, CanExecuteCommand ) );
            this.CommandBindings.Add( new CommandBinding( WebControlCommands.CopyLinkAddress, OnCommandExecuted, CanExecuteCommand ) );
        }
        #endregion

        #region GetUrl
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_webview_get_url( IntPtr request );

        private string GetUrl()
        {
            return StringHelper.ConvertAweString( awe_webview_get_url( Instance ) );
        }
        #endregion

        #region GetZoom
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static int awe_webview_get_zoom( IntPtr webview );

        private int GetZoom()
        {
            return awe_webview_get_zoom( Instance );
        }
        #endregion


        #region Update
        private void Update()
        {
            if ( needsResize && !awe_webview_is_resizing( Instance ) )
            {
                awe_webview_resize( Instance, resizeWidth, resizeHeight, true, 300 );
                needsResize = false;
            }

            RenderBuffer buffer = Render();

            if ( buffer != null )
            {
                if ( bitmap == null || bitmap.Width != buffer.Width || bitmap.Height != buffer.Height )
                {
                    try
                    {
                        bitmap = new WriteableBitmap(
                            buffer.Width,
                            buffer.Height,
                            96,
                            96,
                            PixelFormats.Bgra32,
                            BitmapPalettes.WebPaletteTransparent );
                    }
                    catch { /* */ }
                    finally
                    {
                        GC.Collect();
                    }
                }

                if ( flushAplha )
                    buffer.FlushAlpha();

                buffer.CopyToBitmap( bitmap );
            }
        }
        #endregion

        #region Render
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_webview_render( IntPtr webview );

        /// <summary>
        /// Renders this <see cref="WebControl"/> into an offscreen pixel buffer and clears the dirty state.
        /// </summary>
        /// <remarks>
        /// For maximum efficiency, you should only call this when the <see cref="WebControl"/> is dirty 
        /// (see <see cref="IsDirty"/>).
        /// The most appropriate time to call this method, is from within your <see cref="IsDirtyChanged"/> handler.
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        /// <returns>
        /// An instance of the <see cref="RenderBuffer"/> that this <see cref="WebControl"/> was rendered to. 
        /// This value may change between renders and may return null if the <see cref="WebControl"/> has crashed
        /// (see <see cref="IsCrashed"/>).
        /// </returns>
        protected RenderBuffer Render()
        {
            return new RenderBuffer( awe_webview_render( Instance ) );
        }
        #endregion

        #region PauseRendering
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_pause_rendering( IntPtr webview );

        /// <summary>
        /// Temporarily pauses internal asynchronous rendering.
        /// </summary>
        /// <remarks>
        /// All rendering is actually done asynchronously in a separate process
        /// and so the page is usually continuously rendering even if you never call
        /// <see cref="WebControl.Render"/>. Call this to temporarily pause rendering.
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        protected void PauseRendering()
        {
            awe_webview_pause_rendering( Instance );
        }
        #endregion

        #region ResumeRendering
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_resume_rendering( IntPtr webview );

        /// <summary>
        /// Resume rendering after a call to <see cref="WebControl.PauseRendering"/>.
        /// </summary>
        /// <remarks>
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        protected void ResumeRendering()
        {
            awe_webview_resume_rendering( Instance );
        }
        #endregion

        #region InjectMouseMove
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_move( IntPtr webview, int x, int y );

        /// <summary>
        /// Injects a mouse-move event in local coordinates.
        /// </summary>
        /// <param name="x">
        /// The absolute x-coordinate of the mouse (relative to the <see cref="WebControl"/> itself).
        /// </param>
        /// <param name="y">
        /// The absolute y-coordinate of the mouse (relative to the <see cref="WebControl"/> itself).
        /// </param>
        /// <remarks>
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        protected void InjectMouseMove( int x, int y )
        {
            awe_webview_inject_mouse_move( Instance, x, y );
        }
        #endregion

        #region InjectMouseDown
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_down( IntPtr webview, MouseButton mouseButton );

        /// <summary>
        /// Injects a mouse-down event.
        /// </summary>
        /// <param name="mouseButton">
        /// The mouse button that was pressed.
        /// </param>
        /// <remarks>
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        protected void InjectMouseDown( MouseButton mouseButton )
        {
            awe_webview_inject_mouse_down( Instance, mouseButton );
        }
        #endregion

        #region InjectMouseUp
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_up( IntPtr webview, MouseButton mouseButton );

        /// <summary>
        /// Injects a mouse-up event.
        /// </summary>
        /// <param name="mouseButton">
        /// The mouse button that was released.
        /// </param>
        /// <remarks>
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        protected void InjectMouseUp( MouseButton mouseButton )
        {
            awe_webview_inject_mouse_up( Instance, mouseButton );
        }
        #endregion

        #region InjectMouseWheel
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_wheel( IntPtr webview, int scroll_amount_vert, int scroll_amount_horz );

        /// <summary>
        /// Injects a mouse-wheel event.
        /// </summary>
        /// <param name="scrollAmountVert">
        /// The relative amount of pixels to scroll vertically.
        /// </param>
        /// <param name="scrollAmountHorz">
        /// The relative amount of pixels to scroll horizontally.
        /// </param>
        /// <remarks>
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        protected void InjectMouseWheel( int scrollAmountVert, int scrollAmountHorz = 0 )
        {
            awe_webview_inject_mouse_wheel( Instance, scrollAmountVert, scrollAmountHorz );
        }
        #endregion

        #region InjectKeyboardEvent
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_keyboard_event( IntPtr webview, WebKeyboardEvent key_event );

        /// <summary>
        /// Injects a keyboard event.
        /// </summary>
        /// <param name="keyEvent">
        /// The keyboard event to inject. You'll need to initialize the members of the passed
        /// <see cref="WebKeyboardEvent"/>, yourself.
        /// </param>
        /// <remarks>
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        protected void InjectKeyboardEvent( WebKeyboardEvent keyEvent )
        {
            awe_webview_inject_keyboard_event( Instance, keyEvent );
        }
        #endregion

        #region InjectKeyboardEventWin
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_keyboard_event_win( IntPtr webview, int msg, int wparam, int lparam );

        /// <summary>
        /// Injects a keyboard event by translating the respective Windows Messages.
        /// </summary>
        /// <param name="msg">
        /// The Windows keyboard message (usually <c>WM_KEYDOWN</c>, <c>WM_KEYUP</c> and <c>WM_CHAR</c>). 
        /// </param>
        /// <param name="wparam">
        /// The first parameter of the message as intercepted by the window procedure.
        /// </param>
        /// <param name="lparam">
        /// The second parameter of the message as intercepted by the window procedure.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        /// <remarks>
        /// This is usually easier to use than <see cref="InjectKeyboardEvent"/>. All you have to
        /// do is hook into the window procedure of this view's host, intercept <c>WM_KEYDOWN</c>, 
        /// <c>WM_KEYUP</c> and <c>WM_CHAR</c> and inject them to the view by using this method.
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="tip">
        /// Beware that in WPF, only the parent Window has a window procedure. Make sure
        /// that you only inject messages when the actual host (if it's a child element)
        /// has the focus, and that you do not hook into the same procedure multiple times.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
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
        /// Resizes this <see cref="WebControl"/> to certain dimensions. This operation can fail
        /// if another resize is already pending (see <see cref="WebControl.IsResizing"/>) or if
        /// the repaint timeout was exceeded.
        /// </summary>
        /// <param name="width">
        /// The width in pixels to resize to.
        /// </param>
        /// <param name="height">
        /// The height in pixels to resize to.
        /// </param>
        /// <param name="waitForRepaint">
        /// Whether or not to wait for the <see cref="WebControl"/> to finish repainting to avoid flicker
        /// (default is true).
        /// </param>
        /// <param name="repaintTimeoutMs">
        /// The max amount of time to wait for a repaint, in milliseconds.
        /// </param>
        /// <returns>
        /// True if the resize was successful. False otherwise.
        /// </returns>
        /// <remarks>
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        protected bool Resize( int width, int height, bool waitForRepaint = true, int repaintTimeoutMs = 300 )
        {
            return awe_webview_resize( Instance, width, height, waitForRepaint, repaintTimeoutMs );
        }
        #endregion

        #region UnfocusView
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_unfocus( IntPtr webview );

        /// <summary>
        /// Notifies the current page that it has lost focus.
        /// </summary>
        /// <remarks>
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        protected void UnfocusView()
        {
            awe_webview_unfocus( Instance );
        }
        #endregion

        #region FocusView
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_focus( IntPtr webview );

        /// <summary>
        /// Notifies the current page that it has gained focus.
        /// </summary>
        /// <remarks>
        /// You will need to call this to gain text-box focus, among other things. 
        /// (If you fail to ever see a blinking caret when typing text, this is why.)
        /// </remarks>
        /// <remarks>
        /// <note type="inherit">
        /// <see cref="WebControl"/> handles this internally. Inheritors do not need to call this method unless
        /// they implement custom logic.
        /// </note>
        /// <note type="caution">
        /// For performance reasons, no validity check is performed when calling protected members.
        /// Inheritors should perform any such checks (see <see cref="IsLive"/>), before calling these members.
        /// </note>
        /// </remarks>
        protected void FocusView()
        {
            awe_webview_focus( Instance );
        }
        #endregion

        #endregion

        #region Public

        #region GoToHome
        /// <summary>
        /// Navigates to the Home URL as defined in <see cref="WebCore.HomeURL"/>.
        /// </summary>
        /// <returns>
        /// True if the view is alive and the command was successfully sent. False otherwise.
        /// </returns>
        public bool GoToHome()
        {
            return this.LoadURL( WebCore.HomeURL );
        }
        #endregion

        #region LoadURL
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_resource_interceptor(/*To do?*/);

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_load_url( IntPtr webview, IntPtr url, IntPtr frame_name, IntPtr username, IntPtr password );

        /// <summary>
        /// Loads a URL into the <see cref="WebControl"/> asynchronously.
        /// </summary>
        /// <param name="url">
        /// The URL to load.
        /// </param>
        /// <param name="frameName">
        /// The name of the frame to load the URL in; leave this blank to load in the main frame.
        /// </param>
        /// <param name="username">
        /// If the URL requires authentication, the username to authorize as, otherwise just pass an empty string.
        /// </param>
        /// <param name="password">
        /// If the URL requires authentication, the password to use, otherwise just pass an empty string.
        /// </param>
        /// <returns>
        /// True if the view is alive and the command was successfully sent. False otherwise.
        /// </returns>
        public bool LoadURL( string url, string frameName = "", string username = "", string password = "" )
        {
            if ( !EnsureView() )
                return false;

            StringHelper urlStr = new StringHelper( url );
            StringHelper frameNameStr = new StringHelper( frameName );
            StringHelper usernameStr = new StringHelper( username );
            StringHelper passwordStr = new StringHelper( password );

            awe_webview_load_url( Instance, urlStr.Value, frameNameStr.Value, usernameStr.Value, passwordStr.Value );

            return true;
        }
        #endregion

        #region LoadHTML
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_load_html( IntPtr webview, IntPtr html, IntPtr frame_name );

        /// <summary>
        /// Loads a string of HTML into the <see cref="WebControl"/> asynchronously.
        /// </summary>
        /// <param name="html">
        /// The HTML string (ASCII) to load.
        /// </param>
        /// <param name="frameName">
        /// The name of the frame to load the HTML in.
        /// </param>
        /// <returns>
        /// True if the view is alive and the command was successfully sent. False otherwise.
        /// </returns>
        /// <remarks>
        /// Any assets required by the specified HTML (images etc.), should exist 
        /// within the base directory set with <see cref="WebCore.SetBaseDirectory"/>.
        /// </remarks>
        public bool LoadHTML( string html, string frameName = "" )
        {
            if ( !EnsureView() )
                return false;

            StringHelper htmlStr = new StringHelper( html );
            StringHelper frameNameStr = new StringHelper( frameName );

            awe_webview_load_html( Instance, htmlStr.Value, frameNameStr.Value );

            return true;
        }
        #endregion

        #region LoadFile
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_load_file( IntPtr webview, IntPtr file, IntPtr frame_name );

        /// <summary>
        /// Loads a local file into the <see cref="WebControl"/> asynchronously.
        /// </summary>
        /// <param name="file">
        /// The name of the file to load.
        /// </param>
        /// <param name="frameName">
        /// The name of the frame to load the file in.
        /// </param>
        /// <returns>
        /// True if the view is alive and the command was successfully sent. False otherwise.
        /// </returns>
        /// <remarks>
        /// <note>
        /// The file should exist within the base directory set with <see cref="WebCore.SetBaseDirectory"/>.
        /// </note>
        /// </remarks>
        public bool LoadFile( string file, string frameName = "" )
        {
            if ( !EnsureView() )
                return false;

            StringHelper fileStr = new StringHelper( file );
            StringHelper frameNameStr = new StringHelper( frameName );

            awe_webview_load_file( Instance, fileStr.Value, frameNameStr.Value );

            return true;
        }
        #endregion

        #region GoToHistoryOffset
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_go_to_history_offset( IntPtr webview, int offset );

        /// <summary>
        /// Navigates back/forward in history via a relative offset.
        /// </summary>
        /// <param name="offset">
        /// The relative offset in history to navigate to. (Can be negative)
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void GoToHistoryOffset( int offset )
        {
            VerifyLive();
            awe_webview_go_to_history_offset( Instance, offset );
        }
        #endregion

        #region GoBack
        /// <summary>
        /// Navigates one step back in history.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void GoBack()
        {
            GoToHistoryOffset( -1 );
        }
        #endregion

        #region GoForward
        /// <summary>
        /// Navigates one step forward in history.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void GoForward()
        {
            GoToHistoryOffset( 1 );
        }
        #endregion

        #region Stop
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_stop( IntPtr webview );

        /// <summary>
        /// Stops the current navigation.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void Stop()
        {
            VerifyLive();
            awe_webview_stop( Instance );

            // If we had already received a title, lastTitle refers to the new.
            // If not, restore the one of the previous page.
            this.Title = lastTitle;
        }
        #endregion

        #region Reload
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_reload( IntPtr webview );

        /// <summary>
        /// Reloads the current page.
        /// </summary>
        /// <returns>
        /// True if the view is alive and the command was successfully sent. False otherwise.
        /// </returns>
        public bool Reload()
        {
            if ( IsLive )
                awe_webview_reload( Instance );
            else
                return this.LoadURL( Source.ToString() );

            return true;
        }
        #endregion

        #region ExecuteJavascript
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_execute_javascript( IntPtr webview, IntPtr javascript, IntPtr frame_name );

        /// <summary>
        /// Executes a string of Javascript in the context of the current page
        /// asynchronously.
        /// </summary>
        /// <param name="javascript">The string of Javascript to execute.</param>
        /// <param name="frameName">Optional; the name of the frame to execute in,
        /// leave this blank to execute in the main frame.</param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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
        /// synchronously, and returns the result.
        /// </summary>
        /// <param name="javascript">The string of Javascript to execute.</param>
        /// <param name="frameName">Optional; the name of the frame to execute in,
        /// leave this blank to execute in the main frame.</param>
        /// <param name="timeoutMs">Optional; the maximum time to wait for the result
        /// to be computed. Leave this 0 to wait forever (may hang if Javascript is 
        /// invalid!)</param>
        /// <returns>Returns the result as a <see cref="JSValue"/>. Please note that the returned
        /// result is only a shallow, read-only copy of the original object. This
        /// method does not return system-defined Javascript objects (such as "window",
        /// "document", or any DOM elements).</returns>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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

        /// <summary>
        /// Calls a certain function defined in Javascript, directly.
        /// </summary>
        /// <param name="objectName">
        /// The name of the object that contains the function, pass an empty string if the function is defined in the global scope.
        /// </param>
        /// <param name="function">
        /// The name of the function.
        /// </param>
        /// <param name="frameName">
        /// Optional; the name of the frame to execute in,
        /// leave this blank to execute in the main frame.
        /// </param>
        /// <param name="arguments">
        /// The arguments to pass to the function.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void CallJavascriptFunction( string objectName, string function, string frameName = "", params JSValue[] arguments )
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
        /// the lifetime of this <see cref="WebControl"/>. This is useful for exposing your application's
        /// data and events to Javascript. This object is managed directly by Awesomium
        /// so you can modify its properties and bind callback functions via
        /// <see cref="WebControl.SetObjectProperty"/> and <see cref="WebControl.SetObjectCallback"/>, 
        /// respectively.
        /// </summary>
        /// <param name="objectName">
        /// The name of the object.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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

        /// <summary>
        /// Destroys a Javascript object previously created with <see cref="WebControl.CreateObject"/>.
        /// </summary>
        /// <param name="objectName">
        /// The name of the object to destroy.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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
        /// Sets a property of a Javascript object previously created with <see cref="WebControl.CreateObject"/>.
        /// </summary>
        /// <example>
        /// An example of usage:
        /// <code>
        /// webView.CreateObject("MyObject");
        /// webView.SetObjectProperty("MyObject", "color", "blue");
        /// 
        /// // You can now access this object's property via Javascript on any 
        /// // page loaded into this WebControl:
        /// var myColor = MyObject.color; // value would be "blue"
        /// </code>
        /// </example>
        /// <param name="objectName">
        /// The name of the Javascript object.
        /// </param>
        /// <param name="propertyName">
        /// The name of the property to create.
        /// </param>
        /// <param name="val">
        /// The initial javascript-value of the property.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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
        /// Binds a callback function to a Javascript object previously created with <see cref="WebControl.CreateObject"/>.
        /// This is very useful for passing events from Javascript to your application.
        /// </summary>
        /// <example>
        /// An example of usage:
        /// <code>
        /// public void OnSelectItem(object sender, JSCallbackEventArgs e)
        /// {
        ///     System.Console.WriteLine( "Player selected item: " + e.args[0].ToString() );
        /// }
        /// 
        /// public void initWebControl()
        /// {
        ///     webView.CreateObject("MyObject");
        ///     webView.SetObjectCallback("MyObject", "SelectItem", OnSelectItem);
        /// }
        /// 
        /// // You can now call the function "OnSelectItem" from Javascript:
        /// MyObject.SelectItem("shotgun");
        /// </code>
        /// </example>
        /// <param name="objectName">
        /// The name of the Javascript object.
        /// </param>
        /// <param name="callbackName">
        /// The name of the Javascript function that will call the callback.
        /// </param>
        /// <param name="callback">
        /// Reference to a <see cref="JSCallback"/> implementation.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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

        #region Cut
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_cut( IntPtr webview );

        /// <summary>
        /// Cuts the text currently selected in this <see cref="WebControl"/>, when it has keyboard focus
        /// (usually in a text-box), using the system clipboard.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void Cut()
        {
            VerifyLive();
            awe_webview_cut( Instance );
        }
        #endregion

        #region Copy
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_copy( IntPtr webview );

        /// <summary>
        /// Copies the text currently selected in this <see cref="WebControl"/>, to the system clipboard.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void Copy()
        {
            VerifyLive();
            awe_webview_copy( Instance );
        }
        #endregion

        #region CopyHTML
        /// <summary>
        /// Copies the HTML content currently selected in this <see cref="WebControl"/>, to the system clipboard.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void CopyHTML()
        {
            VerifyLive();

            if ( HasSelection )
                Clipboard.SetText( this.Selection.HTML );
        }
        #endregion

        #region CopyLinkAddress
        /// <summary>
        /// Copies the target URL of the link currently under the cursor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void CopyLinkAddress()
        {
            VerifyLive();

            if ( HasTargetURL )
                Clipboard.SetText( this.TargetURL );
        }
        #endregion

        #region Paste
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_paste( IntPtr webview );

        /// <summary>
        /// Pastes the text currently in the system clipboard, to this <see cref="WebControl"/>,
        /// when it has keyboard focus (usually in a text-box).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void Paste()
        {
            VerifyLive();
            awe_webview_paste( Instance );
        }
        #endregion

        #region SelectAll
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_select_all( IntPtr webview );

        /// <summary>
        /// Selects all content on the current page.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void SelectAll()
        {
            VerifyLive();
            awe_webview_select_all( Instance );
        }
        #endregion

        #region ResetZoom
        /// <summary>
        /// Resets the zoom level.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void ResetZoom()
        {
            VerifyLive();
            this.ClearValue( WebControl.ZoomProperty );
        }
        #endregion

        #region GetZoomForHost
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static int awe_webview_get_zoom_for_host( IntPtr webview, IntPtr host );

        /// <summary>
        /// Gets the zoom factor (percent of page) saved for a certain hostname.
        /// </summary>
        /// <param name="host">
        /// The hostname whose saved zoom setting will be retrieved.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void GetZoomForHost( string host )
        {
            VerifyLive();
            StringHelper hostStr = new StringHelper( host );
            awe_webview_choose_file( Instance, hostStr.Value );
        }
        #endregion

        #region SetTransparent
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_transparent( IntPtr webview, bool is_transparent );

        /// <summary>
        /// Sets whether or not pages should be rendered with transparency
        /// preserved (for ex, for pages with style="background-color: transparent;")
        /// </summary>
        /// <param name="isTransparent">
        /// Whether or not this <see cref="WebControl"/> is transparent.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void SetTransparent( bool isTransparent )
        {
            VerifyLive();
            awe_webview_set_transparent( Instance, isTransparent );
        }
        #endregion

        #region SetURLFilteringMode
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_url_filtering_mode( IntPtr webview, URLFilteringMode filteringMode );

        /// <summary>
        /// Sets the current URL Filtering Mode to use.
        /// </summary>
        /// <param name="filteringMode">
        /// The URL Filtering Mode to use. Default is <see cref="URLFilteringMode.None"/>.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void SetURLFilteringMode( URLFilteringMode filteringMode )
        {
            VerifyLive();
            awe_webview_set_url_filtering_mode( Instance, filteringMode );
        }
        #endregion

        #region AddURLFilter
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_add_url_filter( IntPtr webview, IntPtr filter );

        /// <summary>
        /// Adds a new URL Filter rule.
        /// </summary>
        /// <param name="filter">
        /// A string with optional wildcards that describes a certain URL.
        /// </param>
        /// <example>
        /// For example, to match all URLs from the domain "google.com", your filter string can be: http://google.com/*
        /// </example>
        /// <remarks> 
        /// You may also use the "local://" scheme prefix to describe the URL to the base directory
        /// (set via <see cref="WebCore.SetBaseDirectory"/>).
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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

        /// <summary>
        /// Clears all URL Filter rules previously added with <see cref="AddURLFilter"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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

        /// <summary>
        /// Defines a new Header Definition or updates it if it already exists.
        /// </summary>
        /// <param name="name">
        /// The unique name of the Header Definition; this is used to refer to it later in <see cref="AddHeaderRewriteRule"/> and related methods.
        /// </param>
        /// <param name="fields">
        /// A name/value collection representing field names and their respective values.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void SetHeaderDefinition( string name, NameValueCollection fields )
        {
            VerifyLive();

            StringHelper nameStr = new StringHelper( name );

            int count = fields.Count;
            IntPtr[] keys = new IntPtr[ count ];
            IntPtr[] values = new IntPtr[ count ];

            for ( int i = 0; i < count; i++ )
            {
                keys[ i ] = StringHelper.GetAweString( fields.GetKey( i ) );
                values[ i ] = StringHelper.GetAweString( fields.Get( i ) );
            }

            awe_webview_set_header_definition( Instance, nameStr.Value, (uint)count, keys, values );

            for ( uint i = 0; i < count; i++ )
            {
                StringHelper.DestroyAweString( keys[ i ] );
                StringHelper.DestroyAweString( values[ i ] );
            }
        }
        #endregion

        #region AddHeaderRewriteRule
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_add_header_rewrite_rule( IntPtr webview, IntPtr rule, IntPtr name );

        /// <summary>
        /// Adds a new a header re-write rule. 
        /// All requests whose URL matches the specified rule will have its HTTP headers re-written 
        /// with the specified header definition before sending it to the server.
        /// </summary>
        /// <param name="rule">
        /// A string with optional wildcards (*, ?) that matches the URL(s) that will have their headers 
        /// re-written with the specified header definition.
        /// </param>
        /// <param name="name">
        /// The name of the header definition (specified in <see cref="SetHeaderDefinition"/>).
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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

        /// <summary>
        /// Removes a header re-write rule previously added with <see cref="AddHeaderRewriteRule"/>.
        /// </summary>
        /// <param name="rule">
        /// The rule to remove (should match the string specified in for the "rule" parameter 
        /// in <see cref="AddHeaderRewriteRule"/>, exactly).
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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

        /// <summary>
        /// Removes all header re-write rules that are using a certain header definition.
        /// </summary>
        /// <param name="name">
        /// The name of the header definition (specified in <see cref="WebControl.SetHeaderDefinition"/>).
        /// Specify an empty string, to remove all header re-write rules.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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

        /// <summary>
        /// Call this in response to a <see cref="SelectLocalFiles"/> event,
        /// to indicate the file to be uploaded.
        /// </summary>
        /// <param name="filePath">
        /// The full path to the file that was selected.
        /// </param>
        /// <remarks>
        /// Alternatively, if you opened a modal dialog from within your <see cref="SelectLocalFiles"/> handler,
        /// you can define the files to be uploaded by using the <see cref="SelectLocalFilesEventArgs.SelectedFiles"/>
        /// property.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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
        /// Prints the current page.
        /// </summary>
        /// <remarks>
        /// To suppress the printer selection dialog
        /// and print immediately using OS defaults, 
        /// see <see cref="WebCore.SuppressPrinterDialog"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void Print()
        {
            VerifyLive();
            awe_webview_print( Instance );
        }
        #endregion

        #region RequestScrollData
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_request_scroll_data( IntPtr webview, IntPtr frame_name );

        /// <summary>
        /// Request the page dimensions and scroll position of the page.
        /// </summary>
        /// <remarks>
        /// You can retrieve the response by handling the <see cref="ScrollDataReceived"/> event.
        /// </remarks>
        /// <param name="frameName">
        /// The frame's scroll data to retrieve. Leave blank to get the main frame's scroll data.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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
        /// Start finding a certain string on the current web-page.
        /// </summary>
        /// <remarks>
        /// All matches of the string will be highlighted on the page and you can jump to different 
        /// instances of the string by using the <see cref="FindNext"/> method.
        /// To get actual stats about a certain query, please see <see cref="FindResultsReceived"/>.
        /// </remarks>
        /// <param name="searchStr">
        /// The string to search for.
        /// </param>
        /// <param name="forward">
        /// True to search forward, down the page. False otherwise. The default is true.
        /// </param>
        /// <param name="caseSensitive">
        /// True to perform a case sensitive search. False otherwise. The default is false.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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
        /// True to search forward, down the page. False otherwise. The default is true.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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
        /// Stops the last active search (started with <see cref="WebControl.Find"/>).
        /// </summary>
        /// <remarks>
        /// This will un-highlight all matches of a previous call to <see cref="WebControl.Find"/>.
        /// </remarks>
        /// <param name="clearSelection">
        /// True to also deselect the currently selected string. False otherwise.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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
        /// Attempt automatic translation of the current page via Google Translate.
        /// </summary>
        /// <remarks>
        /// The defined language codes are ISO 639-2.
        /// </remarks>
        /// <param name="sourceLanguage">
        /// The language to translate from (for ex. "en" for English).
        /// </param>
        /// <param name="targetLanguage">
        /// The language to translate to (for ex. "fr" for French).
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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
        /// Call this method to let the <see cref="WebControl"/> know you will be passing
        /// text input via IME and will need to be notified of any IME-related
        /// events (such as caret position, user un-focusing text-box, etc.).
        /// </summary>
        /// <param name="activate">
        /// True to activate IME support. False otherwise.
        /// </param>
        /// <seealso cref="WebControl.ImeUpdated"/>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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
        /// Create or update the current IME text composition.
        /// </summary>
        /// <param name="inputStr">The string generated by your IME.</param>
        /// <param name="cursorPos">The current cursor position in your IME composition.</param>
        /// <param name="targetStart">The position of the beginning of the selection.</param>
        /// <param name="targetEnd">The position of the end of the selection.</param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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

        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
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

        /// <summary>
        /// Cancels IME text composition.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebControl"/> instance
        /// (see <see cref="UIElement.IsEnabled"/>).
        /// </exception>
        public void CancelIMEComposition()
        {
            VerifyLive();
            awe_webview_cancel_ime_composition( Instance );
        }
        #endregion

        #endregion

        #endregion

        #region Properties

        #region Internal
        internal IntPtr Instance { get; private set; }

        /// <summary>
        /// Gets if the control is live and the view is instantiated.
        /// </summary>
        protected bool IsLive
        {
            get
            {
                this.CoerceValue( WebControl.IsEnabledProperty );
                return IsEnabled;
            }
        }

        /// <summary>
        /// Gets if this control wraps a previously crashed
        /// view that can be recreated.
        /// </summary>
        internal bool CanRecreateView
        {
            get
            {
                return ( Instance != IntPtr.Zero ) &&
                    !DesignerProperties.GetIsInDesignMode( this ) &&
                    IsCrashed;
            }
        }

        /// <summary>
        /// Gets if this control displays the HTML source of any web-page loaded.
        /// </summary>
        internal virtual bool IsSourceControl
        {
            get { return false; }
        }

        private static readonly DependencyPropertyKey IsSourceControlPropertyKey =
            DependencyProperty.RegisterReadOnly( "IsSourceControl",
            typeof( bool ), typeof( WebControl ),
            new FrameworkPropertyMetadata( false ) );

        /// <summary>
        /// Identifies the <see cref="IsSourceControl"/> dependency property.
        /// </summary>
        internal static readonly DependencyProperty IsSourceControlProperty =
            IsSourceControlPropertyKey.DependencyProperty;
        #endregion

        #region Static
        /** @name Static Resource Keys
         * Recource keys of the context menu of a <see cref="WebControl"/> and its items.
         */
        /** @{ */

        #region ContextMenuResourceKey
        private static ComponentResourceKey contextMenuKey;
        /// <summary>
        /// Gets the resource key for the context menu of a <see cref="WebControl"/>.
        /// </summary>
        /// <remarks>
        /// This can be used to override the default context menu.
        /// </remarks>
        public static ComponentResourceKey ContextMenuResourceKey
        {
            get
            {
                if ( contextMenuKey == null )
                    contextMenuKey = new ComponentResourceKey( typeof( WebControl ), "WebControlContextMenu" );

                return contextMenuKey;
            }
        }
        #endregion

        #region ContextMenuPageItemsArrayRecourceKey
        private static ComponentResourceKey contextMenuPageItemsArrayRecourceKey;

        /// <summary>
        /// Gets the resource key for an array of items in the context menu of a 
        /// <see cref="WebControl"/>, that are visible when you right-click on a 
        /// page that has no selection and no keyboard focus.
        /// </summary>
        /// <remarks>
        /// <para>
        /// You can use this resource key to override the items in the context menu of a 
        /// <see cref="WebControl"/>, that are visible when you right-click on a 
        /// page that has no selection and no keyboard focus.
        /// </para>
        /// <note>
        /// If you only wish to add items to the predefined ones, keep in mind that you
        /// have to redefine all the items of the array.
        /// </note>
        /// </remarks>
        /// <example>
        /// The default-predefined array in XAML is:
        /// <code lang="XAML">
        /// <![CDATA[
        /// <x:Array x:Key="{x:Static local:WebControl.ContextMenuPageItemsArrayRecourceKey}" Type="{x:Type DependencyObject}">
        ///     <MenuItem Command="BrowseBack" CommandTarget="{Binding}" />
        ///     <MenuItem Command="BrowseForward" CommandTarget="{Binding}" />        
        ///     <MenuItem Command="Refresh" CommandTarget="{Binding}" />
        ///     <Separator />
        ///     <MenuItem Command="Print" CommandTarget="{Binding}" />
        /// </x:Array>
        /// ]]>
        /// </code>
        /// </example>
        public static ComponentResourceKey ContextMenuPageItemsArrayRecourceKey
        {
            get
            {
                if ( contextMenuPageItemsArrayRecourceKey == null )
                    contextMenuPageItemsArrayRecourceKey = new ComponentResourceKey( typeof( WebControl ), "ContextMenuPageItemsArray" );

                return contextMenuPageItemsArrayRecourceKey;
            }
        }
        #endregion

        #region ContextMenuInputItemsArrayRecourceKey
        private static ComponentResourceKey contextMenuInputItemsArrayRecourceKey;

        /// <summary>
        /// Gets the resource key for an array of items in the context menu of a 
        /// <see cref="WebControl"/>, that are visible when the control has keyboard focus.
        /// </summary>
        /// <remarks>
        /// <para>
        /// You can use this resource key to override the items in the context menu of a 
        /// <see cref="WebControl"/>, that are visible when the control has keyboard focus.
        /// </para>
        /// <note>
        /// If you only wish to add items to the predefined ones, keep in mind that you
        /// have to redefine all the items of the array.
        /// </note>
        /// </remarks>
        /// <example>
        /// The default-predefined array in XAML is:
        /// <code lang="XAML">
        /// <![CDATA[
        /// <x:Array x:Key="{x:Static local:WebControl.ContextMenuInputItemsArrayRecourceKey}" Type="{x:Type DependencyObject}">
        ///     <MenuItem Command="Copy" CommandTarget="{Binding}" />
        ///     <MenuItem Command="Cut" CommandTarget="{Binding}" />        
        ///     <MenuItem Command="Paste" CommandTarget="{Binding}" />
        /// </x:Array>
        /// ]]>
        /// </code>
        /// </example>
        public static ComponentResourceKey ContextMenuInputItemsArrayRecourceKey
        {
            get
            {
                if ( contextMenuInputItemsArrayRecourceKey == null )
                    contextMenuInputItemsArrayRecourceKey = new ComponentResourceKey( typeof( WebControl ), "ContextMenuInputItemsArray" );

                return contextMenuInputItemsArrayRecourceKey;
            }
        }
        #endregion

        #region ContextMenuSelectionItemsArrayRecourceKey
        private static ComponentResourceKey contextMenuSelectionItemsArrayRecourceKey;

        /// <summary>
        /// Gets the resource key for an array of items in the context menu of a 
        /// <see cref="WebControl"/>, that are visible when a range of content in the page 
        /// is currently selected.
        /// </summary>
        /// <remarks>
        /// <para>
        /// You can use this resource key to override the items in the context menu of a 
        /// <see cref="WebControl"/>, that are visible when a range of content in the page 
        /// is currently selected.
        /// </para>
        /// <note>
        /// If you only wish to add items to the predefined ones, keep in mind that you
        /// have to redefine all the items of the array.
        /// </note>
        /// </remarks>
        /// <example>
        /// The default-predefined array in XAML is:
        /// <code lang="XAML">
        /// <![CDATA[
        /// <x:Array x:Key="{x:Static local:WebControl.ContextMenuSelectionItemsArrayRecourceKey}" Type="{x:Type DependencyObject}">
        ///     <MenuItem Command="Copy" CommandTarget="{Binding}" />
        ///     <MenuItem Command="{x:Static local:WebControlCommands.CopyHTML}" CommandTarget="{Binding}" />
        ///     <Separator />
        /// </x:Array>
        /// ]]>
        /// </code>
        /// </example>
        public static ComponentResourceKey ContextMenuSelectionItemsArrayRecourceKey
        {
            get
            {
                if ( contextMenuSelectionItemsArrayRecourceKey == null )
                    contextMenuSelectionItemsArrayRecourceKey = new ComponentResourceKey( typeof( WebControl ), "ContextMenuSelectionItemsArray" );

                return contextMenuSelectionItemsArrayRecourceKey;
            }
        }
        #endregion

        #region ContextMenuLinkItemsArrayRecourceKey
        private static ComponentResourceKey contextMenuLinkItemsArrayRecourceKey;

        /// <summary>
        /// Gets the resource key for an array of items in the context menu of a 
        /// <see cref="WebControl"/>, that are visible when you right-click on a link in a page.
        /// </summary>
        /// <remarks>
        /// <para>
        /// You can use this resource key to override the items in the context menu of a 
        /// <see cref="WebControl"/>, that are visible when you right-click on a link in a page.
        /// </para>
        /// <note>
        /// If you only wish to add items to the predefined ones, keep in mind that you
        /// have to redefine all the items of the array.
        /// </note>
        /// </remarks>
        /// <example>
        /// The default-predefined array in XAML is:
        /// <code lang="XAML">
        /// <![CDATA[
        /// <x:Array x:Key="{x:Static local:WebControl.ContextMenuLinkItemsArrayRecourceKey}" Type="{x:Type DependencyObject}">
        ///     <MenuItem Command="{x:Static local:WebControlCommands.CopyLinkAddress}" CommandTarget="{Binding}" />
        ///     <Separator />
        /// </x:Array>
        /// ]]>
        /// </code>
        /// </example>
        public static ComponentResourceKey ContextMenuLinkItemsArrayRecourceKey
        {
            get
            {
                if ( contextMenuLinkItemsArrayRecourceKey == null )
                    contextMenuLinkItemsArrayRecourceKey = new ComponentResourceKey( typeof( WebControl ), "ContextMenuLinkItemsArray" );

                return contextMenuLinkItemsArrayRecourceKey;
            }
        }
        #endregion

        /** @} */
        #endregion


        #region IsDirty
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_webview_is_dirty( IntPtr webview );

        /// <summary>
        /// Gets if this <see cref="WebControl"/> needs to be rendered again.
        /// </summary>
        /// <remarks>
        /// Internal changes to this property fire the <see cref="IsDirtyChanged"/>
        /// and <see cref="INotifyPropertyChanged.PropertyChanged"/> events,
        /// only if <see cref="WebCore.IsAutoUpdateEnabled"/> is true.
        /// </remarks>
        /// <seealso cref="WebControl.IsDirtyChanged"/>
        /// <seealso cref="WebCore.Update"/>
        public bool IsDirty
        {
            get { return (bool)this.GetValue( WebControl.IsDirtyProperty ); }
            internal set
            {
                if ( !IsLive )
                    return;

                SetValue( WebControl.IsDirtyPropertyKey, value );

                // Insist on firing while True.
                if ( value )
                {
                    OnPropertyChanged( new DependencyPropertyChangedEventArgs( WebControl.IsDirtyProperty, false, true ) );
                    CoerceValue( WebControl.DirtyBoundsProperty );
                    Update();
                    OnIsDirtyChanged( this, EventArgs.Empty );
                }
            }
        }

        private static readonly DependencyPropertyKey IsDirtyPropertyKey =
                                DependencyProperty.RegisterReadOnly( "IsDirty",
                                typeof( bool ), typeof( WebControl ),
                                new FrameworkPropertyMetadata( false, IsDirtyPropChanged ) );

        /// <summary>
        /// Identifies the <see cref="IsDirty"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDirtyProperty =
            IsDirtyPropertyKey.DependencyProperty;

        private static void IsDirtyPropChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            WebControl owner = (WebControl)d;
            bool value = (bool)e.NewValue;

            if ( owner.IsLive && !value )
            {
                owner.CoerceValue( WebControl.DirtyBoundsProperty );
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
        /// Gets if there is a resize operation pending.
        /// </summary>
        /// <returns>
        /// True if we are waiting for the <see cref="WebControl"/> to
        /// return acknowledgment of a pending resize operation. False otherwise.
        /// </returns>
        public bool IsResizing
        {
            get { return (bool)this.GetValue( WebControl.IsResizingProperty ); }
        }

        private static readonly DependencyPropertyKey IsResizingPropertyKey =
                                DependencyProperty.RegisterReadOnly( "IsResizing",
                                typeof( bool ), typeof( WebControl ),
                                new FrameworkPropertyMetadata( false, null, CoerceIsResizing ) );

        /// <summary>
        /// Identifies the <see cref="IsResizing"/> dependency property.
        /// </summary>
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

        /// <summary>
        /// Gets if a page is currently loading in the <see cref="WebControl"/>.
        /// </summary>
        public bool IsLoadingPage
        {
            get { return (bool)this.GetValue( WebControl.IsLoadingPageProperty ); }
        }

        private static readonly DependencyPropertyKey IsLoadingPagePropertyKey =
                                DependencyProperty.RegisterReadOnly( "IsLoadingPage",
                                typeof( bool ), typeof( WebControl ),
                                new FrameworkPropertyMetadata( false, null, CoerceIsLoadingPage ) );

        /// <summary>
        /// Identifies the <see cref="IsLoadingPage"/> dependency property.
        /// </summary>
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

        #region IsNavigating
        /// <summary>
        /// Gets if the <see cref="WebControl"/> is currently navigating to a Url.
        /// </summary>
        /// <remarks>
        /// Unlike <see cref="IsLoadingPage"/> that updates when the actual contents
        /// of a page are being downloaded, this property is updated when navigation
        /// starts and updates again when loading completes.
        /// </remarks>
        public bool IsNavigating
        {
            get { return (bool)this.GetValue( WebControl.IsNavigatingProperty ); }
            protected set { SetValue( WebControl.IsNavigatingPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey IsNavigatingPropertyKey =
                                DependencyProperty.RegisterReadOnly( "IsNavigating",
                                typeof( bool ), typeof( WebControl ),
                                new FrameworkPropertyMetadata( false ) );

        /// <summary>
        /// Identifies the <see cref="IsNavigating"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsNavigatingProperty =
            IsNavigatingPropertyKey.DependencyProperty;
        #endregion

        #region DirtyBounds
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern AweRect awe_webview_get_dirty_bounds( IntPtr webview );

        /// <summary>
        /// Gets the bounds of the area that has changed since the last rendering.
        /// </summary>
        /// <returns>
        /// An <see cref="AweRect"/> representing the bounds of the area that has changed 
        /// since the last rendering.
        /// </returns>
        public AweRect DirtyBounds
        {
            get { return (AweRect)this.GetValue( WebControl.DirtyBoundsProperty ); }
        }

        private static readonly DependencyPropertyKey DirtyBoundsPropertyKey =
            DependencyProperty.RegisterReadOnly( "DirtyBounds",
            typeof( AweRect ), typeof( WebControl ),
            new FrameworkPropertyMetadata( new AweRect(), null, CoerceDirtyBounds ) );

        /// <summary>
        /// Identifies the <see cref="DirtyBounds"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty DirtyBoundsProperty =
            DirtyBoundsPropertyKey.DependencyProperty;

        private static object CoerceDirtyBounds( DependencyObject d, object baseValue )
        {
            WebControl owner = (WebControl)d;

            if ( !owner.IsLive )
                return new AweRect();

            AweRect bounds = awe_webview_get_dirty_bounds( owner.Instance );
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

        #region HistoryBackCount
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static int awe_webview_get_history_back_count( IntPtr webview );

        /// <summary>
        /// Gets the available number of steps back in history.
        /// </summary>
        public int HistoryBackCount
        {
            get { return (int)this.GetValue( WebControl.HistoryBackCountProperty ); }
        }

        private static readonly DependencyPropertyKey HistoryBackCountPropertyKey =
            DependencyProperty.RegisterReadOnly( "HistoryBackCount",
            typeof( int ), typeof( WebControl ),
            new FrameworkPropertyMetadata( 0, null, CoerceHistoryBackCount ) );

        /// <summary>
        /// Identifies the <see cref="HistoryBackCount"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HistoryBackCountProperty =
            HistoryBackCountPropertyKey.DependencyProperty;

        private static object CoerceHistoryBackCount( DependencyObject d, object baseValue )
        {
            WebControl owner = (WebControl)d;

            if ( !owner.IsLive )
                return 0;

            return awe_webview_get_history_back_count( owner.Instance );
        }
        #endregion

        #region HistoryForwardCount
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static int awe_webview_get_history_forward_count( IntPtr webview );

        /// <summary>
        /// Gets the available number of steps forward in history.
        /// </summary>
        public int HistoryForwardCount
        {
            get { return (int)this.GetValue( WebControl.HistoryForwardCountProperty ); }
        }

        private static readonly DependencyPropertyKey HistoryForwardCountPropertyKey =
            DependencyProperty.RegisterReadOnly( "HistoryForwardCount",
            typeof( int ), typeof( WebControl ),
            new FrameworkPropertyMetadata( 0, null, CoerceHistoryForwardCount ) );

        /// <summary>
        /// Identifies the <see cref="HistoryForwardCount"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HistoryForwardCountProperty =
            HistoryForwardCountPropertyKey.DependencyProperty;

        private static object CoerceHistoryForwardCount( DependencyObject d, object baseValue )
        {
            WebControl owner = (WebControl)d;

            if ( !owner.IsLive )
                return 0;

            return awe_webview_get_history_forward_count( owner.Instance );
        }
        #endregion

        #region Title
        /// <summary>
        /// Gets the title of the page currently loaded in this <see cref="WebControl"/>.
        /// </summary>
        public string Title
        {
            get { return (string)this.GetValue( WebControl.TitleProperty ); }
            protected set { SetValue( WebControl.TitlePropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey TitlePropertyKey =
            DependencyProperty.RegisterReadOnly( "Title",
            typeof( string ), typeof( WebControl ),
            new FrameworkPropertyMetadata( null ) );

        /// <summary>
        /// Identifies the <see cref="Title"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TitleProperty =
            TitlePropertyKey.DependencyProperty;
        #endregion

        #region HasKeyboardFocus
        /// <summary>
        /// Gets if this <see cref="WebControl"/> currently has keyboard focus.
        /// </summary>
        public bool HasKeyboardFocus
        {
            get { return (bool)this.GetValue( WebControl.HasKeyboardFocusProperty ); }
            protected set { SetValue( WebControl.HasKeyboardFocusPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey HasKeyboardFocusPropertyKey =
            DependencyProperty.RegisterReadOnly( "HasKeyboardFocus",
            typeof( bool ), typeof( WebControl ),
            new FrameworkPropertyMetadata( false ) );

        /// <summary>
        /// Identifies the <see cref="HasKeyboardFocus"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HasKeyboardFocusProperty =
            HasKeyboardFocusPropertyKey.DependencyProperty;
        #endregion

        #region HasTargetURL
        /// <summary>
        /// Gets if this <see cref="WebControl"/> is currently indicating a target URL,
        /// usually as a result of hovering over a link on the page.
        /// </summary>
        public bool HasTargetURL
        {
            get { return (bool)this.GetValue( WebControl.HasTargetURLProperty ); }
        }

        private static readonly DependencyPropertyKey HasTargetURLPropertyKey =
            DependencyProperty.RegisterReadOnly( "HasTargetURL",
            typeof( bool ), typeof( WebControl ),
            new FrameworkPropertyMetadata( false, null, CoerceHasTargetURL ) );

        /// <summary>
        /// Identifies the <see cref="HasTargetURL"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HasTargetURLProperty =
            HasTargetURLPropertyKey.DependencyProperty;

        private static object CoerceHasTargetURL( DependencyObject d, object baseValue )
        {
            WebControl owner = (WebControl)d;

            if ( !owner.IsLive )
                return false;

            return !String.IsNullOrWhiteSpace( owner.TargetURL );
        }
        #endregion

        #region TargetURL
        /// <summary>
        /// Gets the target URL indicated by the <see cref="WebControl"/>,
        /// usually as a result of hovering over a link on the page.
        /// </summary>
        public string TargetURL
        {
            get { return (string)this.GetValue( WebControl.TargetURLProperty ); }
            protected set { SetValue( WebControl.TargetURLPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey TargetURLPropertyKey =
            DependencyProperty.RegisterReadOnly( "TargetURL",
            typeof( string ), typeof( WebControl ),
            new FrameworkPropertyMetadata( null ) );

        /// <summary>
        /// Identifies the <see cref="TargetURL"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty TargetURLProperty =
            TargetURLPropertyKey.DependencyProperty;
        #endregion

        #region IsCrashed
        /// <summary>
        /// Gets if the renderer of this <see cref="WebControl"/> (which is isolated in a separate process) has crashed.
        /// </summary>
        /// <remarks>
        /// When crashed, this control will attempt to recreate its underlying view when any of the following
        /// methods or properties is called:
        /// <list type="bullet">
        /// <item><see cref="GoToHome"/></item>
        /// <item><see cref="LoadURL"/></item>
        /// <item><see cref="LoadHTML"/></item>
        /// <item><see cref="LoadFile"/></item>
        /// <item><see cref="Reload"/></item>
        /// <item><see cref="Source"/></item>
        /// </list>
        /// <note>
        /// It is suggested that you avoid using <see cref="Reload"/> since what was there in the current
        /// page that caused the crash in the first place, may crash the view again.
        /// </note>
        /// </remarks>
        public bool IsCrashed
        {
            get { return (bool)this.GetValue( WebControl.IsCrashedProperty ); }
            protected set { SetValue( WebControl.IsCrashedPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey IsCrashedPropertyKey =
            DependencyProperty.RegisterReadOnly( "IsCrashed",
            typeof( bool ), typeof( WebControl ),
            new FrameworkPropertyMetadata( false ) );

        /// <summary>
        /// Identifies the <see cref="IsCrashed"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsCrashedProperty =
            IsCrashedPropertyKey.DependencyProperty;
        #endregion

        #region PageContents
        /// <summary>
        /// Gets the textual representation of the contents of the page currently loaded.
        /// </summary>
        public string PageContents
        {
            get { return (string)this.GetValue( WebControl.PageContentsProperty ); }
            protected set { SetValue( WebControl.PageContentsPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey PageContentsPropertyKey =
            DependencyProperty.RegisterReadOnly( "PageContents",
            typeof( string ), typeof( WebControl ),
            new FrameworkPropertyMetadata( null ) );

        /// <summary>
        /// Identifies the <see cref="PageContents"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PageContentsProperty =
            PageContentsPropertyKey.DependencyProperty;
        #endregion

        #region IsDomReady
        /// <summary>
        /// Gets if DOM (Document Object Model) for the page being loaded, is ready.
        /// </summary>
        /// <remarks>
        /// This is very useful for executing Javascript on a page before its content has finished loading.
        /// </remarks>
        public bool IsDomReady
        {
            get { return (bool)this.GetValue( WebControl.IsDomReadyProperty ); }
            protected set { SetValue( WebControl.IsDomReadyPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey IsDomReadyPropertyKey =
            DependencyProperty.RegisterReadOnly( "IsDomReady",
            typeof( bool ), typeof( WebControl ),
            new FrameworkPropertyMetadata( null ) );

        /// <summary>
        /// Identifies the <see cref="IsDomReady"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsDomReadyProperty =
            IsDomReadyPropertyKey.DependencyProperty;
        #endregion

        #region FlushAlpha
        /// <summary>
        /// Gets or sets if we should flush the alpha channel to completely opaque values, during rendering.
        /// </summary>
        public bool FlushAlpha
        {
            get { return (bool)this.GetValue( FlushAlphaProperty ); }
            set { SetValue( FlushAlphaProperty, value ); }
        }

        /// <summary>
        /// Identifies the <see cref="FlushAlpha"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FlushAlphaProperty =
            DependencyProperty.Register( "FlushAlpha",
            typeof( bool ), typeof( WebControl ),
            new FrameworkPropertyMetadata( true, FlushAlphaChanged ) );

        private static void FlushAlphaChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            WebControl owner = (WebControl)d;
            bool value = (bool)e.NewValue;

            // Performance is important when we access this
            // so we also persist the value to a simple local
            // variable that can be accessed faster.
            owner.flushAplha = value;
        }
        #endregion

        #region HasSelection
        /// <summary>
        /// Gets if the user has selected content in the current page.
        /// </summary>
        public bool HasSelection
        {
            get { return (bool)this.GetValue( WebControl.HasSelectionProperty ); }
        }

        private static readonly DependencyPropertyKey HasSelectionPropertyKey =
            DependencyProperty.RegisterReadOnly( "HasSelection",
            typeof( bool ), typeof( WebControl ),
            new FrameworkPropertyMetadata( false, null, CoerceHasSelection ) );

        /// <summary>
        /// Identifies the <see cref="HasSelection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HasSelectionProperty =
            HasSelectionPropertyKey.DependencyProperty;

        private static object CoerceHasSelection( DependencyObject d, object baseValue )
        {
            WebControl owner = (WebControl)d;
            return owner.Selection != Selection.Empty;
        }
        #endregion

        #region Selection
        /// <summary>
        /// Gets a <see cref="Selection"/> providing information about the current selection range.
        /// </summary>
        public Selection Selection
        {
            get { return (Selection)this.GetValue( WebControl.SelectionProperty ); }
            protected set { this.SetValue( WebControl.SelectionPropertyKey, value ); }
        }

        private static readonly DependencyPropertyKey SelectionPropertyKey =
            DependencyProperty.RegisterReadOnly( "Selection",
            typeof( Selection ), typeof( WebControl ),
            new FrameworkPropertyMetadata( Selection.Empty, OnSelectionChanged ) );

        /// <summary>
        /// Identifies the <see cref="Selection"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SelectionProperty =
            SelectionPropertyKey.DependencyProperty;

        private static void OnSelectionChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            WebControl owner = (WebControl)d;
            Selection value = (Selection)e.NewValue;

            owner.CoerceValue( WebControl.HasSelectionProperty );
            owner.OnSelectionChanged( owner, new WebSelectionEventArgs( value ) );
        }
        #endregion

        #region Zoom
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_zoom( IntPtr webview, int zoom_percent );
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_reset_zoom( IntPtr webview );

        private int actualZoom;

        /// <summary>
        /// Gets or sets the zoom factor (page percentage).
        /// </summary>
        /// <returns>
        /// An integer value representing the zoom factor (page percentage)
        /// for the current hostname. The default is 100.
        /// </returns>
        /// <remarks>
        /// Valid range is from 10 to 500.
        /// </remarks>
        public int Zoom
        {
            get { return (int)this.GetValue( ZoomProperty ); }
            set { SetValue( ZoomProperty, value ); }
        }

        /// <summary>
        /// Identifies the <see cref="Zoom"/> dependency property.
        /// </summary>
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
            else if ( owner.actualZoom != value )
            {
                owner.actualZoom = value;
                awe_webview_set_zoom( owner.Instance, value );
            }
        }
        #endregion

        #region Source
        private string actualSource;

        /// <summary>
        /// Gets or sets the current URL presented by this <see cref="WebControl"/>.
        /// </summary>
        /// <returns>
        /// An absolute <see cref="Uri"/> representing the current URL presented 
        /// by this <see cref="WebControl"/>.
        /// </returns>
        /// <seealso cref="LoadURL"/>
        public Uri Source
        {
            get { return (Uri)this.GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }

        /// <summary>
        /// Identifies the <see cref="Source"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( "Source",
            typeof( Uri ), typeof( WebControl ),
            new FrameworkPropertyMetadata( null, SourceChanged, CoerceSource ) );

        private static object CoerceSource( DependencyObject d, object baseValue )
        {
            Uri value = (Uri)baseValue;

            if ( value != null )
            {
                if ( !value.IsAbsoluteUri )
                    return new Uri( "http://" + value.ToString() );
            }
            else
                return new Uri( "about:blank" );

            return baseValue;
        }

        private static void SourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            WebControl owner = (WebControl)d;
            Uri value = (Uri)e.NewValue;

            if ( !owner.EnsureView() )
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

        #region OnCommandExecuted
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

                    case "CopyHTML":
                        this.CopyHTML();
                        break;

                    case "CopyLinkAddress":
                        this.CopyLinkAddress();
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

                    case "BrowseHome":
                        this.GoToHome();
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

                    case "Print":
                        this.Print();
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
                            this.Source = e.Parameter is Uri ? (Uri)e.Parameter : new Uri( e.Parameter.ToString() );
                        }
                        break;

                    case "LoadFile":
                        if ( e.Parameter != null )
                        {
                            this.LoadFile( e.Parameter is Uri ? ( (Uri)e.Parameter ).ToString() : e.Parameter.ToString() );
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
                        {
                            if ( e.Parameter is string )
                                this.ChooseFile( e.Parameter.ToString() );
                            else if ( e.Parameter is string[] )
                            {
                                foreach ( string f in (string[])e.Parameter )
                                    this.ChooseFile( f );
                            }
                        }
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
        #endregion

        #region CanExecuteCommand
        private void CanExecuteCommand( object sender, CanExecuteRoutedEventArgs e )
        {
            bool canExecute = true;
            RoutedCommand command = (RoutedCommand)e.Command;

            if ( IsLive )
            {
                switch ( command.Name )
                {
                    case "Copy":
                        canExecute = !String.IsNullOrEmpty( this.Selection.Text );
                        break;

                    case "CopyHTML":
                        canExecute = !String.IsNullOrEmpty( this.Selection.HTML );
                        break;

                    case "CopyLinkAddress":
                        canExecute = HasTargetURL;
                        break;

                    case "Cut":
                        canExecute = !String.IsNullOrEmpty( this.Selection.Text );
                        break;

                    case "Paste":
                        canExecute = Clipboard.ContainsText();
                        break;

                    case "BrowseHome":
                        break;

                    case "BrowseForward":
                        canExecute = this.HistoryForwardCount > 0;
                        break;

                    case "BrowseBack":
                        canExecute = this.HistoryBackCount > 0;
                        break;

                    case "LoadURL":
                        canExecute = ( e.Parameter != null ) && ( String.Compare( e.Parameter.ToString(), GetUrl(), true ) != 0 );
                        break;

                    case "Find":
                    case "Zoom":
                    case "LoadFile":
                    case "ActivateIME":
                    case "AddURLFilter":
                    case "ChooseFile":
                    case "ConfirmIMEComposition":
                    case "CreateObject":
                    case "DestroyObject":
                        canExecute = ( e.Parameter != null );
                        break;

                }

                e.CanExecute = canExecute;
            }
            else
                e.CanExecute = ( command.Name.Equals( "BrowseHome" ) || command.Name.Equals( "Refresh" ) ) ? CanRecreateView : false;
        }
        #endregion

        #endregion


        #region Loaded / Enabled
        // In WPF, the Loaded/Unloaded events may be fired more than once
        // in the lifetime of a control. Such as when the control is hidden/shown
        // or when the control is completely covered by another control and
        // then appears again (through a change in Panel.ZIndex for example).
        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            if ( !hookAdded )
            {
                hwndSource = (HwndSource)PresentationSource.FromVisual( this );
                if ( hwndSource != null )
                {
                    // See OnThreadFilterMessage.
                    ComponentDispatcher.ThreadFilterMessage += OnThreadFilterMessage;
                    hwndSource.AddHook( HandleMessages );
                    hookAdded = true;
                }
            }

            ResumeRendering();
        }

        private void OnUnloaded( object sender, RoutedEventArgs e )
        {
            PauseRendering();
        }

        private static object CoerceIsEnabled( DependencyObject d, object baseValue )
        {
            WebControl owner = (WebControl)d;
            bool value = (bool)baseValue;

            return value &&
                ( owner.Instance != IntPtr.Zero ) &&
                !DesignerProperties.GetIsInDesignMode( owner ) &&
                !owner.IsCrashed;
        }

        private void OnIsEnabledChanged( Object sender, DependencyPropertyChangedEventArgs e )
        {
            bool value = (bool)e.NewValue;

            if ( !value )
            {
                this.AddVisualChild( controlLayer );

                this.IsNavigating = false;
                this.CoerceValue( WebControl.IsLoadingPageProperty );
                this.Title = this.IsCrashed ? AwesomiumSharp.Resources.TITLE_Crashed : lastTitle;
            }
            else
            {
                this.RemoveVisualChild( controlLayer );
                this.InvalidateMeasure();
                this.InvalidateArrange();
                this.InvalidateVisual();
            }
        }
        #endregion

        #region HandleMessages
        // Believe it or not, unlike what you may think, HwndSourceHook will not
        // be called for all window messages. Though it seems like it, it is not
        // equivalent to WinForm's WndPrc. The CLR will call HwndSourceHook hooks
        // in the order they were registered through HwndSource.AddHook and if
        // any of them sets handled to true, the rest are not called. Another major
        // difference is that unlike WinForms, a WPF application first calls events
        // and then calls HwndSourceHook! Additionally, the WPF application internally
        // filters some messages that never reach the HwndSourceHook! These messages
        // include the TAB, HOME and END keys that are processed to control TAB Navigation.
        // In order to capture these messages and inject them to the view, we need
        // to handle ComponentDispatcher.ThreadFilterMessage. However, this handle
        // only works well for messages about to be filtered by the WPF application;
        // thus, we still need the HwndSourceHook below to handle normal text, arrows etc.
        private void OnThreadFilterMessage( ref MSG msg, ref bool handled )
        {
            if ( !IsLive )
                return;

            int message = ( msg.message & 65535 );

            if ( ( message == WM_KEYDOWN || message == WM_KEYUP ) && IsFocused )
            {
                switch ( (int)msg.wParam )
                {
                    case VK_TAB:
                    case VK_HOME:
                    case VK_END:
                        awe_webview_inject_keyboard_event_win( Instance, msg.message, (int)msg.wParam, (int)msg.lParam );
                        handled = true;
                        break;
                }
            }
        }

        // See Also: OnThreadFilterMessage.
        private IntPtr HandleMessages( IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled )
        {
            if ( !IsLive )
                return IntPtr.Zero;

            int message = ( msg & 65535 );

            if ( ( message == WM_KEYDOWN || message == WM_KEYUP || message == WM_CHAR ) && IsFocused )
            {
                switch ( (int)wParam )
                {
                    case VK_TAB:
                    case VK_HOME:
                    case VK_END:
                        // Though these will never reach here, we add this to be sure.
                        // See OnThreadFilterMessage.
                        break;

                    default:
                        awe_webview_inject_keyboard_event_win( Instance, msg, (int)wParam, (int)lParam );
                        handled = true;
                        break;
                }
            }

            return IntPtr.Zero;
        }
        #endregion


        #region WebView Events

        #region BeginNavigation
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackBeginNavigationCallback( IntPtr caller, IntPtr url, IntPtr frame_name );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_begin_navigation( IntPtr webview, CallbackBeginNavigationCallback callback );

        private void internalBeginNavigationCallback( IntPtr caller, IntPtr url, IntPtr frame_name )
        {
            BeginNavigationEventArgs e = new BeginNavigationEventArgs( StringHelper.ConvertAweString( url ),
                StringHelper.ConvertAweString( frame_name ) );

            if ( String.IsNullOrEmpty( e.Url ) )
            {
                this.Title = AwesomiumSharp.Resources.TITLE_Error;
                return;
            }

            actualSource = e.Url;

            this.Title = AwesomiumSharp.Resources.TITLE_Navigating;
            this.Source = new Uri( e.Url );
            this.IsNavigating = true;
            this.CoerceValue( WebControl.IsLoadingPageProperty );
            this.CoerceValue( WebControl.HistoryBackCountProperty );
            this.CoerceValue( WebControl.HistoryForwardCountProperty );
            this.OnBeginNavigation( this, e );

            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region BeginLoading
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackBeginLoadingCallback( IntPtr caller, IntPtr url, IntPtr frame_name, int status_code, IntPtr mime_type );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_begin_loading( IntPtr webview, CallbackBeginLoadingCallback callback );

        private void internalBeginLoadingCallback( IntPtr caller, IntPtr url, IntPtr frame_name, int status_code, IntPtr mime_type )
        {
            BeginLoadingEventArgs e = new BeginLoadingEventArgs(
                StringHelper.ConvertAweString( url ),
                StringHelper.ConvertAweString( frame_name ),
                status_code,
                StringHelper.ConvertAweString( mime_type ) );

            actualSource = e.Url;
            actualZoom = GetZoom();

            if ( String.Compare( this.Title, AwesomiumSharp.Resources.TITLE_Error, false ) != 0 )
                this.Title = AwesomiumSharp.Resources.TITLE_Loading;

            this.IsDomReady = false; // Reset
            this.Source = new Uri( e.Url );
            this.Zoom = actualZoom;
            this.CoerceValue( WebControl.IsLoadingPageProperty );
            this.CoerceValue( WebControl.HistoryBackCountProperty );
            this.CoerceValue( WebControl.HistoryForwardCountProperty );
            this.OnBeginLoading( this, e );

            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region FinishLoading
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackFinishLoadingCallback( IntPtr caller );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_callback_finish_loading( IntPtr webview, CallbackFinishLoadingCallback callback );

        private void internalFinishLoadingCallback( IntPtr caller )
        {
            actualSource = GetUrl();
            actualZoom = GetZoom();

            this.CoerceValue( WebControl.IsLoadingPageProperty );
            this.CoerceValue( WebControl.HistoryBackCountProperty );
            this.CoerceValue( WebControl.HistoryForwardCountProperty );
            this.Source = new Uri( actualSource );
            this.IsNavigating = false;
            this.Zoom = actualZoom;
            this.OnLoadCompleted( this, EventArgs.Empty );

            if ( this.IsSourceControl )
                this.Title = this.Source.AbsoluteUri;

            CommandManager.InvalidateRequerySuggested();
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

            JSCallbackEventArgs e = new JSCallbackEventArgs(
                StringHelper.ConvertAweString( object_name ),
                StringHelper.ConvertAweString( callback_name ),
                args );

            if ( JSCallbackCalled != null )
                JSCallbackCalled( this, e );

            CommandManager.InvalidateRequerySuggested();
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

            lastTitle = e.Title;

            this.Title = e.Title;
            this.OnTitleReceived( this, e );

            CommandManager.InvalidateRequerySuggested();
        }
        #endregion

        #region ChangeTooltip
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackChangeTooltipCallback( IntPtr caller, IntPtr tooltip );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_change_tooltip( IntPtr webview, CallbackChangeTooltipCallback callback );

        private void internalChangeTooltipCallback( IntPtr caller, IntPtr tooltip )
        {
            ChangeToolTipEventArgs e = new ChangeToolTipEventArgs( StringHelper.ConvertAweString( tooltip ) );

            if ( String.IsNullOrEmpty( e.ToolTip ) )
            {
                toolTip.IsOpen = false;
            }
            else
            {
                toolTip.Content = e.ToolTip;
                toolTip.IsOpen = true;
            }

            this.OnToolTipChanged( this, e );

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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
            this.CoerceValue( WebControl.HasTargetURLProperty );
            this.OnTargetUrlChanged( this, e );

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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

            Uri uriTest;
            Uri.TryCreate( e.Url, UriKind.Absolute, out uriTest );

            if ( String.IsNullOrWhiteSpace( e.Url ) ||
                ( uriTest == null ) ||
                !uriTest.IsAbsoluteUri ||
                ( e.Url.IndexOfAny( Path.GetInvalidPathChars() ) != -1 ) ||
                String.IsNullOrWhiteSpace( Path.GetFileName( e.Url ) ) ||
                ( Path.GetFileName( e.Url ).IndexOfAny( Path.GetInvalidFileNameChars() ) != -1 ) )
                return;

            this.OnDownload( this, e );

            // We get a BeginNavigation before this.
            // Restore the title.
            this.Title = lastTitle;

            CommandManager.InvalidateRequerySuggested();
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
            this.CoerceValue( WebControl.IsEnabledProperty );
            this.OnCrashed( this, EventArgs.Empty );

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();

            selectionHelper.InjectSelectionHandlers();
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

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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

            CommandManager.InvalidateRequerySuggested();
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
                CommandManager.InvalidateRequerySuggested();

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

            CommandManager.InvalidateRequerySuggested();
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

        #region Temporary Selection Logic
        /// <summary>
        /// Helper callback.
        /// </summary>
        private void OnWebSelectionChanged( object sender, WebSelectionEventArgs e )
        {
            this.Selection = e.Selection;
        }
        #endregion
    }
}
