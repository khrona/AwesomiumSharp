using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.OpenExternalLink"/> and 
    /// <see cref="Windows.Controls.WebControl.OpenExternalLink"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="OpenExternalLinkEventArgs"/> that contains the event data.</param>
    public delegate void OpenExternalLinkEventHandler( object sender, OpenExternalLinkEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.OpenExternalLink"/> and 
    /// <see cref="Windows.Controls.WebControl.OpenExternalLink"/> events.
    /// </summary>
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
