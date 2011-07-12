/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : IWebView.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/03/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    Allows WebCore to communicate with views of any kind. This interface
 *    is internal.
 *    
 ********************************************************************************/

using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    internal interface IWebView
    {
        void Close();
        IntPtr Instance { get; set; }
        bool IsDirty { get; set; }
    }
}
