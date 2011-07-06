using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void FindResultsReceivedEventHandler( object sender, GetFindResultsEventArgs e );

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
