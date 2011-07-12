using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.PageContentsReceived"/> and 
    /// <see cref="Windows.Controls.WebControl.PageContentsReceived"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="GetPageContentsEventArgs"/> that contains the event data.</param>
    public delegate void PageContentsReceivedEventHandler( object sender, GetPageContentsEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.PageContentsReceived"/> and 
    /// <see cref="Windows.Controls.WebControl.PageContentsReceived"/> events.
    /// </summary>
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
