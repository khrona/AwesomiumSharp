using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the callback that is invoked when the respective function of a Javascript object previously created with
    /// <see cref="WebView.CreateObject"/>, is called from Javascript.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="JSCallbackEventArgs"/> that contains the event data.</param>
    public delegate void JSCallback(object sender, JSCallbackEventArgs e);

    /// <summary>
    /// Provides data to a <see cref="JSCallback"/> callback.
    /// </summary>
    public class JSCallbackEventArgs : EventArgs
    {
        public JSCallbackEventArgs( string objectName, string callbackName, JSValue[] args )
        {
            this.objectName = objectName;
            this.callbackName = callbackName;
            this.args = args;
        }

        private string objectName;
        public string ObjectName
        {
            get
            {
                return objectName;
            }
        }
        private string callbackName;
        public string CallbackName
        {
            get
            {
                return callbackName;
            }
        }
        private JSValue[] args;
        public JSValue[] Arguments
        {
            get
            {
                return args;
            }
        }
    }
}
