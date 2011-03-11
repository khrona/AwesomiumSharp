using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;

namespace AwesomiumSharp
{
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

    [StructLayout(LayoutKind.Sequential)]
    public struct WebKeyboardEvent
    {
        public WebKeyType type;
        public WebKeyModifiers modifiers;
        public int virtualKeyCode;
        public int nativeKeyCode;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] text;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public ushort[] unmodifiedText;
        public bool isSystemKey;
    };

    public struct Rect
    {
        int x;
        int y;
        int width;
        int height;
    };

    internal class StringHelper
    {
        private IntPtr aweString;

        public StringHelper(string val)
        {
            byte[] utf16string = Encoding.Unicode.GetBytes(val);
            aweString = awe_string_create_from_utf16(utf16string, (uint)val.Length);
        }

        ~StringHelper()
        {
            awe_string_destroy(aweString);
        }

        internal IntPtr value()
        {
            return aweString;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_string_empty();

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_string_create_from_utf16(byte[] str, uint len);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_string_destroy(IntPtr str);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint awe_string_get_length(IntPtr str);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_string_get_utf16(IntPtr str);

        public static string ConvertAweString(IntPtr aweStr)
        {
            byte[] stringBytes = new byte[awe_string_get_length(aweStr) * 2];
            Marshal.Copy(awe_string_get_utf16(aweStr), stringBytes, 0, (int)awe_string_get_length(aweStr) * 2);

            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();

            return unicodeEncoding.GetString(stringBytes);
        }
    }

    public class WebCore
    {
#if DEBUG_AWESOMIUM
        internal const string DLLName = "Awesomium_d.dll";
#else
        internal const string DLLName = "Awesomium.dll";
#endif

        internal static List<Object> activeWebViews;

        public class Config
        {
            public bool enablePlugins = false;
            public bool enableJavascript = true;
            public string userDataPath = "";
            public string pluginPath = "";
            public string logPath = "";
            public LogLevel logLevel = LogLevel.Normal;
            public string userAgentOverride = "";
            public string proxyServer = "";
            public string proxyConfigScript = "";
            public bool saveCacheAndCookies = false;
            public int maxCacheSize = 0;
            public bool disableSameOriginPolicy = false;
            public string customCSS = "";
        };

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webcore_initialize(bool enable_plugins,
                                       bool enable_javascript,
                                       IntPtr user_data_path,
                                       IntPtr plugin_path,
                                       IntPtr log_path,
                                       LogLevel log_level,
                                       IntPtr user_agent_override,
                                       IntPtr proxy_server,
                                       IntPtr proxy_config_script,
                                       bool save_cache_and_cookies,
                                       int max_cache_size,
                                       bool disable_same_origin_policy,
                                       IntPtr custom_css);

        public static void Initialize(WebCore.Config config)
        {
            StringHelper userDataPathStr = new StringHelper(config.userDataPath);
            StringHelper pluginPathStr = new StringHelper(config.pluginPath);
            StringHelper logPathStr = new StringHelper(config.logPath);
            StringHelper userAgentOverrideStr = new StringHelper(config.userAgentOverride);
            StringHelper proxyServerStr = new StringHelper(config.proxyServer);
            StringHelper proxyConfigScriptStr = new StringHelper(config.proxyConfigScript);
            StringHelper customCSSStr = new StringHelper(config.customCSS);

            awe_webcore_initialize(config.enablePlugins,
                                     config.enableJavascript,
                                     userDataPathStr.value(),
                                     pluginPathStr.value(),
                                     logPathStr.value(),
                                     config.logLevel,
                                     userAgentOverrideStr.value(),
                                     proxyServerStr.value(),
                                     proxyConfigScriptStr.value(),
                                     config.saveCacheAndCookies,
                                     config.maxCacheSize,
                                     config.disableSameOriginPolicy,
                                     customCSSStr.value());

            activeWebViews = new List<Object>();
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webcore_shutdown();

        public static void Shutdown()
        {
            foreach (WebView i in activeWebViews)
                i.instance = IntPtr.Zero;

            awe_webcore_shutdown();

            activeWebViews = null;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webcore_set_base_directory(IntPtr base_dir_path);

        public static void SetBaseDirectory(string baseDirPath)
        {
            StringHelper baseDirPathStr = new StringHelper(baseDirPath);

            awe_webcore_set_base_directory(baseDirPathStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_webcore_create_webview(int width, int height);

        public static WebView CreateWebview(int width, int height)
        {
            IntPtr webviewPtr = awe_webcore_create_webview(width, height);
            WebView result = new WebView(webviewPtr);
            activeWebViews.Add(result);

            return result;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webcore_set_custom_response_page(int status_code, IntPtr file_path);

        public static void SetCustomResponsePage(int statusCode, string filePath)
        {
            StringHelper filePathStr = new StringHelper(filePath);

            awe_webcore_set_custom_response_page(statusCode, filePathStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webcore_update();

        public static void Update()
        {
            awe_webcore_update();
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_webcore_get_base_directory();

        public static string GetBaseDirectory()
        {
            return StringHelper.ConvertAweString(awe_webcore_get_base_directory());
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_webcore_are_plugins_enabled();

        public static bool ArePluginsEnabled()
        {
            return awe_webcore_are_plugins_enabled();
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webcore_clear_cache();

        public static void ClearCache()
        {
            awe_webcore_clear_cache();
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webcore_clear_cookies();

        public static void ClearCookies()
        {
            awe_webcore_clear_cookies();
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webcore_set_cookie(IntPtr url,
                                       IntPtr cookie_string,
                                       bool is_http_only,
                                       bool force_session_cookie);

        public static void SetCookie(string url,
                                    string cookieString,
                                    bool isHttpOnly = false,
                                    bool forceSessionCookie = false)
        {
            StringHelper urlStr = new StringHelper(url);
            StringHelper cookieStringStr = new StringHelper(cookieString);

            awe_webcore_set_cookie(urlStr.value(), cookieStringStr.value(), isHttpOnly, forceSessionCookie);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_webcore_get_cookies(IntPtr url, bool exclude_http_only);

        public static String GetCookies(string url, bool excludeHttpOnly = true)
        {
            StringHelper urlStr = new StringHelper(url);

            IntPtr temp = awe_webcore_get_cookies(urlStr.value(), excludeHttpOnly);

            return StringHelper.ConvertAweString(temp);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webcore_delete_cookie(IntPtr url, IntPtr cookie_name);

        public static void DeleteCookie(string url, string cookieName)
        {
            StringHelper urlStr = new StringHelper(url);
            StringHelper cookieNameStr = new StringHelper(cookieName);

            awe_webcore_delete_cookie(urlStr.value(), cookieNameStr.value());
        }

    }

}





