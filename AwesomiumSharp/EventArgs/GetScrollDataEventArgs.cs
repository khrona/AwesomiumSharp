using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.ScrollDataReceived"/> and 
    /// <see cref="Windows.Controls.WebControl.ScrollDataReceived"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="ScrollDataEventArgs"/> that contains the event data.</param>
    public delegate void ScrollDataReceivedEventHandler( object sender, ScrollDataEventArgs e );

    #region ScrollData
    /// <summary>
    /// Contains the page dimensions and scroll position of the page.
    /// </summary>
    public struct ScrollData
    {
        private int contentWidth, contentHeight, preferredWidth, scrollX, scrollY;

        #region Ctor
        internal ScrollData( int contentWidth, int contentHeight, int preferredWidth, int scrollX, int scrollY )
        {
            this.contentWidth = contentWidth;
            this.contentHeight = contentHeight;
            this.preferredWidth = preferredWidth;
            this.scrollX = scrollX;
            this.scrollY = scrollY;
        }
        #endregion


        #region Methods
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals( object obj )
        {
            if ( obj is ScrollData )
                return this == (ScrollData)obj;

            return false;
        }
        #endregion

        #region Properties
        public int ContentHeight
        {
            get
            {
                return contentHeight;
            }
        }

        public int ContentWidth
        {
            get
            {
                return contentWidth;
            }
        }

        public int PreferredWidth
        {
            get
            {
                return preferredWidth;
            }
        }

        public int ScrollX
        {
            get
            {
                return scrollX;
            }
        }

        public int ScrollY
        {
            get
            {
                return scrollY;
            }
        }
        #endregion

        #region Operators
        public static bool operator ==( ScrollData sd1, ScrollData sd2 )
        {
            return sd1.ContentHeight == sd2.ContentHeight &&
                sd1.ContentWidth == sd2.ContentWidth &&
                sd1.PreferredWidth == sd2.PreferredWidth &&
                sd1.ScrollX == sd2.ScrollX &&
                sd1.ScrollY == sd2.ScrollY;
        }

        public static bool operator !=( ScrollData sd1, ScrollData sd2 )
        {
            return sd1.ContentHeight != sd2.ContentHeight ||
                sd1.ContentWidth != sd2.ContentWidth ||
                sd1.PreferredWidth != sd2.PreferredWidth ||
                sd1.ScrollX != sd2.ScrollX ||
                sd1.ScrollY != sd2.ScrollY;
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// Provides data for the <see cref="WebView.ScrollDataReceived"/> and 
    /// <see cref="Windows.Controls.WebControl.ScrollDataReceived"/> events.
    /// </summary>
    public class ScrollDataEventArgs : EventArgs
    {
        private ScrollData scrollData;

        public ScrollDataEventArgs( ScrollData data )
        {
            this.scrollData = data;
        }

        public ScrollData ScrollData
        {
            get
            {
                return scrollData;
            }
        }
    }
}
