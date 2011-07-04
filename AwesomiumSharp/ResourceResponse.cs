using System;
using System.Runtime.InteropServices;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// This class allows you to override the response for a certain ResourceRequest.
    /// </summary>
    public class ResourceResponse
    {
        private IntPtr instance;

        /// <summary>
        /// Create a ResourceResponse from a byte array
        /// </summary>
        /// <param name="data">The data to be initialized from (a copy is made)</param>
        /// <param name="mimeType">The mime-type of the data (for ex. "text/html")</param>
        public ResourceResponse( byte[] data, string mimeType )
        {
            StringHelper mimeTypeStr = new StringHelper( mimeType );

            IntPtr dataPtr = Marshal.AllocHGlobal( data.Length );
            Marshal.Copy( data, 0, dataPtr, data.Length );

            instance = awe_resource_response_create( (uint)data.Length, dataPtr, mimeTypeStr.value() );

            Marshal.FreeHGlobal( dataPtr );
        }

        /// <summary>
        /// Create a ResourceResponse from a file on disk
        /// </summary>
        /// <param name="filePath"></param>
        public ResourceResponse( string filePath )
        {
            StringHelper filePathStr = new StringHelper( filePath );

            instance = awe_resource_response_create_from_file( filePathStr.value() );
        }

        internal IntPtr getInstance()
        {
            return instance;
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_resource_response_create( uint num_bytes,
                                                                    IntPtr buffer,
                                                                    IntPtr mime_type );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_resource_response_create_from_file( IntPtr file_path );
    }
}
