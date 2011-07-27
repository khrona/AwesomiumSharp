/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    Utils.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 ***************************************************************************/

#region Using
using System;
using System.IO;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Globalization;
#endregion

namespace My
{
    #region ApplicationInfo
    sealed class ApplicationInfo
    {
        internal ApplicationInfo()
        {
            foreach ( Attribute a in Assembly.GetExecutingAssembly().GetCustomAttributes( true ) )
            {
                if ( a is AssemblyCompanyAttribute )
                    _CompanyName = ( (AssemblyCompanyAttribute)a ).Company;

                if ( a is AssemblyProductAttribute )
                    _ProductName = ( (AssemblyProductAttribute)a ).Product;

                if ( a is AssemblyFileVersionAttribute )
                    _Version = new Version( ( (AssemblyFileVersionAttribute)a ).Version );

                if ( a is AssemblyVersionAttribute )
                    _Version = new Version( ( (AssemblyVersionAttribute)a ).Version );
            }
        }

        private string _CompanyName;
        public string CompanyName
        {
            get
            {
                return _CompanyName;
            }
        }

        private string _ProductName;
        public string ProductName
        {
            get
            {
                return _ProductName;
            }
        }

        private Version _Version;
        public Version Version
        {
            get
            {
                return _Version;
            }
        }
    }
    #endregion

    #region Application
    sealed class Application
    {
        #region Fields
        private static ApplicationInfo info;
        #endregion


        #region Ctors
        static Application()
        {
            info = new ApplicationInfo();
        }
        #endregion


        #region Methods
        private static string GetDataPath( string basePath )
        {
            string companyName = Info.CompanyName;
            string productName = Info.ProductName;
            string productVersion = Info.Version.ToString();

            string dataPath = string.Format(
                CultureInfo.CurrentCulture,
                "{0}{4}{1}{4}{2}{4}{3}",
                basePath, companyName, productName, productVersion,
                Path.DirectorySeparatorChar );

            if ( !( Directory.Exists( dataPath ) ) )
                Directory.CreateDirectory( dataPath );

            return dataPath;
        }
        #endregion

        #region Properties
        public static ApplicationInfo Info
        {
            get { return info; }
        }

        public static string LocalUserAppDataPath
        {
            get
            {
                return GetDataPath( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ) );
            }
        }

        public static string UserAppDataPath
        {
            get
            {
                return GetDataPath( Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData ) );
            }
        }
        #endregion
    }
    #endregion
}


namespace TabbedWPFSample
{
    static class Utils
    {

        public static string GetLocalUserAppDataPath( this Application app )
        {
            return My.Application.LocalUserAppDataPath;
        }

        public static string GetUserAppDataPath( this Application app )
        {
            return My.Application.UserAppDataPath;
        }

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
