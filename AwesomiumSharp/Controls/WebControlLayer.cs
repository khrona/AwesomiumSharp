using System;
using System.Windows.Media;
using System.Windows.Controls;

namespace AwesomiumSharp.Windows.Controls
{
    class WebControlLayer : Image
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
