using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void MoveEventHandler( object sender, MoveEventArgs e );

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
