using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.JSConsoleMessageAdded"/> and 
    /// <see cref="Windows.Controls.WebControl.JSConsoleMessageAdded"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="JSConsoleMessageEventArgs"/> that contains the event data.</param>
    public delegate void JSConsoleMessageAddedEventHandler( object sender, JSConsoleMessageEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.JSConsoleMessageAdded"/> and 
    /// <see cref="Windows.Controls.WebControl.JSConsoleMessageAdded"/> events.
    /// </summary>
    public class JSConsoleMessageEventArgs : EventArgs
    {
        public JSConsoleMessageEventArgs( string message, int lineNumber, string source )
        {
            this.message = message;
            this.lineNumber = lineNumber;
            this.source = source;
        }

        private string message;
        public string Message
        {
            get
            {
                return message;
            }
        }
        private int lineNumber;
        public int LineNumber
        {
            get
            {
                return lineNumber;
            }
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
