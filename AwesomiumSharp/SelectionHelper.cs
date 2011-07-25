/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : SelectionHelper.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/24/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    Temporary halper class that uses Javascript objects, callbacks and
 *    injected code, to get information about the current selection range
 *    in a page. Used by both WebView and WebControl.
 *    
 *    This class may be removed if we get a native way of accessing this
 *    information.
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
    /// This helper class is temporarily added, until we get a native way to access
    /// current selection changes and properties.
    /// </summary>
    internal class SelectionHelper : IDisposable
    {
        #region Fields
        private IWebView view;
        private WebSelectionChangedHandler selectionChangedHandler;
        private Selection selection;

        private const string SELECTION_OBJECT = "WebControlSelectionHelper";
        private const string SELECTION_HTML_CALLBACK = "webControlHTMLSelectionChanged";
        private const string SELECTION_TEXT_CALLBACK = "webControlSelectionChanged";
        #endregion


        #region Ctor
        public SelectionHelper( IWebView parent, WebSelectionChangedHandler handler )
        {
            view = parent;
            selectionChangedHandler = handler;
        }
        #endregion


        #region Methods
        /// <summary>
        /// Must be called once when the view is created.
        /// </summary>
        public void RegisterSelectionHelper()
        {
            view.CreateObject( SELECTION_OBJECT );
            view.SetObjectCallback( SELECTION_OBJECT, SELECTION_TEXT_CALLBACK, OnTextSelectionChanged );
            view.SetObjectCallback( SELECTION_OBJECT, SELECTION_HTML_CALLBACK, OnHTMLSelectionChanged );
        }

        /// <summary>
        /// Must be called every time a new page is being loaded, at DOM ready time.
        /// </summary>
        public void InjectSelectionHandlers()
        {
            this.ClearSelection();

            view.ExecuteJavascript(
                "var webControlSelectionHandler = function(e){ " +
                "if ( window.getSelection().rangeCount > 0 ) { " +
                    "var range = window.getSelection().getRangeAt(0);" +
                    "var clonedSelection = range.cloneContents();" +
                    "var div = document.createElement( 'div' );" +
                    "div.appendChild( clonedSelection );" +
                    String.Format( "{0}.{1}", SELECTION_OBJECT, SELECTION_HTML_CALLBACK ) + "( div.innerHTML ); }" +
                "else { " +
                    String.Format( "{0}.{1}", SELECTION_OBJECT, SELECTION_HTML_CALLBACK ) + "( '' ); }" +
                String.Format( "{0}.{1}", SELECTION_OBJECT, SELECTION_TEXT_CALLBACK ) + "( window.getSelection().toString() ); };" );

            view.ExecuteJavascript( "document.addEventListener('select', webControlSelectionHandler, true);" );
            view.ExecuteJavascript( "document.addEventListener('selectstart', webControlSelectionHandler, true);" );
        }

        /// <summary>
        /// Informs listeners of an empty selection.
        /// </summary>
        public void ClearSelection()
        {
            selection = Selection.Empty;

            if ( selectionChangedHandler != null )
                selectionChangedHandler( this, new WebSelectionEventArgs( selection ) );
        }

        /// <summary>
        /// Plain text callback.
        /// </summary>
        protected void OnTextSelectionChanged( object sender, JSCallbackEventArgs e )
        {
            //System.Diagnostics.Debug.Print( "Text: " + e.Arguments[ 0 ].ToString() );
            selection.Text = e.Arguments[ 0 ].ToString();

            if ( selectionChangedHandler != null )
                selectionChangedHandler( this, new WebSelectionEventArgs( selection ) );
        }

        /// <summary>
        /// HTML text callback.
        /// </summary>
        protected void OnHTMLSelectionChanged( object sender, JSCallbackEventArgs e )
        {
            //System.Diagnostics.Debug.Print( "HTML: " + e.Arguments[ 0 ].ToString() );
            selection.HTML = e.Arguments[ 0 ].ToString();
        }
        #endregion

        #region Properties
        public IWebView View
        {
            get
            {
                return view;
            }
        }

        public Selection Selection
        {
            get
            {
                return selection;
            }
        }
        #endregion


        #region IDisposable
        private bool isDisposed;

        ~SelectionHelper()
        {
            this.Dispose();
        }


        public void Dispose()
        {
            if ( !IsDisposed )
            {
                selectionChangedHandler = null;
                view = null;

                IsDisposed = true;
            }

            GC.SuppressFinalize( this );
        }

        /// <summary>
        /// Gets if this instance is already disposed and eligible for garbage collection.
        /// </summary>
        public bool IsDisposed
        {
            get
            {
                return isDisposed;
            }
            protected set
            {
                if ( isDisposed == value )
                    return;

                isDisposed = value;
            }
        }
        #endregion
    }
}
