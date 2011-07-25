//***************************************************************************
//    Project: AmadeusIRC®
//    File:    WeakEventListener.vb
//    Version: 1.0.0.0
//
//    Copyright ©2010 Perikles C. Stephanidis; All rights reserved.
//    This code is provided "AS IS" without warranty of any kind.
//    Use in a production environment is prohibited.
//***************************************************************************

#region Using
using System;
using System.Windows;
using System.ComponentModel;
using System.Collections.Specialized;
#endregion

namespace TabbedWPFSample
{
    #region  PropertyChangedEventListener
    /// <summary>
    /// A common <see cref="INotifyPropertyChanged.PropertyChanged" /> weak event listener.
    /// </summary>
    internal class PropertyChangedEventListener : IWeakEventListener
    {

        private readonly INotifyPropertyChanged _Source;
        private readonly PropertyChangedEventHandler _Handler;

        /// <summary>
        /// Initializes a new instance of the PropertyChangedEventListener class.
        /// </summary>
        /// <param name="handler">The handler for the event.</param>
        /// <param name="source">The source of the property.</param>
        public PropertyChangedEventListener( INotifyPropertyChanged source, PropertyChangedEventHandler handler )
        {
            if ( source == null )
                throw new ArgumentNullException( "source" );

            if ( handler == null )
                throw new ArgumentNullException( "handler" );

            _Source = source;
            _Handler = handler;
        }

        public INotifyPropertyChanged Source
        {
            get
            {
                return _Source;
            }
        }

        public PropertyChangedEventHandler Handler
        {
            get
            {
                return _Handler;
            }
        }

        /// <summary>
        /// Receives events from the centralized event manager.
        /// </summary>
        /// <param name="managerType">The type of the WeakEventManager calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns>
        /// true if the listener handled the event. It is considered an error by the WeakEventManager handling in WPF 
        /// to register a listener for an event that the listener does not handle. Regardless, the method should return false 
        /// if it receives an event that it does not recognize or handle.
        /// </returns>
        public bool ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
        {
            PropertyChangedEventArgs realArgs = (PropertyChangedEventArgs)e;
            _Handler( sender, realArgs );
            return true;
        }
    }
    #endregion

    #region  CollectionChangedEventListener
    /// <summary>
    /// A common <see cref="INotifyCollectionChanged.CollectionChanged" /> weak event listener.
    /// </summary>
    internal class CollectionChangedEventListener : IWeakEventListener
    {

        private readonly INotifyCollectionChanged _Source;
        private readonly NotifyCollectionChangedEventHandler _Handler;

        /// <summary>
        /// Initializes a new instance of the PropertyChangedEventListener class.
        /// </summary>
        /// <param name="handler">The handler for the event.</param>
        /// <param name="source">The source of the property.</param>
        public CollectionChangedEventListener( INotifyCollectionChanged source, NotifyCollectionChangedEventHandler handler )
        {
            if ( source == null )
                throw new ArgumentNullException( "source" );

            if ( handler == null )
                throw new ArgumentNullException( "handler" );

            _Source = source;
            _Handler = handler;
        }

        public INotifyCollectionChanged Source
        {
            get
            {
                return _Source;
            }
        }

        public NotifyCollectionChangedEventHandler Handler
        {
            get
            {
                return _Handler;
            }
        }

        /// <summary>
        /// Receives events from the centralized event manager.
        /// </summary>
        /// <param name="managerType">The type of the WeakEventManager calling this method.</param>
        /// <param name="sender">Object that originated the event.</param>
        /// <param name="e">Event data.</param>
        /// <returns>
        /// true if the listener handled the event. It is considered an error by the WeakEventManager handling in WPF 
        /// to register a listener for an event that the listener does not handle. 
        /// Regardless, the method should return false if it receives an event that it does not recognize or handle.
        /// </returns>
        public bool ReceiveWeakEvent( Type managerType, object sender, EventArgs e )
        {
            NotifyCollectionChangedEventArgs realArgs = (NotifyCollectionChangedEventArgs)e;
            _Handler( sender, realArgs );
            return true;
        }
    }
    #endregion
}
