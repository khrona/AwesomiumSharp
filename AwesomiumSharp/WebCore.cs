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
 *-------------------------------------------------------------------------------
 *
 *    !Changes may need to be tested with AwesomiumMono. I didn't test them!
 * 
 ********************************************************************************/

#region Using
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
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
    [StructLayout( LayoutKind.Sequential )]
    public struct WebKeyboardEvent
    {
        public WebKeyType Type;
        public int Modifiers;
        public int VirtualKeyCode;
        public int NativeKeyCode;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
        public ushort[] Text;
        [MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
        public ushort[] UnmodifiedText;
        public bool IsSystemKey;
    };

    [StructLayout( LayoutKind.Sequential )]
    public struct AweRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    };
    #endregion

    #region StringHelper
    internal class StringHelper
    {
        private IntPtr aweString;

        public StringHelper( string val )
        {
            byte[] utf16string = Encoding.Unicode.GetBytes( val );
            aweString = awe_string_create_from_utf16( utf16string, (uint)val.Length );
        }

        ~StringHelper()
        {
            awe_string_destroy( aweString );
        }

        internal IntPtr Value
        {
            get
            {
                return aweString;
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        internal static extern IntPtr awe_string_empty();

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        internal static extern IntPtr awe_string_create_from_utf16( byte[] str, uint len );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        internal static extern void awe_string_destroy( IntPtr str );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern uint awe_string_get_length( IntPtr str );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_string_get_utf16( IntPtr str );

        public static string ConvertAweString( IntPtr aweStr, bool shouldDestroy = false )
        {
            byte[] stringBytes = new byte[ awe_string_get_length( aweStr ) * 2 ];
            Marshal.Copy( awe_string_get_utf16( aweStr ), stringBytes, 0, (int)awe_string_get_length( aweStr ) * 2 );

            if ( shouldDestroy )
                awe_string_destroy( aweStr );

            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();

            return unicodeEncoding.GetString( stringBytes );
        }
    }
    #endregion

    /// <summary>
    /// The WebCore is the "core" of Awesomium; it manages the lifetime of
    /// all WebViews (see AwesomiumSharp::WebView) and maintains useful services
    /// like resource caching and network connections.
    /// Generally, you should create an instance of the WebCore (WebCore.Initialize) at the
    /// beginning of your program and then destroy the instance (WebCore.Shutdown) at the end
    /// of your program.
    /// </summary>
    public static class WebCore
    {
        #region Fields
#if DEBUG_AWESOMIUM
        internal const string DLLName = "Awesomium_d";
#else
        internal const string DLLName = "Awesomium";
#endif

        private static List<IWebView> activeWebViews;
        private static WebCoreConfig configuration;
        private static Timer updateTimer;
        private static int updatePeriod;
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
            if ( view.Instance != IntPtr.Zero )
                awe_webview_destroy( view.Instance );

            if ( activeWebViews != null )
            {
                if ( !isShuttingDown && activeWebViews.Contains( view ) )
                    activeWebViews.Remove( view );

                if ( activeWebViews.Count == 0 )
                    Shutdown();
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
            bool save_cache_and_cookies,
            int max_cache_size,
            bool disable_same_origin_policy,
            bool disable_win_message_pump,
            IntPtr custom_css );

        /// <summary>
        /// Initializes the WebCore singleton with certain configuration settings. You must call
        /// this before creating any WebViews or WebControls.
        /// </summary>
        /// <param name="config">
        /// An instance of <see cref="WebCoreConfig"/> specifying configuration settings.
        /// </param>
        /// <param name="start">
        /// True if the Awesomium process should immediately start. False to start the process
        /// when the first view is created. The default is true.
        /// </param>
        /// <remarks>
        /// <para>
        /// If <paramref name="start"/> is set to false, you may call this method and re-initialize
        /// WebCore by providing new configuration settings for as long as no view has been
        /// created and WebCore has not started. You have to keep defining false for the
        /// <paramref name="start"/> to prevent <see cref="WebCore"/> from starting.
        /// </para>
        /// <para>
        /// If you are not sure if <see cref="WebCore"/> is running, check <see cref="IsRunning"/>
        /// before calling this method. If <see cref="WebCore"/> is running, you will have
        /// to shut it down by closing all views and their respective hosts, or by calling
        /// <see cref="Shutdown"/> before initializing the <see cref="WebCore"/> again.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called while WebCore is running.
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
            WebCoreConfig config = configuration ?? new WebCoreConfig { SaveCacheAndCookies = true, EnablePlugins = true };

            StringHelper userDataPathStr = new StringHelper( config.UserDataPath );
            StringHelper pluginPathStr = new StringHelper( config.PluginPath );
            StringHelper logPathStr = new StringHelper( config.LogPath );
            StringHelper acceptLanguageStr = new StringHelper( config.AcceptLanguageOverride );
            StringHelper defaultCharsetStr = new StringHelper( config.DefaultCharsetOverride );
            StringHelper userAgentOverrideStr = new StringHelper( config.UserAgentOverride );
            StringHelper proxyServerStr = new StringHelper( config.ProxyServer );
            StringHelper proxyConfigScriptStr = new StringHelper( config.ProxyConfigScript );
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
                config.SaveCacheAndCookies,
                config.MaxCacheSize,
                config.DisableSameOriginPolicy,
                config.DisableWinMessagePump,
                customCSSStr.Value );

            activeWebViews = new List<IWebView>();
            syncCtx = SynchronizationContext.Current;

            if ( updateTimer != null )
                updateTimer.Dispose();

            // We will not start auto-update unless we get a synchronization context.
            // Read the updated documentation of Update for details.
            if ( syncCtx != null )
                updateTimer = new Timer( UpdateTimerCallback, null, 20, config.AutoUpdatePeriod );

            isRunning = true;
        }
        #endregion

        #region Shutdown
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_shutdown();

        /// <summary>
        /// Destroys the WebCore and any lingering WebView instances.
        /// </summary>
        /// <remarks>
        /// Make sure that this is not called while the hosting UI of any views
        /// created by this <see cref="WebCore"/>, is still live and visible. 
        /// This method will destroy all views created by this <see cref="WebCore"/>.
        /// Any attempt to access them or any member of this class (other than <see cref="Initialize"/>)
        /// after calling this method, may throw a <see cref="InvalidOperationException"/>.
        /// </remarks>
        public static void Shutdown()
        {
            if ( isRunning )
            {
                isShuttingDown = true;

                if ( updateTimer != null )
                    updateTimer.Dispose();

                foreach ( IWebView i in activeWebViews )
                {
                    i.PrepareForShutdown();
                }

                try
                {
                    awe_webcore_shutdown();
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
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_update();

        /// <summary>
        /// Updates the WebCore and allows it to conduct various operations such
        /// as updating the RenderBuffer of each WebView, destroying any
        /// WebViews that are queued for destruction, and invoking any queued WebView events.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If you are using Awesomium from a UI thread (regular use), you never need to
        /// call this method. Internal auto-update takes care of this and you only need to
        /// watch for the IsDirtyChanged event of WebView or WebControl. If you are
        /// using Awesomium from a non graphical environment (Console application, Service
        /// or non-UI thread, auto-update is not available and you must manually call this method
        /// from either your application message loop or by creating a timer. In such a case,
        /// you must make sure that any calls to any of the classes of this assembly,
        /// are called from the same thread.
        /// </para>
        /// <para>
        /// Instances of AwesomiumSharp and their members, are not thread safe.
        /// </para>
        /// <para>
        /// You can check <see cref="IsAutoUpdateEnabled"/> to know if auto-update 
        /// is already enabled.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
        /// </exception>
        /// <exception cref="AccessViolationException">
        /// You attempted to access the member from a thread other than
        /// thread where <see cref="WebCore"/> was created.
        /// </exception>
        public static void Update()
        {
            VerifyLive();

            try
            {
                awe_webcore_update();
            }
            catch
            { 
                /* TODO: Design an error handling model. 
                 * AccessViolation is the most typical exception for the time being
                 * and it appears in many occasions; not only in wrong thread scenarios */
            }
        }
        #endregion

        #region SetBaseDirectory
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_set_base_directory( IntPtr base_dir_path );

        /// <summary>
        /// Sets the base directory (used with WebView.LoadHTML)
        /// </summary>
        /// <param name="baseDirPath"></param>
        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
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
        /// Create a WebView (think of it like a tab in Chrome, you can load web-pages
        /// into it, interact with it, and render it to a buffer).
        /// </summary>
        /// <param name="width">the initial width (pixels)</param>
        /// <param name="height">the initial height (pixels)</param>
        /// <param name="viewSource">Enable View-Source mode on this WebView to view the
        /// HTML source of any web-page (must be loaded via WebView.LoadURL)</param>
        /// <returns>Returns a new WebView instance</returns>
        public static WebView CreateWebview( int width, int height, bool viewSource = false )
        {
            if ( !IsRunning )
                Start();

            IntPtr webviewPtr = awe_webcore_create_webview( width, height, viewSource );
            WebView view = new WebView( webviewPtr );
            activeWebViews.Add( view );

            return view;
        }

        // Used by WebControl.
        internal static IntPtr CreateWebviewInstance( int width, int height, IWebView host )
        {
            if ( !IsRunning )
                Start();

            IntPtr webviewPtr = awe_webcore_create_webview( width, height, false );
            activeWebViews.Add( host );
            return webviewPtr;
        }
        #endregion

        #region SetCustomResponsePage
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_set_custom_response_page( int status_code, IntPtr file_path );

        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
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

        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
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

        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
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

        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
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

        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
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

        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
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
        /// using the OS's default printer when WebView.Print is called.
        /// The default is false is you never call this.
        /// </summary>
        /// <param name="suppress">
        /// True to suppress the dialog. False otherwise.
        /// </param>
        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
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

        /// <exception cref="InvalidOperationException">
        /// The member is accessed before initializing WebCore.
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
        /// The member is accessed before initializing WebCore.
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
        /// Attempted to set this before initializing WebCore.
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
                // API calls should normally be thread safe but they are not.
                // We need the synchronization context to marshal calls.
                syncCtx.Post( UpdateSync, state );
            }
            else if ( updateTimer != null )
            {
                // We should not be here anyway!
                updateTimer.Dispose();
            }
        }

        private static void UpdateSync( object state )
        {
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

}


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
 * starting with AwesomiumSharp::WebCore and AwesomiumSharp::WebView.
 * 
 * Here's a simple example of using the API to render a page once:
 * <pre>
 *   WebCore.Initialize(new WebCoreConfig());
 *
 *   WebView webView = WebCore.CreateWebview(800, 600);
 *   webView.LoadURL("http://www.google.com");
 *
 *   while (webView.IsLoadingPage)
 *       WebCore.Update();
 *
 *   webView.Render().SaveToPNG("result.png", true);
 *
 *   WebCore.Shutdown();
 * </pre>
 * 
 * If you are interested in just adding a standalone WebView to your
 * WPF application with minimal work, take a look at AwesomiumSharp::WebControl
 * (it should be available in your Toolbox if you add a reference to
 * AwesomiumSharp in your project, just drag-and-drop and you're done).
 *
 * For more help and tips with the API, please visit our Knowledge Base
 *     <http://support.awesomium.com/faqs>
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
