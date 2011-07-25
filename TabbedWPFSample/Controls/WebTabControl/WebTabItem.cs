using System;
using System.Windows;
using System.Windows.Controls;

namespace TabbedWPFSample
{
    class WebTabItem : TabItem
    {
        static WebTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( WebTabItem ), new FrameworkPropertyMetadata( typeof( WebTabItem ) ) );
        }
    }
}
