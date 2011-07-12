using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.ToolTipChanged"/> and 
    /// <see cref="Windows.Controls.WebControl.ToolTipChanged"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="ChangeToolTipEventArgs"/> that contains the event data.</param>
    public delegate void ToolTipChangedEventHandler( object sender, ChangeToolTipEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.ToolTipChanged"/> and 
    /// <see cref="Windows.Controls.WebControl.ToolTipChanged"/> events.
    /// </summary>
    public class ChangeToolTipEventArgs : EventArgs
    {
        public ChangeToolTipEventArgs( string tooltip )
        {
            this.tooltip = tooltip;
        }

        private string tooltip;
        public string ToolTip
        {
            get
            {
                return tooltip;
            }
        }
    }
}
