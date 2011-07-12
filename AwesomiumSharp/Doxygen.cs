/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : Doxygen.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/08/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    This file is not compiled with the project. It includes important
 *    additional documentation for inherited members. It is used by
 *    Doxygen (http://www.doxygen.org) for documentation purposes.
 *    Ignore it!
 *    
 * 
 ********************************************************************************/

using System;

namespace AwesomiumSharp.Windows.Controls
{
    public class WebControl
    {
        /// <summary>
        /// Gets or sets if the view is valid and enabled.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A <see cref="WebControl"/> is considered invalid when it has been destroyed 
        /// (by either calling <see cref="WebControl.Close"/> or <see cref="WebCore.Shutdown"/>)
        /// or was never properly instantiated.
        /// </para>
        /// <para>
        /// @note
        /// Manually setting this property to true, will temporarily render the control disabled.
        /// @note
        /// Inheritors should rely on the <see cref="IsLive"/> property. Accessing <see cref="IsLive"/>
        /// also updates the value of <see cref="IsEnabled"/>.
        /// @note
        /// There is no way to revive a <see cref="WebControl"/> whose underlying view has been destroyed.
        /// When you are done with reporting any errors to the user, close it and release any references to it 
        /// to avoid memory leaks.
        /// @warning
        /// While disabled (either because the view is destroyed or because you manually set this property)
        /// attempting to access members of this control, may cause a <see cref="InvalidOperationException"/>
        /// (see the documentation of each member).
        /// </para>
        /// </remarks>
        /// <seealso cref="IsLive"/>
        public bool IsEnabled { get; set; }
    }
}
