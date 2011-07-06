using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void BeginLoadingEventHandler( object sender, BeginLoadingEventArgs e );

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
