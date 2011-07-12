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
    /// <summary>
    /// Indicates the Javascript data type that a <see cref="JSValue"/> represents.
    /// </summary>
    public enum JSValueType
    {
        Null,
        Boolean,
        Integer,
        Double,
        String,
        Object,
        Array
    }

    /// <summary>
    /// Represents a Javascript value. It can be initialized from and to several types: 
    /// boolean, integer, double, string, object, and array.
    /// </summary>
    public class JSValue : IDisposable
    {
        #region Fields
        private IntPtr instance;
        internal bool ownsInstance = true;
        private bool isDisposed;
        #endregion


        #region Ctors
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsvalue_create_null_value();

        /// <summary>
        /// Creates a <see cref="JSValue"/> representing a null value.
        /// </summary>
        public JSValue()
        {
            instance = awe_jsvalue_create_null_value();
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsvalue_create_bool_value( bool value );

        /// <summary>
        /// Creates a <see cref="JSValue"/> initialized with a boolean.
        /// </summary>
        public JSValue( bool value )
        {
            instance = awe_jsvalue_create_bool_value( value );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsvalue_create_integer_value( int value );

        /// <summary>
        /// Creates a <see cref="JSValue"/> initialized with an integer.
        /// </summary>
        public JSValue( int value )
        {
            instance = awe_jsvalue_create_integer_value( value );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsvalue_create_double_value( double value );

        /// <summary>
        /// Creates a <see cref="JSValue"/> initialized with a double.
        /// </summary>
        public JSValue( double value )
        {
            instance = awe_jsvalue_create_double_value( value );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsvalue_create_string_value( IntPtr value );

        /// <summary>
        /// Creates a <see cref="JSValue"/> initialized with a string.
        /// </summary>
        public JSValue( string value )
        {
            StringHelper valueStr = new StringHelper( value );

            instance = awe_jsvalue_create_string_value( valueStr.Value );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsvalue_create_object_value( IntPtr value );

        /// <summary>
        /// Creates a <see cref="JSValue"/> initialized with a <see cref="JSObject"/>.
        /// </summary>
        public JSValue( JSObject value )
        {
            instance = awe_jsvalue_create_object_value( value.instance );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsvalue_create_array_value( IntPtr value );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static IntPtr awe_jsarray_create( IntPtr[] jsvalue_array, uint length );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_jsarray_destroy( IntPtr jsarray );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern uint awe_jsarray_get_size( IntPtr jsarray );

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private extern static IntPtr awe_jsarray_get_element( IntPtr jsarray, uint index );

        /// <summary>
        /// Creates a <see cref="JSValue"/> representing an array of <see cref="JSValue"/>.
        /// </summary>
        public JSValue( JSValue[] value )
        {
            IntPtr jsarray = JSArrayHelper.CreateArray( value );

            instance = awe_jsvalue_create_array_value( jsarray );
            JSArrayHelper.DestroyArray( jsarray );
        }

        internal JSValue( IntPtr cVal )
        {
            instance = cVal;
            ownsInstance = false;
        }
        #endregion


        #region Methods
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsvalue_to_string( IntPtr jsvalue );

        /// <summary>
        /// Returns this <see cref="JSValue"/> as a wide string (converting if necessary). 
        /// </summary>
        new public string ToString()
        {
            return StringHelper.ConvertAweString( awe_jsvalue_to_string( instance ), true );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern int awe_jsvalue_to_integer( IntPtr jsvalue );

        /// <summary>
        /// Returns this <see cref="JSValue"/> as an integer (converting if necessary). 
        /// </summary>
        public int ToInteger()
        {
            return awe_jsvalue_to_integer( instance );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern double awe_jsvalue_to_double( IntPtr jsvalue );

        /// <summary>
        /// Returns this <see cref="JSValue"/> as a double (converting if necessary). 
        /// </summary>
        public double ToDouble()
        {
            return awe_jsvalue_to_double( instance );
        }

        [return: MarshalAs( UnmanagedType.I1 )]
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern bool awe_jsvalue_to_boolean( IntPtr jsvalue );

        /// <summary>
        /// Returns this <see cref="JSValue"/> as a boolean (converting if necessary). 
        /// </summary>
        public bool ToBoolean()
        {
            return awe_jsvalue_to_boolean( instance );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsvalue_get_array( IntPtr jsvalue );

        /// <summary>
        /// Gets this value as an Array, this will assert if not an Array type.
        /// </summary>
        public JSValue[] GetArray()
        {
            return JSArrayHelper.getArray( awe_jsvalue_get_array( instance ) );
        }

        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern IntPtr awe_jsvalue_get_object( IntPtr jsvalue );

        /// <summary>
        /// Gets this value as an Object, this will assert if not an Object type.
        /// </summary>
        public JSObject GetObject()
        {
            return new JSObject( awe_jsvalue_get_object( instance ) );
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
        private static extern int awe_jsvalue_get_type( IntPtr jsvalue );

        /// <summary>
        /// Gets the data type that this <see cref="JSValue"/> represents.
        /// </summary>
        public JSValueType Type
        {
            get
            {
                return (JSValueType)awe_jsvalue_get_type( instance );
            }
        }
        #endregion

        #region IDisposable
        [DllImport( WebCore.DLLName, CallingConvention = CallingConvention.Cdecl )]
        private static extern void awe_jsvalue_destroy( IntPtr jsvalue );

        ~JSValue()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes and destroys this <see cref="JSValue"/>.
        /// </summary>
        public void Dispose()
        {
            if ( !isDisposed && ownsInstance )
            {
                if ( instance != IntPtr.Zero )
                    awe_jsvalue_destroy( instance );

                isDisposed = true;
            }

            GC.SuppressFinalize( this );
        }
        #endregion
    }
}
