using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Runtime.InteropServices;
#if !USING_MONO
using System.Linq;
#endif

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// The WebView is sort of like a tab in Chrome: you can load web-pages into it, interact with it, 
    /// and render it to a buffer (we give you the raw pixels, its your duty to display it).
    /// You can create a WebView using WebCore.CreateWebview
    /// </summary>
    public class WebView
    {
        internal IntPtr instance;
        private bool disposed = false;

        internal Dictionary<string, JSCallback> jsObjectCallbackMap;

        public class BeginLoadingEventArgs : EventArgs
        {
            public BeginLoadingEventArgs(WebView webView, string url, string frameName, int statusCode, string mimeType)
            {
                this.webView = webView;
                this.url = url;
                this.frameName = frameName;
                this.statusCode = statusCode;
                this.mimeType = mimeType;
            }

            public WebView webView;
            public string url;
            public string frameName;
            public int statusCode;
            public string mimeType;
        };

        public delegate void BeginLoadingEventHandler(object sender, BeginLoadingEventArgs e);
        /// <summary>
        /// This event occurs when a WebView begins loading a new page (first bits of data received from server).
        /// </summary>
        public event BeginLoadingEventHandler OnBeginLoading;

        public class BeginNavigationEventArgs : EventArgs
        {

            public BeginNavigationEventArgs(WebView webView, string url, string frameName)
            {
                this.webView = webView;
                this.url = url;
                this.frameName = frameName;
            }

            public WebView webView;
            public string url;
            public string frameName;
        };

        public delegate void BeginNavigationEventArgsHandler(object sender, BeginNavigationEventArgs e);
        /// <summary>
        /// This event occurs when a WebView begins navigating to a new URL.
        /// </summary>
        public event BeginNavigationEventArgsHandler OnBeginNavigation;

        public class JSCallbackEventArgs : EventArgs
        {
            public JSCallbackEventArgs(WebView webView, string objectName, string callbackName, JSValue[] args)
            {
                this.webView = webView;
                this.objectName = objectName;
                this.callbackName = callbackName;
                this.args = args;
            }

            public WebView webView;
            public string objectName;
            public string callbackName;
            public JSValue[] args;
        };

        internal delegate void JSCallbackEventArgsHandler(object sender, JSCallbackEventArgs e);
        internal event JSCallbackEventArgsHandler OnJSCallback;

        public class ChangeCursorEventArgs : EventArgs
        {
            public ChangeCursorEventArgs(WebView webView, CursorType cursorType)
            {
                this.webView = webView;
                this.cursorType = cursorType;
            }

            public WebView webView;
            public CursorType cursorType;
        };

        public delegate void ChangeCursorEventArgsHandler(object sender, ChangeCursorEventArgs e);
        /// <summary>
        /// This event occurs when the mouse cursor type changes.
        /// </summary>
        public event ChangeCursorEventArgsHandler OnChangeCursor;

        public class ChangeKeyboardFocusEventArgs : EventArgs
        {
            public ChangeKeyboardFocusEventArgs(WebView webView, bool isFocused)
            {
                this.webView = webView;
                this.isFocused = isFocused;
            }

            public WebView webView;
            public bool isFocused;
        };

        public delegate void ChangeKeyboardFocusEventArgsHandler(object sender, ChangeKeyboardFocusEventArgs e);
        /// <summary>
        /// This event occurs when keyboard focus changes (usually as a result of a textbox being focused).
        /// </summary>
        public event ChangeKeyboardFocusEventArgsHandler OnChangeKeyboardFocus;

        public class ChangeTargetUrlEventArgs : EventArgs
        {
            public ChangeTargetUrlEventArgs(WebView webView, string url)
            {
                this.webView = webView;
                this.url = url;
            }

            public WebView webView;
            public string url;
        };

        public delegate void ChangeTargetUrlEventArgsHandler(object sender, ChangeTargetUrlEventArgs e);
        /// <summary>
        /// This event occurs when the target URL changes (usually the result of hovering over a link).
        /// </summary>
        public event ChangeTargetUrlEventArgsHandler OnChangeTargetUrl;

        public class ChangeTooltipEventArgs : EventArgs
        {
            public ChangeTooltipEventArgs(WebView webView, string tooltip)
            {
                this.webView = webView;
                this.tooltip = tooltip;
            }

            public WebView webView;
            public string tooltip;
        };

        public delegate void ChangeTooltipEventArgsHandler(object sender, ChangeTooltipEventArgs e);
        /// <summary>
        /// This event occurs when the tooltip text changes.
        /// </summary>
        public event ChangeTooltipEventArgsHandler OnChangeTooltip;

        public class DomReadyEventArgs : EventArgs
        {
            public DomReadyEventArgs(WebView webView)
            {
                this.webView = webView;
            }

            public WebView webView;
        };

        public delegate void DomReadyEventArgsHandler(object sender, DomReadyEventArgs e);
        /// <summary>
        /// This event occurs once the document has been parsed for a page but before all resources (images, etc.)
        /// have been loaded. This is your first chance to execute Javascript on a page (useful for initialization purposes).
        /// </summary>
        public event DomReadyEventArgsHandler OnDomReady;

        public class FinishLoadingEventArgs : EventArgs
        {
            public FinishLoadingEventArgs(WebView webView)
            {
                this.webView = webView;
            }

            public WebView webView;
        };

        public delegate void FinishLoadingEventArgsHandler(object sender, FinishLoadingEventArgs e);
        /// <summary>
        /// This event occurs once a page (and all of its sub-frames) has completely finished loading.
        /// </summary>
        public event FinishLoadingEventArgsHandler OnFinishLoading;

        public class GetPageContentsEventArgs : EventArgs
        {
            public GetPageContentsEventArgs(WebView webView, string url, string contents)
            {
                this.webView = webView;
                this.url = url;
                this.contents = contents;
            }

            public WebView webView;
            public string url;
            public string contents;
        };

        public delegate void GetPageContentsEventArgsHandler(object sender, GetPageContentsEventArgs e);
        /// <summary>
        /// This event occurs once the page contents (as text) have been retrieved (usually after the end
        /// of each page load). This plain text is useful for indexing/search purposes.
        /// </summary>
        public event GetPageContentsEventArgsHandler OnGetPageContents;

        public class OpenExternalLinkEventArgs : EventArgs
        {
            public OpenExternalLinkEventArgs(WebView webView, string url, string source)
            {
                this.webView = webView;
                this.url = url;
                this.source = source;
            }

            public WebView webView;
            public string url;
            public string source;
        };

        public delegate void OpenExternalLinkEventArgsHandler(object sender, OpenExternalLinkEventArgs e);
        /// <summary>
        /// This event occurs when an external link is attempted to be opened. An external link
        /// is any link that normally opens in a new window (for example, links with target="_blank", calls
        /// to window.open(), and URL open events from Flash plugins). You are responsible for
        /// creating a new WebView to handle these URLs yourself.
        /// </summary>
        public event OpenExternalLinkEventArgsHandler OnOpenExternalLink;

        public class PluginCrashedEventArgs : EventArgs
        {
            public PluginCrashedEventArgs(WebView webView, string pluginName)
            {
                this.webView = webView;
                this.pluginName = pluginName;
            }

            public WebView webView;
            public string pluginName;
        };

        public delegate void PluginCrashedEventArgsHandler(object sender, PluginCrashedEventArgs e);
        /// <summary>
        /// This event occurs whenever a plugin crashes on a page (usually Flash).
        /// </summary>
        public event PluginCrashedEventArgsHandler OnPluginCrashed;

        public class ReceiveTitleEventArgs : EventArgs
        {
            public ReceiveTitleEventArgs(WebView webView, string title, string frameName)
            {
                this.webView = webView;
                this.title = title;
                this.frameName = frameName;
            }

            public WebView webView;
            public string title;
            public string frameName;
        };

        public delegate void ReceiveTitleEventArgsHandler(object sender, ReceiveTitleEventArgs e);
        /// <summary>
        /// This event occurs once we receive the page title.
        /// </summary>
        public event ReceiveTitleEventArgsHandler OnReceiveTitle;

        public class RequestMoveEventArgs : EventArgs
        {
            public RequestMoveEventArgs(WebView webView, int x, int y)
            {
                this.webView = webView;
                this.x = x;
                this.y = y;
            }

            public WebView webView;
            public int x;
            public int y;
        };

        public delegate void RequestMoveEventArgsHandler(object sender, RequestMoveEventArgs e);
        /// <summary>
        /// This event occurs whenever the window is requested to be moved (via Javascript).
        /// </summary>
        public event RequestMoveEventArgsHandler OnRequestMove;

        public class RequestDownloadEventArgs : EventArgs
        {
            public RequestDownloadEventArgs(WebView webView, string url)
            {
                this.webView = webView;
                this.url = url;
            }

            public WebView webView;
            public string url;
        };

        public delegate void RequestDownloadEventArgsHandler(object sender, RequestDownloadEventArgs e);
        /// <summary>
        /// This event occurs whenever a URL is requested to be downloaded (you must handle this yourself).
        /// </summary>
        public event RequestDownloadEventArgsHandler OnRequestDownload;

        public class WebViewCrashedEventArgs : EventArgs
        {
            public WebViewCrashedEventArgs(WebView webView)
            {
                this.webView = webView;
            }

            public WebView webView;
        };

        public delegate void WebViewCrashedEventArgsHandler(object sender, WebViewCrashedEventArgs e);
        /// <summary>
        /// This event occurs when the renderer (which is isolated in a separate process) crashes unexpectedly.
        /// </summary>
        public event WebViewCrashedEventArgsHandler OnWebViewCrashed;

        public class RequestFileChooserEventArgs : EventArgs
        {
            public RequestFileChooserEventArgs(WebView webView, bool selectMultipleFiles, string title, string defaultPaths)
            {
                this.webView = webView;
                this.selectMultipleFiles = selectMultipleFiles;
                this.title = title;
                this.defaultPaths = defaultPaths;
            }

            public WebView webView;
            public bool selectMultipleFiles;
            public string title;
            public string defaultPaths;
        };

        public delegate void RequestFileChooserEventArgsHandler(object sender, RequestFileChooserEventArgs e);
        /// <summary>
        /// This event occurs whenever a page requests a file chooser dialog to be displayed (usually due
        /// to an upload form being clicked by a user). You will need to display your own dialog (it does
        /// not have to be modal, this request is non-blocking). Once a file has been chosen by the user,
        /// you should call WebView.chooseFile
        /// </summary>
        public event RequestFileChooserEventArgsHandler OnRequestFileChooser;

        public class GetScrollDataEventArgs : EventArgs
        {
            public GetScrollDataEventArgs(WebView webView, int contentWidth, int contentHeight, int preferredWidth, int scrollX, int scrollY)
            {
                this.webView = webView;
                this.contentWidth = contentWidth;
                this.contentHeight = contentHeight;
                this.preferredWidth = preferredWidth;
                this.scrollX = scrollX;
                this.scrollY = scrollY;
            }

            public WebView webView;
            public int contentWidth;
            public int contentHeight;
            public int preferredWidth;
            public int scrollX;
            public int scrollY;
        };

        public delegate void GetScrollDataEventArgsHandler(object sender, GetScrollDataEventArgs e);
        /// <summary>
        /// This event occurs as a response to WebView.RequestScrollData
        /// </summary>
        public event GetScrollDataEventArgsHandler OnGetScrollData;

        public class JSConsoleMessageEventArgs : EventArgs
        {
            public JSConsoleMessageEventArgs(WebView webView, string message, int lineNumber, string source)
            {
                this.webView = webView;
                this.message = message;
                this.lineNumber = lineNumber;
                this.source = source;
            }

            public WebView webView;
            public string message;
            public int lineNumber;
            public string source;
        };

        public delegate void JSConsoleMessageEventArgsHandler(object sender, JSConsoleMessageEventArgs e);
        /// <summary>
        /// This event occurs whenever a new message is added to the Javascript Console (usually
        /// the result of a Javascript error).
        /// </summary>
        public event JSConsoleMessageEventArgsHandler OnJSConsoleMessage;

        public class GetFindResultsEventArgs : EventArgs
        {
            public GetFindResultsEventArgs(WebView webView, int requestID, int numMatches, AweRect selection,
                int curMatch, bool finalUpdate)
            {
                this.webView = webView;
                this.requestID = requestID;
                this.numMatches = numMatches;
                this.selection = selection;
                this.curMatch = curMatch;
                this.finalUpdate = finalUpdate;
            }

            public WebView webView;
            public int requestID;
            public int numMatches;
            public AweRect selection;
            public int curMatch;
            public bool finalUpdate;
        };

        public delegate void GetFindResultsEventArgsHandler(object sender, GetFindResultsEventArgs e);
        /// <summary>
        /// This event occurs whenever we receive results back from an in-page find operation (WebView.Find).
        /// </summary>
        public event GetFindResultsEventArgsHandler OnGetFindResults;

        public class UpdateIMEEventArgs : EventArgs
        {
            public UpdateIMEEventArgs(WebView webView, IMEState state, AweRect caretRect)
            {
                this.webView = webView;
                this.state = state;
                this.caretRect = caretRect;
            }

            public WebView webView;
            public IMEState state;
            public AweRect caretRect;
        };

        public delegate void UpdateIMEEventArgsHandler(object sender, UpdateIMEEventArgs e);
        /// <summary>
        /// This event occurs whenever the user does something that changes the 
        /// position or visiblity of the IME Widget. This event is only active when 
        /// IME is activated (please see WebView.ActivateIME).
        /// </summary>
        public event UpdateIMEEventArgsHandler OnUpdateIME;

        public class ResourceRequestEventArgs : EventArgs
        {
            public ResourceRequestEventArgs(WebView webView, ResourceRequest request)
            {
                this.webView = webView;
                this.request = request;
            }

            public WebView webView;
            public ResourceRequest request;
        }

        public delegate ResourceResponse ResourceRequestEventArgsHandler(object sender, ResourceRequestEventArgs e);
        /// <summary>
        /// This event occurs whenever there is a request for a certain resource (URL). You can either modify the request
        /// before it is sent or immediately return your own custom response. This is useful for implementing your own
        /// custom resource-loading back-end or for tracking of resource loads.
        /// </summary>
        public event ResourceRequestEventArgsHandler OnResourceRequest;

        public class ResourceResponseEventArgs : EventArgs
        {
            public ResourceResponseEventArgs(WebView webView, string url, int statusCode, bool wasCached, long requestTimeMs,
                long responseTimeMs, long expectedContentSize, string mimeType)
            {
                this.webView = webView;
                this.url = url;
                this.statusCode = statusCode;
                this.wasCached = wasCached;
                this.requestTimeMs = requestTimeMs;
                this.responseTimeMs = responseTimeMs;
                this.expectedContentSize = expectedContentSize;
                this.mimeType = mimeType;
            }

            public WebView webView;
            public string url;
            public int statusCode;
            public bool wasCached;
            public long requestTimeMs;
            public long responseTimeMs;
            public long expectedContentSize;
            public string mimeType;
        }

        public delegate void ResourceResponseEventArgsHandler(object sender, ResourceResponseEventArgs e);
        /// <summary>
        /// This event occurs whenever a response has been received from a server. This is useful for statistics
        /// and resource tracking purposes.
        /// </summary>
        public event ResourceResponseEventArgsHandler OnResourceResponse;

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

        internal WebView(IntPtr webview)
        {
            this.instance = webview;

            beginNavigationCallback = internalBeginNavigationCallback;
            awe_webview_set_callback_begin_navigation(webview, beginNavigationCallback);

            beginLoadingCallback = internalBeginLoadingCallback;
            awe_webview_set_callback_begin_loading(webview, beginLoadingCallback);

            finishLoadingCallback = internalFinishLoadingCallback;
            awe_webview_set_callback_finish_loading(webview, finishLoadingCallback);

            jsCallback = internalJsCallback;
            awe_webview_set_callback_js_callback(webview, jsCallback);

            receiveTitleCallback = internalReceiveTitleCallback;
            awe_webview_set_callback_receive_title(webview, receiveTitleCallback);

            changeTooltipCallback = internalChangeTooltipCallback;
            awe_webview_set_callback_change_tooltip(webview, changeTooltipCallback);

            changeCursorCallback = internalChangeCursorCallback;
            awe_webview_set_callback_change_cursor(webview, changeCursorCallback);

            changeKeyboardFocusCallback = internalChangeKeyboardFocusCallback;
            awe_webview_set_callback_change_keyboard_focus(webview, changeKeyboardFocusCallback);

            changeTargetURLCallback = internalChangeTargetURLCallback;
            awe_webview_set_callback_change_target_url(webview, changeTargetURLCallback);

            openExternalLinkCallback = internalOpenExternalLinkCallback;
            awe_webview_set_callback_open_external_link(webview, openExternalLinkCallback);

            requestDownloadCallback = internalRequestDownloadCallback;
            awe_webview_set_callback_request_download(webview, requestDownloadCallback);

            webviewCrashedCallback = internalWebviewCrashedCallback;
            awe_webview_set_callback_web_view_crashed(webview, webviewCrashedCallback);

            pluginCrashedCallback = internalPluginCrashedCallback;
            awe_webview_set_callback_plugin_crashed(webview, pluginCrashedCallback);

            requestMoveCallback = internalRequestMoveCallback;
            awe_webview_set_callback_request_move(webview, requestMoveCallback);

            getPageContentsCallback = internalGetPageContentsCallback;
            awe_webview_set_callback_get_page_contents(webview, getPageContentsCallback);

            domReadyCallback = internalDomReadyCallback;
            awe_webview_set_callback_dom_ready(webview, domReadyCallback);

            requestFileChooserCallback = internalRequestFileChooser;
            awe_webview_set_callback_request_file_chooser(webview, requestFileChooserCallback);

            getScrollDataCallback = internalGetScrollData;
            awe_webview_set_callback_get_scroll_data(webview, getScrollDataCallback);

            jsConsoleMessageCallback = internalJSConsoleMessage;
            awe_webview_set_callback_js_console_message(webview, jsConsoleMessageCallback);

            getFindResultsCallback = internalGetFindResults;
            awe_webview_set_callback_get_find_results(webview, getFindResultsCallback);

            updateIMECallback = internalUpdateIME;
            awe_webview_set_callback_update_ime(webview, updateIMECallback);

            resourceRequestCallback = internalResourceRequestCallback;
            awe_webview_set_callback_resource_request(webview, resourceRequestCallback);

            resourceResponseCallback = internalResourceResponseCallback;
            awe_webview_set_callback_resource_response(webview, resourceResponseCallback);

            jsObjectCallbackMap = new Dictionary<string, JSCallback>();
            this.OnJSCallback += handleJSCallback;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_destroy(IntPtr webview);

        ~WebView()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                if (instance != IntPtr.Zero)
                {
                    WebCore.activeWebViews.Remove(this);
                    awe_webview_destroy(instance);
                    instance = IntPtr.Zero;
                }

                disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_resource_interceptor(/*To do?*/);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_load_url(IntPtr webview,
                                         IntPtr url,
                                         IntPtr frame_name,
                                         IntPtr username,
                                         IntPtr password);

        public void LoadURL(string url,
                            string frameName = "",
                            string username = "",
                            string password = "")
        {
            StringHelper urlStr = new StringHelper(url);
            StringHelper frameNameStr = new StringHelper(frameName);
            StringHelper usernameStr = new StringHelper(username);
            StringHelper passwordStr = new StringHelper(password);

            awe_webview_load_url(instance, urlStr.value(), frameNameStr.value(), usernameStr.value(), passwordStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_load_html(IntPtr webview,
                                          IntPtr html,
                                          IntPtr frame_name);

        public void LoadHTML(string html,
                             string frameName = "")
        {
            StringHelper htmlStr = new StringHelper(html);
            StringHelper frameNameStr = new StringHelper(frameName);

            awe_webview_load_html(instance, htmlStr.value(), frameNameStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_load_file(IntPtr webview,
                                          IntPtr file,
                                          IntPtr frame_name);

        public void LoadFile(string file,
                             string frameName = "")
        {
            StringHelper fileStr = new StringHelper(file);
            StringHelper frameNameStr = new StringHelper(frameName);

            awe_webview_load_file(instance, fileStr.value(), frameNameStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_go_to_history_offset(IntPtr webview,
                                                     int offset);

        public void GoBack()
        {
            GoToHistoryOffset(-1);
        }

        public void GoForward()
        {
            GoToHistoryOffset(1);
        }

        public void GoToHistoryOffset(int offset)
        {
            awe_webview_go_to_history_offset(instance, offset);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_stop(IntPtr webview);

        public void Stop()
        {
            awe_webview_stop(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_reload(IntPtr webview);

        public void Reload()
        {
            awe_webview_reload(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_execute_javascript(IntPtr webview,
                                                   IntPtr javascript,
                                                   IntPtr frame_name);

        /// <summary>
        /// Executes a string of Javascript in the context of the current page
        /// asynchronously.
        /// </summary>
        /// <param name="javascript">The string of Javascript to execute</param>
        /// <param name="frameName">Optional; the name of the frame to execute in,
        /// leave this blank to execute in the main frame.</param>
        public void ExecuteJavascript(string javascript,
                                      string frameName = "")
        {
            StringHelper javascriptStr = new StringHelper(javascript);
            StringHelper frameNameStr = new StringHelper(frameName);

            awe_webview_execute_javascript(instance, javascriptStr.value(), frameNameStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_webview_execute_javascript_with_result(
                                                        IntPtr webview,
                                                        IntPtr javascript,
                                                        IntPtr frame_name,
                                                        int timeout_ms);

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
        public JSValue ExecuteJavascriptWithResult(string javascript,
                                                  string frameName = "",
                                                  int timeoutMs = 0)
        {
            StringHelper javascriptStr = new StringHelper(javascript);
            StringHelper frameNameStr = new StringHelper(frameName);

            IntPtr temp = awe_webview_execute_javascript_with_result(instance, javascriptStr.value(), frameNameStr.value(), timeoutMs);

            JSValue result = new JSValue(temp);
            result.ownsInstance = true;

            return result;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_call_javascript_function(IntPtr webview,
                                                         IntPtr objectName,
                                                         IntPtr function,
                                                         IntPtr arguments,
                                                         IntPtr frame_name);

        public void CallJavascriptFunction(string objectName,
                                           string function,
                                           params JSValue[] arguments)
        {
            IntPtr jsarray = JSArrayHelper.createArray(arguments);

            StringHelper objectNameStr = new StringHelper(objectName);
            StringHelper functionStr = new StringHelper(function);
            StringHelper frameNameStr = new StringHelper("");

            awe_webview_call_javascript_function(instance, objectNameStr.value(), functionStr.value(), jsarray, frameNameStr.value());

            JSArrayHelper.destroyArray(jsarray);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_create_object(IntPtr webview,
                                              IntPtr object_name);

        /// <summary>
        /// Creates a new global Javascript object that will persist throughout
        /// the lifetime of this WebView. This is useful for exposing your application's
        /// data and events to Javascript. This object is managed directly by Awesomium
        /// so you can modify its properties and bind callback functions via
        /// WebView.SetObjectProperty and WebView.SetObjectCallback, respectively.
        /// </summary>
        /// <param name="objectName"></param>
        public void CreateObject(string objectName)
        {
            StringHelper objectNameStr = new StringHelper(objectName);

            awe_webview_create_object(instance, objectNameStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_destroy_object(IntPtr webview,
                                               IntPtr object_name);

        public void DestroyObject(string objectName)
        {
            StringHelper objectNameStr = new StringHelper(objectName);

            awe_webview_destroy_object(instance, objectNameStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_object_property(IntPtr webview,
                                                     IntPtr object_name,
                                                     IntPtr property_name,
                                                     IntPtr val);

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
        public void SetObjectProperty(string objectName,
                                      string propertyName,
                                      JSValue val)
        {
            StringHelper objectNameStr = new StringHelper(objectName);
            StringHelper propertyNameStr = new StringHelper(propertyName);

            awe_webview_set_object_property(instance, objectNameStr.value(), propertyNameStr.value(), val.getInstance());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_object_callback(IntPtr webview,
                                                     IntPtr object_name,
                                                     IntPtr callback_name);

        public delegate void JSCallback(object sender, JSCallbackEventArgs e);

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
        public void SetObjectCallback(string objectName,
                                      string callbackName,
                                      JSCallback callback)
        {
            StringHelper objectNameStr = new StringHelper(objectName);
            StringHelper callbackNameStr = new StringHelper(callbackName);

            awe_webview_set_object_callback(instance, objectNameStr.value(), callbackNameStr.value());

            string key = objectName + "." + callbackName;

            if (jsObjectCallbackMap.ContainsKey(key))
                jsObjectCallbackMap.Remove(key);

            if(callback != null)
                jsObjectCallbackMap.Add(key, callback);
        }

        internal void handleJSCallback(object sender, JSCallbackEventArgs e)
        {
            string key = e.objectName + "." + e.callbackName;

            if (jsObjectCallbackMap.ContainsKey(key))
                jsObjectCallbackMap[key](sender, e);
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_webview_is_loading_page(IntPtr webview);

        public bool IsLoadingPage()
        {
            return awe_webview_is_loading_page(instance);
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_webview_is_dirty(IntPtr webview);

        /// <summary>
        /// Whether or not this WebView needs to be rendered again.
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            return awe_webview_is_dirty(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern AweRect awe_webview_get_dirty_bounds(IntPtr webview);

        public AweRect GetDirtyBounds()
        {
            AweRect bounds = awe_webview_get_dirty_bounds(instance);

            AweRect result = new AweRect();
            result.x = bounds.x;
            result.y = bounds.y;
            result.width = bounds.width;
            result.height = bounds.height;

            return result;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_webview_render(IntPtr webview);

        /// <summary>
        /// Renders this WebView into an offscreen pixel buffer and clears the dirty state.
        /// For maximum efficiency, you should only call this when the WebView is dirty (WebView.IsDirty).
        /// </summary>
        /// <returns>An instance of the RenderBuffer that this WebView was rendered to. This
        /// value may change between renders and may return null if the WebView has crashed.</returns>
        public RenderBuffer Render()
        {
            return new RenderBuffer(awe_webview_render(instance));
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_pause_rendering(IntPtr webview);

        public void PauseRendering()
        {
            awe_webview_pause_rendering(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_resume_rendering(IntPtr webview);

        public void ResumeRendering()
        {
            awe_webview_resume_rendering(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_inject_mouse_move(IntPtr webview,
                                                  int x,
                                                  int y);

        public void InjectMouseMove(int x, int y)
        {
            awe_webview_inject_mouse_move(instance, x, y);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_inject_mouse_down(IntPtr webview,
                                                  MouseButton mouseButton);

        public void InjectMouseDown(MouseButton mouseButton)
        {
            awe_webview_inject_mouse_down(instance, mouseButton);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_inject_mouse_up(IntPtr webview,
                                                MouseButton mouseButton);

        public void InjectMouseUp(MouseButton mouseButton)
        {
            awe_webview_inject_mouse_up(instance, mouseButton);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_inject_mouse_wheel(IntPtr webview,
                                                   int scroll_amount_vert,
                                                   int scroll_amount_horz);

        public void InjectMouseWheel(int scrollAmountVert, int scrollAmountHorz = 0)
        {
            awe_webview_inject_mouse_wheel(instance, scrollAmountVert, scrollAmountHorz);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_inject_keyboard_event(IntPtr webview,
                                                      WebKeyboardEvent key_event);

        public void InjectKeyboardEvent(WebKeyboardEvent keyEvent)
        {
            awe_webview_inject_keyboard_event(instance, keyEvent);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_inject_keyboard_event_win(IntPtr webview,
                                                  int msg,
                                                  int wparam,
                                                  int lparam);

        public void InjectKeyboardEventWin(int msg,
                                           int wparam,
                                           int lparam)
        {
            awe_webview_inject_keyboard_event_win(instance, msg, wparam, lparam);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_cut(IntPtr webview);

        public void Cut()
        {
            awe_webview_cut(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_copy(IntPtr webview);

        public void Copy()
        {
            awe_webview_copy(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_paste(IntPtr webview);

        public void Paste()
        {
            awe_webview_paste(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_select_all(IntPtr webview);

        public void SelectAll()
        {
            awe_webview_select_all(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_zoom(IntPtr webview,
                                         int zoom_percent);

        public void SetZoom(int zoomPercent)
        {
            awe_webview_set_zoom(instance, zoomPercent);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_reset_zoom(IntPtr webview);

        public void ResetZoom()
        {
            awe_webview_reset_zoom(instance);
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_webview_resize(IntPtr webview,
                                       int width,
                                       int height,
                                       bool wait_for_repaint,
                                       int repaint_timeout_ms);

        public bool Resize(int width,
                           int height,
                           bool waitForRepaint = true,
                           int repaintTimeoutMs = 300)
        {
            return awe_webview_resize(instance, width, height, waitForRepaint, repaintTimeoutMs);
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_webview_is_resizing(IntPtr webview);

        public bool IsResizing()
        {
            return awe_webview_is_resizing(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_unfocus(IntPtr webview);

        public void Unfocus()
        {
            awe_webview_unfocus(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_focus(IntPtr webview);

        public void Focus()
        {
            awe_webview_focus(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_transparent(IntPtr webview,
                                                bool is_transparent);

        public void SetTransparent(bool isTransparent)
        {
            awe_webview_set_transparent(instance, isTransparent);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_url_filtering_mode(IntPtr webview,
                                                       URLFilteringMode filteringMode);

        public void SetURLFilteringMode(URLFilteringMode filteringMode)
        {
            awe_webview_set_url_filtering_mode(instance, filteringMode);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_add_url_filter(IntPtr webview,
                                                IntPtr filter);

        public void AddURLFilter(string filter)
        {
            StringHelper filterStr = new StringHelper(filter);

            awe_webview_add_url_filter(instance, filterStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_clear_all_url_filters(IntPtr webview);

        public void ClearAllURLFilters()
        {
            awe_webview_clear_all_url_filters(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_header_definition(IntPtr webview,
                                                 IntPtr name,
                                                 uint num_fields,
                                                 IntPtr[] field_names,
                                                 IntPtr[] field_values);

        public void SetHeaderDefinition(string name,
                                        NameValueCollection fields)
        {
            StringHelper nameStr = new StringHelper(name);

            int count = fields.Count;
            IntPtr[] keys = new IntPtr[count];
            IntPtr[] values = new IntPtr[count];

            for (int i = 0; i < count; i++ )
            {
                byte[] utf16string;

                utf16string = Encoding.Unicode.GetBytes(fields.GetKey(i));
                keys[i] = StringHelper.awe_string_create_from_utf16(utf16string, (uint)fields.GetKey(i).Length);

                utf16string = Encoding.Unicode.GetBytes(fields.Get(i));
                values[i] = StringHelper.awe_string_create_from_utf16(utf16string, (uint)fields.Get(i).Length);
            }

            awe_webview_set_header_definition(instance, nameStr.value(), (uint)count, keys, values);

            for (uint i = 0; i < count; i++)
            {
                StringHelper.awe_string_destroy(keys[i]);
                StringHelper.awe_string_destroy(values[i]);
            }
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_add_header_rewrite_rule(IntPtr webview,
                                                         IntPtr rule,
                                                         IntPtr name);

        public void AddHeaderRewriteRule(string rule, string name)
        {
            StringHelper ruleStr = new StringHelper(rule);
            StringHelper nameStr = new StringHelper(name);

            awe_webview_add_header_rewrite_rule(instance, ruleStr.value(), nameStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_remove_header_rewrite_rule(IntPtr webview,
                                                                          IntPtr rule);
        public void RemoveHeaderRewriteRule(string rule)
        {
            StringHelper ruleStr = new StringHelper(rule);

            awe_webview_remove_header_rewrite_rule(instance, ruleStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_remove_header_rewrite_rules_by_definition_name(IntPtr webview,
                                                                                              IntPtr name);

        public void RemoveHeaderRewriteRulesByDefinition(string name)
        {
            StringHelper nameStr = new StringHelper(name);

            awe_webview_remove_header_rewrite_rules_by_definition_name(instance, nameStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_choose_file(IntPtr webview,
                                                           IntPtr file_path);

        public void ChooseFile(string filePath)
        {
            StringHelper filePathStr = new StringHelper(filePath);

            awe_webview_choose_file(instance, filePathStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_print(IntPtr webview);

        public void Print()
        {
            awe_webview_print(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_request_scroll_data(IntPtr webview,
                                                                   IntPtr frame_name);

        public void RequestScrollData(string frameName = "")
        {
            StringHelper frameNameStr = new StringHelper(frameName);

            awe_webview_request_scroll_data(instance, frameNameStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_find(IntPtr webview,
                                                    int request_id,
                                                    IntPtr search_string,
                                                    bool forward, 
                                                    bool case_sensitive, 
                                                    bool find_next);

        public void Find(int requestID, string searchStr, bool forward,
            bool caseSensitive, bool findNext)
        {
            StringHelper searchCStr = new StringHelper(searchStr);

            awe_webview_find(instance, requestID, searchCStr.value(), forward, caseSensitive, findNext);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_stop_find(IntPtr webview,
                                                     bool clear_selection);

        public void StopFind(bool clearSelection)
        {
            awe_webview_stop_find(instance, clearSelection);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_translate_page(IntPtr webview,
                                                              IntPtr source_language,
                                                              IntPtr target_language);

        public void TranslatePage(string sourceLanguage, string targetLanguage)
        {
            StringHelper sourceLanguageStr = new StringHelper(sourceLanguage);
            StringHelper targetLanguageStr = new StringHelper(targetLanguage);

            awe_webview_translate_page(instance, sourceLanguageStr.value(), targetLanguageStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_activate_ime(IntPtr webview,
                                                            bool activate);

        public void ActivateIME(bool activate)
        {
            awe_webview_activate_ime(instance, activate);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_ime_composition(IntPtr webview,
                                                                IntPtr input_string,
                                                                int cursor_pos,
                                                                int target_start,
                                                                int target_end);

        public void SetIMEComposition(string inputStr, int cursorPos, int targetStart, int targetEnd)
        {
            StringHelper inputCStr = new StringHelper(inputStr);

            awe_webview_set_ime_composition(instance, inputCStr.value(), cursorPos, targetStart, targetEnd);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_confirm_ime_composition(IntPtr webview,
                                                                 IntPtr input_string);

        public void ConfirmIMEComposition(string inputStr)
        {
            StringHelper inputCStr = new StringHelper(inputStr);

            awe_webview_confirm_ime_composition(instance, inputCStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_cancel_ime_composition(IntPtr webview);

        public void CancelIMEComposition()
        {
            awe_webview_cancel_ime_composition(instance);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackBeginNavigationCallback(IntPtr caller, IntPtr url, IntPtr frame_name);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_begin_navigation(IntPtr webview, CallbackBeginNavigationCallback callback);

        private void internalBeginNavigationCallback(IntPtr caller, IntPtr url, IntPtr frame_name)
        {
            BeginNavigationEventArgs e = new BeginNavigationEventArgs(this, StringHelper.ConvertAweString(url),
                StringHelper.ConvertAweString(frame_name));

            if (OnBeginNavigation != null)
                OnBeginNavigation(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackBeginLoadingCallback(IntPtr caller, IntPtr url, IntPtr frame_name, int status_code, IntPtr mime_type);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_begin_loading(IntPtr webview, CallbackBeginLoadingCallback callback);

        private void internalBeginLoadingCallback(IntPtr caller, IntPtr url, IntPtr frame_name, int status_code, IntPtr mime_type)
        {
            BeginLoadingEventArgs e = new BeginLoadingEventArgs(this, StringHelper.ConvertAweString(url), StringHelper.ConvertAweString(frame_name), status_code, StringHelper.ConvertAweString(mime_type));

            if (OnBeginLoading != null)
                OnBeginLoading(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackFinishLoadingCallback(IntPtr caller);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_finish_loading(
                            IntPtr webview,
                            CallbackFinishLoadingCallback callback);

        private void internalFinishLoadingCallback(IntPtr caller)
        {
            FinishLoadingEventArgs e = new FinishLoadingEventArgs(this);
            if (OnFinishLoading != null)
                OnFinishLoading(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackJsCallback(IntPtr caller, IntPtr object_name, IntPtr callback_name, IntPtr arguments);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_js_callback(IntPtr webview, CallbackJsCallback callback);


        private void internalJsCallback(IntPtr caller, IntPtr object_name, IntPtr callback_name, IntPtr arguments)
        {
            JSValue[] args = JSArrayHelper.getArray(arguments);

            JSCallbackEventArgs e = new JSCallbackEventArgs(this, StringHelper.ConvertAweString(object_name), StringHelper.ConvertAweString(callback_name), args);

            if (OnJSCallback != null)
                OnJSCallback(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackReceiveTitleCallback(IntPtr caller, IntPtr title, IntPtr frame_name);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_receive_title(IntPtr webview, CallbackReceiveTitleCallback callback);

        private void internalReceiveTitleCallback(IntPtr caller, IntPtr title, IntPtr frame_name)
        {
            ReceiveTitleEventArgs e = new ReceiveTitleEventArgs(this, StringHelper.ConvertAweString(title), StringHelper.ConvertAweString(frame_name));

            if (OnReceiveTitle != null)
                OnReceiveTitle(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackChangeTooltipCallback(IntPtr caller, IntPtr tooltip);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_change_tooltip(IntPtr webview, CallbackChangeTooltipCallback callback);

        private void internalChangeTooltipCallback(IntPtr caller, IntPtr tooltip)
        {
            ChangeTooltipEventArgs e = new ChangeTooltipEventArgs(this, StringHelper.ConvertAweString(tooltip));

            if (OnChangeTooltip != null)
                OnChangeTooltip(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackChangeCursorCallback(IntPtr caller, int cursor);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_change_cursor(IntPtr webview, CallbackChangeCursorCallback callback);

        private void internalChangeCursorCallback(IntPtr caller, int cursor)
        {
            ChangeCursorEventArgs e = new ChangeCursorEventArgs(this, (CursorType)cursor);

            if (OnChangeCursor != null)
                OnChangeCursor(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackChangeKeyboardFocusCallback(IntPtr caller, bool is_focused);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_change_keyboard_focus(IntPtr webview, CallbackChangeKeyboardFocusCallback callback);

        private void internalChangeKeyboardFocusCallback(IntPtr caller, bool is_focused)
        {
            ChangeKeyboardFocusEventArgs e = new ChangeKeyboardFocusEventArgs(this, is_focused);

            if (OnChangeKeyboardFocus != null)
                OnChangeKeyboardFocus(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackChangeTargetURLCallback(IntPtr caller, IntPtr url);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_change_target_url(IntPtr webview, CallbackChangeTargetURLCallback callback);

        private void internalChangeTargetURLCallback(IntPtr caller, IntPtr url)
        {
            ChangeTargetUrlEventArgs e = new ChangeTargetUrlEventArgs(this, StringHelper.ConvertAweString(url));

            if (OnChangeTargetUrl != null)
                OnChangeTargetUrl(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackOpenExternalLinkCallback(IntPtr caller, IntPtr url, IntPtr source);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_open_external_link(IntPtr webview, CallbackOpenExternalLinkCallback callback);

        private void internalOpenExternalLinkCallback(IntPtr caller, IntPtr url, IntPtr source)
        {
            OpenExternalLinkEventArgs e = new OpenExternalLinkEventArgs(this, StringHelper.ConvertAweString(url), StringHelper.ConvertAweString(source));

            if (OnOpenExternalLink != null)
                OnOpenExternalLink(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackRequestDownloadCallback(IntPtr caller, IntPtr download);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_request_download(IntPtr webview, CallbackRequestDownloadCallback callback);

        private void internalRequestDownloadCallback(IntPtr caller, IntPtr download)
        {
            RequestDownloadEventArgs e = new RequestDownloadEventArgs(this, StringHelper.ConvertAweString(download));

            if (OnRequestDownload != null)
                OnRequestDownload(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackWebviewCrashedCallback(IntPtr caller);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_web_view_crashed(IntPtr webview, CallbackWebviewCrashedCallback callback);

        private void internalWebviewCrashedCallback(IntPtr caller)
        {
            WebViewCrashedEventArgs e = new WebViewCrashedEventArgs(this);

            if (OnWebViewCrashed != null)
                OnWebViewCrashed(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackPluginCrashedCallback(IntPtr caller, IntPtr plugin_name);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_plugin_crashed(IntPtr webview, CallbackPluginCrashedCallback callback);

        private void internalPluginCrashedCallback(IntPtr caller, IntPtr plugin_name)
        {
            PluginCrashedEventArgs e = new PluginCrashedEventArgs(this, StringHelper.ConvertAweString(plugin_name));

            if (OnPluginCrashed != null)
                OnPluginCrashed(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackRequestMoveCallback(IntPtr caller, int x, int y);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_request_move(IntPtr webview, CallbackRequestMoveCallback callback);

        private void internalRequestMoveCallback(IntPtr caller, int x, int y)
        {
            RequestMoveEventArgs e = new RequestMoveEventArgs(this, x, y);

            if (OnRequestMove != null)
                OnRequestMove(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackGetPageContentsCallback(IntPtr caller, IntPtr url, IntPtr contents);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_get_page_contents(IntPtr webview, CallbackGetPageContentsCallback callback);

        private void internalGetPageContentsCallback(IntPtr caller, IntPtr url, IntPtr contents)
        {
            GetPageContentsEventArgs e = new GetPageContentsEventArgs(this, StringHelper.ConvertAweString(url), StringHelper.ConvertAweString(contents));

            if (OnGetPageContents != null)
                OnGetPageContents(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackDomReadyCallback(IntPtr caller);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_dom_ready(IntPtr webview, CallbackDomReadyCallback callback);

        private void internalDomReadyCallback(IntPtr caller)
        {
            DomReadyEventArgs e = new DomReadyEventArgs(this);

            if (OnDomReady != null)
                OnDomReady(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackRequestFileChooserCallback(IntPtr caller, bool select_multiple_files, IntPtr title, IntPtr default_path);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_request_file_chooser(IntPtr webview, CallbackRequestFileChooserCallback callback);

        private void internalRequestFileChooser(IntPtr caller, bool select_multiple_files, IntPtr title, IntPtr default_paths)
        {
            RequestFileChooserEventArgs e = new RequestFileChooserEventArgs(this, select_multiple_files, 
                StringHelper.ConvertAweString(title), StringHelper.ConvertAweString(default_paths));

            if (OnRequestFileChooser != null)
                OnRequestFileChooser(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackGetScrollDataCallback(IntPtr caller, int contentWidth, int contentHeight, int preferredWidth, int scrollX, int scrollY);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_get_scroll_data(IntPtr webview, CallbackGetScrollDataCallback callback);

        private void internalGetScrollData(IntPtr caller, int contentWidth, int contentHeight, int preferredWidth, int scrollX, int scrollY)
        {
            GetScrollDataEventArgs e = new GetScrollDataEventArgs(this, contentWidth, contentHeight, preferredWidth, scrollX, scrollY);

            if (OnGetScrollData != null)
                OnGetScrollData(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackJSConsoleMessageCallback(IntPtr caller, IntPtr message, int lineNumber, IntPtr source);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_js_console_message(IntPtr webview, CallbackJSConsoleMessageCallback callback);

        private void internalJSConsoleMessage(IntPtr caller, IntPtr message, int lineNumber, IntPtr source)
        {
            JSConsoleMessageEventArgs e = new JSConsoleMessageEventArgs(this, StringHelper.ConvertAweString(message), lineNumber, 
                StringHelper.ConvertAweString(source));

            if (OnJSConsoleMessage != null)
                OnJSConsoleMessage(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackGetFindResultsCallback(IntPtr caller, int request_id, int num_matches, AweRect selection, int cur_match, bool finalUpdate);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_get_find_results(IntPtr webview, CallbackGetFindResultsCallback callback);

        private void internalGetFindResults(IntPtr caller, int request_id, int num_matches, AweRect selection, int cur_match, bool finalUpdate)
        {
            GetFindResultsEventArgs e = new GetFindResultsEventArgs(this, request_id, num_matches, selection, cur_match, finalUpdate);

            if (OnGetFindResults != null)
                OnGetFindResults(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackUpdateIMECallback(IntPtr caller, int state, AweRect caret_rect);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_update_ime(IntPtr webview, CallbackUpdateIMECallback callback);

        private void internalUpdateIME(IntPtr caller, int state, AweRect caret_rect)
        {
            UpdateIMEEventArgs e = new UpdateIMEEventArgs(this, (IMEState)state, caret_rect);

            if (OnUpdateIME != null)
                OnUpdateIME(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate IntPtr CallbackResourceRequestCallback(IntPtr caller, IntPtr request);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_resource_request(IntPtr webview, CallbackResourceRequestCallback callback);

        private IntPtr internalResourceRequestCallback(IntPtr caller, IntPtr request)
        {
            ResourceRequest requestWrap = new ResourceRequest(request);

            ResourceRequestEventArgs e = new ResourceRequestEventArgs(this, requestWrap);

            if (OnResourceRequest != null)
            {
                ResourceResponse response = OnResourceRequest(this, e);

                if (response != null)
                    return response.getInstance();
            }

            return IntPtr.Zero;
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        internal delegate void CallbackResourceResponseCallback(IntPtr caller, IntPtr url, int statusCode, bool wasCached, long requestTimeMs,
            long responseTimeMs, long expectedContentSize, IntPtr mimeType);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_resource_response(IntPtr webview, CallbackResourceResponseCallback callback);

        private void internalResourceResponseCallback(IntPtr caller, IntPtr url, int statusCode, bool wasCached, long requestTimeMs,
            long responseTimeMs, long expectedContentSize, IntPtr mimeType)
        {
            ResourceResponseEventArgs e = new ResourceResponseEventArgs(this, StringHelper.ConvertAweString(url), statusCode, wasCached,
                requestTimeMs, responseTimeMs, expectedContentSize, StringHelper.ConvertAweString(mimeType));

            if (OnResourceResponse != null)
                OnResourceResponse(this, e);
        }

        internal void PrepareForShutdown()
        {
            if (instance != IntPtr.Zero)
            {
                resourceRequestCallback = null;
                awe_webview_set_callback_resource_request(instance, null);

                resourceResponseCallback = null;
                awe_webview_set_callback_resource_response(instance, null);
            }
        }
    }

    public class ResourceRequest
    {
        private IntPtr instance;

        internal ResourceRequest(IntPtr instance)
        {
            this.instance = instance;
        }

        internal IntPtr getInstance()
        {
            return instance;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_resource_request_cancel(IntPtr request);

        public void cancel()
        {
            awe_resource_request_cancel(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_resource_request_get_url(IntPtr request);

        public string getURL()
        {
            return StringHelper.ConvertAweString(awe_resource_request_get_url(instance), true);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_resource_request_get_method(IntPtr request);

        /// <summary>
        /// Get the method for the request (usually either "GET" or "POST")
        /// </summary>
        /// <returns></returns>
        public string getMethod()
        {
            return StringHelper.ConvertAweString(awe_resource_request_get_method(instance), true);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_resource_request_set_method(IntPtr request,
												IntPtr method);

        /// <summary>
        /// Sets the method for the request (usually either "GET" or "POST")
        /// </summary>
        /// <returns></returns>
        public void setMethod(string method)
        {
            StringHelper methodStr = new StringHelper(method);

            awe_resource_request_set_method(instance, methodStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_resource_request_get_referrer(IntPtr request);

        public string getReferrer()
        {
            return StringHelper.ConvertAweString(awe_resource_request_get_referrer(instance), true);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_resource_request_set_referrer(IntPtr request,
												  IntPtr referrer);

        public void setReferrer(string referrer)
        {
            StringHelper referrerStr = new StringHelper(referrer);

            awe_resource_request_set_referrer(instance, referrerStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_resource_request_get_extra_headers(IntPtr request);

        public string getExtraHeaders()
        {
            return StringHelper.ConvertAweString(awe_resource_request_get_extra_headers(instance), true);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_resource_request_set_extra_headers(IntPtr request,
												IntPtr headers);

        /// <summary>
        /// Override the extra headers for the request. Each header is delimited by /r/n (CRLF)
        /// Headers should NOT end in /r/n (CRLF).
        /// </summary>
        /// <param name="headers"></param>
        public void setExtraHeaders(string headers)
        {
            StringHelper headersStr = new StringHelper(headers);

            awe_resource_request_set_extra_headers(instance, headersStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_resource_request_append_extra_header(IntPtr request,
														 IntPtr name,
														 IntPtr value);

        /// <summary>
        /// Appends a new header to this request
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void appendExtraHeader(string name, string value)
        {
            StringHelper nameStr = new StringHelper(name);
            StringHelper valueStr = new StringHelper(value);

            awe_resource_request_append_extra_header(instance, nameStr.value(), valueStr.value());
        }
	
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint awe_resource_request_get_num_upload_elements(IntPtr request);

        /// Get the number of upload elements (essentially, batches of POST data).	
        public uint getNumUploadElements()
        {
            return awe_resource_request_get_num_upload_elements(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_resource_request_get_upload_element(IntPtr request,
																			 uint idx);

        /// <summary>
        /// Get a certain upload element (returned instance is owned by this class)	
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public UploadElement getUploadElement(uint idx)
        {
            return new UploadElement(awe_resource_request_get_upload_element(instance, idx));
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_resource_request_clear_upload_elements(IntPtr request);

        public void clearUploadElements()
        {
            awe_resource_request_clear_upload_elements(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_resource_request_append_upload_file_path(IntPtr request,
															 IntPtr file_path);

        /// <summary>
        ///  Append a file for POST data (adds a new UploadElement)	
        /// </summary>
        /// <param name="filePath"></param>
        public void appendUploadFilePath(string filePath)
        {
            StringHelper filePathStr = new StringHelper(filePath);

            awe_resource_request_append_upload_file_path(instance, filePathStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_resource_request_append_upload_bytes(IntPtr request,
														 IntPtr bytes);

        /// <summary>
        /// Append a string of bytes for POST data (adds a new UploadElement)	
        /// </summary>
        /// <param name="bytes"></param>
        public void appendUploadBytes(string bytes)
        {
            StringHelper bytesStr = new StringHelper(bytes);

            awe_resource_request_append_upload_bytes(instance, bytesStr.value());
        }
    }

    /// <summary>
    /// This class represents a single batch of "upload data" to be sent with
    /// a ResourceRequest. Also commonly known as "POST" data.
    /// </summary>
    public class UploadElement
    {
        private IntPtr instance;

        internal UploadElement(IntPtr instance)
        {
            this.instance = instance;
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_upload_element_is_file_path(IntPtr ele);

        public bool isFilePath()
        {
            return awe_upload_element_is_file_path(instance);
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_upload_element_is_bytes(IntPtr ele);

        public bool isBytes()
        {
            return awe_upload_element_is_bytes(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_upload_element_get_bytes(IntPtr ele);

        public string getBytes()
        {
            return StringHelper.ConvertAweString(awe_upload_element_get_bytes(instance), true);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_upload_element_get_file_path(IntPtr ele);

        public string getFilePath()
        {
            return StringHelper.ConvertAweString(awe_upload_element_get_file_path(instance), true);
        }
    }

    /// <summary>
    /// This class allows you to override the response for a certain ResourceRequest.
    /// </summary>
    public class ResourceResponse
    {
        private IntPtr instance;

        /// <summary>
        /// Create a ResourceResponse from a byte array
        /// </summary>
        /// <param name="data">The data to be initialized from (a copy is made)</param>
        /// <param name="mimeType">The mime-type of the data (for ex. "text/html")</param>
        public ResourceResponse(byte[] data, string mimeType)
        {
            StringHelper mimeTypeStr = new StringHelper(mimeType);

            IntPtr dataPtr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, dataPtr, data.Length);

            instance = awe_resource_response_create((uint)data.Length, dataPtr, mimeTypeStr.value());

            Marshal.FreeHGlobal(dataPtr);
        }

        /// <summary>
        /// Create a ResourceResponse from a file on disk
        /// </summary>
        /// <param name="filePath"></param>
        public ResourceResponse(string filePath)
        {
            StringHelper filePathStr = new StringHelper(filePath);

            instance = awe_resource_response_create_from_file(filePathStr.value());
        }

        internal IntPtr getInstance()
        {
            return instance;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_resource_response_create(uint num_bytes,
                                                                    IntPtr buffer,
                                                                    IntPtr mime_type);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_resource_response_create_from_file(IntPtr file_path);
    }

}
