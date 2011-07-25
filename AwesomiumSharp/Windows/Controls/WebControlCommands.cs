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
    /// <summary>
    /// Defines routed commands that are common to a <see cref="WebControl"/>.
    /// </summary>
    public static class  WebControlCommands
    {
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.LoadURL"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        /// <remarks>
        /// When binding to this command, specify the URL as a command parameter.
        /// The URL can be a <see cref="Uri"/> instance or a string representing the source URL.
        /// </remarks>
        public static RoutedUICommand LoadURL { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.LoadFile"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        /// <remarks>
        /// When binding to this command, specify the file name as a command parameter.
        /// The file name can be represented by a <see cref="Uri"/> instance or a string.
        /// </remarks>
        public static RoutedUICommand LoadFile { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.ActivateIME"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        /// <remarks>
        /// When binding to this command, specify a boolean value as a command parameter,
        /// that indicates whether to activate or deactivate IME.
        /// </remarks>
        public static RoutedUICommand ActivateIME { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.AddURLFilter"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        /// <remarks>
        /// When binding to this command, specify the URL filter (as a string), as a command parameter.
        /// </remarks>
        public static RoutedUICommand AddURLFilter { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.CancelIMEComposition"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        public static RoutedUICommand CancelIMEComposition { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.ChooseFile"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        /// <remarks>
        /// When binding to this command, specify the chosen file name(s) as a command parameter.
        /// The specified value can be an array of <see cref="String"/> when you have selected multiple files,
        /// or a single string for a single file. The value of <see cref="SelectLocalFilesEventArgs.SelectMultipleFiles"/>
        /// provided by the <see cref="WebControl.SelectLocalFiles"/> event, indicates if you should
        /// specify one or multiple files. You can always specify an array of <see cref="String"/>
        /// containing a single string, for all scenarios.
        /// </remarks>
        public static RoutedUICommand ChooseFile { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.ClearAllURLFilters"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        public static RoutedUICommand ClearAllURLFilters { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.ConfirmIMEComposition"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        /// <remarks>
        /// When binding to this command, specify the string to examine, as a command parameter.
        /// </remarks>
        public static RoutedUICommand ConfirmIMEComposition { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.CreateObject"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        /// <remarks>
        /// When binding to this command, specify the name of the Javascript object to create, as a command parameter.
        /// </remarks>
        public static RoutedUICommand CreateObject { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.DestroyObject"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        /// <remarks>
        /// When binding to this command, specify the name of the Javascript object to destroy, as a command parameter.
        /// </remarks>
        public static RoutedUICommand DestroyObject { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.ResetZoom"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        public static RoutedUICommand ResetZoom { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.StopFind"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        public static RoutedUICommand StopFind { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.CopyHTML"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        public static RoutedUICommand CopyHTML { get; private set; }
        /// <summary>
        /// Gets a command that invokes <see cref="WebControl.CopyLinkAddress"/> when targeting a <see cref="WebControl"/>.
        /// </summary>
        public static RoutedUICommand CopyLinkAddress { get; private set; }

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
            CopyHTML = new RoutedUICommand( Resources.CopyHTML, "CopyHTML", typeof( WebControlCommands ) );
            CopyLinkAddress = new RoutedUICommand( Resources.CopyLinkAddress, "CopyLinkAddress", typeof( WebControlCommands ) );
        }

    }
}
