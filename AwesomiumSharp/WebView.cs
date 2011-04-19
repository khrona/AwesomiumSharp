using System;
using System.Collections.Generic;
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
    public class WebView
    {
        internal IntPtr instance;
        private bool disposed = false;

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

        public delegate void JSCallbackEventArgsHandler(object sender, JSCallbackEventArgs e);
        public event JSCallbackEventArgsHandler OnJSCallback;

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
        public event WebViewCrashedEventArgsHandler OnWebViewCrashed;

        public class RequestFileChooserEventArgs : EventArgs
        {
            public RequestFileChooserEventArgs(WebView webView, bool selectMultipleFiles, IntPtr title, IntPtr defaultPaths)
            {
                this.webView = webView;
                this.selectMultipleFiles = selectMultipleFiles;
                this.title = title;
                this.defaultPaths = defaultPaths;
            }

            public WebView webView;
            public bool selectMultipleFiles;
            public IntPtr title;
            public IntPtr defaultPaths;
        };

        public delegate void RequestFileChooserEventArgsHandler(object sender, RequestFileChooserEventArgs e);
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
        public event GetScrollDataEventArgsHandler OnGetScrollData;

        public class ResourceRequestEventArgs : EventArgs
        {
            public ResourceRequestEventArgs(WebView webView, string url, string referrer, string httpMethod, byte[] postData)
            {
                this.webView = webView;
                this.url = url;
                this.referrer = referrer;
                this.httpMethod = httpMethod;
                this.postData = postData;
            }

            public WebView webView;
            public string url;
            public string referrer;
            public string httpMethod;
            public byte[] postData;
        }

        public delegate ResourceResponse ResourceRequestEventArgsHandler(object sender, ResourceRequestEventArgs e);
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


            resourceRequestCallback = internalResourceRequestCallback;
            awe_webview_set_callback_resource_request(webview, resourceRequestCallback);

           // resourceResponseCallback = internalResourceResponseCallback;
           // awe_webview_set_callback_resource_response(webview, resourceResponseCallback);
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
        public void SetObjectCallback(string objectName,
                                      string callbackName)
        {
            StringHelper objectNameStr = new StringHelper(objectName);
            StringHelper callbackNameStr = new StringHelper(callbackName);

            awe_webview_set_object_callback(instance, objectNameStr.value(), callbackNameStr.value());
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

        public bool IsDirty()
        {
            return awe_webview_is_dirty(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern Rect awe_webview_get_dirty_bounds(IntPtr webview);

        public Rect GetDirtyBounds()
        {
            return awe_webview_get_dirty_bounds(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_webview_render(IntPtr webview);

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
                                                   int scroll_amount);

        public void InjectMouseWheel(int scrollAmount)
        {
            awe_webview_inject_mouse_wheel(instance, scrollAmount);
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

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_resize(IntPtr webview,
                                       int width,
                                       int height,
                                       bool wait_for_repaint,
                                       int repaint_timeout_ms);

        public void Resize(int width,
                           int height,
                           bool waitForRepaint = true,
                           int repaintTimeoutMs = 300)
        {
            awe_webview_resize(instance, width, height, waitForRepaint, repaintTimeoutMs);
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
                                                 IntPtr field_names,
                                                 IntPtr field_values);

        public void SetHeaderDefinition(string name,
                                        uint numFields,
                                        string fieldNames,
                                        string fieldValues)
        {
            StringHelper nameStr = new StringHelper(name);
            StringHelper fieldNamesStr = new StringHelper(fieldNames);
            StringHelper fieldValuesStr = new StringHelper(fieldValues);

            awe_webview_set_header_definition(instance, nameStr.value(), numFields, fieldNamesStr.value(), fieldValuesStr.value());
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

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackBeginNavigationCallback(IntPtr caller, IntPtr url, IntPtr frame_name);

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
        public delegate void CallbackBeginLoadingCallback(IntPtr caller, IntPtr url, IntPtr frame_name, int status_code, IntPtr mime_type);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_begin_loading(IntPtr webview, CallbackBeginLoadingCallback callback);

        private void internalBeginLoadingCallback(IntPtr caller, IntPtr url, IntPtr frame_name, int status_code, IntPtr mime_type)
        {
            BeginLoadingEventArgs e = new BeginLoadingEventArgs(this, StringHelper.ConvertAweString(url), StringHelper.ConvertAweString(frame_name), status_code, StringHelper.ConvertAweString(mime_type));

            if (OnBeginLoading != null)
                OnBeginLoading(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackFinishLoadingCallback(IntPtr caller);

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
        public delegate void CallbackJsCallback(IntPtr caller, IntPtr object_name, IntPtr callback_name, IntPtr arguments);

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
        public delegate void CallbackReceiveTitleCallback(IntPtr caller, IntPtr title, IntPtr frame_name);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_receive_title(IntPtr webview, CallbackReceiveTitleCallback callback);

        private void internalReceiveTitleCallback(IntPtr caller, IntPtr title, IntPtr frame_name)
        {
            ReceiveTitleEventArgs e = new ReceiveTitleEventArgs(this, StringHelper.ConvertAweString(title), StringHelper.ConvertAweString(frame_name));

            if (OnReceiveTitle != null)
                OnReceiveTitle(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackChangeTooltipCallback(IntPtr caller, IntPtr tooltip);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_change_tooltip(IntPtr webview, CallbackChangeTooltipCallback callback);

        private void internalChangeTooltipCallback(IntPtr caller, IntPtr tooltip)
        {
            ChangeTooltipEventArgs e = new ChangeTooltipEventArgs(this, StringHelper.ConvertAweString(tooltip));

            if (OnChangeTooltip != null)
                OnChangeTooltip(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackChangeCursorCallback(IntPtr caller, int cursor);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_change_cursor(IntPtr webview, CallbackChangeCursorCallback callback);

        private void internalChangeCursorCallback(IntPtr caller, int cursor)
        {
            ChangeCursorEventArgs e = new ChangeCursorEventArgs(this, (CursorType)cursor);

            if (OnChangeCursor != null)
                OnChangeCursor(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackChangeKeyboardFocusCallback(IntPtr caller, bool is_focused);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_change_keyboard_focus(IntPtr webview, CallbackChangeKeyboardFocusCallback callback);

        private void internalChangeKeyboardFocusCallback(IntPtr caller, bool is_focused)
        {
            ChangeKeyboardFocusEventArgs e = new ChangeKeyboardFocusEventArgs(this, is_focused);

            if (OnChangeKeyboardFocus != null)
                OnChangeKeyboardFocus(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackChangeTargetURLCallback(IntPtr caller, IntPtr url);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_change_target_url(IntPtr webview, CallbackChangeTargetURLCallback callback);

        private void internalChangeTargetURLCallback(IntPtr caller, IntPtr url)
        {
            ChangeTargetUrlEventArgs e = new ChangeTargetUrlEventArgs(this, StringHelper.ConvertAweString(url));

            if (OnChangeTargetUrl != null)
                OnChangeTargetUrl(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackOpenExternalLinkCallback(IntPtr caller, IntPtr url, IntPtr source);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_open_external_link(IntPtr webview, CallbackOpenExternalLinkCallback callback);

        private void internalOpenExternalLinkCallback(IntPtr caller, IntPtr url, IntPtr source)
        {
            OpenExternalLinkEventArgs e = new OpenExternalLinkEventArgs(this, StringHelper.ConvertAweString(url), StringHelper.ConvertAweString(source));

            if (OnOpenExternalLink != null)
                OnOpenExternalLink(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackRequestDownloadCallback(IntPtr caller, IntPtr download);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_request_download(IntPtr webview, CallbackRequestDownloadCallback callback);

        private void internalRequestDownloadCallback(IntPtr caller, IntPtr download)
        {
            RequestDownloadEventArgs e = new RequestDownloadEventArgs(this, StringHelper.ConvertAweString(download));

            if (OnRequestDownload != null)
                OnRequestDownload(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackWebviewCrashedCallback(IntPtr caller);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_web_view_crashed(IntPtr webview, CallbackWebviewCrashedCallback callback);

        private void internalWebviewCrashedCallback(IntPtr caller)
        {
            WebViewCrashedEventArgs e = new WebViewCrashedEventArgs(this);

            if (OnWebViewCrashed != null)
                OnWebViewCrashed(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackPluginCrashedCallback(IntPtr caller, IntPtr plugin_name);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_plugin_crashed(IntPtr webview, CallbackPluginCrashedCallback callback);

        private void internalPluginCrashedCallback(IntPtr caller, IntPtr plugin_name)
        {
            PluginCrashedEventArgs e = new PluginCrashedEventArgs(this, StringHelper.ConvertAweString(plugin_name));

            if (OnPluginCrashed != null)
                OnPluginCrashed(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackRequestMoveCallback(IntPtr caller, int x, int y);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_request_move(IntPtr webview, CallbackRequestMoveCallback callback);

        private void internalRequestMoveCallback(IntPtr caller, int x, int y)
        {
            RequestMoveEventArgs e = new RequestMoveEventArgs(this, x, y);

            if (OnRequestMove != null)
                OnRequestMove(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackGetPageContentsCallback(IntPtr caller, IntPtr url, IntPtr contents);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_get_page_contents(IntPtr webview, CallbackGetPageContentsCallback callback);

        private void internalGetPageContentsCallback(IntPtr caller, IntPtr url, IntPtr contents)
        {
            GetPageContentsEventArgs e = new GetPageContentsEventArgs(this, StringHelper.ConvertAweString(url), StringHelper.ConvertAweString(contents));

            if (OnGetPageContents != null)
                OnGetPageContents(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackDomReadyCallback(IntPtr caller);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_dom_ready(IntPtr webview, CallbackDomReadyCallback callback);

        private void internalDomReadyCallback(IntPtr caller)
        {
            DomReadyEventArgs e = new DomReadyEventArgs(this);

            if (OnDomReady != null)
                OnDomReady(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackRequestFileChooserCallback(IntPtr caller, bool select_multiple_files, IntPtr title, IntPtr default_path);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_request_file_chooser(IntPtr webview, CallbackRequestFileChooserCallback callback);

        private void internalRequestFileChooser(IntPtr caller, bool select_multiple_files, IntPtr title, IntPtr default_paths)
        {
            RequestFileChooserEventArgs e = new RequestFileChooserEventArgs(this, select_multiple_files, title, default_paths);

            if (OnRequestFileChooser != null)
                OnRequestFileChooser(this, e);
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackGetScrollDataCallback(IntPtr caller, int contentWidth, int contentHeight, int preferredWidth, int scrollX, int scrollY);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_get_scroll_data(IntPtr webview, CallbackGetScrollDataCallback callback);

        private void internalGetScrollData(IntPtr caller, int contentWidth, int contentHeight, int preferredWidth, int scrollX, int scrollY)
        {
            GetScrollDataEventArgs e = new GetScrollDataEventArgs(this, contentWidth, contentHeight, preferredWidth, scrollX, scrollY);

            if (OnGetScrollData != null)
                OnGetScrollData(this, e);
        }


        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate IntPtr CallbackResourceRequestCallback(IntPtr caller, IntPtr url, IntPtr referrer, IntPtr http_method, 
                                                                IntPtr post_data, uint post_data_length);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_webview_set_callback_resource_request(IntPtr webview, CallbackResourceRequestCallback callback);

        private IntPtr internalResourceRequestCallback(IntPtr caller, IntPtr url, IntPtr referrer, IntPtr http_method,
                                                                IntPtr post_data, uint post_data_length)
        {
            byte[] postDataBytes = null;

            if (post_data_length > 0)
            {
                postDataBytes = new byte[post_data_length];
                Marshal.Copy(post_data, postDataBytes, 0, (int)post_data_length);
            }

            ResourceRequestEventArgs e = new ResourceRequestEventArgs(this, StringHelper.ConvertAweString(url), StringHelper.ConvertAweString(referrer),
                            StringHelper.ConvertAweString(http_method), postDataBytes);

            if (OnResourceRequest != null)
            {
                ResourceResponse response = OnResourceRequest(this, e);

                if (response != null)
                    return response.getInstance();
            }

            return IntPtr.Zero;
        }

        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        public delegate void CallbackResourceResponseCallback(IntPtr caller, IntPtr url, int statusCode, bool wasCached, long requestTimeMs,
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
    }

    public class ResourceResponse
    {
        private IntPtr instance;

        public ResourceResponse(byte[] data, string mimeType)
        {
            StringHelper mimeTypeStr = new StringHelper(mimeType);

            IntPtr dataPtr = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, dataPtr, data.Length);

            instance = awe_resource_response_create((uint)data.Length, dataPtr, mimeTypeStr.value());

            Marshal.FreeHGlobal(dataPtr);
        }

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
