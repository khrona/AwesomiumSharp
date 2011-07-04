using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void PluginCrashedEventHandler( object sender, PluginCrashedEventArgs e );

    public class PluginCrashedEventArgs : EventArgs
    {
        public PluginCrashedEventArgs( string pluginName )
        {
            this.pluginName = pluginName;
        }

        private string pluginName;
        public string PluginName
        {
            get
            {
                return pluginName;
            }
        }
    }
}
