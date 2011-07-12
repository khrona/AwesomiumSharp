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
    /// Represents an Object type in Javascript (similar to a Dictionary in C#).
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
        /// <summary>
        /// Creates an instance of <see cref="JSObject"/>.
        /// </summary>
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

        /// <summary>
        /// Disposes and destroys this object.
        /// </summary>
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
        /// <summary>
        /// Gets if this object has a certain named property.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to search for.
        /// </param>
        /// <returns>
        /// True if this object has the specified named property. 
        /// False otherwise.
        /// </returns>
        public bool HasProperty( string propertyName )
        {
            StringHelper propertyNameStr = new StringHelper( propertyName );
            return awe_jsobject_has_property( instance, propertyNameStr.Value );
        }
        #endregion

        #region Properties
        /// <summary>
        /// Indicates if this object is already disposed and destroyed.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
        }

        /// <summary>
        /// Gets or sets the value of the specified named property.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property whose value will be set or retrieved.
        /// </param>
        /// <returns>
        /// A <see cref="JSValue"/> representing the value of the specified named property.
        /// </returns>
        [IndexerName( "Property" )]
        public JSValue this[ string propertyName ]
        {
            get
            {
                StringHelper propertyNameStr = new StringHelper( propertyName );
                return new JSValue( awe_jsobject_get_property( instance, propertyNameStr.Value ) );
            }
            set
            {
                StringHelper propertyNameStr = new StringHelper( propertyName );
                awe_jsobject_set_property( instance, propertyNameStr.Value, value.Instance );
            }
        }

        public uint Size
        {
            get
            {
                return awe_jsobject_get_size( instance );
            }
        }

        /// <summary>
        /// Gets an array of keys representing the available named properties of this <see cref="JSObject"/>.
        /// </summary>
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
