using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void JSConsoleMessageAddedEventHandler( object sender, JSConsoleMessageEventArgs e );

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
