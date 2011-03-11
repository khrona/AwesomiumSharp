using System;
using System.Collections.Generic;
using System.Linq;
using AwesomiumSharp;
using System.Threading;

namespace Basic
{
    class Program
    {
        static bool finishedLoading = false;

        static void Main(string[] args)
        {
            WebCore.Config conf = new WebCore.Config();
            conf.customCSS = "body { font-size: 2px !important; }";
            WebCore.Initialize(conf);

            WebView webView = WebCore.CreateWebview(800, 600);
            webView.LoadURL("http://www.google.com");
            webView.OnFinishLoading += onFinishLoading;

            while (webView.IsLoadingPage())
            {
                Thread.Sleep(100);
                WebCore.Update();
            }

            webView.Render().SaveToPNG("result.png", true);
            System.Diagnostics.Process.Start("result.png");

            WebCore.Shutdown();
        }

        private static void onFinishLoading(object sender, WebView.FinishLoadingEventArgs e)
        {
            finishedLoading = true;
        }
    }
}
