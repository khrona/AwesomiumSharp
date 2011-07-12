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
 *    Base class of layers added to the WebControl.
 *    
 * 
 ********************************************************************************/

#region Using
using System;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
#endregion

namespace AwesomiumSharp.Windows.Controls
{
    internal abstract class WebControlLayer : Control
    {
        #region Fields
        private WebControl parentControl;
        #endregion

        #region Ctor
        public WebControlLayer( WebControl parent )
        {
            parentControl = parent;
        }
        #endregion

        #region Methods
        protected override GeometryHitTestResult HitTestCore( GeometryHitTestParameters hitTestParameters )
        {
            return IsHitTestVisible ? base.HitTestCore( hitTestParameters ) : null;
        }

        protected override HitTestResult HitTestCore( PointHitTestParameters hitTestParameters )
        {
            return IsHitTestVisible ? base.HitTestCore( hitTestParameters ) : null;
        }
        #endregion

        #region Properties
        public WebControl ParentControl
        {
            get
            {
                return parentControl;
            }
        }
        #endregion
    }
}
