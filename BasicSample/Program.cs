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
            // We only want to save an image in this example so
            // we don't set AutoUpdate to true.
            WebCore.Initialize( new WebCoreConfig() );

            WebView webView = WebCore.CreateWebview( 800, 600 );
            webView.LoadURL( "http://www.google.com" );
            webView.LoadCompleted += onFinishLoading;

            while ( !finishedLoading )
            {
                Thread.Sleep( 100 );
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
