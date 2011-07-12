using System;
using System.Text;
using System.Runtime.InteropServices;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    internal class StringHelper
    {
        #region Fields
        private IntPtr aweString;
        #endregion

        #region API
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_string_empty();

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_string_create_from_utf16( byte[] str, uint len );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_string_destroy( IntPtr str );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern uint awe_string_get_length( IntPtr str );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_string_get_utf16( IntPtr str );
        #endregion


        #region Ctor / Dtor
        public StringHelper( string val )
        {
            byte[] utf16string = Encoding.Unicode.GetBytes( val );
            aweString = awe_string_create_from_utf16( utf16string, (uint)val.Length );
        }

        ~StringHelper()
        {
            awe_string_destroy( aweString );
        }
        #endregion


        #region Static Methods
        public static void DestroyAweString( IntPtr aweStr )
        {
            awe_string_destroy( aweStr );
        }

        public static IntPtr GetAweString( string val )
        {
            byte[] utf16string = Encoding.Unicode.GetBytes( val );
            return awe_string_create_from_utf16( utf16string, (uint)val.Length );
        }

        public static string ConvertAweString( IntPtr aweStr, bool shouldDestroy = false )
        {
            byte[] stringBytes = new byte[ awe_string_get_length( aweStr ) * 2 ];
            Marshal.Copy( awe_string_get_utf16( aweStr ), stringBytes, 0, (int)awe_string_get_length( aweStr ) * 2 );

            if ( shouldDestroy )
                awe_string_destroy( aweStr );

            UnicodeEncoding unicodeEncoding = new UnicodeEncoding();

            return unicodeEncoding.GetString( stringBytes );
        }
        #endregion

        #region Properties
        internal IntPtr Value
        {
            get
            {
                return aweString;
            }
        }
        #endregion
    }
}
