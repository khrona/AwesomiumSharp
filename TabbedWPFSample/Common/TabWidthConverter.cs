using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace TabbedWPFSample
{
    class TabWidthConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, CultureInfo culture )
        {
            if ( ( value != null ) && ( value is WebTabItem ) )
            {
                WebTabItem item = (WebTabItem)value;
                WebTabControl parent = item.FindAncestor<WebTabControl>();

                if ( parent != null )
                {

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
