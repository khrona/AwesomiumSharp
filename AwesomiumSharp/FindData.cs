using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    class FindData
    {
        #region Fields
        private int requestID;
        private string searchText;
        private bool caseSensitive;
        #endregion

        #region Ctor
        internal FindData( int id, string txt, bool caseSensitive )
        {
            this.requestID = id;
            this.searchText = txt;
            this.caseSensitive = caseSensitive;
        }
        #endregion

        #region Properties
        public int RequestID
        {
            get
            {
                return requestID;
            }
        }

        public string SearchText
        {
            get
            {
                return searchText;
            }
        }

        public bool CaseSensitive
        {
            get
            {
                return caseSensitive;
            }
        }
        #endregion
    }
}
