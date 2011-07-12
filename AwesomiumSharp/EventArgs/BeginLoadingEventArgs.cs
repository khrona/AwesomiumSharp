using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.BeginLoading"/> and 
    /// <see cref="Windows.Controls.WebControl.BeginLoading"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="BeginLoadingEventArgs"/> that contains the event data.</param>
    public delegate void BeginLoadingEventHandler( object sender, BeginLoadingEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.BeginLoading"/> and 
    /// <see cref="Windows.Controls.WebControl.BeginLoading"/> events.
    /// </summary>
    public class BeginLoadingEventArgs : UrlEventArgs
    {
        public BeginLoadingEventArgs( string url, string frameName, int statusCode, string mimeType )
            : base( url )
        {
            this.frameName = frameName;
            this.statusCode = statusCode;
            this.mimeType = mimeType;
        }

        private string frameName;
        public string FrameName
        {
            get
            {
                return frameName;
            }
        }
        private int statusCode;
        public int StatusCode
        {
            get
            {
                return statusCode;
            }
        }
        private string mimeType;
        public string MimeType
        {
            get
            {
                return mimeType;
            }
        }
    }
}
