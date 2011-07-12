using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.TitleReceived"/> and 
    /// <see cref="Windows.Controls.WebControl.TitleReceived"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="ReceiveTitleEventArgs"/> that contains the event data.</param>
    public delegate void TitleReceivedEventHandler( object sender, ReceiveTitleEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.TitleReceived"/> and <see cref="Windows.Controls.WebControl.TitleReceived"/> events.
    /// </summary>
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
