using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void OpenExternalLinkEventHandler( object sender, OpenExternalLinkEventArgs e );

    public class OpenExternalLinkEventArgs : UrlEventArgs
    {
        public OpenExternalLinkEventArgs( string url, string source )
            : base( url )
        {
            this.source = source;
        }

        private string source;
        public string Source
        {
            get
            {
                return source;
            }
        }
    }
}
