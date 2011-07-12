/*******************************************************************************
 *    Project : AwesomiumSharp
 *    File    : ViewModel.cs
 *    Version : 1.0.0.0 
 *    Date    : 07/03/2011
 *    Author  : Perikles C. Stephanidis (AmaDeuS)
 *    Contact : perikles@stephanidis.net
 *-------------------------------------------------------------------------------
 *
 *    Notes   :
 *
 *    This abstract class implements IDisposable and INotifyPropertyChanged
 *    and provides the main logic that helps subclasses be MVVM friendly.
 *    
 * 
 ********************************************************************************/

#region Using
using System;
using System.ComponentModel;
#if !USING_MONO
using System.Linq;
#endif
#endregion

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// This abstract class implements <see cref="IDisposable"/> and <see cref="INotifyPropertyChanged"/> 
    /// and provides the main logic that helps subclasses be MVVM friendly.
    /// </summary>
    public abstract class ViewModel : IDisposable, INotifyPropertyChanged
    {
        private bool isDisposed;

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Helper method to raise the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property whose value has changed.
        /// </param>
        protected virtual void RaisePropertyChanged( string propertyName )
        {
            OnPropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        protected virtual void OnPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if ( PropertyChanged != null )
                PropertyChanged( sender, e );
        }

        /// <summary>
        /// Raised when the value of a property of this class, has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IDisposable Members
        ~ViewModel()
        {
            this.Dispose();
        }

        /// <summary>
        /// Called when an instance of this class is being disposed.
        /// </summary>
        protected virtual void OnDispose()
        {
            //
        }

        /// <summary>
        /// For Awesomium scenario, we do not let access to this method.
        /// To maintain a common contract for both WebView and WebControl,
        /// we prefer to expose this functionality through a Close method.
        /// </summary>
        void IDisposable.Dispose()
        {
            this.Dispose();
        }

        internal void Dispose()
        {
            if ( !IsDisposed )
            {
                OnDispose();
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
