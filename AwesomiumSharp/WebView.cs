#region Changelog
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
 *      
 *    07/08/2011:
 *    
 *    - Full documentation. Updated documentation will soon be available online
 *      at: <http://awesomium.com/docs/1_6_2/sharp_api/>
 *      
 *    - Few fixes and renaming of members.
 *    
 *    07/12/2011:
 *    
 *    - Synchronized with Awesomium r148 (1.6.2 Pre-Release)
 * 
 *-------------------------------------------------------------------------------
 *
 *    !Changes may need to be tested with AwesomiumMono. I didn't test them!
 * 
 ********************************************************************************/
#endregion

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
    /// <summary>
    /// The WebView is sort of like a tab in Chrome: you can load web-pages into it, interact with it, 
    /// and render it to a buffer (we give you the raw pixels, its your duty to display it).
    /// </summary>
    /// <remarks>
    /// @note
    /// You can create a WebView using <see cref="WebCore.CreateWebview"/>. If you have not initialized
    /// the <see cref="WebCore"/> (see <see cref="WebCore.Initialize"/>) before creating a <see cref="WebView"/>,
    /// the core will be initialized using default configuration settings and automatically be started
    /// before creating the view.
    /// @warning
    /// Instances of classes in this assembly and their members, are not thread safe.
    /// </remarks>
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

        #region JSCallbackCalled
        internal event JSCallback JSCallbackCalled;
        #endregion


        #region IsDirtyChanged
        /// <summary>
        /// Occurs when this <see cref="WebView"/> needs to be rendered again.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This event is fired continuously while <see cref="IsDirty"/> is true and until a call 
        /// to <see cref="Render"/> is made that will render the updated WebView into an offscreen
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
        /// This event occurs when a <see cref="WebView"/> begins loading a new page (first bits of data received from server).
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
        /// This event occurs when a <see cref="WebView"/> begins navigating to a new URL.
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

        #region TooltipChanged
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
        /// you can manually report this back to the view by calling <see cref="WebView.ChooseFile"/>.
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
        /// This event occurs as a response to <see cref="WebView.RequestScrollData"/>.
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
        /// This event occurs whenever we receive results back from an in-page find operation (see <see cref="WebView.Find"/>).
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

        #region ImeUpdated
        /// <summary>
        /// This event occurs whenever the user does something that changes the 
        /// position or visibility of the IME Widget. This event is only active when 
        /// IME is activated (please see <see cref="WebView.ActivateIME"/>).
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

            RaisePropertyChanged( "IsEnabled" );
        }
        #endregion

        #region Dtor
        /// <summary>
        /// Destroys and removes this <see cref="WebView"/> instance. Any call to members of this view
        /// after calling this method, will cause a <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <remarks>
        /// @warning
        /// To avoid exceptions, Make sure you do not call this method when the hosting UI 
        /// (if any) of this view, is still alive and visible.
        /// </remarks>
        public void Close()
        {
            this.Dispose();
            RaisePropertyChanged( "IsEnabled" );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_destroy( IntPtr webview );

        /// <summary>
        /// Overrides <see cref="ViewModel.OnDispose"/>. This method is called
        /// when the view is being destroyed by either calling <see cref="WebCore.Shutdown"/>,
        /// <see cref="Close"/> or explicitly calling <see cref="IDisposable.Dispose"/> on this instance.
        /// </summary>
        protected override void OnDispose()
        {
            if ( Instance != IntPtr.Zero )
            {
                resourceRequestCallback = null;
                awe_webview_set_callback_resource_request( Instance, null );

                resourceResponseCallback = null;
                awe_webview_set_callback_resource_response( Instance, null );

                WebCore.DestroyView( this );
                Instance = IntPtr.Zero;
            }
        }
        #endregion


        #region Methods

        #region Internal

        #region VerifyValid
        private void VerifyValid()
        {
            if ( !IsEnabled )
#if USING_MONO
throw new InvalidOperationException( "This WebView instance is invalid. It has either been destroyed or it was never properly instantiated." );
#else
                throw new InvalidOperationException( Resources.ERR_InvalidWebView );
#endif

        }
        #endregion

        #endregion

        #region Public

        #region LoadURL
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_set_resource_interceptor(/*To do?*/);

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_load_url( IntPtr webview, IntPtr url, IntPtr frame_name, IntPtr username, IntPtr password );

        /// <summary>
        /// Loads a URL into the WebView asynchronously.
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
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void LoadURL( string url, string frameName = "", string username = "", string password = "" )
        {
            VerifyValid();

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

        /// <summary>
        /// Loads a string of HTML into the WebView asynchronously.
        /// </summary>
        /// <param name="html">
        /// The HTML string (ASCII) to load.
        /// </param>
        /// <param name="frameName">
        /// The name of the frame to load the HTML in.
        /// </param>
        /// <remarks>
        /// @note
        /// Any assets requires by the specified HTML (images etc.), should exist 
        /// within the base directory set with <see cref="WebCore.SetBaseDirectory"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void LoadHTML( string html, string frameName = "" )
        {
            VerifyValid();

            StringHelper htmlStr = new StringHelper( html );
            StringHelper frameNameStr = new StringHelper( frameName );

            awe_webview_load_html( Instance, htmlStr.Value, frameNameStr.Value );
        }
        #endregion

        #region LoadFile
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_load_file( IntPtr webview, IntPtr file, IntPtr frame_name );

        /// <summary>
        /// Loads a local file into the <see cref="WebView"/> asynchronously.
        /// </summary>
        /// <param name="file">
        /// The name of the file to load.
        /// </param>
        /// <param name="frameName">
        /// The name of the frame to load the file in.
        /// </param>
        /// <remarks>
        /// @note
        /// The file should exist within the base directory set with <see cref="WebCore.SetBaseDirectory"/>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void LoadFile( string file, string frameName = "" )
        {
            VerifyValid();

            StringHelper fileStr = new StringHelper( file );
            StringHelper frameNameStr = new StringHelper( frameName );

            awe_webview_load_file( Instance, fileStr.Value, frameNameStr.Value );
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void GoToHistoryOffset( int offset )
        {
            VerifyValid();
            awe_webview_go_to_history_offset( Instance, offset );
        }
        #endregion

        #region GoBack
        /// <summary>
        /// Navigates one step back in history.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void Stop()
        {
            VerifyValid();
            awe_webview_stop( Instance );
        }
        #endregion

        #region Reload
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_reload( IntPtr webview );

        /// <summary>
        /// Reloads the current page.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void Reload()
        {
            VerifyValid();
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
        /// <param name="javascript">The string of Javascript to execute.</param>
        /// <param name="frameName">Optional; the name of the frame to execute in,
        /// leave this blank to execute in the main frame.</param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void ExecuteJavascript( string javascript, string frameName = "" )
        {
            VerifyValid();

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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public JSValue ExecuteJavascriptWithResult( string javascript, string frameName = "", int timeoutMs = 0 )
        {
            VerifyValid();

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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void CallJavascriptFunction( string objectName, string function, string frameName = "", params JSValue[] arguments )
        {
            VerifyValid();

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
        /// the lifetime of this <see cref="WebView"/>. This is useful for exposing your application's
        /// data and events to Javascript. This object is managed directly by Awesomium
        /// so you can modify its properties and bind callback functions via
        /// <see cref="WebView.SetObjectProperty"/> and <see cref="WebView.SetObjectCallback"/>, 
        /// respectively.
        /// </summary>
        /// <param name="objectName">
        /// The name of the object.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void CreateObject( string objectName )
        {
            VerifyValid();

            StringHelper objectNameStr = new StringHelper( objectName );
            awe_webview_create_object( Instance, objectNameStr.Value );
        }
        #endregion

        #region DestroyObject
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_destroy_object( IntPtr webview, IntPtr object_name );

        /// <summary>
        /// Destroys a Javascript object previously created with <see cref="WebView.CreateObject"/>.
        /// </summary>
        /// <param name="objectName">
        /// The name of the object to destroy.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void DestroyObject( string objectName )
        {
            VerifyValid();

            StringHelper objectNameStr = new StringHelper( objectName );
            awe_webview_destroy_object( Instance, objectNameStr.Value );
        }
        #endregion

        #region SetObjectProperty
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_object_property( IntPtr webview, IntPtr object_name, IntPtr property_name, IntPtr val );

        /// <summary>
        /// Sets a property of a Javascript object previously created with <see cref="WebView.CreateObject"/>.
        /// </summary>
        /// <example>
        /// An example of usage:
        /// <code>
        /// webView.CreateObject("MyObject");
        /// webView.SetObjectProperty("MyObject", "color", "blue");
        /// 
        /// // You can now access this object's property via Javascript on any 
        /// // page loaded into this WebView:
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void SetObjectProperty( string objectName, string propertyName, JSValue val )
        {
            VerifyValid();

            StringHelper objectNameStr = new StringHelper( objectName );
            StringHelper propertyNameStr = new StringHelper( propertyName );

            awe_webview_set_object_property( Instance, objectNameStr.Value, propertyNameStr.Value, val.Instance );
        }
        #endregion

        #region SetObjectCallback
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_object_callback( IntPtr webview, IntPtr object_name, IntPtr callback_name );

        /// <summary>
        /// Binds a callback function to a Javascript object previously created with <see cref="WebView.CreateObject"/>.
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
        /// public void initWebView()
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void SetObjectCallback( string objectName, string callbackName, JSCallback callback )
        {
            VerifyValid();

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

        #region Render
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_webview_render( IntPtr webview );

        /// <summary>
        /// Renders this <see cref="WebView"/> into an offscreen pixel buffer and clears the dirty state.
        /// </summary>
        /// <remarks>
        /// For maximum efficiency, you should only call this when the <see cref="WebView"/> is dirty 
        /// (see <see cref="IsDirty"/>).
        /// @note
        /// The most appropriate time to call this method, is from within your <see cref="IsDirtyChanged"/> handler.
        /// </remarks>
        /// <returns>
        /// An instance of the <see cref="RenderBuffer"/> that this <see cref="WebView"/> was rendered to. 
        /// This value may change between renders and may return null if the <see cref="WebView"/> has crashed
        /// (see <see cref="IsCrashed"/>).
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public RenderBuffer Render()
        {
            VerifyValid();
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
        /// <see cref="WebView.Render"/>. Call this to temporarily pause rendering.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void PauseRendering()
        {
            VerifyValid();
            awe_webview_pause_rendering( Instance );
        }
        #endregion

        #region ResumeRendering
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_resume_rendering( IntPtr webview );

        /// <summary>
        /// Resume rendering after a call to <see cref="WebView.PauseRendering"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void ResumeRendering()
        {
            VerifyValid();
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
        /// The absolute x-coordinate of the mouse (relative to the <see cref="WebView"/> itself).
        /// </param>
        /// <param name="y">
        /// The absolute y-coordinate of the mouse (relative to the <see cref="WebView"/> itself).
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void InjectMouseMove( int x, int y )
        {
            VerifyValid();
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
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void InjectMouseDown( MouseButton mouseButton )
        {
            VerifyValid();
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
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void InjectMouseUp( MouseButton mouseButton )
        {
            VerifyValid();
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
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void InjectMouseWheel( int scrollAmountVert, int scrollAmountHorz = 0 )
        {
            VerifyValid();
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
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void InjectKeyboardEvent( WebKeyboardEvent keyEvent )
        {
            VerifyValid();
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
        /// The Windows keyboard message (usually WM_KEYDOWN, WM_KEYUP and WM_CHAR). 
        /// </param>
        /// <param name="wparam">
        /// The first parameter of the message as intercepted by the window procedure.
        /// </param>
        /// <param name="lparam">
        /// The second parameter of the message as intercepted by the window procedure.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        /// <remarks>
        /// This is usually easier to use than <see cref="InjectKeyboardEvent"/>. All you have to
        /// do is hook into the window procedure of this view's host, intercept WM_KEYDOWN, WM_KEYUP and WM_CHAR
        /// and inject them to the view by using this method.
        /// @warning
        /// Beware that in WPF, only the parent Window has a window procedure. Make sure
        /// that you only inject messages when the actual host (if it's a child element)
        /// has the focus, and that you do not hook into the same procedure multiple times.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void InjectKeyboardEventWin( int msg, int wparam, int lparam )
        {
            VerifyValid();
            awe_webview_inject_keyboard_event_win( Instance, msg, wparam, lparam );
        }
        #endregion

        #region Cut
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_cut( IntPtr webview );

        /// <summary>
        /// Cuts the text currently selected in this <see cref="WebView"/>, when it has keyboard focus
        /// (usually in a text-box), using the system clipboard.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void Cut()
        {
            VerifyValid();
            awe_webview_cut( Instance );
        }
        #endregion

        #region Copy
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_copy( IntPtr webview );

        /// <summary>
        /// Copies the text currently selected in this <see cref="WebView"/>, to the system clipboard.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void Copy()
        {
            VerifyValid();
            awe_webview_copy( Instance );
        }
        #endregion

        #region Paste
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_paste( IntPtr webview );

        /// <summary>
        /// Pastes the text currently in the system clipboard, to this <see cref="WebView"/>,
        /// when it has keyboard focus (usually in a text-box).
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void Paste()
        {
            VerifyValid();
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void SelectAll()
        {
            VerifyValid();
            awe_webview_select_all( Instance );
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void GetZoomForHost( string host )
        {
            VerifyValid();
            StringHelper hostStr = new StringHelper( host );
            awe_webview_choose_file( Instance, hostStr.Value );
        }
        #endregion

        #region ResetZoom
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_reset_zoom( IntPtr webview );

        /// <summary>
        /// Resets the zoom level.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void ResetZoom()
        {
            VerifyValid();
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
        /// Resizes this <see cref="WebView"/> to certain dimensions. This operation can fail
        /// if another resize is already pending (see <see cref="WebView.IsResizing"/>) or if
        /// the repaint timeout was exceeded.
        /// </summary>
        /// <param name="width">
        /// The width in pixels to resize to.
        /// </param>
        /// <param name="height">
        /// The height in pixels to resize to.
        /// </param>
        /// <param name="waitForRepaint">
        /// Whether or not to wait for the <see cref="WebView"/> to finish repainting to avoid flicker
        /// (default is true).
        /// </param>
        /// <param name="repaintTimeoutMs">
        /// The max amount of time to wait for a repaint, in milliseconds.
        /// </param>
        /// <returns>
        /// True if the resize was successful. False otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public bool Resize( int width, int height, bool waitForRepaint = true, int repaintTimeoutMs = 300 )
        {
            VerifyValid();
            return awe_webview_resize( Instance, width, height, waitForRepaint, repaintTimeoutMs );
        }
        #endregion

        #region Unfocus
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_unfocus( IntPtr webview );

        /// <summary>
        /// Notifies the current page that it has lost focus.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void Unfocus()
        {
            VerifyValid();
            awe_webview_unfocus( Instance );
        }
        #endregion

        #region Focus
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_focus( IntPtr webview );

        /// <summary>
        /// Notifies the current page that it has gained focus.
        /// </summary>
        /// <remarks>
        /// You will need to call this to gain text-box focus, among other things. 
        /// (If you fail to ever see a blinking caret when typing text, this is why.)
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void Focus()
        {
            VerifyValid();
            awe_webview_focus( Instance );
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
        /// Whether or not this <see cref="WebView"/> is transparent.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void SetTransparent( bool isTransparent )
        {
            VerifyValid();
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void SetURLFilteringMode( URLFilteringMode filteringMode )
        {
            VerifyValid();
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
        /// @note
        /// You may also use the "local://" scheme prefix to describe the URL to the base directory
        /// (set via <see cref="WebCore.SetBaseDirectory"/>).
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void AddURLFilter( string filter )
        {
            VerifyValid();

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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void ClearAllURLFilters()
        {
            VerifyValid();
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void SetHeaderDefinition( string name, NameValueCollection fields )
        {
            VerifyValid();

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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void AddHeaderRewriteRule( string rule, string name )
        {
            VerifyValid();

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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void RemoveHeaderRewriteRule( string rule )
        {
            VerifyValid();

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
        /// The name of the header definition (specified in <see cref="WebView.SetHeaderDefinition"/>).
        /// @note
        /// Specify an empty string, to remove all header re-write rules.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void RemoveHeaderRewriteRulesByDefinition( string name )
        {
            VerifyValid();

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
        /// @note
        /// Alternatively, if you opened a modal dialog from within your <see cref="SelectLocalFiles"/> handler,
        /// you can define the files to be uploaded by using the <see cref="SelectLocalFilesEventArgs.SelectedFiles"/>
        /// property.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void ChooseFile( string filePath )
        {
            VerifyValid();

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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void Print()
        {
            VerifyValid();
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void RequestScrollData( string frameName = "" )
        {
            VerifyValid();

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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void Find( string searchStr, bool forward = true, bool caseSensitive = false )
        {
            VerifyValid();

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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void FindNext( bool forward = true )
        {
            VerifyValid();

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
        /// Stops the last active search (started with <see cref="WebView.Find"/>).
        /// </summary>
        /// <remarks>
        /// This will un-highlight all matches of a previous call to <see cref="WebView.Find"/>.
        /// </remarks>
        /// <param name="clearSelection">
        /// True to also deselect the currently selected string. False otherwise.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void StopFind( bool clearSelection )
        {
            VerifyValid();
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void TranslatePage( string sourceLanguage, string targetLanguage )
        {
            VerifyValid();

            StringHelper sourceLanguageStr = new StringHelper( sourceLanguage );
            StringHelper targetLanguageStr = new StringHelper( targetLanguage );

            awe_webview_translate_page( Instance, sourceLanguageStr.Value, targetLanguageStr.Value );
        }
        #endregion

        #region ActivateIME
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_activate_ime( IntPtr webview, bool activate );

        /// <summary>
        /// Call this method to let the <see cref="WebView"/> know you will be passing
        /// text input via IME and will need to be notified of any IME-related
        /// events (such as caret position, user un-focusing text-box, etc.).
        /// </summary>
        /// <param name="activate">
        /// True to activate IME support. False otherwise.
        /// </param>
        /// <seealso cref="WebView.ImeUpdated"/>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void ActivateIME( bool activate )
        {
            VerifyValid();
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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void SetIMEComposition( string inputStr, int cursorPos, int targetStart, int targetEnd )
        {
            VerifyValid();

            StringHelper inputCStr = new StringHelper( inputStr );
            awe_webview_set_ime_composition( Instance, inputCStr.Value, cursorPos, targetStart, targetEnd );
        }
        #endregion

        #region ConfirmIMEComposition
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_confirm_ime_composition( IntPtr webview, IntPtr input_string );

        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void ConfirmIMEComposition( string inputStr )
        {
            VerifyValid();

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
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public void CancelIMEComposition()
        {
            VerifyValid();
            awe_webview_cancel_ime_composition( Instance );
        }
        #endregion

        #endregion

        #endregion

        #region Properties

        #region Instance
        internal IntPtr Instance { get; private set; }
        #endregion


        #region IsEnabled
        /// <summary>
        /// Gets if the view is valid and enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <see cref="WebView"/> is considered invalid when it has been destroyed 
        /// (by either calling <see cref="WebView.Close"/> or <see cref="WebCore.Shutdown"/>)
        /// or was never properly instantiated. Attempting to access members of this
        /// view while the value of this property is false, may cause a <see cref="InvalidOperationException"/>
        /// </para>
        /// <para>
        /// @note
        /// There is no way to revive an invalid view. When you are done with reporting any errors
        /// to the user, dispose it and release any references to it to avoid memory leaks.
        /// </para>
        /// </remarks>
        public bool IsEnabled
        {
            get
            {
                return ( Instance != IntPtr.Zero );
            }
        }
        #endregion

        #region IsDirty
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_webview_is_dirty( IntPtr webview );
        private bool isDirty;

        /// <summary>
        /// Gets whether or not this <see cref="WebView"/> needs to be rendered again.
        /// </summary>
        /// <remarks>
        /// Internal changes to this property fire the <see cref="IsDirtyChanged"/>
        /// and <see cref="INotifyPropertyChanged.PropertyChanged"/> events,
        /// only if <see cref="WebCore.IsAutoUpdateEnabled"/> is true.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is accessed on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        /// <seealso cref="WebView.IsDirtyChanged"/>
        /// <seealso cref="WebCore.Update"/>
        public bool IsDirty
        {
            get
            {
                if ( !WebCore.IsAutoUpdateEnabled )
                {
                    VerifyValid();
                    isDirty = awe_webview_is_dirty( Instance );
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
                RaisePropertyChanged( "DirtyBounds" );
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
        /// <returns>
        /// True if we are waiting for the <see cref="WebView"/> process to
        /// return acknowledgment of a pending resize operation. False otherwise.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The member is accessed on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public bool IsResizing
        {
            get
            {
                VerifyValid();
                return awe_webview_is_resizing( Instance );
            }
        }
        #endregion

        #region IsLoadingPage
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_webview_is_loading_page( IntPtr webview );

        /// <summary>
        /// Gets if a page is currently loading in the <see cref="WebView"/>.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is accessed on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public bool IsLoadingPage
        {
            get
            {
                VerifyValid();
                return awe_webview_is_loading_page( Instance );
            }
        }
        #endregion

        #region Title
        private String title;

        /// <summary>
        /// Gets the title of the page currently loaded in this <see cref="WebView"/>.
        /// </summary>
        public String Title
        {
            get
            {
                return title;
            }
            protected set
            {
                if ( String.Compare( title, value, false ) == 0 )
                    return;

                title = value;
                RaisePropertyChanged( "Title" );
            }
        }
        #endregion

        #region DirtyBounds
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern AweRect awe_webview_get_dirty_bounds( IntPtr webview );

        /// <summary>
        /// Gets the bounds of the area that has changed since the last call to <see cref="Render"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="AweRect"/> representing the bounds of the area that has changed 
        /// since the last call to <see cref="Render"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The member is accessed on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public AweRect DirtyBounds
        {
            get
            {
                VerifyValid();

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
        }
        #endregion

        #region HistoryBackCount
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static int awe_webview_get_history_back_count( IntPtr webview );

        /// <summary>
        /// Gets the available number of steps back in history.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is accessed on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public int HistoryBackCount
        {
            get
            {
                VerifyValid();
                return awe_webview_get_history_back_count( Instance );
            }
        }
        #endregion

        #region HistoryForwardCount
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static int awe_webview_get_history_forward_count( IntPtr webview );

        /// <summary>
        /// Gets the available number of steps forward in history.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is accessed on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public int HistoryForwardCount
        {
            get
            {
                VerifyValid();
                return awe_webview_get_history_forward_count( Instance );
            }
        }
        #endregion

        #region ToolTip
        private String tooltip;

        /// <summary>
        /// Gets the current tooltip for the element under the cursor.
        /// </summary>
        public String ToolTip
        {
            get
            {
                return tooltip;
            }
            protected set
            {
                if ( String.Compare( tooltip, value, false ) == 0 )
                    return;

                tooltip = value;
                RaisePropertyChanged( "Tooltip" );
            }
        }
        #endregion

        #region Cursor
        private CursorType cursor;

        /// <summary>
        /// Gets the current cursor type indicated by the <see cref="WebView"/>.
        /// </summary>
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

        /// <summary>
        /// Gets if this <see cref="WebView"/> currently has keyboard focus.
        /// </summary>
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

        /// <summary>
        /// Gets the target URL indicated by the <see cref="WebView"/>,
        /// usually as a result of hovering over a link on the page.
        /// </summary>
        public String TargetURL
        {
            get
            {
                return targetURL;
            }
            protected set
            {
                if ( String.Compare( targetURL, value, false ) == 0 )
                    return;

                targetURL = value;
                RaisePropertyChanged( "TargetURL" );
            }
        }
        #endregion

        #region IsCrashed
        private bool isCrashed;

        /// <summary>
        /// Gets if the renderer of this <see cref="WebView"/> (which is isolated in a separate process) has crashed.
        /// </summary>
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

        /// <summary>
        /// Gets the textual representation of the contents of the page currently loaded.
        /// </summary>
        public String PageContents
        {
            get
            {
                return pageContents;
            }
            protected set
            {
                if ( String.Compare( pageContents, value, false ) == 0 )
                    return;

                pageContents = value;
                RaisePropertyChanged( "PageContents" );
            }
        }
        #endregion

        #region IsDomReady
        private bool isDomReady;

        /// <summary>
        /// Gets if DOM (Document Object Model) for the page being loaded, is ready.
        /// </summary>
        /// <remarks>
        /// This is very useful for executing Javascript on a page before its content has finished loading.
        /// </remarks>
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

        #region Zoom
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webview_set_zoom( IntPtr webview, int zoom_percent );
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static int awe_webview_get_zoom( IntPtr webview );

        /// <summary>
        /// Gets or sets the zoom factor (page percentage) for the current hostname.
        /// Valid range is from 10% to 500%.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        public int Zoom
        {
            get
            {
                VerifyValid();
                return awe_webview_get_zoom( Instance );
            }
            set
            {
                VerifyValid();

                if ( ( value < 10 ) || ( value > 500 ) )
                    throw new ArgumentOutOfRangeException( "Zoom", "Valid range is from 10 to 500." );

                awe_webview_set_zoom( Instance, value );
                RaisePropertyChanged( "Zoom" );
            }
        }
        #endregion

        #region Source
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_webview_get_url( IntPtr request );
        private String actualUrl;

        /// <summary>
        /// Gets or sets the current URL presented by this <see cref="WebView"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="String"/> representing the current URL presented 
        /// by this <see cref="WebView"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The member is called on an invalid <see cref="WebView"/> instance
        /// (see <see cref="IsEnabled"/>).
        /// </exception>
        /// <seealso cref="LoadURL"/>
        public string Source
        {
            get
            {
                VerifyValid();
                actualUrl= StringHelper.ConvertAweString( awe_webview_get_url( Instance ) );
                return actualUrl;
            }
            set
            {
                if ( String.Compare( actualUrl, value, true ) == 0 )
                    return;

                VerifyValid();
                actualUrl = value;
                LoadURL( actualUrl );                
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

            RaisePropertyChanged( "IsLoading" );
            RaisePropertyChanged( "HistoryBackCount" );
            RaisePropertyChanged( "HistoryForwardCount" );
            RaisePropertyChanged( "Source" );
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

            // Reset
            this.IsDomReady = false;
            this.PageContents = String.Empty;

            RaisePropertyChanged( "IsLoading" );
            RaisePropertyChanged( "HistoryBackCount" );
            RaisePropertyChanged( "HistoryForwardCount" );
            RaisePropertyChanged( "Source" );
            RaisePropertyChanged( "Zoom" );
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
            RaisePropertyChanged( "HistoryBackCount" );
            RaisePropertyChanged( "HistoryForwardCount" );
            RaisePropertyChanged( "Source" );
            RaisePropertyChanged( "Zoom" );
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
            ChangeToolTipEventArgs e = new ChangeToolTipEventArgs( StringHelper.ConvertAweString( tooltip ) );

            this.ToolTip = e.ToolTip;
            this.OnToolTipChanged( this, e );
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
            RaisePropertyChanged( "Source" );
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


        #region JSCallback Handler
        internal void handleJSCallback( object sender, JSCallbackEventArgs e )
        {
            string key = String.Format( "{0}.{1}", e.ObjectName, e.CallbackName );

            if ( jsObjectCallbackMap.ContainsKey( key ) )
                jsObjectCallbackMap[ key ]( sender, e );
        }
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
    }
}
