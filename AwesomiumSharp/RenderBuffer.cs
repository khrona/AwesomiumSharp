using System;
using System.Runtime.InteropServices;
#if !USING_MONO
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
#endif

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// RenderBuffer represents a 32-bit BGRA pixel buffer. You can save it
    /// directly to an image or copy it to some other graphics surface for 
    /// display in your application. An instance of this class is returned by
    /// WebView.Render
    /// </summary>
    public class RenderBuffer
    {
        private IntPtr renderbuffer;

        internal RenderBuffer( IntPtr renderbuffer )
        {
            this.renderbuffer = renderbuffer;
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern int awe_renderbuffer_get_width( IntPtr renderbuffer );

        public int Width
        {
            get
            {
                return awe_renderbuffer_get_width( renderbuffer );
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern int awe_renderbuffer_get_height( IntPtr renderbuffer );

        public int Height
        {
            get
            {
                return awe_renderbuffer_get_height( renderbuffer );
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern int awe_renderbuffer_get_rowspan( IntPtr renderbuffer );

        public int Rowspan
        {
            get
            {
                return awe_renderbuffer_get_rowspan( renderbuffer );
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_renderbuffer_get_buffer(
                                         IntPtr renderbuffer );

        public IntPtr Buffer
        {
            get
            {
                return awe_renderbuffer_get_buffer( renderbuffer );
            }
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_renderbuffer_copy_to( IntPtr renderbuffer,
                                         IntPtr dest_buffer,
                                         int dest_rowspan,
                                         int dest_depth,
                                         bool convert_to_rgba );

        public void CopyTo( IntPtr destBuffer,
                           int destRowspan,
                           int destDepth,
                           bool convertToRGBA )
        {
            awe_renderbuffer_copy_to( renderbuffer, destBuffer, destRowspan, destDepth, convertToRGBA );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_renderbuffer_copy_to_float( IntPtr renderbuffer,
                                          IntPtr destination );

        public void CopyToFloat( IntPtr destination )
        {
            awe_renderbuffer_copy_to_float( renderbuffer, destination );
        }

#if !USING_MONO
        public void CopyToBitmap( WriteableBitmap destination )
        {
            int width = this.Width;
            int height = this.Height;
            Int32Rect rect = new Int32Rect( 0, 0, width, height );
            try
            {
                destination.WritePixels( rect, this.Buffer, (int)( this.Rowspan * this.Height ), this.Rowspan, 0, 0 );
            }
            catch { /* Some sort of handling for this. */ }
        }
#endif

        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_renderbuffer_save_to_png( IntPtr renderbuffer,
                                              IntPtr file_path,
                                             bool preserve_transparency );

        public bool SaveToPNG( string filePath,
                            bool preserveTransparency = false )
        {
            StringHelper filePathStr = new StringHelper( filePath );

            bool temp = awe_renderbuffer_save_to_png( renderbuffer, filePathStr.Value, preserveTransparency );

            return temp;
        }

        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_renderbuffer_save_to_jpeg( IntPtr renderbuffer,
                                               IntPtr file_path,
                                              int quality );

        public bool SaveToJPEG( string filePath,
                              int quality = 90 )
        {
            StringHelper filePathStr = new StringHelper( filePath );

            bool temp = awe_renderbuffer_save_to_jpeg( renderbuffer, filePathStr.Value, quality );

            return temp;
        }
    }
}
