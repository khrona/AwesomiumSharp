using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.CursorChanged"/> and 
    /// <see cref="Windows.Controls.WebControl.CursorChanged"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="ChangeCursorEventArgs"/> that contains the event data.</param>
    public delegate void CursorChangedEventHandler( object sender, ChangeCursorEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.CursorChanged"/> and 
    /// <see cref="Windows.Controls.WebControl.CursorChanged"/> events.
    /// </summary>
    public class ChangeCursorEventArgs : EventArgs
    {
        public ChangeCursorEventArgs( CursorType cursorType )
        {
            this.cursorType = cursorType;
        }

        private CursorType cursorType;
        public CursorType CursorType
        {
            get
            {
                return cursorType;
            }
        }
    }
}
