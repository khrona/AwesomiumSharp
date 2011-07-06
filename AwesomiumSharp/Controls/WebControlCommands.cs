/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : WebControlCommands.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/03/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    This static class exposes WebControl specific routed commands that
 *    can be used directly from XAML. UI Text is taken from Resources.
 *    You can add localized resource files named accordingly 
 *    (e.g. Resources.el-GR.resx) to achieve localization.
 *    
 * 
 ********************************************************************************/

using System;
using System.Windows.Input;

namespace AwesomiumSharp.Windows.Controls
{
    public static class  WebControlCommands
    {
        public static RoutedUICommand LoadURL { get; private set; }
        public static RoutedUICommand LoadFile { get; private set; }
        public static RoutedUICommand ActivateIME { get; private set; }
        public static RoutedUICommand AddURLFilter { get; private set; }
        public static RoutedUICommand CancelIMEComposition { get; private set; }
        public static RoutedUICommand ChooseFile { get; private set; }
        public static RoutedUICommand ClearAllURLFilters { get; private set; }
        public static RoutedUICommand ConfirmIMEComposition { get; private set; }
        public static RoutedUICommand CreateObject { get; private set; }
        public static RoutedUICommand DestroyObject { get; private set; }
        public static RoutedUICommand ResetZoom { get; private set; }
        public static RoutedUICommand StopFind { get; private set; }

        static WebControlCommands()
        {
            LoadURL = new RoutedUICommand( Resources.LoadURL, "LoadURL", typeof( WebControlCommands ) );
            LoadFile = new RoutedUICommand( Resources.LoadFile, "LoadFile", typeof( WebControlCommands ) );
            ActivateIME = new RoutedUICommand( Resources.ActivateIME, "ActivateIME", typeof( WebControlCommands ) );
            AddURLFilter = new RoutedUICommand( Resources.AddURLFilter, "AddURLFilter", typeof( WebControlCommands ) );
            CancelIMEComposition = new RoutedUICommand( Resources.CancelIMEComposition, "CancelIMEComposition", typeof( WebControlCommands ) );
            ChooseFile = new RoutedUICommand( Resources.ChooseFile, "ChooseFile", typeof( WebControlCommands ) );
            ClearAllURLFilters = new RoutedUICommand( Resources.ClearAllURLFilters, "ClearAllURLFilters", typeof( WebControlCommands ) );
            ConfirmIMEComposition = new RoutedUICommand( Resources.ConfirmIMEComposition, "ConfirmIMEComposition", typeof( WebControlCommands ) );
            CreateObject = new RoutedUICommand( Resources.CreateObject, "CreateObject", typeof( WebControlCommands ) );
            DestroyObject = new RoutedUICommand( Resources.DestroyObject, "DestroyObject", typeof( WebControlCommands ) );
            ResetZoom = new RoutedUICommand( Resources.ResetZoom, "ResetZoom", typeof( WebControlCommands ) );
            StopFind = new RoutedUICommand( Resources.StopFind, "StopFind", typeof( WebControlCommands ) );
        }

    }
}
