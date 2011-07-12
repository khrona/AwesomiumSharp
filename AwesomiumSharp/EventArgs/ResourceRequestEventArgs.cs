using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.ResourceRequest"/> and 
    /// <see cref="Windows.Controls.WebControl.ResourceRequest"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="ResourceRequestEventArgs"/> that contains the event data.</param>
    public delegate ResourceResponse ResourceRequestEventHandler( object sender, ResourceRequestEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.ResourceRequest"/> and 
    /// <see cref="Windows.Controls.WebControl.ResourceRequest"/> events.
    /// </summary>
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
