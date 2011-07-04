using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void ImeUpdatedEventHandler( object sender, UpdateImeEventArgs e );

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
