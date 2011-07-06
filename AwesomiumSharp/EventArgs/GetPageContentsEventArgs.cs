using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void PageContentsReceivedEventHandler( object sender, GetPageContentsEventArgs e );

    public class GetPageContentsEventArgs : UrlEventArgs
    {
        public GetPageContentsEventArgs( string url, string contents )
            : base( url )
        {
            this.contents = contents;
        }

        private string contents;
        public string Contents
        {
            get
            {
                return contents;
            }
        }
    }
}
