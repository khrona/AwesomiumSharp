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
    public abstract class ViewModel : IDisposable, INotifyPropertyChanged
    {
        private bool isDisposed;

        #region INotifyPropertyChanged Members

        protected virtual void RaisePropertyChanged( string propertyName )
        {
            OnPropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
        }

        protected virtual void OnPropertyChanged( object sender, PropertyChangedEventArgs e )
        {
            if ( PropertyChanged != null )
                PropertyChanged( sender, e );
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IDisposable Members
        ~ViewModel()
        {
            Dispose();
        }

        protected virtual void OnDispose()
        {
            //
        }

        public void Dispose()
        {
            if ( !IsDisposed )
            {
                OnDispose();
                IsDisposed = true;
            }

            GC.SuppressFinalize( this );
        }

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
