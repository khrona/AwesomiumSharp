using System;

namespace AwesomiumSharp.Windows.Controls
{
    /// <summary>
    /// Represents a WPF control that displays the HTML source of any web-page loaded using <see cref="WebSourceControl.LoadURL"/>.
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
