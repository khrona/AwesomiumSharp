using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    internal interface IWebView
    {
        void OnCoreAutoUpdateChanged( bool newValue );
        void PrepareForShutdown();
        IntPtr Instance { get; set; }
        bool IsDirty { get; set; }
    }
}
