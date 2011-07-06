using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void KeyboardFocusChangedEventHandler( object sender, ChangeKeyboardFocusEventArgs e );

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
