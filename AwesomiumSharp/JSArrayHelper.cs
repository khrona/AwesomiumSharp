using System;
using System.Runtime.InteropServices;
#if !USING_MONO
using System.Linq;
#endif

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    internal class JSArrayHelper
    {
        // Returns a wrapper JSValue array from a const jsarray* instance. Ownership remains
        // with the original instance.
        internal static JSValue[] getArray( IntPtr instance )
        {
            uint size = awe_jsarray_get_size( instance );
            JSValue[] temp = new JSValue[ size ];
            for ( uint i = 0; i < size; i++ )
            {
                temp[ i ] = new JSValue( awe_jsarray_get_element( instance, i ) );
            }

            return temp;
        }

        internal static JSValue GetElement( IntPtr instance, uint idx )
        {
            return new JSValue( awe_jsarray_get_element( instance, idx ) );
        }

        internal static uint GetSize( IntPtr instance )
        {
            return awe_jsarray_get_size( instance );
        }

        internal static IntPtr CreateArray( JSValue[] vals )
        {
            IntPtr[] temp = new IntPtr[ vals.Length ];
            int count = 0;
            foreach ( JSValue i in vals )
            {
                temp[ count ] = i.Instance;
                count++;
            }

            return awe_jsarray_create( temp, (uint)vals.Length );
        }

        internal static void DestroyArray( IntPtr instance )
        {
            if ( instance != IntPtr.Zero )
                awe_jsarray_destroy( instance );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsarray_create( IntPtr[] jsvalue_array,
                                           uint length );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_jsarray_destroy( IntPtr jsarray );


        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern uint awe_jsarray_get_size( IntPtr jsarray );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsarray_get_element( IntPtr jsarray,
                                                       uint index );
    }
}
