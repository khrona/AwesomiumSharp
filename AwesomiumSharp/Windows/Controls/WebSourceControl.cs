/********************************************************************************
 *    Project  : AwesomiumSharp
 *    File     : WebSourceControl.cs
 *    Version  : 1.0.0.0 
 *    Date     : 08/02/2011
 *    Author   : Perikles C. Stephanidis (AmaDeuS)
 *    Contact  : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes    :
 *
 *    Represents a WPF control that displays the HTML source of any web-page 
 *    loaded using WebControl.LoadURL.
 *    
 *    Changelog: 
 *    
 *    https://github.com/khrona/AwesomiumSharp/commits/master.atom
 *    
 ********************************************************************************/

using System;

namespace AwesomiumSharp.Windows.Controls
{
    /// <summary>
    /// Represents a WPF control that displays the HTML source of any web-page loaded using <see cref="WebControl.LoadURL"/>.
    /// </summary>
    public class WebSourceControl : WebControl
    {
        internal override bool IsSourceControl
        {
            get
            {
                return true;
            }
        }
    }
}
