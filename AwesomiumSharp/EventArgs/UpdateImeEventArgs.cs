using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.ImeUpdated"/> and 
    /// <see cref="Windows.Controls.WebControl.ImeUpdated"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="UpdateImeEventArgs"/> that contains the event data.</param>
    public delegate void ImeUpdatedEventHandler( object sender, UpdateImeEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.ImeUpdated"/> and <see cref="Windows.Controls.WebControl.ImeUpdated"/> events.
    /// </summary>
    public class UpdateImeEventArgs : EventArgs
    {
        public UpdateImeEventArgs( IMEState state, AweRect caretRect )
        {
            this.state = state;
            this.caretRect = caretRect;
        }

        private IMEState state;
        public IMEState State
        {
            get
            {
                return state;
            }
        }
        private AweRect caretRect;
        public AweRect CaretRectangle
        {
            get
            {
                return caretRect;
            }
        }
    }
}
