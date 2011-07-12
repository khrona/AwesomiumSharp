using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.FindResultsReceived"/> and 
    /// <see cref="Windows.Controls.WebControl.FindResultsReceived"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="GetFindResultsEventArgs"/> that contains the event data.</param>
    public delegate void FindResultsReceivedEventHandler( object sender, GetFindResultsEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.FindResultsReceived"/> and 
    /// <see cref="Windows.Controls.WebControl.FindResultsReceived"/> events.
    /// </summary>
    public class GetFindResultsEventArgs : EventArgs
    {
        public GetFindResultsEventArgs( int requestID, 
            int numMatches, 
            AweRect selection,
            int curMatch, 
            bool finalUpdate )
        {
            this.requestID = requestID;
            this.numMatches = numMatches;
            this.selection = selection;
            this.curMatch = curMatch;
            this.finalUpdate = finalUpdate;
        }

        private int requestID;
        public int RequestID
        {
            get
            {
                return requestID;
            }
        }
        private int numMatches;
        public int NumberOfMatches
        {
            get
            {
                return numMatches;
            }
        }
        private AweRect selection;
        public AweRect Selection
        {
            get
            {
                return selection;
            }
        }
        private int curMatch;
        public int CurrentMatch
        {
            get
            {
                return curMatch;
            }
        }
        private bool finalUpdate;
        public bool IsFinalUpdate
        {
            get
            {
                return finalUpdate;
            }
        }
    }
}
