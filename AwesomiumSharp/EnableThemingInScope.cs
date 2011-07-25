using System;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Runtime.InteropServices;

namespace AwesomiumSharp
{
    /// <summary>
    /// This class is intended to be used with the 'using' statement,
    /// to activate an activation context for turning on visual theming at
    /// the beginning of a scope, and have it automatically deactivated
    /// when the scope is exited.
    /// </summary>
    /// <remarks>
    /// With a little bit of tuning, this old traditional model, still works even
    /// in Windows 7. In our case we need it to apply theming to the native
    /// Print dialog of the view. It may be useless in the future.
    /// </remarks>
    [SuppressUnmanagedCodeSecurity()]
    internal class EnableThemingInScope : IDisposable
    {
        #region  Fields
        private UInt32 activationContextCookie;
        private static ACTCTX enableThemingActivationContext;
        private static IntPtr hActCtx;
        private static bool contextCreationSucceeded;
        #endregion

        #region  Activation API
        [DllImport( "Kernel32.dll" )]
        private extern static IntPtr CreateActCtx( ref ACTCTX actctx );

        [DllImport( "Kernel32.dll" )]
        private extern static bool ActivateActCtx( IntPtr hActCtx, out UInt32 lpCookie );

        [DllImport( "Kernel32.dll" )]
        private extern static bool DeactivateActCtx( UInt32 dwFlags, UInt32 lpCookie );

        [DllImport( "Kernel32.dll" )]
        private extern static bool ReleaseActCtx( IntPtr hActCtx );

        [Flags()]
        private enum ACTCTX_FLAGS : int
        {
            ACTCTX_FLAG_PROCESSOR_ARCHITECTURE_VALID = 0X1,
            ACTCTX_FLAG_LANGID_VALID = 0X2,
            ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID = 0X4,
            ACTCTX_FLAG_RESOURCE_NAME_VALID = 0X8,
            ACTCTX_FLAG_SET_PROCESS_DEFAULT = 0X10,
            ACTCTX_FLAG_APPLICATION_NAME_VALID = 0X20,
            ACTCTX_FLAG_HMODULE_VALID = 0X80
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct ACTCTX
        {
            public int cbSize;
            public ACTCTX_FLAGS dwFlags;
            public string lpSource;
            public UInt16 wProcessorArchitecture;
            public UInt16 wLangId;
            public string lpAssemblyDirectory;
            public string lpResourceName;
            public string lpApplicationName;
        }
        #endregion

        #region  Constructor
        public EnableThemingInScope()
        {
            activationContextCookie = 0;

            // We used to check for the Windows version here.
            // Nowadays, we use .NET 4. This can't be installed to 
            // older than Windows XP versions. No need for a check.

            if ( EnsureActivateContextCreated() )
            {
                if ( !ActivateActCtx( hActCtx, out activationContextCookie ) )
                {
                    // Be sure cookie is always zero if activation failed.
                    activationContextCookie = 0;
                }
            }
        }
        #endregion

        #region  Methods
        private static bool EnsureActivateContextCreated()
        {
            lock ( typeof( EnableThemingInScope ) )
            {
                if ( !contextCreationSucceeded )
                {
                    // Pull manifest from the .NET Framework install directory.
                    string assemblyLoc = null;
                    FileIOPermission fiop = new FileIOPermission( PermissionState.None ) { AllFiles = FileIOPermissionAccess.PathDiscovery };
                    fiop.Assert();

                    try
                    {
                        assemblyLoc = typeof( Object ).Assembly.Location;
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }

                    string manifestLoc = null;
                    string installDir = null;

                    if ( assemblyLoc != null )
                    {
                        installDir = Path.GetDirectoryName( assemblyLoc );
                        const string manifestName = "XPThemes.manifest";
                        manifestLoc = Path.Combine( installDir, manifestName );
                    }

                    if ( manifestLoc != null && installDir != null )
                    {
                        enableThemingActivationContext = new ACTCTX();
                        enableThemingActivationContext.cbSize = Marshal.SizeOf( typeof( ACTCTX ) );
                        enableThemingActivationContext.lpSource = manifestLoc;

                        // Set the lpAssemblyDirectory to the install
                        // directory to prevent Win32 Side by Side from
                        // looking for comctl32 in the application
                        // directory, which could cause a bogus dll to be
                        // placed there and open a security hole.
                        enableThemingActivationContext.lpAssemblyDirectory = installDir;
                        enableThemingActivationContext.dwFlags = ACTCTX_FLAGS.ACTCTX_FLAG_ASSEMBLY_DIRECTORY_VALID;

                        // Note this will fail gracefully if file specified
                        // by manifestLoc doesn't exist.
                        hActCtx = CreateActCtx( ref enableThemingActivationContext );
                        contextCreationSucceeded = ( hActCtx != new IntPtr( -1 ) );
                    }
                }

                // If we return false, we'll try again on the next call into
                // EnsureActivateContextCreated(), which is fine.
                return contextCreationSucceeded;
            }
        }
        #endregion

        #region  IDisposable
        private void Dispose( bool disposing )
        {
            if ( activationContextCookie != 0 )
            {
                if ( DeactivateActCtx( 0, activationContextCookie ) )
                {
                    // deactivation succeeded...
                    activationContextCookie = 0;
                    ReleaseActCtx( hActCtx );
                }
            }
        }

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        ~EnableThemingInScope()
        {
            Dispose( false );
        }
        #endregion
    }
}
