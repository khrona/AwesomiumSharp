/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    WebTabControlMenu.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 *__________________________________________________________________________
 *
 *  Notes:
 *
 *  Subclassed ItemsControl needed to handle TabViews as data providers.
 *  We can then visualize data as Menu items. This is used in the
 *  openned tabs menu.
 *   
 ***************************************************************************/

using System;
using System.Windows.Controls;

namespace TabbedWPFSample
{
    class WebTabControlMenu : ItemsControl
    {
        protected override bool IsItemItsOwnContainerOverride( object item )
        {
            // This is needed to avoid the:
            // System.Windows.Data Error: 26 : ItemTemplate and ItemTemplateSelector are ignored for items already of the ItemsControl's container type;
            // This error occurs because the items we try to add to the ItemsControl, are already UIElements, therefore, they do not
            // need a DataTemplate to be visualized. In such cases, the ItemsControl ignores the DataTemplate defined in ItemTemplate.
            // In our case, we feed this ItemsControl with TabViews. We do not want these to be removed from the visual tree and added in the popup
            // (which is what would happen without this override)! We want our ItemTemplate to be respected.
            return false;
        }
    }
}
