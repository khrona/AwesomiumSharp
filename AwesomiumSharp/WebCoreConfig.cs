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
 * 
 ********************************************************************************/

using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
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
        /// Indicates the time interval between invocations of WebCore's update, in milliseconds.
        /// The default is 20.
        /// </summary>
        public int AutoUpdatePeriod { get; set; }
    };
}
