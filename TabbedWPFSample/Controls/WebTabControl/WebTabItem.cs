/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    WebTabItem.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 ***************************************************************************/

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
