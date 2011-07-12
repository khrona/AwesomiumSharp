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
            // Some informative message.
            Console.WriteLine( "Getting a 800x600 snapshot of http://www.google.com ..." );

            // We demonstrate an easy way to hide the scrollbars by providing
            // custom CSS. Read more about how to style the scrollbars here:
            // http://www.webkit.org/blog/363/styling-scrollbars/.
            // Just consider that this setting is global. If you want to apply
            // a similar effect for single pages, you can use ExecuteJavascript
            // and pass: document.documentElement.style.overflow = 'hidden';
            // (Unfortunately WebKit's scrollbar does not have a DOM equivalent yet)
            WebCore.Initialize( new WebCoreConfig() { CustomCSS = "::-webkit-scrollbar { visibility: hidden; }" } );

            // WebView implements IDisposable. You can dispose and destroy
            // the view by calling WebView.Close(). Here we demonstrate
            // wrapping it in a using statement.
            using ( WebView webView = WebCore.CreateWebview( 800, 600 ) )
            {
                webView.LoadURL( "http://www.google.com" );
                webView.LoadCompleted += OnFinishLoading;

                while ( !finishedLoading )
                {
                    Thread.Sleep( 100 );
                    // A Console application does not have a synchronization
                    // context, thus auto-update won't be enabled on WebCore.
                    // We need to manually call Update here.
                    WebCore.Update();
                }

                // Render to a pixel buffer and save the buffer
                // to a .png image.
                webView.Render().SaveToPNG( "result.png", true );
            }

            // Announce.
            Console.Write( "Hit any key to see the result..." );
            Console.ReadKey( true );

            // Start the application associated with .png files
            // and display the file.
            Process.Start( "result.png" );

            // Shut down Awesomium before exiting.
            WebCore.Shutdown();
        }

        private static void OnFinishLoading( object sender, EventArgs e )
        {
            finishedLoading = true;
        }
    }
}
