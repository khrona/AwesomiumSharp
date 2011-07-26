/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    ViewModel.cs
 *  Version: 1.5.1.0
 *
 *  Copyright ©2011 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 *__________________________________________________________________________
 *
 *  Notes:
 *
 *  This abstract class implements IDisposable and INotifyPropertyChanged
 *  and provides the main logic that helps subclasses be MVVM friendly.
 *   
 ***************************************************************************/

#region  Using
using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Windows.Threading;
using System.Collections.Generic;
using System.Collections.Specialized;
#endregion

namespace TabbedWPFSample
{
    /// <summary>
    /// Base class for all view model classes.
    /// </summary>
    [Serializable()]
    internal abstract class ViewModel : INotifyPropertyChanged, IDataErrorInfo, IDisposable
    {
        #region  Fields
        [NonSerialized()]
        private List<IDelegateCommand> _Commands;
        [NonSerialized()]
        private bool _IsActive;
        [NonSerialized()]
        private Dictionary<string, string> _Errors;

        [NonSerialized()]
        private List<PropertyChangedEventListener> propertyChangedListeners;
        [NonSerialized()]
        private List<CollectionChangedEventListener> collectionChangedListeners;

        [NonSerialized()]
        private DelegateCommand<bool> UpdateActiveCommand;
        #endregion

        #region  Events
        [field: NonSerialized()]
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion


        #region  Constructors
        /// <summary>
        /// Creates and initializes a view model instance.
        /// </summary>
        public ViewModel()
        {
            _Commands = new List<IDelegateCommand>();
            _Errors = new Dictionary<string, string>();

            propertyChangedListeners = new List<PropertyChangedEventListener>();
            collectionChangedListeners = new List<CollectionChangedEventListener>();

            UpdateActiveCommand = new DelegateCommand<bool>( OnUpdateActive, CanUpdateActive );
            Commands.Add( UpdateActiveCommand );
        }
        #endregion


        #region  Methods

        #region  UpdateUI
        /// <summary>
        /// Updates connected views by raising <see cref="ICommand.CanExecuteChanged" />
        /// for all commands in <see cref="Commands" /> list.
        /// </summary>
        public virtual void UpdateUI()
        {
            try
            {
                if ( IsDisposed )
                    return;

                foreach ( IDelegateCommand cmd in Commands )
                    cmd.RaiseCanExecuteChanged();
            }
            catch
            {
                Dispatcher.CurrentDispatcher.BeginInvoke( DispatcherPriority.Background, (Action)UpdateUI );
            }
        }
        #endregion

        #region  Activate
        /// <summary>
        /// Attempts to activate the assigned view, if any.
        /// </summary>
        /// <param name="updateUI">
        /// Indicates if the property value changes regularly
        /// or silently. If this is set to false, neither
        /// <see cref="PropertyChanged" /> nor <see cref="OnIsActiveChanged" />
        /// are fired. Use with caution and only if you know there are no
        /// listeners for the state of the <see cref="IsActive" /> property.
        /// </param>
        public virtual void Activate( bool updateUI = true )
        {
            if ( updateUI )
                this.IsActive = true;
            else
                _IsActive = true;
        }

        /// <summary>
        /// Attempts to deactivate the assigned view, if any.
        /// </summary>
        /// <param name="updateUI">
        /// Indicates if the property value changes regularly
        /// or silently. If this is set to false, neither
        /// <see cref="PropertyChanged" /> nor <see cref="OnIsActiveChanged" />
        /// are fired. Use with caution and only if you know there are no
        /// listeners for the state of the <see cref="IsActive" /> property.
        /// </param>
        public void Deactivate( bool updateUI = true )
        {
            if ( updateUI )
                this.IsActive = false;
            else
                _IsActive = false;
        }

        /// <summary>
        /// <see cref="UpdateActive" /> command callback.
        /// </summary>
        protected virtual void OnUpdateActive( bool activate )
        {
            IsActive = activate;
        }

        /// <summary>
        /// <see cref="UpdateActive" /> command callback.
        /// </summary>
        protected virtual bool CanUpdateActive( bool activate )
        {
            return ( activate != IsActive );
        }

        /// <summary>
        /// Called when the <see cref="IsActive" /> property value changes.
        /// </summary>
        /// <remarks>
        /// This can be useful to avoid overriding <see cref="OnPropertyChanged" />
        /// for a property that often needs to be checked. If you don't call
        /// this base method from your implementation, <see cref="Common.ActiveModel" />
        /// will not be set.
        /// </remarks>
        protected virtual void OnIsActiveChanged()
        {
            if ( _IsActive )
            {
                // Nothing to do for this sample.
            }
        }
        #endregion

        #region  OnValidate
        /// <summary>
        /// Called when a binding on a view requests validation info
        /// for the value of a property.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property being validated.
        /// </param>
        protected virtual void OnValidate( string propertyName )
        {
            //
        }
        #endregion

        #region  ClearError
        /// <summary>
        /// Clears any errors previously defined for the specified property.
        /// </summary>
        protected void ClearError( string propertyName )
        {
            if ( _Errors.ContainsKey( propertyName ) )
            {
                _Errors.Remove( propertyName );
            }
        }
        #endregion

        #region  RaisePropertyChanged
        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The property name of the property that has changed.</param>
        /// <remarks>Thread safe.</remarks>
        protected void RaisePropertyChanged( string propertyName )
        {
            if ( ( Application.Current == null ) || ( Application.Current.Dispatcher == null ) )
                return;

            if ( Application.Current.Dispatcher.CheckAccess() )
                OnPropertyChanged( new PropertyChangedEventArgs( propertyName ) );
            else
                Application.Current.Dispatcher.BeginInvoke( (Action<PropertyChangedEventArgs>)OnPropertyChanged, new PropertyChangedEventArgs( propertyName ) );
        }
        #endregion

        #region  OnPropertyChanged
        /// <summary>
        /// Raises the <see cref="E:PropertyChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnPropertyChanged( PropertyChangedEventArgs e )
        {
            if ( PropertyChanged != null )
            {
                PropertyChanged( this, e );
            }
        }
        #endregion

        #region  AddValidationError
        /// <summary>
        /// Adds a validation error for the value of a property.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property with a validation error.
        /// </param>
        /// <param name="errorInfo">
        /// Info message for the validation error.
        /// </param>
        protected void AddValidationError( string propertyName, string errorInfo )
        {
            _Errors[ propertyName ] = errorInfo;
            RaisePropertyChanged( propertyName );
        }
        #endregion

        #region  AddWeakEventListener
        /// <summary>
        /// Adds a weak event listener for a PropertyChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void AddWeakEventListener( INotifyPropertyChanged source, PropertyChangedEventHandler handler )
        {
            if ( source == null )
                throw new ArgumentNullException( "source" );

            if ( handler == null )
                throw new ArgumentNullException( "handler" );

            PropertyChangedEventListener listener = propertyChangedListeners.LastOrDefault( ( l ) => l.Source == source && l.Handler == handler );

            if ( listener == null )
                listener = new PropertyChangedEventListener( source, handler );
            else
                return;

            propertyChangedListeners.Add( listener );
            PropertyChangedEventManager.AddListener( source, listener, string.Empty );
        }

        /// <summary>
        /// Adds a weak event listener for a CollectionChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void AddWeakEventListener( INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler )
        {
            if ( source == null )
                throw new ArgumentNullException( "source" );

            if ( handler == null )
                throw new ArgumentNullException( "handler" );

            CollectionChangedEventListener listener = collectionChangedListeners.LastOrDefault( ( l ) => l.Source == source && l.Handler == handler );

            if ( listener == null )
                listener = new CollectionChangedEventListener( source, handler );
            else
                return;

            collectionChangedListeners.Add( listener );
            CollectionChangedEventManager.AddListener( source, listener );
        }
        #endregion

        #region  RemoveWeakEventListener
        /// <summary>
        /// Removes the weak event listener for a PropertyChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void RemoveWeakEventListener( INotifyPropertyChanged source, PropertyChangedEventHandler handler )
        {
            if ( source == null )
                throw new ArgumentNullException( "source" );

            if ( handler == null )
                throw new ArgumentNullException( "handler" );

            PropertyChangedEventListener listener = propertyChangedListeners.LastOrDefault( ( l ) => l.Source == source && l.Handler == handler );

            if ( listener != null )
            {
                propertyChangedListeners.Remove( listener );
                PropertyChangedEventManager.RemoveListener( source, listener, string.Empty );
            }
        }

        /// <summary>
        /// Removes the weak event listener for a CollectionChanged event.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="handler">The event handler.</param>
        /// <exception cref="ArgumentNullException">source must not be <c>null</c>.</exception>
        /// <exception cref="ArgumentNullException">handler must not be <c>null</c>.</exception>
        protected void RemoveWeakEventListener( INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler )
        {
            if ( source == null )
                throw new ArgumentNullException( "source" );

            if ( handler == null )
                throw new ArgumentNullException( "handler" );

            CollectionChangedEventListener listener = collectionChangedListeners.LastOrDefault( ( l ) => l.Source == source && l.Handler == handler );

            if ( listener != null )
            {
                collectionChangedListeners.Remove( listener );
                CollectionChangedEventManager.RemoveListener( source, listener );
            }
        }
        #endregion

        #endregion

        #region  Properties
        /// <summary>
        /// Gets a list of commands exposed by the view model.
        /// </summary>
        protected List<IDelegateCommand> Commands
        {
            get
            {
                return _Commands;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the view model has been disposed of.
        /// </summary>
        [Browsable( false )]
        public bool IsDisposed
        {
            get
            {
                return _IsDisposed;
            }
            private set
            {
                if ( _IsDisposed == value )
                {
                    return;
                }

                _IsDisposed = value;
                RaisePropertyChanged( "IsDisposed" );
            }
        }

        /// <summary>
        /// Gets or sets if a view this model is attached to, is currently active.
        /// </summary>
        [Browsable( false ), XmlIgnore(), DesignerSerializationVisibility( DesignerSerializationVisibility.Hidden )]
        public virtual bool IsActive
        {
            get
            {
                return _IsActive;
            }
            set
            {
                if ( _IsActive == value )
                {
                    return;
                }

                _IsActive = value;
                RaisePropertyChanged( "IsActive" );
                OnIsActiveChanged();
            }
        }

        /// <summary>
        /// Command helpful to manipulate IsActive property from
        /// templates.
        /// </summary>
        [Browsable( false )]
        public ICommand UpdateActive
        {
            get
            {
                return UpdateActiveCommand;
            }
        }

        [Browsable( false )]
        public bool HasErrors
        {
            get
            {
                return _Errors.Count > 0;
            }
        }

        /// <summary>
        /// Gets a dictionary of errors in this model where the key
        /// of each entry is the model's property name and the value
        /// is the corresponding error info.
        /// </summary>
        [Browsable( false )]
        public IDictionary<string, string> Errors
        {
            get
            {
                return _Errors;
            }
        }
        #endregion

        #region  IDisposable
        private bool _IsDisposed; // To detect redundant calls

        /// <summary>
        /// Override to remove event listeners and run cleanup code concerning <b>managed</b> objects.
        /// </summary>
        protected virtual void OnDispose()
        {
        }

        // IDisposable
        private void Dispose( bool disposing )
        {
            if ( !IsDisposed )
            {
                if ( disposing )
                {
                    OnDispose();

                    if ( propertyChangedListeners != null )
                    {
                        propertyChangedListeners.Clear();
                        propertyChangedListeners = null;
                    }

                    if ( collectionChangedListeners != null )
                    {
                        collectionChangedListeners.Clear();
                        collectionChangedListeners = null;
                    }

                    if ( _Commands != null )
                    {
                        _Commands.Clear();
                        _Commands = null;
                    }

                    UpdateActiveCommand = null;
                }
            }

            IsDisposed = true;
        }

        ~ViewModel()
        {
            Dispose( false );
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }
        #endregion

        #region  IDataErrorInfo
        string IDataErrorInfo.Error
        {
            get
            {
                return null;
            }
        }

        [Browsable( false )]
        public string this[ string propertyName ]
        {
            get
            {
                OnValidate( propertyName );

                if ( _Errors.ContainsKey( propertyName ) )
                {
                    return _Errors[ propertyName ];
                }

                return null;
            }
        }
        #endregion
    }
}
