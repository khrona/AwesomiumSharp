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
using System.Collections.ObjectModel;
#endregion

// Yes, I love VB.
namespace My
{
    #region AssemblyInfo
    public class AssemblyInfo
    {
        #region Fields
        private Assembly m_Assembly;
        private string m_CompanyName;
        private string m_Copyright;
        private string m_Description;
        private string m_ProductName;
        private string m_Title;
        private string m_Trademark;
        #endregion


        #region Ctors
        public AssemblyInfo( Assembly currentAssembly )
            {
                if ( currentAssembly == null )
                    throw new ArgumentNullException( "currentAssembly" );

                this.m_Assembly = currentAssembly;
            }
        #endregion


        #region Methods
        private object GetAttribute( Type attributeType )
        {
            object[] customAttributes = this.m_Assembly.GetCustomAttributes( attributeType, true );

            if ( customAttributes.Length == 0 )
                return null;

            return customAttributes[ 0 ];
        }
        #endregion

        #region Properties
        public string AssemblyName
        {
            get
            {
                return this.m_Assembly.GetName().Name;
            }
        }

        public string CompanyName
        {
            get
            {
                if ( this.m_CompanyName == null )
                {
                    AssemblyCompanyAttribute attribute = (AssemblyCompanyAttribute)this.GetAttribute( typeof( AssemblyCompanyAttribute ) );
                    this.m_CompanyName = attribute == null ? "" : attribute.Company;
                }

                return this.m_CompanyName;
            }
        }

        public string Copyright
        {
            get
            {
                if ( this.m_Copyright == null )
                {
                    AssemblyCopyrightAttribute attribute = (AssemblyCopyrightAttribute)this.GetAttribute( typeof( AssemblyCopyrightAttribute ) );
                    this.m_Copyright = attribute == null ? "" : attribute.Copyright;
                }

                return this.m_Copyright;
            }
        }

        public string Description
        {
            get
            {
                if ( this.m_Description == null )
                {
                    AssemblyDescriptionAttribute attribute = (AssemblyDescriptionAttribute)this.GetAttribute( typeof( AssemblyDescriptionAttribute ) );
                    this.m_Description = attribute == null ? "" : attribute.Description;
                }

                return this.m_Description;
            }
        }

        public string DirectoryPath
        {
            get
            {
                return Path.GetDirectoryName( this.m_Assembly.Location );
            }
        }

        public ReadOnlyCollection<Assembly> LoadedAssemblies
        {
            get
            {
                Collection<Assembly> list = new Collection<Assembly>();
                foreach ( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() )
                {
                    list.Add( assembly );
                }
                return new ReadOnlyCollection<Assembly>( list );
            }
        }

        public string ProductName
        {
            get
            {
                if ( this.m_ProductName == null )
                {
                    AssemblyProductAttribute attribute = (AssemblyProductAttribute)this.GetAttribute( typeof( AssemblyProductAttribute ) );
                    this.m_ProductName = attribute == null ? "" : attribute.Product;
                }
                return this.m_ProductName;
            }
        }

        public string StackTrace
        {
            get
            {
                return Environment.StackTrace;
            }
        }

        public string Title
        {
            get
            {
                if ( this.m_Title == null )
                {
                    AssemblyTitleAttribute attribute = (AssemblyTitleAttribute)this.GetAttribute( typeof( AssemblyTitleAttribute ) );
                    this.m_Title = attribute == null ? "" : attribute.Title;
                }
                return this.m_Title;
            }
        }

        public string Trademark
        {
            get
            {
                if ( this.m_Trademark == null )
                {
                    AssemblyTrademarkAttribute attribute = (AssemblyTrademarkAttribute)this.GetAttribute( typeof( AssemblyTrademarkAttribute ) );
                    this.m_Trademark = attribute == null ? "" : attribute.Trademark;
                }
                return this.m_Trademark;
            }
        }

        public Version Version
        {
            get
            {
                return this.m_Assembly.GetName().Version;
            }
        }

        public long WorkingSet
        {
            get
            {
                return Environment.WorkingSet;
            }
        }
        #endregion
    }
    #endregion

    #region Application
    sealed class Application
    {
        #region Fields
        private static AssemblyInfo info;
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
        public static AssemblyInfo Info
        {
            get
            {
                if ( info == null )
                {
                    Assembly entryAssembly = Assembly.GetEntryAssembly();

                    if ( entryAssembly == null )
                        entryAssembly = Assembly.GetCallingAssembly();

                    info = new AssemblyInfo( entryAssembly );
                }

                return info;
            }
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

        public static string GetFileSize( this FileInfo file )
        {
            long bytes = file.Length;

            if ( bytes >= 1073741824 )
            {
                Decimal size = Decimal.Divide( bytes, 1073741824 );
                return String.Format( "{0:##.##} GB", size );
            }
            else if ( bytes >= 1048576 )
            {
                Decimal size = Decimal.Divide( bytes, 1048576 );
                return String.Format( "{0:##.##} MB", size );
            }
            else if ( bytes >= 1024 )
            {
                Decimal size = Decimal.Divide( bytes, 1024 );
                return String.Format( "{0:##.##} KB", size );
            }
            else if ( bytes > 0 & bytes < 1024 )
            {
                Decimal size = bytes;
                return String.Format( "{0:##.##} Bytes", size );
            }
            else
            {
                return "0 Bytes";
            }
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
