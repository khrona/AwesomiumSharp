#region Changelog
/*******************************************************************************/
/*************************** EDITING NOTES *************************************/
/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : WebCore.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/03/2011
 *    Editor  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    This static class is a wrapper to AwesomiumSharp::WebCore. Read XML
 *    comments for details. The major differences and edits made are:
 *    
 *    - Many changes with respect to standard .NET guidelines and naming
 *      convention were made. These include, among others:
 *          * Get/Set accessors were turned into Properties wherever
 *            this was appropriate.
 *          * The names of many members were changed to follow proper
 *            naming convention.
 *          * WebCoreConfig is created, documented and taken out of
 *            the class.
 *    
 *    - Background auto-updating is added and can be configured during
 *      initialization through WebCoreConfig, or afterwards through
 *      the AutoUpdate property.
 *    
 *    - Extensive pro-exception verification of validity before every API
 *      call is added.
 *      
 * 
 *    07/06/2011:
 *    
 *    - I have decided to completely isolate auto-update to the WebCore
 *      itself, and disallow access to to the auto-Update logic from outside
 *      the assembly. The scenario and analysis is:
 *          * If a developer creates many WebView instances and provides for
 *            them his/her own update logic, or if he/she uses many WebControls
 *            (that include their own auto-update) we may end up with
 *            awe_webcore_update being called tens of times almost 
 *            simultaneously, for no reason. Documentation for awe_webcore_update
 *            says that this method is "...updating the RenderBuffer of each 
 *            WebView, destroying any WebViews that are queued for destruction, 
 *            and invoking any queued WebView events. You should call this 
 *            as part of your application's update loop.". This means that one
 *            call is enough to cover all created views.
 *          * We could demand for a hwnd and hook into the host application's
 *            message loop, but the idea of calling awe_webcore_update
 *            from inside our application's message loop is actually not
 *            appropriate. Awesomium runs as a separate process and has its
 *            own message loop. Our application may be idle while Awesomium
 *            is not. So we will keep relying on a timer.
 *            
 *    - WebCoreConfig is moved to a separate file.
 *    
 *    - Changes in initialization logic and minor bug fixes.
 *    
 *    - Added and improved existing documentation to reflect changes and
 *      help with the understanding of the new logic.
 *      
 *    07/08/2011:
 *    
 *    - Full XML documentation. Updated HTML documentation will soon be available
 *      online at: <http://awesomium.com/docs/1_6_2/sharp_api/>
 *      
 *    - Improved auto-updating.
 *
 *    - Improved initialization logic.
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
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.InteropServices;
#if !USING_MONO
using System.Linq;
using System.Runtime.ExceptionServices;
#endif
#endregion

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{

    #region Enums
    public enum LogLevel
    {
        None,
        Normal,
        Verbose
    };

    public enum MouseButton
    {
        Left,
        Middle,
        Right
    };

    public enum URLFilteringMode
    {
        None,
        Blacklist,
        Whitelist
    };

    public enum WebKeyType
    {
        KeyDown,
        KeyUp,
        Char
    };

    public enum WebKeyModifiers
    {
        /// Whether or not a Shift key is down
        ShiftKey = 1 << 0,
        /// Whether or not a Control key is down
        ControlKey = 1 << 1,
        /// Whether or not an ALT key is down
        AltKey = 1 << 2,
        /// Whether or not a meta key (Command-key on Mac, Windows-key on Windows) is down
        MetaKey = 1 << 3,
        /// Whether or not the key pressed is on the keypad
        IsKeypad = 1 << 4,
        /// Whether or not the character input is the result of an auto-repeat timer.
        IsAutoRepeat = 1 << 5,
    };

    public enum CursorType
    {
        Pointer,
        Cross,
        Hand,
        Ibeam,
        Wait,
        Help,
        EastResize,
        NorthResize,
        NortheastResize,
        NorthwestResize,
        SouthResize,
        SoutheastResize,
        SouthwestResize,
        WestResize,
        NorthSouthResize,
        EastWestResize,
        NortheastSouthwestResize,
        NorthwestSoutheastResize,
        ColumnResize,
        RowResize,
        MiddlePanning,
        EastPanning,
        NorthPanning,
        NortheastPanning,
        NorthwestPanning,
        SouthPanning,
        SoutheastPanning,
        SouthwestPanning,
        WestPanning,
        Move,
        VerticalText,
        Cell,
        ContextMenu,
        Alias,
        Progress,
        NoDrop,
        Copy,
        None,
        NotAllowed,
        ZoomIn,
        ZoomOut,
        Custom
    };

    public enum IMEState
    {
        Disable = 0,
        MoveWindow = 1,
        CompleteComposition = 2
    };
    #endregion

    #region Structs
    /// <summary>
    /// Represents a generic keyboard event that can be created from a platform-specific event or 
    /// synthesized from a virtual event. Used by <see cref="WebView.InjectKeyboardEvent"/> and
    /// <see cref="Windows.Controls.WebControl.InjectKeyboardEvent"/>.
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct WebKeyboardEvent
    {
        /// <summary>
        /// The type of this <see cref="WebKeyboardEvent"/>.
        /// </summary>
        public WebKeyType Type;
        /// <summary>
        /// The current state of the keyboard. Modifiers may be OR'd together to represent multiple values.
        /// </summary>
        public WebKeyModifiers Modifiers;
        /// <summary>
        /// The virtual key-code associated with this keyboard event. 
        /// This is either directly from the event (ie, WPARAM on Windows) or via a mapping function.
        /// </summary>
        public VirtualKey VirtualKeyCode;
        /// <summary>
        /// The actual key-code generated by the platform. The DOM specification primarily uses 
        /// Windows-equivalent codes (hence virtualKeyCode above) but it helps to additionally 
        /// specify the platform-specific key-code as well.
        /// </summary>
        public int NativeKeyCode;
        /// <summary>
        /// The actual text generated by this keyboard event. 
        /// This is usually only a single character but we're generous and cap it at a max of 4 characters.
        /// </summary>
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
        public ushort[] Text;
        /// <summary>
        /// The text generated by this keyboard event before all modifiers except shift are applied. 
        /// This is used internally for working out shortcut keys. 
        /// This is usually only a single character but we're generous and cap it at a max of 4 characters.
        /// </summary>
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
        public ushort[] UnmodifiedText;
        /// <summary>
        /// Whether or not the pressed key is a "system key". 
        /// This is a Windows-only concept and should be "false" for all non-Windows platforms. 
        /// For more information, see the following link: http://msdn.microsoft.com/en-us/library/ms646286.aspx
        /// </summary>
        public bool IsSystemKey;
    };

    /// <summary>
    /// A simple rectangle class. Used with <see cref="WebView.GetDirtyBounds"/>, <see cref="Windows.Controls.WebControl.GetDirtyBounds"/>
    /// and various <see cref="RenderBuffer"/> methods.
    /// </summary>
    [StructLayout( LayoutKind.Sequential )]
    public struct AweRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    };
    #endregion


    /// <summary>
    /// The WebCore is the "core" of Awesomium; it manages the lifetime of all views
    /// (see <see cref="WebView"/> and <see cref="Windows.Controls.WebControl"/>) and maintains useful services
    /// like resource caching and network connections.
    /// </summary>
    /// <remarks>
    /// Generally, you should initialize the WebCore (<see cref="WebCore.Initialize"/>) providing
    /// your custom configuration, before creating any views and shut it down (<see cref="WebCore.Shutdown"/>)
    /// at the end of your program.
    /// @note
    /// <para>
    /// If you do not initialize <see cref="WebCore"/>, the core will automatically
    /// start, using default configuration, when you create the first view by either calling
    /// <see cref="WebCore.CreateWebview"/> or by instantiating a <see cref="Windows.Controls.WebControl"/>.
    /// </para>
    /// @warning
    /// <para>
    /// Do not call any of the members of this class (other than <see cref="WebCore.Initialize"/>
    /// or <see cref="WebCore.CreateWebview"/>) before starting the core.
    /// </para>
    /// @warning
    /// <para>
    /// Instances of classes in this assembly and their members, are not thread safe.
    /// </para>
    /// </remarks>
    public static class WebCore
    {
        #region Fields
#if DEBUG_AWESOMIUM
        internal const string DLLName = "Awesomium_d";
#else
        internal const string DLLName = "Awesomium";
#endif

        private static List<IWebView> activeWebViews;
        private static List<IntPtr> pendingWebViews = new List<IntPtr>();
        private static WebCoreConfig configuration;
        private static Timer updateTimer;
        private static int updatePeriod = 20;
        private static bool isRunning;
        private static bool isShuttingDown;
        private static SynchronizationContext syncCtx;
        #endregion


        #region Methods

        #region Internal

        #region VerifyLive
        internal static void VerifyLive()
        {
            if ( !isRunning )
                throw new InvalidOperationException( "The WebCore is not running. At least one view needs to be created before" );
        }
        #endregion

        #region DestroyView
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webview_destroy( IntPtr webview );

        internal static void DestroyView( IWebView view )
        {
            if ( isRunning && ( view.Instance != IntPtr.Zero ) )
            {
                if ( !isUpdating )
                    awe_webview_destroy( view.Instance );
                else
                    pendingWebViews.Add( view.Instance );

                RemoveView( view );
            }
        }
        #endregion

        #region RemoveView
        private static void RemoveView( IWebView view )
        {
            if ( !isShuttingDown )
            {
                if ( ( activeWebViews != null ) && activeWebViews.Contains( view ) )
                    activeWebViews.Remove( view );

                //if ( activeWebViews.Count == 0 )
                //    Shutdown();
            }
        }
        #endregion

        #endregion

        #region Public

        #region Initialize
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_webcore_initialize( bool enable_plugins,
            bool enable_javascript,
            IntPtr user_data_path,
            IntPtr plugin_path,
            IntPtr log_path,
            LogLevel log_level,
            bool enable_auto_detect_encoding,
            IntPtr accept_language_override,
            IntPtr default_charset_override,
            IntPtr user_agent_override,
            IntPtr proxy_server,
            IntPtr proxy_config_script,
            IntPtr auth_server_whitelist,
            bool save_cache_and_cookies,
            int max_cache_size,
            bool disable_same_origin_policy,
            bool disable_win_message_pump,
            IntPtr custom_css );

        /// <summary>
        /// Initializes the <see cref="WebCore"/> singleton with certain configuration settings.
        /// </summary>
        /// <param name="config">
        /// An instance of <see cref="WebCoreConfig"/> specifying configuration settings.
        /// </param>
        /// <param name="start">
        /// True if the <see cref="WebCore"/> should immediately start. False to perform lazy instantiation.
        /// The <see cref="WebCore"/> will start when the first view (<see cref="WebView"/> or <see cref="Windows.Controls.WebControl"/>)
        /// is created. The default is true.
        /// </param>
        /// <remarks>
        /// @note
        /// <para>
        /// If you do not call this method, the <see cref="WebCore"/> will start automatically,
        /// using default configuration settings, when you first create a view through <see cref="CreateWebview"/>
        /// or by instantiating a <see cref="Windows.Controls.WebControl"/>.
        /// </para>
        /// <para>
        /// If you are not sure if <see cref="WebCore"/> is running, check <see cref="IsRunning"/>
        /// before calling this method. If <see cref="WebCore"/> is running, you will have
        /// to shut it down (see <see cref="Shutdown"/>) and <b>restart the hosting application</b> before 
        /// initializing <see cref="WebCore"/> again. <see cref="WebCore"/> is a singleton. Only a single
        /// initialization/instantiation is available per application session.
        /// </para>
        /// @warning
        /// <para>
        /// For this (r148) test release, if you set <see cref="WebCoreConfig.SaveCacheAndCookies"/> to true, 
        /// please make sure that your hosting application is a single instance application, 
        /// unless you are sure that you provide a unique <see cref="WebCoreConfig.UserDataPath"/>
        /// for each of your application's instances.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called while <see cref="WebCore"/> is running.
        /// </exception>
        /// <seealso cref="IsRunning"/>
        public static void Initialize( WebCoreConfig config, bool start = true )
        {
            if ( isRunning )
                throw new InvalidOperationException( "The WebCore is already initialized. Call Shutdown() before initializing it again." );

            isShuttingDown = false;
            configuration = config;

            if ( start )
                Start();
        }

        private static void Start()
        {
            if ( !isRunning )
            {
                WebCoreConfig config = configuration ?? new WebCoreConfig { SaveCacheAndCookies = true, EnablePlugins = true };

                StringHelper userDataPathStr = new StringHelper( config.UserDataPath );
                StringHelper pluginPathStr = new StringHelper( config.PluginPath );
                StringHelper logPathStr = new StringHelper( config.LogPath );
                StringHelper acceptLanguageStr = new StringHelper( config.AcceptLanguageOverride );
                StringHelper defaultCharsetStr = new StringHelper( config.DefaultCharsetOverride );
                StringHelper userAgentOverrideStr = new StringHelper( config.UserAgentOverride );
                StringHelper proxyServerStr = new StringHelper( config.ProxyServer );
                StringHelper proxyConfigScriptStr = new StringHelper( config.ProxyConfigScript );
                StringHelper authServerWhitelistStr = new StringHelper( config.AuthServerWhitelist );
                StringHelper customCSSStr = new StringHelper( config.CustomCSS );

                awe_webcore_initialize( config.EnablePlugins,
                    config.EnableJavascript,
                    userDataPathStr.Value,
                    pluginPathStr.Value,
                    logPathStr.Value,
                    config.LogLevel,
                    config.EnableAutoDetectEncoding,
                    acceptLanguageStr.Value,
                    defaultCharsetStr.Value,
                    userAgentOverrideStr.Value,
                    proxyServerStr.Value,
                    proxyConfigScriptStr.Value,
                    authServerWhitelistStr.Value,
                    config.SaveCacheAndCookies,
                    config.MaxCacheSize,
                    config.DisableSameOriginPolicy,
                    config.DisableWinMessagePump,
                    customCSSStr.Value );

                updatePeriod = config.AutoUpdatePeriod;

                activeWebViews = new List<IWebView>();
                syncCtx = SynchronizationContext.Current;

                if ( updateTimer != null )
                {
                    updateTimer.Dispose();
                    updateTimer = null;
                }

                // We will not start auto-update unless we get a synchronization context.
                // Read the updated documentation of Update for details.
                if ( syncCtx != null )
                    updateTimer = new Timer( UpdateTimerCallback, null, 20, updatePeriod );

                isRunning = true;
            }
        }
        #endregion

        #region Shutdown
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_shutdown();

        /// <summary>
        /// Destroys the <see cref="WebCore"/> and any lingering <see cref="WebView"/>
        /// and <see cref="Windows.Controls.WebControl"/> instances.
        /// </summary>
        /// <remarks>
        /// Make sure that this is not called while the hosting UI of any views
        /// created by this <see cref="WebCore"/>, is still live and visible. 
        /// This method will destroy all views created by this <see cref="WebCore"/>.
        /// Any attempt to access them or any member of this class (other than <see cref="Initialize"/>
        /// and <see cref="CreateWebview"/>) after calling this method,
        /// may throw a <see cref="InvalidOperationException"/>.
        /// </remarks>
#if !USING_MONO
        [HandleProcessCorruptedStateExceptions]
#endif
        public static void Shutdown()
        {
            if ( isRunning )
            {
                isShuttingDown = true;

                // Stop the update timer.
                if ( updateTimer != null )
                {
                    updateTimer.Dispose();
                    updateTimer = null;
                }

                // Inform views by closing them.
                foreach ( IWebView i in activeWebViews )
                {
                    i.Close();
                }

                try
                {
                    // We may be attempting to shutdown from
                    // within an event handler, fired during Update.
                    // Let the update complete and queue the shutdown.
                    if ( !isUpdating )
                        awe_webcore_shutdown();
                    else
                        pendingShutdown = true;
                }
                catch { }
                finally
                {
                    activeWebViews.Clear();
                    activeWebViews = null;

                    isRunning = false;
                    isShuttingDown = false;
                }
            }
        }
        #endregion

        #region Update
        private static bool isUpdating;
        private static bool pendingShutdown;

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_update();

        /// <summary>
        /// Updates the <see cref="WebCore"/> and allows it to conduct various operations such
        /// as updating the render buffer of each view, destroying any views that are queued for destruction,
        /// and invoking any queued events (including <see cref="WebView.IsDirtyChanged"/> and 
        /// <see cref="Windows.Controls.WebControl.IsDirtyChanged"/>).
        /// </summary>
        /// <remarks>
        /// <para>
        /// If you are using Awesomium from a UI thread (regular use), you never need to call this method.
        /// Internal auto-update takes care of this and you only need to watch for the <see cref="WebView.IsDirtyChanged"/>
        /// or <see cref="Windows.Controls.WebControl.IsDirtyChanged"/> events. If you are using Awesomium from a
        /// non graphical environment (Console application, Service or non-UI thread), auto-update is not available and
        /// you must manually call this method from either your application's message loop or by creating a timer. 
        /// In this case, you must make sure that any calls to any of the classes of this assembly,
        /// are made from the same thread.
        /// </para>
        /// <para>
        /// @note
        /// You can check <see cref="IsAutoUpdateEnabled"/> to know if auto-update is already enabled.
        /// </para>
        /// <para>
        /// @warning
        /// Instances of classes in this assembly and their members, are not thread safe.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called before starting <see cref="WebCore"/>.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// You attempted to access the member from a thread other than
        /// thread where <see cref="WebCore"/> was created.
        /// </exception>
#if !USING_MONO
        [HandleProcessCorruptedStateExceptions]
#endif
        public static void Update()
        {
            VerifyLive();

            isUpdating = true;

            try
            {
                awe_webcore_update();

                // We may have attempted to destroy views from within
                // an event handler fired during awe_webcore_update.
                if ( pendingWebViews.Count > 0 )
                {
                    if ( isRunning )
                    {
                        foreach ( IntPtr instance in pendingWebViews )
                        {
                            awe_webview_destroy( instance );
                        }
                    }

                    pendingWebViews.Clear();
                }

                // We may have attempted to shutdown from within
                // an event handler fired during awe_webcore_update.
                if ( pendingShutdown )
                {
                    pendingShutdown = false;
                    awe_webcore_shutdown();
                }
            }
            catch
            {
                /* TODO: Design an error handling model. 
                 * AccessViolation is the most typical exception for the time being
                 * and it appears in many occasions; not only in wrong thread scenarios */
                Shutdown();
            }
            finally
            {
                isUpdating = false;
            }
        }
        #endregion

        #region SetBaseDirectory
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_set_base_directory( IntPtr base_dir_path );

        /// <summary>
        /// Sets the base directory all of your local assets.
        /// </summary>
        /// <param name="baseDirPath">
        /// The absolute path to your base directory. The base directory is a location that holds all of your local assets.
        /// It will be used with <see cref="WebView.LoadHTML"/>, <see cref="WebView.LoadFile"/>, 
        /// <see cref="Windows.Controls.WebControl.LoadHTML"/> and <see cref="Windows.Controls.WebControl.LoadFile"/>
        /// to resolve relative URLs.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called before starting <see cref="WebCore"/>.
        /// </exception>
        public static void SetBaseDirectory( string baseDirPath )
        {
            VerifyLive();

            StringHelper baseDirPathStr = new StringHelper( baseDirPath );
            awe_webcore_set_base_directory( baseDirPathStr.Value );
        }
        #endregion

        #region CreateWebview
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_webcore_create_webview( int width, int height, bool viewSource );

        /// <summary>
        /// Create a <see cref="WebView"/> (think of it like a tab in Chrome, you can load web-pages
        /// into it, interact with it, and render it to a buffer).
        /// </summary>
        /// <param name="width">The initial width of the view in pixels</param>
        /// <param name="height">The initial height of the view in pixels</param>
        /// <param name="viewSource">
        /// Enable View-Source mode on this <see cref="WebView"/> to view 
        /// the HTML source of any web-page (must be loaded via <see cref="WebView.LoadURL"/>)
        /// </param>
        /// <returns>
        /// A new <see cref="WebView"/> instance.
        /// </returns>
        /// <remarks>
        /// If you call this method before initializing the <see cref="WebCore"/>, the Awesomium
        /// process will automatically start with default configuration settings.
        /// </remarks>
        public static WebView CreateWebview( int width, int height, bool viewSource = false )
        {
            if ( !isRunning )
                Start();

            IntPtr webviewPtr = awe_webcore_create_webview( width, height, viewSource );
            WebView view = new WebView( webviewPtr );
            activeWebViews.Add( view );

            return view;
        }

        // Used by WebControl.
        internal static IntPtr CreateWebviewInstance( int width, int height, IWebView host )
        {
            if ( !isRunning )
                Start();

            IntPtr webviewPtr = awe_webcore_create_webview( width, height, false );
            activeWebViews.Add( host );
            return webviewPtr;
        }
        #endregion

        #region SetCustomResponsePage
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_set_custom_response_page( int status_code, IntPtr file_path );

        /// <summary>
        /// Sets a custom response page to use when a WebView encounters a certain HTML status code from the server (like '404 - File not found').
        /// </summary>
        /// <param name="statusCode">
        /// The status code this response page should be associated with. See <http://en.wikipedia.org/wiki/List_of_HTTP_status_codes>.
        /// </param>
        /// <param name="filePath">
        /// The local page to load as a response, should be a path relative to the base directory.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called before starting <see cref="WebCore"/>.
        /// </exception>
        public static void SetCustomResponsePage( int statusCode, string filePath )
        {
            VerifyLive();

            StringHelper filePathStr = new StringHelper( filePath );
            awe_webcore_set_custom_response_page( statusCode, filePathStr.Value );
        }
        #endregion

        #region ClearCache
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_clear_cache();

        /// <summary>
        /// Clears the disk cache and media cache.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called before starting <see cref="WebCore"/>.
        /// </exception>
        public static void ClearCache()
        {
            VerifyLive();
            awe_webcore_clear_cache();
        }
        #endregion

        #region ClearCookies
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_clear_cookies();

        /// <summary>
        /// Clears all stored cookies.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called before starting <see cref="WebCore"/>.
        /// </exception>
        public static void ClearCookies()
        {
            VerifyLive();
            awe_webcore_clear_cookies();
        }
        #endregion

        #region SetCookie
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_set_cookie( IntPtr url,
                                       IntPtr cookie_string,
                                       bool is_http_only,
                                       bool force_session_cookie );

        /// <summary>
        /// Sets a cookie for a certain URL.
        /// </summary>
        /// <param name="url">
        /// The URL to set the cookie on.
        /// </param>
        /// <param name="cookieString">
        /// The cookie string, for example:
        /// <example>
        /// "key1=value1; key2=value2"
        /// </example>
        /// </param>
        /// <param name="isHttpOnly">
        /// Whether or not this cookie is HTTP-only.
        /// </param>
        /// <param name="forceSessionCookie">
        /// Whether or not to force this as a session cookie.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called before starting <see cref="WebCore"/>.
        /// </exception>
        public static void SetCookie( string url, string cookieString, bool isHttpOnly = false, bool forceSessionCookie = false )
        {
            VerifyLive();

            StringHelper urlStr = new StringHelper( url );
            StringHelper cookieStringStr = new StringHelper( cookieString );

            awe_webcore_set_cookie( urlStr.Value, cookieStringStr.Value, isHttpOnly, forceSessionCookie );
        }
        #endregion

        #region GetCookies
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_webcore_get_cookies( IntPtr url, bool exclude_http_only );

        /// <summary>
        /// Gets all cookies for a certain URL.
        /// </summary>
        /// <param name="url">
        /// The URL whose cookies will be retrieved.
        /// </param>
        /// <param name="excludeHttpOnly">
        /// Whether or not to exclude HTTP-only cookies from the result.
        /// </param>
        /// <returns>
        /// A <see cref="String"/> representing the cookie.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The member is called before starting <see cref="WebCore"/>.
        /// </exception>
        public static String GetCookies( string url, bool excludeHttpOnly = true )
        {
            VerifyLive();

            StringHelper urlStr = new StringHelper( url );
            IntPtr temp = awe_webcore_get_cookies( urlStr.Value, excludeHttpOnly );

            return StringHelper.ConvertAweString( temp );
        }
        #endregion

        #region DeleteCookie
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_delete_cookie( IntPtr url, IntPtr cookie_name );

        /// <summary>
        /// Deletes a certain cookie on a certain URL.
        /// </summary>
        /// <param name="url">
        /// The URL that we will be deleting cookies on.
        /// </param>
        /// <param name="cookieName">
        /// The name of the cookie that will be deleted.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called before starting <see cref="WebCore"/>.
        /// </exception>
        public static void DeleteCookie( string url, string cookieName )
        {
            VerifyLive();

            StringHelper urlStr = new StringHelper( url );
            StringHelper cookieNameStr = new StringHelper( cookieName );

            awe_webcore_delete_cookie( urlStr.Value, cookieNameStr.Value );
        }
        #endregion

        #region SuppressPrinterDialog
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_set_suppress_printer_dialog( bool suppress );

        /// <summary>
        /// Set whether or not the printer dialog should be suppressed or not.
        /// Set this to true to hide printer dialogs and print immediately
        /// using the OS's default printer when <see cref="WebView.Print"/> or
        /// <see cref="Windows.Controls.WebControl.Print"/> is called.
        /// The default is false is you never call this.
        /// </summary>
        /// <param name="suppress">
        /// True to suppress the dialog. False otherwise.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called before starting <see cref="WebCore"/>.
        /// </exception>
        public static void SuppressPrinterDialog( bool suppress )
        {
            VerifyLive();
            awe_webcore_set_suppress_printer_dialog( suppress );
        }
        #endregion

        #endregion

        #endregion

        #region Properties

        #region SynchronizationContext
        internal static SynchronizationContext SynchronizationContext
        {
            get
            {
                if ( syncCtx == null )
                    syncCtx = SynchronizationContext.Current;

                return syncCtx;
            }
        }
        #endregion

        #region PluginsEnabled
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_webcore_are_plugins_enabled();

        /// <summary>
        /// Returns whether or not plugins are enabled.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is accessed before starting <see cref="WebCore"/>.
        /// </exception>
        public static bool PluginsEnabled
        {
            get
            {
                VerifyLive();
                return awe_webcore_are_plugins_enabled();
            }
        }
        #endregion

        #region BaseDirectory
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_webcore_get_base_directory();

        /// <summary>
        /// Gets or sets the base directory (used with <see cref="WebView.LoadHTML"/>)
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is accessed before starting <see cref="WebCore"/>.
        /// </exception>
        public static string BaseDirectory
        {
            get
            {
                VerifyLive();
                return StringHelper.ConvertAweString( awe_webcore_get_base_directory() );
            }
            set
            {
                VerifyLive();
                StringHelper baseDirPathStr = new StringHelper( value );
                awe_webcore_set_base_directory( baseDirPathStr.Value );
            }
        }
        #endregion

        #region IsRunning
        /// <summary>
        /// Gets if the <see cref="WebCore"/> is currently running.
        /// </summary>
        /// <seealso cref="Initialize"/>
        public static bool IsRunning
        {
            get
            {
                return isRunning;
            }
        }
        #endregion

        #region IsShuttingDown
        /// <summary>
        /// Gets if the WebCore is currently shutting down.
        /// </summary>
        public static bool IsShuttingDown
        {
            get
            {
                return isShuttingDown;
            }
        }
        #endregion

        #region IsAutoUpdateEnabled
        /// <summary>
        /// Gets if automatic update is successfully enabled.
        /// </summary>
        /// <seealso cref="Update"/>
        public static bool IsAutoUpdateEnabled
        {
            get
            {
                return ( updateTimer != null );
            }
        }
        #endregion

        #region AutoUpdatePeriod
        /// <summary>
        /// Gets or sets the time interval between invocations of <see cref="WebCore.Update"/>, in milliseconds.
        /// The default is 20.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Attempted to set this before starting <see cref="WebCore"/>.
        /// </exception>
        public static int AutoUpdatePeriod
        {
            get
            {
                return updatePeriod;
            }
            set
            {
                VerifyLive();

                if ( updatePeriod == value )
                    return;

                updatePeriod = value;

                if ( updateTimer != null )
                    updateTimer.Change( 0, value );
            }
        }
        #endregion

        #endregion

        #region Event Handlers
        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_webview_is_dirty( IntPtr webview );

        private static void UpdateTimerCallback( object state )
        {
            if ( syncCtx != null )
            {
                // Wait for the previous update to complete before you post another.
                if ( !isUpdating )
                    // API calls should normally be thread safe but they are not.
                    // We need the synchronization context to marshal calls.
                    syncCtx.Post( UpdateSync, state );
            }
            else if ( updateTimer != null )
            {
                // We should not be here anyway!
                updateTimer.Dispose();
                updateTimer = null;
            }
        }

        private static void UpdateSync( object state )
        {
            // Prevent race condition. We use Post, not Send.
            if ( !isRunning || isShuttingDown || isUpdating )
                return;

            Update();

            if ( activeWebViews != null )
            {
                foreach ( IWebView view in activeWebViews )
                {
                    if ( view.Instance != IntPtr.Zero )
                        view.IsDirty = awe_webview_is_dirty( view.Instance );
                }
            }
        }
        #endregion
    }



#region Doxygen Intro
    /**
 * @mainpage AwesomiumSharp API
 *
 * @section intro_sec Introduction
 *
 * Hi there, welcome to the Awesomium .NET API docs! Awesomium is a software 
 * library that makes it easy to put the web in your applications. Whether 
 * that means embedded web browsing, rendering pages as images, streaming 
 * pages over the net, or manipulating web content live for some other 
 * purpose, Awesomium does it all.
 *
 * If this is your first time exploring the API, we recommend
 * starting with <see cref="WebCore"/> and <see cref="WebView"/>.
 * 
 * Here's a simple example of using the API to render a page once:
 * <code>
 *   using ( webView = WebCore.CreateWebview( 800, 600 ) )
 *   {
 *       webView.LoadURL( "http://www.google.com" );
 *
 *       while ( webView.IsLoadingPage )
 *           WebCore.Update();
 *
 *       webView.Render().SaveToPNG( "result.png", true );
 *   }
 *
 *   WebCore.Shutdown();
 * </code>
 * 
 * If you are interested in just adding a standalone <see cref="WebView"/> to your
 * WPF application with minimal work, take a look at <see cref="Windows.Controls.WebControl"/>
 * (it should be available in your Toolbox if you add a reference to
 * AwesomiumSharp in your project, just drag-and-drop and you're done).
 *
 * For more help and tips with the API, please visit our Knowledge Base
 *     <http://support.awesomium.com/faqs>
 *     
 * @note
 * <para>
 * This version of AwesomiumSharp & AwesomiumMono, is compiled against
 * the new test release of Awesomium: r148 (tentatively, 1.6.2).
 * You can get this release from:
 *     <http://www.awesomium.com/downloads/awesomium_r148_sdk_win.zip>
 * </para>
 * 
 * @note
 * <para>
 * You can get the source of this pre-release version of AwesomiumSharp
 * from Github (follow the link bellow). This is the last AwesomiumSharp 
 * version before the official version that will be released alongside
 * Awesomium 1.6.2, incorporating all new features of Awesomium 1.6.2.
 * It is provided for testing purposes.
 *</para>
 *
 * @warning
 * <para>
 * For this (r148) test release, if you set <see cref="WebCoreConfig.SaveCacheAndCookies"/>
 * to true during initialization of <see cref="WebCore"/>, please make sure 
 * that your hosting application is a single instance application, 
 * unless you are sure that you provide a unique <see cref="WebCoreConfig.UserDataPath"/>
 * for each of your application's instances.
 * </para>
 *
 * @section usefullinks_sec Useful Links
 * - AwesomimSharp on GitHub: <https://github.com/khrona/AwesomiumSharp>
 * - Awesomium Main: <http://www.awesomium.com>
 * - Support Home: <http://support.awesomium.com>
 * 
 * @section copyright_sec Copyright
 * This documentation is copyright (C) 2011 Khrona. All rights reserved. 
 * Awesomium is a trademark of Khrona.
 */
#endregion

}
