using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.SelectionChanged"/> and 
    /// <see cref="Windows.Controls.WebControl.SelectionChanged"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="WebSelectionEventArgs"/> that contains the event data.</param>
    public delegate void WebSelectionChangedHandler( object sender, WebSelectionEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.SelectionChanged"/> and <see cref="Windows.Controls.WebControl.SelectionChanged"/> events.
    /// </summary>
    public class WebSelectionEventArgs : EventArgs
    {
        public WebSelectionEventArgs( Selection sel )
        {
            this.selection = sel;
        }

        private Selection selection;
        public Selection Selection
        {
            get
            {
                return selection;
            }
        }
    }
}
