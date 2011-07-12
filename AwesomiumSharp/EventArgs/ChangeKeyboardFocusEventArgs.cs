using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.KeyboardFocusChanged"/> and 
    /// <see cref="Windows.Controls.WebControl.KeyboardFocusChanged"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="ChangeKeyboardFocusEventArgs"/> that contains the event data.</param>
    public delegate void KeyboardFocusChangedEventHandler( object sender, ChangeKeyboardFocusEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.KeyboardFocusChanged"/> and 
    /// <see cref="Windows.Controls.WebControl.KeyboardFocusChanged"/> events.
    /// </summary>
    public class ChangeKeyboardFocusEventArgs : EventArgs
    {
        public ChangeKeyboardFocusEventArgs( bool isFocused )
        {
            this.isFocused = isFocused;
        }

        private bool isFocused;
        public bool IsFocused
        {
            get
            {
                return isFocused;
            }
        }
    }
}
