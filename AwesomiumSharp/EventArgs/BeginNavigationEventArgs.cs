using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void BeginNavigationEventHandler( object sender, BeginNavigationEventArgs e );

    public class BeginNavigationEventArgs : UrlEventArgs
    {

        public BeginNavigationEventArgs( string url, string frameName )
            : base( url )
        {
            this.frameName = frameName;
        }

        private string frameName;
        public string FrameName
        {
            get
            {
                return frameName;
            }
        }
    }
}
