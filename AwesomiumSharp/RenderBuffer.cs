using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AwesomiumSharp
{
    public class RenderBuffer
    {
        private IntPtr renderbuffer;

        internal RenderBuffer(IntPtr renderbuffer)
        {
            this.renderbuffer = renderbuffer;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int awe_renderbuffer_get_width(IntPtr renderbuffer);

        public int GetWidth()
        {
            return awe_renderbuffer_get_width(renderbuffer);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int awe_renderbuffer_get_height(IntPtr renderbuffer);

        public int GetHeight()
        {
            return awe_renderbuffer_get_height(renderbuffer);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int awe_renderbuffer_get_rowspan(IntPtr renderbuffer);

        public int GetRowspan()
        {
            return awe_renderbuffer_get_rowspan(renderbuffer);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_renderbuffer_get_buffer(
                                         IntPtr renderbuffer);

        public IntPtr GetBuffer()
        {
            return awe_renderbuffer_get_buffer(renderbuffer);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_renderbuffer_copy_to(IntPtr renderbuffer,
                                         IntPtr dest_buffer,
                                         int dest_rowspan,
                                         int dest_depth,
                                         bool convert_to_rgba);

        public void CopyTo(IntPtr destBuffer,
                           int destRowspan,
                           int destDepth,
                           bool convertToRGBA)
        {
            awe_renderbuffer_copy_to(renderbuffer, destBuffer, destRowspan, destDepth, convertToRGBA);
        }

        public void CopyToBitmap(WriteableBitmap destination)
        {
            int width = GetWidth();
            int height = GetHeight();
            Int32Rect rect = new Int32Rect(0, 0, width, height);
            destination.WritePixels(rect, this.GetBuffer(), (int)(this.GetRowspan() * this.GetHeight()), this.GetRowspan(), 0, 0);
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_renderbuffer_save_to_png(IntPtr renderbuffer,
                                              IntPtr file_path,
                                             bool preserve_transparency);

        public bool SaveToPNG(string filePath,
                            bool preserveTransparency = false)
        {
            StringHelper filePathStr = new StringHelper(filePath);

            bool temp = awe_renderbuffer_save_to_png(renderbuffer, filePathStr.value(), preserveTransparency);

            return temp;
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_renderbuffer_save_to_jpeg(IntPtr renderbuffer,
                                               IntPtr file_path,
                                              int quality);

        public bool SaveToJPEG(string filePath,
                              int quality = 90)
        {
            StringHelper filePathStr = new StringHelper(filePath);

            bool temp = awe_renderbuffer_save_to_jpeg(renderbuffer, filePathStr.value(), quality);

            return temp;
        }
    }
}
