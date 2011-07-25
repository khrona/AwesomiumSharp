/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : WebCoreConfig.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/03/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    This class provides initialization settings to the WebCore. It is now
 *    fully documented and taken out of WebCore.cs
 *    
 *    07/12/2011:
 *    
 *    - Synchronized with Awesomium r148 (1.6.2 Pre-Release)
 *    
 *    07/22/2011:
 *    
 *    - Synchronized with Awesomium r159 (1.6.2 Pre-Release)
 *    
 *    - Added EnableVisualStyles property used to apply visual styles to the
 *      Print dialog. Read details in remarks section of EnableThemingInScope.
 * 
 ********************************************************************************/

using System;
using System.ComponentModel;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Configuration settings for the <see cref="WebCore"/>.
    /// Used in <see cref="WebCore.Initialize"/>.
    /// </summary>
    public class WebCoreConfig
    {

        /// <summary>
        /// Singleton with default configuration settings.
        /// </summary>
        public readonly static WebCoreConfig Default;

        static WebCoreConfig()
        {
            Default = new WebCoreConfig();
        }

        /// <summary>
        /// Creates an instance of <see cref="WebCoreConfig"/>
        /// initialized with default configuration settings.
        /// </summary>
        public WebCoreConfig()
        {
            EnableJavascript = true;
            UserDataPath = "";
            PluginPath = "";
            LogPath = "";
            LogLevel = LogLevel.Normal;
            _ChildProcessPath = ""; // See notes in property setter.
            EnableAutoDetectEncoding = true;
            AcceptLanguageOverride = "";
            DefaultCharsetOverride = "";
            UserAgentOverride = "";
            ProxyServer = "";
            ProxyConfigScript = "";
            AuthServerWhitelist = "";
            CustomCSS = "";
            AutoUpdatePeriod = 20;

#if !USING_MONO
            homeURL = "about:blank";
            EnableVisualStyles = true;
#endif
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
        /// Indicates whether or not local HTML5 databases are enabled. (Will create a 
        /// databases folder in the user data path if this is enabled). The default is false.
        /// </summary>
        public bool EnableDatabases { get; set; }
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
        /// <summary>
        /// Indicates whether or not all WebViews and should be forced to render inside
        /// the main process (we typically launch a separate child-process to
        /// render each WebView and plugin safely). This mode currently only works
        /// on Windows and automatically disables plugins and local databases.
        /// The default is false.
        /// </summary>
        public bool ForceSingleProcess { get; set; }

        private string _ChildProcessPath;
        [EditorBrowsable( EditorBrowsableState.Never )]
        public string ChildProcessPath
        {
            get
            {
                return _ChildProcessPath;
            }
            set
            {
                // Left empty until completely implemented in .NET
            }
        }

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
        /// Example:
        /// <pre>
        /// "none"                         -- No proxy. (Default).
        /// 
        /// "auto"                         -- Detect system proxy settings.
        /// 
        /// "http=myproxy:80;ftp=myproxy2" -- Use HTTP proxy "myproxy:80"  
        ///                                   for http:// URLs, and HTTP proxy 
        ///                                   "myproxy2:80" for ftp:// URLs.
        ///                                   
        /// "myproxy:80"                   -- Use HTTP proxy "foopy:80" for
        ///                                   all URLs.
        ///                                   
        /// "socks4://myproxy"             -- Use SOCKS v4 proxy "foopy:1080" 
        ///                                   for all URLs.
        /// </pre>
        /// </example>
        public string ProxyServer { get; set; }
        /// <summary>
        /// Indicates the URL to the PAC (Proxy Auto-Config) Script to use. See <http://en.wikipedia.org/wiki/Proxy_auto-config> for more info.
        /// </summary>
        public string ProxyConfigScript { get; set; }
        /// <summary>
        /// Gets or sets the list of servers that Integrated Authentication is allowed to silently provide user credentials for, when challenged.
        /// </summary>
        /// <remarks>
        /// Integrated Authentication can authenticate the user to an Intranet server or proxy without prompting the user for a username or password. 
        /// It does this by using cached credentials which are established when the user initially logs in to the machine that Awesomium is running on. 
        /// Integrated Authentication is supported for Negotiate and NTLM challenges only.
        /// </remarks>
        /// <example>
        /// The list is set using a comma-separated string of URLs. For example, you can specify:
        /// <code>
        /// config.AuthServerWhitelist = "*example.com,*foobar.com,*baz";
        /// </code>
        /// which would tell Awesomium that any URL ending in either 'example.com', 'foobar.com' or 'baz' is in the permitted list.
        /// Without the '*' prefix, the URL has to match exactly.
        /// </example>
        /// @note
        /// In Windows only, if you do not set this property, the permitted list consists of those servers in the Local Machine or 
        /// Local Intranet security zone (for example, when the host in the URL includes a "." character it is outside the Local Intranet security zone), 
        /// which is the behavior present in IE.
        public string AuthServerWhitelist { get; set; }
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
        /// Indicates whether or not we should automatically pump Windows messages during
        /// a call to <see cref="WebCore.Update"/>. You may wish to set this to true if you are already
        /// pumping messages (Peek/Dispatch). Default is false.
        /// </summary>
        public bool DisableWinMessagePump { get; set; }
        /// <summary>
        /// Indicates a string of custom CSS to be included as the global default style for all pages. 
        /// This is especially useful for customizing scrollbars and other look-and-feel elements.
        /// </summary>
        public string CustomCSS { get; set; }


        // The following are used by the wrapper.
        // Properties are also added for these in WebCore.

        #region AutoUpdatePeriod
        /// <summary>
        /// Indicates the time interval between invocations of WebCore's update, in milliseconds.
        /// The default is 20.
        /// </summary>
        public int AutoUpdatePeriod { get; set; }
        #endregion

#if !USING_MONO
        #region HomeURL
        private string homeURL;
        /// <summary>
        /// Gets or sets the URL that will be used as the Home URL
        /// for <see cref="WebControl"/>s.
        /// </summary>
        /// <remarks>
        /// This setting is used by <see cref="WebControl"/>s to automatically
        /// handle the <see cref="NavigationCommands.BrowseHome"/> command.
        /// The default is: "about:blank".
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// A null reference or an empty string defined.
        /// </exception>
        public string HomeURL
        {
            get
            {
                return homeURL;
            }
            set
            {
                if ( String.Compare( homeURL, value, false ) == 0 )
                    return;

                if ( String.IsNullOrWhiteSpace( value ) )
                    throw new ArgumentNullException();

                homeURL = value;
            }
        }
        #endregion

        #region EnableVisualStyles
        /// <summary>
        /// Gets or sets if default Windows visual styles should be applied to common dialogs shown by views.
        /// Default is true.
        /// </summary>
        /// <remarks>
        /// Currently, this is used to apply visual styles to the Print dialog.
        /// </remarks>
        public bool EnableVisualStyles { get; set; }
        #endregion
#endif

    };
}
