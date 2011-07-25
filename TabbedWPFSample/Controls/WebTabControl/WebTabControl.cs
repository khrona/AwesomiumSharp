using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Specialized;

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

                if ( totalWidth > ( constraint.Width - 140 ) )
                    finalWidth = ( constraint.Width - 140 ) / this.Items.Count;

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
        #endregion
    }
}
