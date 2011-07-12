/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : WebControlInvalidLayer.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/08/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    This represents a layer added to a WebControl to display elementary
 *    error or design time messages.
 *    
 * 
 ********************************************************************************/

using System;
using System.Windows;

namespace AwesomiumSharp.Windows.Controls
{
    internal class WebControlInvalidLayer : WebControlLayer
    {
        static WebControlInvalidLayer()
        {
            DefaultStyleKeyProperty.OverrideMetadata( typeof( WebControlInvalidLayer ), new FrameworkPropertyMetadata( typeof( WebControlInvalidLayer ) ) );
        }

        internal WebControlInvalidLayer( WebControl parent )
            : base( parent )
        {
            this.DataContext = parent;
        }
    }
}
