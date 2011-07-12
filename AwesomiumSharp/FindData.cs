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
 *    Helper class used internally by the new WebView.Find and WebControl.Find
 *    logic.
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
