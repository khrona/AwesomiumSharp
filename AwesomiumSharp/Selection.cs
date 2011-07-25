/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : Selection.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/24/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    Provides textual information about the current selection range of a page.
 *    
 * 
 ********************************************************************************/

using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Provides textual information about the current selection range of a page.
    /// </summary>
    public struct Selection
    {
        /// <summary>
        /// Represents an empty selection range.
        /// </summary>
        public static Selection Empty = new Selection() { Text = String.Empty, HTML = String.Empty };

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals( object obj )
        {
            if ( obj is Selection )
                return this == (Selection)obj;

            return false;
        }

        /// <summary>
        /// Gets the selected content in a page, in plain text form.
        /// </summary>
        public string Text { get; set; }
        /// <summary>
        /// Gets the selected content in a page, in HTML form.
        /// </summary>
        public string HTML { get; set; }

        public static bool operator ==( Selection sd1, Selection sd2 )
        {
            if ( Object.ReferenceEquals( sd1, null ) )
                return Object.ReferenceEquals( sd2, null );

            if ( Object.ReferenceEquals( sd2, null ) )
                return Object.ReferenceEquals( sd1, null );

            return ( String.Compare( sd1.Text, sd2.Text, false ) == 0 ) && 
                ( String.Compare( sd1.HTML, sd2.HTML, false ) == 0 );
        }

        public static bool operator !=( Selection sd1, Selection sd2 )
        {
            return !( sd1 == sd2 );
        }

    }
}
