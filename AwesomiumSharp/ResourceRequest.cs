using System;
using System.Runtime.InteropServices;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public class ResourceRequest
    {
        #region Fields
        private IntPtr instance;
        #endregion


        #region Ctor
        internal ResourceRequest( IntPtr instance )
        {
            this.instance = instance;
        }
        #endregion


        #region Methods
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_resource_request_cancel( IntPtr request );

        public void Cancel()
        {
            awe_resource_request_cancel( instance );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_resource_request_append_extra_header( IntPtr request, IntPtr name, IntPtr value );

        /// <summary>
        /// Appends a new header to this request.
        /// </summary>
        /// <param name="name">The name of the header to append.</param>
        /// <param name="value">The value of the header.</param>
        public void AppendExtraHeader( string name, string value )
        {
            StringHelper nameStr = new StringHelper( name );
            StringHelper valueStr = new StringHelper( value );

            awe_resource_request_append_extra_header( instance, nameStr.value(), valueStr.value() );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static IntPtr awe_resource_request_get_upload_element( IntPtr request, uint idx );

        /// <summary>
        /// Get a certain upload element (returned instance is owned by this class)	
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public UploadElement GetUploadElement( uint idx )
        {
            return new UploadElement( awe_resource_request_get_upload_element( instance, idx ) );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_resource_request_clear_upload_elements( IntPtr request );

        public void ClearUploadElements()
        {
            awe_resource_request_clear_upload_elements( instance );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_resource_request_append_upload_file_path( IntPtr request, IntPtr file_path );

        /// <summary>
        ///  Append a file for POST data (adds a new UploadElement)	
        /// </summary>
        /// <param name="filePath"></param>
        public void AppendUploadFilePath( string filePath )
        {
            StringHelper filePathStr = new StringHelper( filePath );
            awe_resource_request_append_upload_file_path( instance, filePathStr.value() );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_resource_request_append_upload_bytes( IntPtr request, IntPtr bytes );

        /// <summary>
        /// Append a string of bytes for POST data (adds a new UploadElement)	
        /// </summary>
        /// <param name="bytes"></param>
        public void AppendUploadBytes( string bytes )
        {
            StringHelper bytesStr = new StringHelper( bytes );
            awe_resource_request_append_upload_bytes( instance, bytesStr.value() );
        }
        #endregion

        #region Properties
        internal IntPtr Instance
        {
            get
            {
                return instance;
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_resource_request_get_url( IntPtr request );

        public string Url
        {
            get
            {
                return StringHelper.ConvertAweString( awe_resource_request_get_url( instance ), true );
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_resource_request_get_method( IntPtr request );
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_resource_request_set_method( IntPtr request, IntPtr method );

        /// <summary>
        /// Get or sets the method for the request (usually either "GET" or "POST")
        /// </summary>
        public string Method
        {
            get
            {
                return StringHelper.ConvertAweString( awe_resource_request_get_method( instance ), true );
            }
            set
            {
                StringHelper methodStr = new StringHelper( value );
                awe_resource_request_set_method( instance, methodStr.value() );
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_resource_request_get_referrer( IntPtr request );
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_resource_request_set_referrer( IntPtr request, IntPtr referrer );

        public string Referrer
        {
            get
            {
                return StringHelper.ConvertAweString( awe_resource_request_get_referrer( instance ), true );
            }
            set
            {
                StringHelper referrerStr = new StringHelper( value );
                awe_resource_request_set_referrer( instance, referrerStr.value() );
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_resource_request_get_extra_headers( IntPtr request );
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_resource_request_set_extra_headers( IntPtr request, IntPtr headers );

        /// <summary>
        /// Gets or sets the extra headers for the request. Each header is delimited by /r/n (CRLF)
        /// Headers should NOT end in /r/n (CRLF).
        /// </summary>
        public string ExtraHeaders
        {
            get
            {
                return StringHelper.ConvertAweString( awe_resource_request_get_extra_headers( instance ), true );
            }
            set
            {
                StringHelper headersStr = new StringHelper( value );
                awe_resource_request_set_extra_headers( instance, headersStr.value() );
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern uint awe_resource_request_get_num_upload_elements( IntPtr request );

        /// <summary>
        /// Gets the number of upload elements (essentially, batches of POST data).
        /// </summary>
        public uint UploadElementsCount
        {
            get
            {
                return awe_resource_request_get_num_upload_elements( instance );
            }
        }
        #endregion
    }

}
