using System;
using System.Windows;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Controls;

namespace TabbedWPFSample
{
    class ZIndexConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( ( value != null ) && ( value is UIElement ) )
            {
                UIElement element = (UIElement)value;
                ItemsControl parent = element.FindAncestor<ItemsControl>();

                if ( parent != null )
                {
                    return -1 * parent.Items.IndexOf( element );
                }
            }

            return Binding.DoNothing;
        }

        public object ConvertBack( object value, Type targetType, object parameter, CultureInfo culture )
        {
            return Binding.DoNothing;
        }
    }
}
