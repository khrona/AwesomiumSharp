using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.PluginCrashed"/> and 
    /// <see cref="Windows.Controls.WebControl.PluginCrashed"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="PluginCrashedEventArgs"/> that contains the event data.</param>
    public delegate void PluginCrashedEventHandler( object sender, PluginCrashedEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.PluginCrashed"/> and <see cref="Windows.Controls.WebControl.PluginCrashed"/> events.
    /// </summary>
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
