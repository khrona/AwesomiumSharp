/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : WebControlLayer.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/03/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    To be used later on. Currently not compiled with the library.
 *    
 * 
 ********************************************************************************/

using System;
using System.Windows.Media;
using System.Windows;

namespace AwesomiumSharp.Windows.Controls
{
    class WebControlLayer : FrameworkElement
    {


        protected override GeometryHitTestResult HitTestCore( GeometryHitTestParameters hitTestParameters )
        {
            return null;
        }

        protected override HitTestResult HitTestCore( PointHitTestParameters hitTestParameters )
        {
            return null;
        }
    }
}
