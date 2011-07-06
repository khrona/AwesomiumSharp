using System;
using System.Runtime.InteropServices;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// This class represents a single batch of "upload data" to be sent with
    /// a ResourceRequest. Also commonly known as "POST" data.
    /// </summary>
    public class UploadElement
    {
        private IntPtr instance;

        internal UploadElement( IntPtr instance )
        {
            this.instance = instance;
        }

        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_upload_element_is_file_path( IntPtr ele );

        public bool IsFilePath
        {
            get
            {
                return awe_upload_element_is_file_path( instance );
            }
        }

        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_upload_element_is_bytes( IntPtr ele );

        public bool IsBytes
        {
            get
            {
                return awe_upload_element_is_bytes( instance );
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_upload_element_get_bytes( IntPtr ele );

        public string GetBytes()
        {
            return StringHelper.ConvertAweString( awe_upload_element_get_bytes( instance ), true );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_upload_element_get_file_path( IntPtr ele );

        public string GetFilePath()
        {
            return StringHelper.ConvertAweString( awe_upload_element_get_file_path( instance ), true );
        }
    }

}
