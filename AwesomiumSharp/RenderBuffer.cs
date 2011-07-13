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
    /// Represents a 32-bit BGRA pixel buffer. You can save it
    /// directly to an image or copy it to some other graphics surface for 
    /// display in your application. An instance of this class is returned by
    /// <see cref="WebView.Render"/> and <see cref="Windows.Controls.WebControl.Render"/>.
    /// </summary>
    public class RenderBuffer
    {
        #region Fields
        private IntPtr renderbuffer;
        #endregion

        #region Ctor
        internal RenderBuffer(IntPtr renderbuffer)
        {
            this.renderbuffer = renderbuffer;
        }
        #endregion


        #region Method

        #region CopyTo
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_renderbuffer_copy_to(IntPtr renderbuffer,
                                         IntPtr dest_buffer,
                                         int dest_rowspan,
                                         int dest_depth,
                                         bool convert_to_rgba);

        /// <summary>
        /// Copy this buffer to a specified destination buffer.
        /// </summary>
        /// <param name="destBuffer">The destination buffer (should have same dimensions).</param>
        /// <param name="destRowspan"></param>
        /// <param name="destDepth">The depth (either 3 BPP or 4 BPP).</param>
        /// <param name="convertToRGBA">True to convert to RGBA format. False otherwise.</param>
        public void CopyTo(IntPtr destBuffer,
                           int destRowspan,
                           int destDepth,
                           bool convertToRGBA)
        {
            awe_renderbuffer_copy_to(renderbuffer, destBuffer, destRowspan, destDepth, convertToRGBA);
        }
        #endregion

        #region CopyToFloat
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_renderbuffer_copy_to_float(IntPtr renderbuffer,
                                          IntPtr destination);
        /// <summary>
        /// Copy this buffer to a pixel buffer with a floating-point pixel format for use with game engines like Unity3D.
        /// </summary>
        /// <param name="destination"></param>
        public void CopyToFloat(IntPtr destination)
        {
            awe_renderbuffer_copy_to_float(renderbuffer, destination);
        }
        #endregion

#if !USING_MONO
        #region CopyToBitmap
        /// <summary>
        /// Copy this buffer to a <see cref="WriteableBitmap"/> that can be rendered in WPF.
        /// </summary>
        /// <param name="destination">
        /// The <see cref="WriteableBitmap"/> to write to. Must have the same dimensions.
        /// </param>
        /// <remarks>
        /// @warning
        /// Once again: The <paramref name="destination"/> <see cref="WriteableBitmap"/>
        /// must have the same dimensions.
        /// </remarks>
        /// <exception cref="AccessViolationException">
        /// Attempted to write to a <see cref="WriteableBitmap"/> with different dimensions 
        /// than this buffer.
        /// </exception>
        public void CopyToBitmap(WriteableBitmap destination)
        {
            int width = this.Width;
            int height = this.Height;
            Int32Rect rect = new Int32Rect(0, 0, width, height);
            try
            {
                destination.WritePixels(rect, this.Buffer, (int)(this.Rowspan * this.Height), this.Rowspan, 0, 0);
            }
            catch { /* Some sort of handling for this. */ }
        }
        #endregion
#endif

        #region SaveToPNG
        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool awe_renderbuffer_save_to_png(IntPtr renderbuffer, IntPtr file_path, bool preserve_transparency);
        /// <summary>
        /// Save this buffer to a PNG image.
        /// </summary>
        /// <param name="filePath">
        /// The path to the file that will be written.
        /// </param>
        /// <param name="preserveTransparency">
        /// True to preserve transparency (alpha) values. False otherwise.
        /// </param>
        /// <returns>
        /// True if the image was successfully saved. False otherwise.
        /// </returns>
        public bool SaveToPNG(string filePath, bool preserveTransparency = false)
        {
            StringHelper filePathStr = new StringHelper(filePath);
            bool temp = awe_renderbuffer_save_to_png(renderbuffer, filePathStr.Value, preserveTransparency);
            return temp;
        }
        #endregion

        #region SaveToJPEG
        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private extern static bool awe_renderbuffer_save_to_jpeg(IntPtr renderbuffer, IntPtr file_path, int quality);

        /// <summary>
        /// Save this buffer to a JPEG image.
        /// </summary>
        /// <param name="filePath">
        /// The path to the file that will be written.
        /// </param>
        /// <param name="quality">
        /// The compression quality to use, the valid range is 0 to 100, with 100 being the highest.
        /// </param>
        /// <returns>
        /// True if the image was successfully saved. False otherwise.
        /// </returns>
        public bool SaveToJPEG(string filePath, int quality = 90)
        {
            StringHelper filePathStr = new StringHelper(filePath);
            bool temp = awe_renderbuffer_save_to_jpeg(renderbuffer, filePathStr.Value, quality);
            return temp;
        }
        #endregion

        #endregion

        #region Properties

        #region Width
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int awe_renderbuffer_get_width(IntPtr renderbuffer);

        /// <summary>
        /// The width, in pixels.
        /// </summary>
        public int Width
        {
            get
            {
                return awe_renderbuffer_get_width(renderbuffer);
            }
        }
        #endregion

        #region Height
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int awe_renderbuffer_get_height(IntPtr renderbuffer);

        /// <summary>
        /// The height, in pixels.
        /// </summary>
        public int Height
        {
            get
            {
                return awe_renderbuffer_get_height(renderbuffer);
            }
        }
        #endregion

        #region Rowspan
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int awe_renderbuffer_get_rowspan(IntPtr renderbuffer);

        /// <summary>
        /// The number of bytes per row (this is usually width * 4 but can be different).
        /// </summary>
        public int Rowspan
        {
            get
            {
                return awe_renderbuffer_get_rowspan(renderbuffer);
            }
        }
        #endregion

        #region Buffer
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_renderbuffer_get_buffer(
                                         IntPtr renderbuffer);

        /// <summary>
        /// The raw block of pixel data, BGRA format. 
        /// </summary>
        /// <remarks>If WebView.SetTransparent is not enabled, you may need to flush the alpha
        /// channel of this buffer (eg, set every 4th bit to 255) before displaying it. This is
        /// because Flash on Windows will sometimes corrupt our alpha channel and so you may 
        /// see weird text in Flash if you forget to do this.</remarks>
        public IntPtr Buffer
        {
            get
            {
                return awe_renderbuffer_get_buffer(renderbuffer);
            }
        }
        #endregion

        #endregion

    }
}
