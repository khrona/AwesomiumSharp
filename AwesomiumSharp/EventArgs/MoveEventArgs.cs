using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.Move"/> and 
    /// <see cref="Windows.Controls.WebControl.Move"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="MoveEventArgs"/> that contains the event data.</param>
    public delegate void MoveEventHandler( object sender, MoveEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.Move"/> and <see cref="Windows.Controls.WebControl.Move"/> events.
    /// </summary>
    public class MoveEventArgs : EventArgs
    {
        public MoveEventArgs( int x, int y )
        {
            this.x = x;
            this.y = y;
        }

        private int x;
        public int X
        {
            get
            {
                return x;
            }
        }
        private int y;
        public int Y
        {
            get
            {
                return y;
            }
        }
    }
}
