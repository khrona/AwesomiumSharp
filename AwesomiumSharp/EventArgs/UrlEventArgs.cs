using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle events that provide a URL.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="UrlEventArgs"/> that contains the event data.</param>
    public delegate void UrlEventHandler( object sender, UrlEventArgs e );

    /// <summary>
    /// Provides data for events that provide a URL.
    /// </summary>
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
