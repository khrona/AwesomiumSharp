using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.BeginNavigation"/> and 
    /// <see cref="Windows.Controls.WebControl.BeginNavigation"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="BeginNavigationEventArgs"/> that contains the event data.</param>
    public delegate void BeginNavigationEventHandler( object sender, BeginNavigationEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.BeginNavigation"/> and 
    /// <see cref="Windows.Controls.WebControl.BeginNavigation"/> events.
    /// </summary>
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
