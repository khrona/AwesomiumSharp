using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void UrlEventHandler( object sender, UrlEventArgs e );

    public class UrlEventArgs : EventArgs
    {
        public UrlEventArgs( string url )
        {
            this.url = url;
        }

        private string url;
        public string Url
        {
            get
            {
                return url;
            }
        }
    }
}
