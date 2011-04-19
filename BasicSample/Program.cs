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
            WebCore.Initialize(new WebCore.Config());

            WebView webView = WebCore.CreateWebview(800, 600);
            webView.LoadURL("http://www.google.com");
            webView.OnFinishLoading += onFinishLoading;

            while (!finishedLoading)
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
