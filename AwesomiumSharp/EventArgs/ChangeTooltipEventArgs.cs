using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void TooltipChangedEventHandler( object sender, ChangeTooltipEventArgs e );

    public class ChangeTooltipEventArgs : EventArgs
    {
        public ChangeTooltipEventArgs( string tooltip )
        {
            this.tooltip = tooltip;
        }

        private string tooltip;
        public string Tooltip
        {
            get
            {
                return tooltip;
            }
        }
    }
}
