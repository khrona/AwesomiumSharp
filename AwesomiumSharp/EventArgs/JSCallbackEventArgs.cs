using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    internal delegate void JSCallbackCalledEventHandler( object sender, JSCallbackEventArgs e );

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
