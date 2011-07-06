using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void CursorChangedEventHandler( object sender, ChangeCursorEventArgs e );

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
