using System;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
#if !USING_MONO
using System.Linq;
#endif

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// JSObject represents an Object type in Javascript (similar to a Dictionary in C#).
    /// You can get and set properties (key/value pairs).
    /// </summary>
    public class JSObject : IDisposable
    {
        #region Fields
        internal IntPtr instance;
        private bool ownsInstance;
        private bool isDisposed;
        #endregion

        #region Ctor/Dtor
        public JSObject()
        {
            instance = awe_jsobject_create();
            ownsInstance = true;
        }

        internal JSObject( IntPtr srcInstance )
        {
            instance = srcInstance;
            ownsInstance = false;
        }

        ~JSObject()
        {
            Dispose();
        }

        public void Dispose()
        {
            if ( !isDisposed && ownsInstance )
            {
                if ( instance != IntPtr.Zero )
                    awe_jsobject_destroy( instance );

                isDisposed = true;
            }

            GC.SuppressFinalize( this );
        }
        #endregion


        #region Methods
        public bool HasProperty( string propertyName )
        {
            StringHelper propertyNameStr = new StringHelper( propertyName );
            return awe_jsobject_has_property( instance, propertyNameStr.value() );
        }
        #endregion

        #region Properties
        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }

        [IndexerName( "Property" )]
        public JSValue this[ string propertyName ]
        {
            get
            {
                StringHelper propertyNameStr = new StringHelper( propertyName );
                return new JSValue( awe_jsobject_get_property( instance, propertyNameStr.value() ) );
            }
            set
            {
                StringHelper propertyNameStr = new StringHelper( propertyName );
                awe_jsobject_set_property( instance, propertyNameStr.value(), value.Instance );
            }
        }

        public uint Size
        {
            get
            {
                return awe_jsobject_get_size( instance );
            }
        }

        public string[] Keys
        {
            get
            {
                IntPtr jsArray = awe_jsobject_get_keys( instance );

                uint size = JSArrayHelper.GetSize( jsArray );
                string[] result = new string[ size ];
                for ( uint i = 0; i < size; i++ )
                {
                    JSValue temp = JSArrayHelper.GetElement( jsArray, i );
                    result[ i ] = temp.ToString();
                }

                JSArrayHelper.DestroyArray( jsArray );

                return result;
            }
        }
        #endregion


        #region External
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsobject_create();

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_jsobject_destroy( IntPtr jsobject );

        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static bool awe_jsobject_has_property( IntPtr jsobject, IntPtr property_name );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static IntPtr awe_jsobject_get_property( IntPtr jsobject, IntPtr property_name );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static void awe_jsobject_set_property( IntPtr jsobject, IntPtr property_name, IntPtr value );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern uint awe_jsobject_get_size( IntPtr jsobject );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsobject_get_keys( IntPtr jsobject );
        #endregion

    }
}
