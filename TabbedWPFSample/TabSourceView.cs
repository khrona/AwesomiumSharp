/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    TabSourceView.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 *__________________________________________________________________________
 *
 *  Notes:
 *
 *  Represents the contents of a tab in an application window. This control
 *  contains a WebSourceControl that displays the source code of a Url.
 *   
 ***************************************************************************/

using System;
using System.Windows;

namespace TabbedWPFSample
{
    class TabSourceView : TabView
    {
        static TabSourceView()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( TabSourceView ), new FrameworkPropertyMetadata( typeof( TabSourceView ) ) );
        }

        internal TabSourceView( MainWindow parent, String url )
            : base( parent, url )
        {
            this.IsSourceView = true;
        }
    }
}
