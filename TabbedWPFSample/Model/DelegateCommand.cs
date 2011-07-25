//***************************************************************************
//    Project: TabbedWPFSample
//    File:    DelegateCommand.vb
//    Version: 1.0.0.0
//
//    Copyright ©2010 Perikles C. Stephanidis; All rights reserved.
//    This code is provided "AS IS" without warranty of any kind.
//***************************************************************************

#region  Using
using System;
using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
#endregion

namespace TabbedWPFSample
{
    #region  IDelegateCommand
    internal interface IDelegateCommand : ICommand
    {
        void RaiseCanExecuteChanged();
    }
    #endregion

    #region  DelegateCommand
    /// <summary>
    /// This class allows delegating the commanding logic to methods passed as parameters,
    /// and enables a View to bind commands to objects that are not part of the element tree.
    /// </summary>
    internal class DelegateCommand : IDelegateCommand
    {

        #region  Fields
        private Action _ExecuteMethod;
        private Func<bool> _CanExecuteMethod;
        private bool _IsAutomaticRequeryDisabled;
        private List<WeakReference> _CanExecuteChangedHandlers;
        #endregion


        #region  Constructors
        public DelegateCommand()
            : this( null, null, false )
        {
        }

        public DelegateCommand( Action executeMethod )
            : this( executeMethod, null, false )
        {
        }

        public DelegateCommand( Action executeMethod, Func<bool> canExecuteMethod )
            : this( executeMethod, canExecuteMethod, false )
        {
        }

        public DelegateCommand( Action executeMethod, Func<bool> canExecuteMethod, bool isAutomaticRequeryDisabled )
        {
            _ExecuteMethod = executeMethod;
            _CanExecuteMethod = canExecuteMethod;
            _IsAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }
        #endregion


        #region  Methods
        /// <summary>
        /// Method to determine if the command can be executed
        /// </summary>
        public bool CanExecute()
        {
            if ( _CanExecuteMethod != null )
                return _CanExecuteMethod();

            return true;
        }

        /// <summary>
        /// Execution of the command
        /// </summary>
        public void Execute()
        {
            if ( _ExecuteMethod != null )
                _ExecuteMethod();
        }

        /// <summary>
        /// Raises the CanExecuteChaged event
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if ( ( Application.Current == null ) || ( Application.Current.Dispatcher == null ) )
                return;

            if ( Application.Current.Dispatcher.CheckAccess() )
                OnCanExecuteChanged();
            else
                Application.Current.Dispatcher.Invoke( (Action)OnCanExecuteChanged );
        }

        /// <summary>
        /// Protected virtual method to raise CanExecuteChanged event
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            CommandManagerHelper.CallWeakReferenceHandlers( _CanExecuteChangedHandlers );
        }
        #endregion

        #region  Properties
        public Action ExecuteMethod
        {
            get
            {
                return _ExecuteMethod;
            }
            set
            {
                if ( _ExecuteMethod == value )
                    return;

                _ExecuteMethod = value;
            }
        }

        public Func<bool> CanExecuteMethod
        {
            get
            {
                return _CanExecuteMethod;
            }
            set
            {
                if ( _CanExecuteMethod == value )
                    return;

                _CanExecuteMethod = value;
            }
        }

        /// <summary>
        /// Property to enable or disable CommandManager's automatic requery on this command
        /// </summary>
        public bool IsAutomaticRequeryDisabled
        {
            get
            {
                return _IsAutomaticRequeryDisabled;
            }
            set
            {
                if ( _IsAutomaticRequeryDisabled != value )
                {
                    if ( value )
                        CommandManagerHelper.RemoveHandlersFromRequerySuggested( _CanExecuteChangedHandlers );
                    else
                        CommandManagerHelper.AddHandlersToRequerySuggested( _CanExecuteChangedHandlers );

                    _IsAutomaticRequeryDisabled = value;
                }
            }
        }
        #endregion

        #region  ICommand
        /// <summary>
        /// ICommand.CanExecuteChanged implementation
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if ( !_IsAutomaticRequeryDisabled )
                    CommandManager.RequerySuggested += value;

                CommandManagerHelper.AddWeakReferenceHandler( ref _CanExecuteChangedHandlers, value, 2 );
            }
            remove
            {
                if ( !_IsAutomaticRequeryDisabled )
                    CommandManager.RequerySuggested -= value;

                CommandManagerHelper.RemoveWeakReferenceHandler( _CanExecuteChangedHandlers, value );
            }
        }

        bool ICommand.CanExecute( object parameter )
        {
            return this.CanExecute();
        }

        void ICommand.Execute( object parameter )
        {
            this.Execute();
        }
        #endregion

    }
    #endregion

    #region  DelegateCommand<T>
    /// <summary>
    /// This class allows delegating the commanding logic to methods passed as parameters,
    /// and enables a View to bind commands to objects that are not part of the element tree.
    /// </summary>
    /// <typeparam name="T">Type of the parameter passed to the delegates</typeparam>
    internal class DelegateCommand<T> : IDelegateCommand
    {
        #region  Fields
        private Action<T> _ExecuteMethod;
        private Func<T, bool> _CanExecuteMethod;
        private bool _IsAutomaticRequeryDisabled;
        private List<WeakReference> _CanExecuteChangedHandlers;
        #endregion


        #region  Constructors
        public DelegateCommand()
            : this( null, null, false )
        {
        }

        public DelegateCommand( Action<T> executeMethod )
            : this( executeMethod, null, false )
        {
        }

        public DelegateCommand( Action<T> executeMethod, Func<T, bool> canExecuteMethod )
            : this( executeMethod, canExecuteMethod, false )
        {
        }

        public DelegateCommand( Action<T> executeMethod, Func<T, bool> canExecuteMethod, bool isAutomaticRequeryDisabled )
        {
            _ExecuteMethod = executeMethod;
            _CanExecuteMethod = canExecuteMethod;
            _IsAutomaticRequeryDisabled = isAutomaticRequeryDisabled;
        }
        #endregion


        #region  Methods
        /// <summary>
        /// Determines if the command can be executed.
        /// </summary>
        public bool CanExecute( T parameter )
        {
            if ( _CanExecuteMethod != null )
                return _CanExecuteMethod( parameter );

            return true;
        }

        /// <summary>
        /// Execution of the command.
        /// </summary>
        public void Execute( T parameter )
        {
            if ( _ExecuteMethod != null )
                _ExecuteMethod( parameter );
        }

        /// <summary>
        /// Raises the CanExecuteChaged event.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            if ( ( Application.Current == null ) || ( Application.Current.Dispatcher == null ) )
                return;

            if ( Application.Current.Dispatcher.CheckAccess() )
                OnCanExecuteChanged();
            else
                Application.Current.Dispatcher.Invoke( (Action)OnCanExecuteChanged );
        }

        /// <summary>
        /// Protected virtual method to raise CanExecuteChanged event.
        /// </summary>
        protected virtual void OnCanExecuteChanged()
        {
            CommandManagerHelper.CallWeakReferenceHandlers( _CanExecuteChangedHandlers );
        }
        #endregion

        #region  Properties
        public Action<T> ExecuteMethod
        {
            get
            {
                return _ExecuteMethod;
            }
            set
            {
                if ( _ExecuteMethod == value )
                    return;

                _ExecuteMethod = value;
            }
        }

        public Func<T, bool> CanExecuteMethod
        {
            get
            {
                return _CanExecuteMethod;
            }
            set
            {
                if ( _CanExecuteMethod == value )
                    return;

                _CanExecuteMethod = value;
            }
        }

        /// <summary>
        /// Property to enable or disable CommandManager's automatic requery on this command.
        /// </summary>
        public bool IsAutomaticRequeryDisabled
        {
            get
            {
                return _IsAutomaticRequeryDisabled;
            }
            set
            {
                if ( _IsAutomaticRequeryDisabled != value )
                {
                    if ( value )
                        CommandManagerHelper.RemoveHandlersFromRequerySuggested( _CanExecuteChangedHandlers );
                    else
                        CommandManagerHelper.AddHandlersToRequerySuggested( _CanExecuteChangedHandlers );

                    _IsAutomaticRequeryDisabled = value;
                }
            }
        }
        #endregion

        #region  ICommand
        /// <summary>
        /// ICommand.CanExecuteChanged implementation
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if ( !_IsAutomaticRequeryDisabled )
                    CommandManager.RequerySuggested += value;

                CommandManagerHelper.AddWeakReferenceHandler( ref _CanExecuteChangedHandlers, value, 2 );
            }
            remove
            {
                if ( !_IsAutomaticRequeryDisabled )
                    CommandManager.RequerySuggested -= value;

                CommandManagerHelper.RemoveWeakReferenceHandler( _CanExecuteChangedHandlers, value );
            }
        }

        bool ICommand.CanExecute( object parameter )
        {
            // if T is of value type and the parameter is not
            // set yet, then return false if CanExecute delegate
            // exists, else return true
            if ( parameter == null && typeof( T ).IsValueType )
                return true;

            return this.CanExecute( (T)parameter );
        }

        void ICommand.Execute( object parameter )
        {
            this.Execute( (T)parameter );
        }
        #endregion
    }
    #endregion

    #region  CommandManagerHelper
    /// <summary>
    /// This class contains methods for the CommandManager that help avoid memory leaks by
    /// using weak references.
    /// </summary>
    internal class CommandManagerHelper
    {

        internal static void CallWeakReferenceHandlers( List<WeakReference> handlers )
        {
            if ( handlers != null )
            {
                // Take a snapshot of the handlers before we call out to them since the handlers
                // could cause the array to me modified while we are reading it.

                EventHandler[] callees = new EventHandler[ handlers.Count ];
                int count = 0;

                for ( int i = handlers.Count - 1; i >= 0; i-- )
                {
                    WeakReference reference = handlers[ i ];
                    EventHandler handler = reference.Target as EventHandler;
                    if ( handler == null )
                    {
                        // Clean up old handlers that have been collected
                        handlers.RemoveAt( i );
                    }
                    else
                    {
                        callees[ count ] = handler;
                        count += 1;
                    }
                }

                // Call the handlers that we snapshotted
                for ( int i = 0; i < count; i++ )
                {
                    EventHandler handler = callees[ i ];
                    handler( null, EventArgs.Empty );
                }
            }
        }

        internal static void AddHandlersToRequerySuggested( List<WeakReference> handlers )
        {
            if ( handlers != null )
            {
                foreach ( WeakReference handlerRef in handlers )
                {
                    EventHandler handler = handlerRef.Target as EventHandler;

                    if ( handler != null )
                        CommandManager.RequerySuggested += handler;
                }
            }
        }

        internal static void RemoveHandlersFromRequerySuggested( List<WeakReference> handlers )
        {
            if ( handlers != null )
            {
                foreach ( WeakReference handlerRef in handlers )
                {
                    EventHandler handler = handlerRef.Target as EventHandler;

                    if ( handler != null )
                        CommandManager.RequerySuggested -= handler;
                }
            }
        }

        internal static void AddWeakReferenceHandler( ref List<WeakReference> handlers, EventHandler handler )
        {
            AddWeakReferenceHandler( ref handlers, handler, -1 );
        }

        internal static void AddWeakReferenceHandler( ref List<WeakReference> handlers, EventHandler handler, int defaultListSize )
        {
            if ( handlers == null )
                handlers = ( ( ( defaultListSize > 0 ) ? new List<WeakReference>( defaultListSize ) : new List<WeakReference>() ) );

            handlers.Add( new WeakReference( handler ) );
        }

        internal static void RemoveWeakReferenceHandler( List<WeakReference> handlers, EventHandler handler )
        {
            if ( handlers != null )
            {
                for ( int i = handlers.Count - 1; i >= 0; i-- )
                {
                    WeakReference reference = handlers[ i ];
                    EventHandler existingHandler = reference.Target as EventHandler;
                    if ( ( existingHandler == null ) || ( existingHandler == handler ) )
                    {
                        // Clean up old handlers that have been collected
                        // in addition to the handler that is to be removed.
                        handlers.RemoveAt( i );
                    }
                }
            }
        }
    }
    #endregion
}
