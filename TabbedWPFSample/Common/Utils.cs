/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    Utils.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 ***************************************************************************/

using System;
using System.Windows;
using System.Windows.Media;

namespace TabbedWPFSample
{
    static class Utils
    {
        public static T FindLogicalAncestor<T>( this DependencyObject obj ) where T : DependencyObject
        {
            obj = LogicalTreeHelper.GetParent( obj );

            while ( ( obj != null ) && ( !( obj is T ) ) )
            {
                obj = LogicalTreeHelper.GetParent( obj );
            }

            return obj as T;
        }

        public static T FindVisualAncestor<T>( this DependencyObject obj ) where T : DependencyObject
        {
            obj = VisualTreeHelper.GetParent( obj );

            while ( ( obj != null ) && ( !( obj is T ) ) )
            {
                obj = VisualTreeHelper.GetParent( obj );
            }

            return obj as T;
        }

        public static T FindAncestor<T>( this DependencyObject obj ) where T : DependencyObject
        {
            DependencyObject d = obj.FindLogicalAncestor<T>();

            if ( d == null )
                d = obj.FindVisualAncestor<T>();

            return d as T;
        }

        /// <summary>
        /// Converts a coordinate from the polar coordinate system to the cartesian coordinate system.
        /// </summary>
        public static Point ComputeCartesianCoordinate( double angle, double radius )
        {
            // convert to radians
            double angleRad = ( Math.PI / 180.0 ) * ( angle - 90 );

            double x = radius * Math.Cos( angleRad );
            double y = radius * Math.Sin( angleRad );

            return new Point( x, y );
        }

        public static Point OffsetExt( this Point point, double X, double Y )
        {
            return new Point( point.X + X, point.Y + Y );
        }
    }
}
