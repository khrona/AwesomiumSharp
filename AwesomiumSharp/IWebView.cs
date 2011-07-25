/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : IWebView.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/03/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    Allows WebCore to communicate with views of any kind. This interface
 *    is internal.
 *    
 ********************************************************************************/

using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    internal interface IWebView
    {
        // Used by WebCore.
        void Close();
        IntPtr Instance { get; set; }
        bool IsDirty { get; set; }

        // Used by Javascript helpers.
        void CreateObject( string objectName );
        void DestroyObject( string objectName );
        void SetObjectProperty( string objectName, string propertyName, JSValue val );
        void SetObjectCallback( string objectName, string callbackName, JSCallback callback );
        void ExecuteJavascript( string javascript, string frameName = "" );
        JSValue ExecuteJavascriptWithResult( string javascript, string frameName = "", int timeoutMs = 0 );

        // The intention is to bring all common members of WebView and WebControl
        // to this interface. We will then publicly expose this interface. This will
        // allow developers to access both WebViews and WebControls in their application,
        // securely using a common interface reference. This may also provide some elementary
        // level of version independence for WebView users only. In time...
    }
}
