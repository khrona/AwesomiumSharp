using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate ResourceResponse ResourceRequestEventHandler( object sender, ResourceRequestEventArgs e );

    public class ResourceRequestEventArgs : EventArgs
    {
        public ResourceRequestEventArgs( ResourceRequest request )
        {
            this.request = request;
        }

        private ResourceRequest request;
        public ResourceRequest Request
        {
            get
            {
                return request;
            }
        }
    }
}
