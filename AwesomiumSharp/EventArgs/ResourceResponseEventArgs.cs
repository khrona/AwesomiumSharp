using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.ResourceResponse"/> and 
    /// <see cref="Windows.Controls.WebControl.ResourceResponse"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="ResourceResponseEventArgs"/> that contains the event data.</param>
    public delegate void ResourceResponseEventHandler( object sender, ResourceResponseEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.ResourceResponse"/> and 
    /// <see cref="Windows.Controls.WebControl.ResourceResponse"/> events.
    /// </summary>
    public class ResourceResponseEventArgs : UrlEventArgs
    {
        public ResourceResponseEventArgs( string url, 
            int statusCode, 
            bool wasCached, 
            long requestTimeMs, 
            long responseTimeMs, 
            long expectedContentSize, 
            string mimeType )
            : base( url )
        {
            this.statusCode = statusCode;
            this.wasCached = wasCached;
            this.requestTimeMs = requestTimeMs;
            this.responseTimeMs = responseTimeMs;
            this.expectedContentSize = expectedContentSize;
            this.mimeType = mimeType;
        }

        private int statusCode;
        public int StatusCode
        {
            get
            {
                return statusCode;
            }
        }
        private bool wasCached;
        public bool WasCached
        {
            get
            {
                return wasCached;
            }
        }
        private long requestTimeMs;
        public long RequestTimeMs
        {
            get
            {
                return requestTimeMs;
            }
        }
        private long responseTimeMs;
        public long ResponseTimeMs
        {
            get
            {
                return responseTimeMs;
            }
        }
        private long expectedContentSize;
        public long ExpectedContentSize
        {
            get
            {
                return expectedContentSize;
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
