using System;
using System.Runtime.InteropServices;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents a batch of "upload" data sent along with the <see cref="ResourceRequest"/>. 
    /// This data is usually sent with a POST request.
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

        /// <summary>
        /// Gets if this <see cref="UploadElement"/> is a file.
        /// </summary>
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

        /// <summary>
        /// Gets if this <see cref="UploadElement"/> is a string of bytes.
        /// </summary>
        public bool IsBytes
        {
            get
            {
                return awe_upload_element_is_bytes( instance );
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_upload_element_get_bytes( IntPtr ele );

        /// <summary>
        /// Gets the string of bytes associated with this <see cref="UploadElement"/>.
        /// </summary>
        public string GetBytes()
        {
            return StringHelper.ConvertAweString( awe_upload_element_get_bytes( instance ), true );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_upload_element_get_file_path( IntPtr ele );

        /// <summary>
        /// Get the file path associated with this <see cref="UploadElement"/>.
        /// </summary>
        public string GetFilePath()
        {
            return StringHelper.ConvertAweString( awe_upload_element_get_file_path( instance ), true );
        }
    }

}
