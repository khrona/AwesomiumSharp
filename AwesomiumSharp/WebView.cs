/*******************************************************************************/
/*************************** EDITING NOTES *************************************/
/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : WebView.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/03/2011
 *    Editor  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    This class is a wrapper to the Awesomium C API. The major 
 *    differences and edits made are:
 *    
 *    - Many changes with respect to standard .NET guidelines and naming
 *      convention were made. These include, among others:
 *          * Get/Set accessors were turned into Properties wherever
 *            this was appropriate.
 *          * The names of many events were changed to follow proper
 *            naming convension.
 *          * Protected event triggers were added.
 *          * Former inner classes (such as the various EventArgs) are
 *            taken outside the class and moved to separate files. They
 *            can be found in the EventArgs project folder. All these
 *            classes have also been edited following the same guidelines.
 *    
 *    - A base modeling class (ViewModel) implementing INotifyPropertyChanged
 *      is added and subclassed. This makes WebView MVVM friendly.
 *      
 *    - Part of the Find logic is now handled by this class making it
 *      more straightforward. FindNext is added.
 *    
 *    - Extensive pro-exception verification of validity before every API
 *      call is added.  
 * 
 *-------------------------------------------------------------------------------
 *
 *    !Changes may need to be tested with AwesomiumMono. I didn't test them!
 * 
 ********************************************************************************/

#region Using
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Runtime.InteropServices;
#if !USING_MONO
using System.Linq;
#endif
#endregion

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    #region JSCallback
    public delegate void JSCallback( object sender, JSCallbackEventArgs e );
    #endregion

    /// <summary>
    /// The WebView is sort of like a tab in Chrome: you can load web-pages into it, interact with it, 
    /// and render it to a buffer (we give you the raw pixels, its your duty to display it).
    /// You can create a WebView using <see cref="WebCore.CreateWebview"/>.
    /// </summary>
    public class WebView : ViewModel, IWebView
    {
        #region Fields
        internal Dictionary<string, JSCallback> jsObjectCallbackMap;

        private Random findRequestRandomizer;

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
        /// This event is fired only if <see cref="WebCore.AutoUpdate"/> is set to true.
        /// This event may be fired in a background thread.
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


        #region Ctor
        internal WebView( IntPtr webview )
        {
            findRequestRandomizer = new Random();

            this.Instance = webview;

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

        #region IDisposable
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_destroy( IntPtr webview );

        protected override void OnDispose()
        {
            if ( Instance != IntPtr.Zero )
            {
                if ( !WebCore.IsShuttingDown )
                {
                    if ( ( WebCore.activeWebViews != null ) && WebCore.activeWebViews.Contains( this ) )
                        WebCore.activeWebViews.Remove( this );

                    awe_webview_destroy( Instance );
                }

                Instance = IntPtr.Zero;
            }
        }
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

        #region PrepareForShutdown
        internal void PrepareForShutdown()
        {
            if ( Instance != IntPtr.Zero )
            {
                resourceRequestCallback = null;
                awe_webview_set_callback_resource_request( Instance, null );

                resourceResponseCallback = null;
                awe_webview_set_callback_resource_response( Instance, null );
            }
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

            awe_webview_load_url( Instance, urlStr.value(), frameNameStr.value(), usernameStr.value(), passwordStr.value() );
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

            awe_webview_load_html( Instance, htmlStr.value(), frameNameStr.value() );
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

            awe_webview_load_file( Instance, fileStr.value(), frameNameStr.value() );
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

            awe_webview_execute_javascript( Instance, javascriptStr.value(), frameNameStr.value() );
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

            IntPtr temp = awe_webview_execute_javascript_with_result( Instance, javascriptStr.value(), frameNameStr.value(), timeoutMs );
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

            awe_webview_call_javascript_function( Instance, objectNameStr.value(), functionStr.value(), jsarray, frameNameStr.value() );

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
            awe_webview_create_object( Instance, objectNameStr.value() );
        }
        #endregion

        #region DestroyObject
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_destroy_object( IntPtr webview, IntPtr object_name );

        public void DestroyObject( string objectName )
        {
            VerifyLive();

            StringHelper objectNameStr = new StringHelper( objectName );
            awe_webview_destroy_object( Instance, objectNameStr.value() );
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

            awe_webview_set_object_property( Instance, objectNameStr.value(), propertyNameStr.value(), val.Instance );
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

            awe_webview_set_object_callback( Instance, objectNameStr.value(), callbackNameStr.value() );

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

        #region Render
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_webview_render( IntPtr webview );

        /// <summary>
        /// Renders this WebView into an offscreen pixel buffer and clears the dirty state.
        /// For maximum efficiency, you should only call this when the WebView is dirty (WebView.IsDirty).
        /// </summary>
        /// <returns>An instance of the RenderBuffer that this WebView was rendered to. This
        /// value may change between renders and may return null if the WebView has crashed.</returns>
        public RenderBuffer Render()
        {
            VerifyLive();
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
        public void PauseRendering()
        {
            VerifyLive();
            awe_webview_pause_rendering( Instance );
        }
        #endregion

        #region ResumeRendering
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_resume_rendering( IntPtr webview );

        /// <summary>
        /// Resume rendering after a call to WebView.PauseRendering
        /// </summary>
        public void ResumeRendering()
        {
            VerifyLive();
            awe_webview_resume_rendering( Instance );
        }
        #endregion

        #region InjectMouseMove
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_move( IntPtr webview, int x, int y );

        public void InjectMouseMove( int x, int y )
        {
            VerifyLive();
            awe_webview_inject_mouse_move( Instance, x, y );
        }
        #endregion

        #region InjectMouseDown
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_down( IntPtr webview, MouseButton mouseButton );

        public void InjectMouseDown( MouseButton mouseButton )
        {
            VerifyLive();
            awe_webview_inject_mouse_down( Instance, mouseButton );
        }
        #endregion

        #region InjectMouseUp
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_up( IntPtr webview, MouseButton mouseButton );

        public void InjectMouseUp( MouseButton mouseButton )
        {
            VerifyLive();
            awe_webview_inject_mouse_up( Instance, mouseButton );
        }
        #endregion

        #region InjectMouseWheel
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_mouse_wheel( IntPtr webview, int scroll_amount_vert, int scroll_amount_horz );

        public void InjectMouseWheel( int scrollAmountVert, int scrollAmountHorz = 0 )
        {
            VerifyLive();
            awe_webview_inject_mouse_wheel( Instance, scrollAmountVert, scrollAmountHorz );
        }
        #endregion

        #region InjectKeyboardEvent
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_keyboard_event( IntPtr webview, WebKeyboardEvent key_event );

        public void InjectKeyboardEvent( WebKeyboardEvent keyEvent )
        {
            VerifyLive();
            awe_webview_inject_keyboard_event( Instance, keyEvent );
        }
        #endregion

        #region InjectKeyboardEventWin
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_inject_keyboard_event_win( IntPtr webview, int msg, int wparam, int lparam );

        public void InjectKeyboardEventWin( int msg, int wparam, int lparam )
        {
            VerifyLive();
            awe_webview_inject_keyboard_event_win( Instance, msg, wparam, lparam );
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

        #region SetZoom
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_zoom( IntPtr webview, int zoom_percent );

        public void SetZoom( int zoomPercent )
        {
            VerifyLive();
            awe_webview_set_zoom( Instance, zoomPercent );
        }
        #endregion

        #region ResetZoom
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_reset_zoom( IntPtr webview );

        public void ResetZoom()
        {
            VerifyLive();
            awe_webview_reset_zoom( Instance );
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
        public bool Resize( int width, int height, bool waitForRepaint = true, int repaintTimeoutMs = 300 )
        {
            VerifyLive();
            return awe_webview_resize( Instance, width, height, waitForRepaint, repaintTimeoutMs );
        }
        #endregion

        #region Unfocus
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_unfocus( IntPtr webview );

        public void Unfocus()
        {
            VerifyLive();
            awe_webview_unfocus( Instance );
        }
        #endregion

        #region Focus
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_focus( IntPtr webview );

        /// <summary>
        /// Notifies the current page that it has gained focus. You will need to
        /// call this to gain textbox focus, among other things. (If you fail to
        /// ever see a blinking caret when typing text, this is why.)
        /// </summary>
        public void Focus()
        {
            VerifyLive();
            awe_webview_focus( Instance );
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
            awe_webview_add_url_filter( Instance, filterStr.value() );
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

            awe_webview_set_header_definition( Instance, nameStr.value(), (uint)count, keys, values );

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

            awe_webview_add_header_rewrite_rule( Instance, ruleStr.value(), nameStr.value() );
        }
        #endregion

        #region RemoveHeaderRewriteRule
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_remove_header_rewrite_rule( IntPtr webview, IntPtr rule );

        public void RemoveHeaderRewriteRule( string rule )
        {
            VerifyLive();

            StringHelper ruleStr = new StringHelper( rule );
            awe_webview_remove_header_rewrite_rule( Instance, ruleStr.value() );
        }
        #endregion

        #region RemoveHeaderRewriteRulesByDefinition
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_remove_header_rewrite_rules_by_definition_name( IntPtr webview, IntPtr name );

        public void RemoveHeaderRewriteRulesByDefinition( string name )
        {
            VerifyLive();

            StringHelper nameStr = new StringHelper( name );
            awe_webview_remove_header_rewrite_rules_by_definition_name( Instance, nameStr.value() );
        }
        #endregion

        #region ChooseFile
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_choose_file( IntPtr webview, IntPtr file_path );

        public void ChooseFile( string filePath )
        {
            VerifyLive();

            StringHelper filePathStr = new StringHelper( filePath );
            awe_webview_choose_file( Instance, filePathStr.value() );
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
            awe_webview_request_scroll_data( Instance, frameNameStr.value() );
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
            awe_webview_find( Instance, findRequest.RequestID, searchCStr.value(), forward, caseSensitive, false );
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
            awe_webview_find( Instance, findRequest.RequestID, searchCStr.value(), forward, findRequest.CaseSensitive, true );
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

            awe_webview_translate_page( Instance, sourceLanguageStr.value(), targetLanguageStr.value() );
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
            awe_webview_set_ime_composition( Instance, inputCStr.value(), cursorPos, targetStart, targetEnd );
        }
        #endregion

        #region ConfirmIMEComposition
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_confirm_ime_composition( IntPtr webview, IntPtr input_string );

        public void ConfirmIMEComposition( string inputStr )
        {
            VerifyLive();

            StringHelper inputCStr = new StringHelper( inputStr );
            awe_webview_confirm_ime_composition( Instance, inputCStr.value() );
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
                return ( Instance != IntPtr.Zero );
            }
        }
        #endregion

        #region Instance
        internal IntPtr Instance { get; set; }
        #endregion

        #region IsDirty
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_webview_is_dirty( IntPtr webview );
        private bool isDirty;

        /// <summary>
        /// Gets whether or not this WebView needs to be rendered again.
        /// </summary>
        /// <remarks>
        /// Internal changes to this property fire a <see cref="INotifyPropertyChanged.PropertyChanged"/>
        /// only if <see cref="WebCore.AutoUpdate"/> is set to true.
        /// </remarks>
        public bool IsDirty
        {
            get
            {
                if ( !WebCore.AutoUpdate )
                {
                    isDirty = false;
                    VerifyLive();
                    return awe_webview_is_dirty( Instance );
                }

                return isDirty;
            }
            internal set
            {
                // Insist on firing while True.
                if ( !isDirty && ( isDirty == value ) )
                    return;

                isDirty = value;
                RaisePropertyChanged( "IsDirty" );
                this.OnIsDirtyChanged( this, EventArgs.Empty );
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
            get
            {
                VerifyLive();
                return awe_webview_is_resizing( Instance );
            }
        }
        #endregion

        #region IsLoadingPage
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_webview_is_loading_page( IntPtr webview );

        public bool IsLoadingPage
        {
            get
            {
                VerifyLive();
                return awe_webview_is_loading_page( Instance );
            }
        }
        #endregion

        #region Title
        private String title;
        public String Title
        {
            get
            {
                return title;
            }
            protected set
            {
                if ( title == value )
                    return;

                title = value;
                RaisePropertyChanged( "Title" );
            }
        }
        #endregion

        #region Tooltip
        private String tooltip;

        public String Tooltip
        {
            get
            {
                return tooltip;
            }
            protected set
            {
                if ( tooltip == value )
                    return;

                tooltip = value;
                RaisePropertyChanged( "Tooltip" );
            }
        }
        #endregion

        #region Cursor
        private CursorType cursor;

        public CursorType Cursor
        {
            get
            {
                return cursor;
            }
            protected set
            {
                if ( cursor == value )
                    return;

                cursor = value;
                RaisePropertyChanged( "Cursor" );
            }
        }
        #endregion

        #region HasKeyboardFocus
        private bool hasKeyboardFocus;

        public bool HasKeyboardFocus
        {
            get
            {
                return hasKeyboardFocus;
            }
            protected set
            {
                if ( hasKeyboardFocus == value )
                    return;

                hasKeyboardFocus = value;
                RaisePropertyChanged( "HasKeyboardFocus" );
            }
        }
        #endregion

        #region TargetURL
        private String targetURL;

        public String TargetURL
        {
            get
            {
                return targetURL;
            }
            protected set
            {
                if ( targetURL == value )
                    return;

                targetURL = value;
                RaisePropertyChanged( "TargetURL" );
            }
        }
        #endregion

        #region IsCrashed
        private bool isCrashed;

        public bool IsCrashed
        {
            get
            {
                return isCrashed;
            }
            protected set
            {
                if ( isCrashed == value )
                    return;

                isCrashed = value;
                RaisePropertyChanged( "IsCrashed" );
            }
        }
        #endregion

        #region PageContents
        private String pageContents;

        public String PageContents
        {
            get
            {
                return pageContents;
            }
            protected set
            {
                if ( pageContents == value )
                    return;

                pageContents = value;
                RaisePropertyChanged( "PageContents" );
            }
        }
        #endregion

        #region IsDomReady
        private bool isDomReady;

        public bool IsDomReady
        {
            get
            {
                return isDomReady;
            }
            protected set
            {
                if ( isDomReady == value )
                    return;

                isDomReady = value;
                RaisePropertyChanged( "IsDomReady" );
            }
        }
        #endregion

        #endregion

        #region Internal Event Handlers

        #region BeginNavigation
        [UnmanagedFunctionPointerAttribute( CallingConvention.Cdecl )]
        internal delegate void CallbackBeginNavigationCallback( IntPtr caller, IntPtr url, IntPtr frame_name );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_callback_begin_navigation( IntPtr webview, CallbackBeginNavigationCallback callback );

        private void internalBeginNavigationCallback( IntPtr caller, IntPtr url, IntPtr frame_name )
        {
            BeginNavigationEventArgs e = new BeginNavigationEventArgs( StringHelper.ConvertAweString( url ),
                StringHelper.ConvertAweString( frame_name ) );

            this.IsDomReady = false;
            RaisePropertyChanged( "IsLoading" );
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

            RaisePropertyChanged( "IsLoading" );
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
            RaisePropertyChanged( "IsLoading" );
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

            this.Tooltip = e.Tooltip;
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

            this.Cursor = e.CursorType;
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


        #region IWebView Members
        void IWebView.OnCoreAutoUpdateChanged( bool newValue )
        {
            // WebView does not care about this.
        }

        void IWebView.PrepareForShutdown()
        {
            PrepareForShutdown();
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
