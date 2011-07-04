using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void TitleReceivedEventHandler( object sender, ReceiveTitleEventArgs e );

    public class ReceiveTitleEventArgs : EventArgs
    {
        public ReceiveTitleEventArgs( string title, string frameName )
        {
            this.title = title;
            this.frameName = frameName;
        }

        private string title;
        public string Title
        {
            get
            {
                return title;
            }
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
