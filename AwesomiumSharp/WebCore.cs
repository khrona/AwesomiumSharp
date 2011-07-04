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

        internal IntPtr value()
        {
            return aweString;
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

    #region WebCoreConfig
    /// <summary>
    /// Configuration settings for the WebCore
    /// </summary>
    public class WebCoreConfig
    {
        public WebCoreConfig()
        {
            EnableJavascript = true;
            UserDataPath = "";
            PluginPath = "";
            LogPath = "";
            LogLevel = LogLevel.Normal;
            EnableAutoDetectEncoding = true;
            AcceptLanguageOverride = "";
            DefaultCharsetOverride = "";
            UserAgentOverride = "";
            ProxyServer = "";
            ProxyConfigScript = "";
            CustomCSS = "";

            AutoUpdatePeriod = 30;
        }

        /// <summary>
        /// Indicates whether or not to enable embedded plugins (e.g., Flash).
        /// The default is false.
        /// </summary>
        public bool EnablePlugins { get; set; }
        /// <summary>
        /// Indicates whether or not Javascript is enabled. The default is True.
        /// </summary>
        public bool EnableJavascript { get; set; }
        /// <summary>
        /// Indicates the path to the directory that will be used to store cache, cookies, and other data. 
        /// If an empty string is specified, this path defaults to "./Default".
        /// </summary>
        public string UserDataPath { get; set; }
        /// <summary>
        /// Indicates an absolute path that will be included in the search for plugins. 
        /// This is useful if you wish to bundle certain plugins with your application.
        /// </summary>
        public string PluginPath { get; set; }
        /// <summary>
        /// Indicates the path to store the awesomium.log. 
        /// If none is specified, the log will be stored in the working directory.
        /// </summary>
        public string LogPath { get; set; }
        /// <summary>
        /// Indicates the logging level to use, this can be either <see cref="LogLevel.None"/>, 
        /// <see cref="LogLevel.Normal"/>, or <see cref="LogLevel.Verbose"/>.
        /// The default is <see cref="LogLevel.Normal"/>.
        /// </summary>
        public LogLevel LogLevel { get; set; }
        public bool EnableAutoDetectEncoding { get; set; }
        public string AcceptLanguageOverride { get; set; }
        public string DefaultCharsetOverride { get; set; }
        /// <summary>
        /// Indicates the user agent string that will be used to override the default. 
        /// Leave this empty to use the default user agent.
        /// </summary>
        public string UserAgentOverride { get; set; }
        /// <summary>
        /// Indicates the proxy settings for all network requests. 
        /// Specify "none" to disable all proxy use, specify "auto" to attempt to detect the proxy using system settings 
        /// (e.g., via the Internet Properties dialog on Windows or the Network panel of System Preferences on Mac OSX). 
        /// Specify anything else to set manual proxy settings.
        /// </summary>
        /// <example>
        ///    "none"                         -- No proxy. (Default).
        ///    "auto"                         -- Detect system proxy settings.
        ///    "http=myproxy:80;ftp=myproxy2" -- Use HTTP proxy "myproxy:80"  
        ///                                      for http:// URLs, and HTTP proxy 
        ///                                      "myproxy2:80" for ftp:// URLs.
        ///    "myproxy:80"                   -- Use HTTP proxy "foopy:80" for
        ///                                      all URLs.
        ///    "socks4://myproxy"             -- Use SOCKS v4 proxy "foopy:1080" 
        ///                                      for all URLs.
        /// </example>
        public string ProxyServer { get; set; }
        /// <summary>
        /// Indicates the URL to the PAC (Proxy Auto-Config) Script to use. See <http://en.wikipedia.org/wiki/Proxy_auto-config> for more info.
        /// </summary>
        public string ProxyConfigScript { get; set; }
        /// <summary>
        /// Indicates whether or not cache and cookies should be saved to disk.
        /// </summary>
        public bool SaveCacheAndCookies { get; set; }
        /// <summary>
        /// Indicates the maximum disk space to be used by the disk cache, in bytes. Specify 0 to use no limit.
        /// </summary>
        public int MaxCacheSize { get; set; }
        /// <summary>
        /// Indicates whether or not the "same-origin" policy should be disabled. 
        /// See <http://en.wikipedia.org/wiki/Same_origin_policy>.
        /// </summary>
        public bool DisableSameOriginPolicy { get; set; }
        /// <summary>
        /// Indicates 
        /// </summary>
        public bool DisableWinMessagePump { get; set; }
        /// <summary>
        /// Indicates a string of custom CSS to be included as the global default style for all pages. 
        /// This is especially useful for customizing scrollbars and other look-and-feel elements.
        /// </summary>
        public string CustomCSS { get; set; }


        // The following are added and refer to the wrapper.
        // Properties are also added for these in WebCore.

        /// <summary>
        /// Indicates if automatic update is enabled.
        /// </summary>
        /// <seealso cref="WebCore.Update"/>
        public bool AutoUpdate { get; set; }
        /// <summary>
        /// Indicates the time interval between invocations of <see cref="WebCore.Update"/>, in milliseconds.
        /// The default is 20.
        /// </summary>
        /// <seealso cref="WebCoreConfig.AutoUpdate"/>
        public int AutoUpdatePeriod { get; set; }
    };
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

        internal static List<IWebView> activeWebViews;
        private static Timer updateTimer;
        private static int updatePeriod;
        private static bool isInitialized;
        private static bool isShuttingDown;
        private static SynchronizationContext syncCtx;
        #endregion


        #region Methods

        #region VerifyInitialized
        internal static void VerifyInitialized()
        {
            if ( !isInitialized )
                throw new InvalidOperationException( "The WebCore is not initialized." );
        }
        #endregion

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
        /// Create the WebCore singleton with certain configuration settings. You must call
        /// this before creating any WebViews.
        /// </summary>
        /// <param name="config"></param>
        /// <exception cref="InvalidOperationException">
        /// The member is called while WebCore is already initialized.
        /// </exception>
        public static void Initialize( WebCoreConfig config )
        {
            if ( isInitialized )
                throw new InvalidOperationException( "The WebCore is already initialized. Call Shutdown() before initializing it again." );

            isShuttingDown = false;

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
                                     userDataPathStr.value(),
                                     pluginPathStr.value(),
                                     logPathStr.value(),
                                     config.LogLevel,
                                     config.EnableAutoDetectEncoding,
                                     acceptLanguageStr.value(),
                                     defaultCharsetStr.value(),
                                     userAgentOverrideStr.value(),
                                     proxyServerStr.value(),
                                     proxyConfigScriptStr.value(),
                                     config.SaveCacheAndCookies,
                                     config.MaxCacheSize,
                                     config.DisableSameOriginPolicy,
                                     config.DisableWinMessagePump,
                                     customCSSStr.value() );

            activeWebViews = new List<IWebView>();
            syncCtx = SynchronizationContext.Current;

            if ( config.AutoUpdate )
            {
                if ( syncCtx == null )
                    throw new ArgumentException( "WebCore cannot initiate AutoUpdate. A valid synchronization context could not be obtained." );

                if ( updateTimer != null )
                    updateTimer.Dispose();

                updateTimer = new Timer( UpdateTimerCallback, null, 1000, config.AutoUpdatePeriod );
            }

            isInitialized = true;
        }
        #endregion

        #region Shutdown
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_shutdown();

        /// <summary>
        /// Destroys the WebCore and destroys any lingering WebView instances.
        /// </summary>
        public static void Shutdown()
        {
            if ( isInitialized )
            {
                isShuttingDown = true;

                foreach ( IWebView i in activeWebViews )
                {
                    i.PrepareForShutdown();
                }

                awe_webcore_shutdown();

                activeWebViews.Clear();
                activeWebViews = null;

                isInitialized = false;
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
            VerifyInitialized();

            StringHelper baseDirPathStr = new StringHelper( baseDirPath );
            awe_webcore_set_base_directory( baseDirPathStr.value() );
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
        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
        /// </exception>
        public static WebView CreateWebview( int width, int height, bool viewSource = false )
        {
            VerifyInitialized();

            IntPtr webviewPtr = awe_webcore_create_webview( width, height, viewSource );
            WebView view = new WebView( webviewPtr );
            activeWebViews.Add( view );

            return view;
        }

        internal static IntPtr CreateWebviewInstance( int width, int height )
        {
            VerifyInitialized();
            IntPtr webviewPtr = awe_webcore_create_webview( width, height, false );
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
            VerifyInitialized();

            StringHelper filePathStr = new StringHelper( filePath );
            awe_webcore_set_custom_response_page( statusCode, filePathStr.value() );
        }
        #endregion

        #region Update
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_webcore_update();

        /// <summary>
        /// Updates the WebCore and allows it to conduct various operations such
        /// as updating the RenderBuffer of each WebView, destroying any
        /// WebViews that are queued for destruction, and invoking any queued WebView events.
        /// You should call this as part of your application's update loop.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The member is called before initializing WebCore.
        /// </exception>
        public static void Update()
        {
            VerifyInitialized();

            try
            {
                awe_webcore_update();
            }
            catch { /* TODO: Design an error handling model. 
                     * AccessViolation is the most typical exception for the time being
                     * and it appears in many places. */ }
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
            VerifyInitialized();
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
            VerifyInitialized();
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
            VerifyInitialized();

            StringHelper urlStr = new StringHelper( url );
            StringHelper cookieStringStr = new StringHelper( cookieString );

            awe_webcore_set_cookie( urlStr.value(), cookieStringStr.value(), isHttpOnly, forceSessionCookie );
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
            VerifyInitialized();

            StringHelper urlStr = new StringHelper( url );
            IntPtr temp = awe_webcore_get_cookies( urlStr.value(), excludeHttpOnly );

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
            VerifyInitialized();

            StringHelper urlStr = new StringHelper( url );
            StringHelper cookieNameStr = new StringHelper( cookieName );

            awe_webcore_delete_cookie( urlStr.value(), cookieNameStr.value() );
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
            VerifyInitialized();
            awe_webcore_set_suppress_printer_dialog( suppress );
        }
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
                VerifyInitialized();
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
                VerifyInitialized();
                return StringHelper.ConvertAweString( awe_webcore_get_base_directory() );
            }
            set
            {
                VerifyInitialized();
                StringHelper baseDirPathStr = new StringHelper( value );
                awe_webcore_set_base_directory( baseDirPathStr.value() );
            }
        }
        #endregion

        #region IsInitialized
        /// <summary>
        /// Gets if the <see cref="WebCore"/> is already initialized and views can be created.
        /// </summary>
        public static bool IsInitialized
        {
            get
            {
                return isInitialized;
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

        #region AutoUpdate
        /// <summary>
        /// Gets or sets if automatic update is enabled.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Attempted to set this before initializing WebCore.
        /// </exception>
        public static bool AutoUpdate
        {
            get
            {
                return ( updateTimer != null );
            }
            set
            {
                VerifyInitialized();

                if ( value && ( updateTimer == null ) )
                {
                    if ( WebCore.SynchronizationContext == null )
                        throw new InvalidOperationException( "WebCore cannot initiate AutoUpdate. A valid synchronization context could not be obtained." );

                    updateTimer = new Timer( UpdateTimerCallback, null, 0, AutoUpdatePeriod );
                }
                else if ( updateTimer != null )
                {
                    updateTimer.Dispose();
                    updateTimer = null;
                }
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
                VerifyInitialized();

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
            // API calls should normally be thread safe but they are not.
            // We need the synchronization context to marshal calls.
            syncCtx.Post( UpdateSync, state );
        }

        private static void UpdateSync( object state )
        {
            Update();

            if ( activeWebViews != null )
            {
                foreach ( IWebView view in activeWebViews )
                {
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
 *   WebCore.Initialize(new WebCore.Config());
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
