/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    DownloadsControl.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 ***************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;

namespace TabbedWPFSample
{
    internal class DownloadsControl : Control
    {
        static DownloadsControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( DownloadsControl ), new FrameworkPropertyMetadata( typeof( DownloadsControl ) ) );
        }

        public IEnumerable<Download> Source
        {
            get { return (IEnumerable<Download>)this.GetValue( SourceProperty ); }
            set { SetValue( SourceProperty, value ); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register( "Source",
            typeof( IEnumerable<Download> ), typeof( DownloadsControl ),
            new FrameworkPropertyMetadata( null, SourceChanged ) );

        private static void SourceChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            DownloadsControl owner = (DownloadsControl)d;
            IEnumerable<Download> oldValue = (IEnumerable<Download>)e.OldValue;
            IEnumerable<Download> value = (IEnumerable<Download>)e.NewValue;

            if ( oldValue != null )
                foreach ( Download de in oldValue )
                    de.Dispose();
        }
    }
}
