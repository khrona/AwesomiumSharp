using System;
using AwesomiumSharp;
using System.Threading;
using System.Diagnostics;

namespace Basic
{
    class Program
    {
        static bool finishedLoading;

        static void Main( string[] args )
        {
            // We demonstrate an easy way to hide the scrollbars by providing
            // custom CSS. Read more about how to style the scrollbars here:
            // http://www.webkit.org/blog/363/styling-scrollbars/
            WebCore.Initialize( new WebCoreConfig() { CustomCSS = "::-webkit-scrollbar { visibility: hidden; }" } );

            WebView webView = WebCore.CreateWebview( 800, 600 );
            webView.LoadURL( "http://www.google.com" );
            webView.LoadCompleted += onFinishLoading;

            while ( !finishedLoading )
            {
                Thread.Sleep( 100 );
                // A Console application does not have a synchronization
                // context, thus auto-update won't be enabled on WebCore.
                // We need to manually call Update here.
                WebCore.Update();
            }

            webView.Render().SaveToPNG( "result.png", true );
            Process.Start( "result.png" );

            WebCore.Shutdown();
        }

        private static void onFinishLoading( object sender, EventArgs e )
        {
            finishedLoading = true;
        }
    }
}
