/***************************************************************************
 *  Project: TabbedWPFSample
 *  File:    WPFSingleInstance.cs
 *  Version: 1.0.0.0
 *
 *  Copyright ©2010 Perikles C. Stephanidis; All rights reserved.
 *  This code is provided "AS IS" without warranty of any kind.
 *__________________________________________________________________________
 *
 *  Notes:
 *
 *  Utility class that ensures a single instance of a WPF application per 
 *  Windows user session.
 *   
 ***************************************************************************/

#region Using
using System;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Windows.Threading;
#endregion

namespace TabbedWPFSample
{
    #region SingleInstanceMode
    internal enum SingleInstanceMode : int
    {
        /// <summary>
        /// Do nothing.
        /// </summary>
        NotInited = 0,

        /// <summary>
        /// Every user can have own single instance.
        /// </summary>
        ForEveryUser
    }
    #endregion

    internal sealed class WPFSingleInstance
    {
        #region Fields
        private static DispatcherTimer AutoExitAplicationIfStartupDeadlock;
        private static Action<object> SecondInstanceCallback;
        #endregion


        #region Ctors
        private WPFSingleInstance()
        {
        }
        #endregion


        #region Methods
        /// <summary>
        /// Processing single instance with <see cref="SingleInstanceMode.ForEveryUser"/> mode.
        /// </summary>
        internal static void Make( Action<object> callback )
        {
            Make( SingleInstanceMode.ForEveryUser, callback );
        }

        /// <summary>
        /// Processing single instance.
        /// </summary>
        internal static void Make( SingleInstanceMode mode, Action<object> callback )
        {
            SecondInstanceCallback = callback;

#if DEBUG
            var appName = string.Format( "{0}DEBUG", Application.Current.GetType().Assembly.ManifestModule.ScopeName );
#else
		    var appName = Application.Current.GetType().Assembly.ManifestModule.ScopeName;
#endif

            var windowsIdentity = System.Security.Principal.WindowsIdentity.GetCurrent();
            var keyUserName = ( ( windowsIdentity != null ) ? windowsIdentity.User.ToString() : string.Empty );

            // Be careful! Max 260 chars!
            var eventWaitHandleName = string.Format( "{0}{1}", appName, ( ( mode == SingleInstanceMode.ForEveryUser ) ? keyUserName : string.Empty ) );

            try
            {
                using ( var waitHandle = EventWaitHandle.OpenExisting( eventWaitHandleName ) )
                {
                    // It informs first instance about other startup attempting.
                    waitHandle.Set();
                }

                // Let's terminate this posterior startup.
                // For that exit no interception.
                Environment.Exit( 0 );
            }
            catch
            {
                // It's first instance.
                // Register EventWaitHandle.
                using ( var eventWaitHandle = new EventWaitHandle( false, EventResetMode.AutoReset, eventWaitHandleName ) )
                {
                    ThreadPool.RegisterWaitForSingleObject( eventWaitHandle, OtherInstanceAttemptedToStart, null, Timeout.Infinite, false );
                }

                RemoveApplicationsStartupDeadlockForStartupCrushedWindows();
            }
        }
        #endregion

        #region Event Handlers
        private static void OtherInstanceAttemptedToStart( object state, bool timedOut )
        {
            RemoveApplicationsStartupDeadlockForStartupCrushedWindows();

            if ( SecondInstanceCallback != null )
                Application.Current.Dispatcher.BeginInvoke( SecondInstanceCallback, state );
        }

        private static void RemoveApplicationsStartupDeadlockForStartupCrushedWindows()
        {
            Application.Current.Dispatcher.BeginInvoke( (Action)( () =>
            {
                AutoExitAplicationIfStartupDeadlock =
                    new DispatcherTimer(
                        TimeSpan.FromSeconds( 6 ),
                        DispatcherPriority.ApplicationIdle,
                        ( sender, e ) =>
                        {
                            if ( ( Application.Current != null ) &&
                                Application.Current.Windows.Cast<Window>().Count( ( window ) => !( double.IsNaN( window.Left ) ) ) == 0 )
                            {
                                // For that exit no interception.
                                Environment.Exit( 0 );
                            }
                        },
                        Application.Current.Dispatcher );
            } ),
            DispatcherPriority.ApplicationIdle );
        }
        #endregion
    }
}
