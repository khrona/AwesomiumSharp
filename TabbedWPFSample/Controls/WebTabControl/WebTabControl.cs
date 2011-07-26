/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    WebTabControl.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 *__________________________________________________________________________
 *
 *  Notes:
 *
 *  Re-styled TabControl that handles the size of its tabs.
 *   
 ***************************************************************************/

#region Using
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;
#endregion

namespace TabbedWPFSample
{
    class WebTabControl : TabControl
    {

        #region Ctor
        static WebTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( WebTabControl ), new FrameworkPropertyMetadata( typeof( WebTabControl ) ) );
        }
        #endregion

        #region Overrides
        protected override void OnItemsChanged( NotifyCollectionChangedEventArgs e )
        {
            base.OnItemsChanged( e );

            if ( ( e.NewItems != null ) && ( e.NewItems.Count > 0 ) )
            {
                TabView view = (TabView)e.NewItems[ 0 ];
                this.SelectedItem = view;

                if ( !view.IsLoaded )
                {
                    RoutedEventHandler loaded = null;
                    loaded = ( sender, ea ) =>
                    {
                        view.Loaded -= loaded;
                        this.InvalidateMeasure();
                    };

                    view.Loaded += loaded;
                }
            }

            this.InvalidateMeasure();
        }

        protected override Size MeasureOverride( Size constraint )
        {
            Size size = base.MeasureOverride( constraint );

            if ( constraint.Width > 0 )
            {
                double totalWidth = this.Items.Count * 180;
                double finalWidth = 180;

                if ( totalWidth > ( constraint.Width - 170 ) )
                    finalWidth = ( constraint.Width - 170 ) / this.Items.Count;

                foreach ( TabView item in this.Items )
                {
                    WebTabItem container = (WebTabItem)this.ItemContainerGenerator.ContainerFromItem( item );

                    if ( ( container != null ) && ( container.ActualWidth > 0 ) )
                        container.Width = finalWidth;
                }
            }

            return size;
        }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new WebTabItem();
        }

        protected override bool IsItemItsOwnContainerOverride( object item )
        {
            return item is WebTabItem;
        }

        protected override void OnSelectionChanged( SelectionChangedEventArgs e )
        {
            // Prevent the TabControl from staying with no selected item.
            // This can occur is we try to un-select the selected tab from code,
            // or by clicking it in the tabs menu.
            if ( this.Items.Count > 0 && e.AddedItems.Count == 0 )
                ( (TabView)this.Items[ 0 ] ).IsSelected = true;
            else
                base.OnSelectionChanged( e );
        }
        #endregion
    }
}
