using System;
using System.Collections.Generic;
using System.Text;
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
    public class JSValue : IDisposable
    {
        private IntPtr instance;
        internal bool ownsInstance = true;
        private bool disposed = false;

        public enum Type
        {
            Null,
            Boolean,
            Integer,
            Double,
            String,
            Object,
            Array
        };

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsvalue_create_null_value();

        public JSValue()
        {
            instance = awe_jsvalue_create_null_value();
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsvalue_create_bool_value(bool value);

        public JSValue(bool value)
        {
            instance = awe_jsvalue_create_bool_value(value);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsvalue_create_integer_value(int value);

        public JSValue(int value)
        {
            instance = awe_jsvalue_create_integer_value(value);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsvalue_create_double_value(double value);

        public JSValue(double value)
        {
            instance = awe_jsvalue_create_double_value(value);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsvalue_create_string_value(IntPtr value);

        public JSValue(string value)
        {
            StringHelper valueStr = new StringHelper(value);

            instance = awe_jsvalue_create_string_value(valueStr.value());
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsvalue_create_object_value(IntPtr value);

        public JSValue(JSObject value)
        {
            instance = awe_jsvalue_create_object_value(value.instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsvalue_create_array_value(IntPtr value);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsarray_create(IntPtr[] jsvalue_array,
                                           uint length);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_jsarray_destroy(IntPtr jsarray);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint awe_jsarray_get_size(IntPtr jsarray);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsarray_get_element(IntPtr jsarray,
                                                       uint index);

        public JSValue(JSValue[] value)
        {
            IntPtr jsarray = JSArrayHelper.createArray(value);

            instance = awe_jsvalue_create_array_value(jsarray);

            JSArrayHelper.destroyArray(jsarray);
        }

        internal JSValue(IntPtr cVal)
        {
            instance = cVal;
            ownsInstance = false;
        }

        internal IntPtr getInstance()
        {
            return instance;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_jsvalue_destroy(IntPtr jsvalue);

        ~JSValue()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed && ownsInstance)
            {
                if(instance != IntPtr.Zero)
                    awe_jsvalue_destroy(instance);

                disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int awe_jsvalue_get_type(IntPtr jsvalue);

        new public Type GetType()
        {
            return (Type)awe_jsvalue_get_type(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsvalue_to_string(IntPtr jsvalue);

        new public string ToString()
        {
            return StringHelper.ConvertAweString(awe_jsvalue_to_string(instance));
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern int awe_jsvalue_to_integer(IntPtr jsvalue);

        public int ToInteger()
        {
            return awe_jsvalue_to_integer(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern double awe_jsvalue_to_double(IntPtr jsvalue);

        public double ToDouble()
        {
            return awe_jsvalue_to_double(instance);
        }

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_jsvalue_to_boolean(IntPtr jsvalue);

        public bool ToBoolean()
        {
            return awe_jsvalue_to_boolean(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsvalue_get_array(IntPtr jsvalue);

        public JSValue[] GetArray()
        {
            return JSArrayHelper.getArray(awe_jsvalue_get_array(instance));
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsvalue_get_object(IntPtr jsvalue);

        public JSObject GetObject()
        {
            return new JSObject(awe_jsvalue_get_object(instance));
        }
    }

    internal class JSArrayHelper
    {
        // Returns a wrapper JSValue array from a const jsarray* instance. Ownership remains
        // with the original instance.
        internal static JSValue[] getArray(IntPtr instance)
        {
            uint size = awe_jsarray_get_size(instance);
            JSValue[] temp = new JSValue[size];
            for (uint i = 0; i < size; i++)
            {
                temp[i] = new JSValue(awe_jsarray_get_element(instance, i));
            }

            return temp;
        }

        internal static JSValue getElement(IntPtr instance, uint idx)
        {
            return new JSValue(awe_jsarray_get_element(instance, idx));
        }

        internal static uint getSize(IntPtr instance)
        {
            return awe_jsarray_get_size(instance);
        }

        internal static IntPtr createArray(JSValue[] vals)
        {
            IntPtr[] temp = new IntPtr[vals.Length];
            int count = 0;
            foreach (JSValue i in vals)
            {
                temp[count] = i.getInstance();
                count++;
            }

            return awe_jsarray_create(temp, (uint)vals.Length);
        }

        internal static void destroyArray(IntPtr instance)
        {
            if (instance != IntPtr.Zero)
                awe_jsarray_destroy(instance);
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsarray_create(IntPtr[] jsvalue_array,
                                           uint length);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_jsarray_destroy(IntPtr jsarray);


        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint awe_jsarray_get_size(IntPtr jsarray);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsarray_get_element(IntPtr jsarray,
                                                       uint index);
    }

    public class JSObject
    {
        internal IntPtr instance;
        private bool ownsInstance;
        private bool disposed = false;

        public JSObject()
        {
            instance = awe_jsobject_create();
            ownsInstance = true;
        }

        internal JSObject(IntPtr srcInstance)
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
            if (!disposed && ownsInstance)
            {
                if(instance != IntPtr.Zero)
                    awe_jsobject_destroy(instance);

                disposed = true;
            }

            GC.SuppressFinalize(this);
        }

        public bool HasProperty(string propertyName)
        {
            StringHelper propertyNameStr = new StringHelper(propertyName);

            return awe_jsobject_has_property(instance, propertyNameStr.value());
        }

        public JSValue GetProperty(string propertyName)
        {
            StringHelper propertyNameStr = new StringHelper(propertyName);

            return new JSValue(awe_jsobject_get_property(instance, propertyNameStr.value()));
        }

        public void SetPropery(string propertyName,
                               JSValue value)
        {
            StringHelper propertyNameStr = new StringHelper(propertyName);

            awe_jsobject_set_property(instance, propertyNameStr.value(), value.getInstance());
        }

        public uint GetSize()
        {
            return awe_jsobject_get_size(instance);
        }

        public string[] GetKeys()
        {
            IntPtr jsArray = awe_jsobject_get_keys(instance);

            uint size = JSArrayHelper.getSize(jsArray);
            string[] result = new string[size];
            for (uint i = 0; i < size; i++)
            {
                JSValue temp = JSArrayHelper.getElement(jsArray, i);
                result[i] = temp.ToString();
            }

            JSArrayHelper.destroyArray(jsArray);

            return result;
        }

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsobject_create();

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_jsobject_destroy(IntPtr jsobject);

        [return: MarshalAs(UnmanagedType.I1)]
        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern bool awe_jsobject_has_property(IntPtr jsobject,
                                           IntPtr property_name);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsobject_get_property(IntPtr jsobject,
                                                               IntPtr property_name);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void awe_jsobject_set_property(IntPtr jsobject,
                                           IntPtr property_name,
                                           IntPtr value);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern uint awe_jsobject_get_size(IntPtr jsobject);

        [DllImport(WebCore.DLLName, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr awe_jsobject_get_keys(IntPtr jsobject);


    }
}
