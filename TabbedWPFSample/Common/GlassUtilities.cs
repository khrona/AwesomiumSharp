using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace TabbedWPFSample
{
    static class GlassUtilities
    {
        [DllImport( "dwmapi.dll" )]
        private extern static void DwmIsCompositionEnabled( ref bool pfEnabled );
        [DllImport( "dwmapi.dll" )]
        private extern static void DwmExtendFrameIntoClientArea( IntPtr hWnd, ref MARGINS pMargins );
        [DllImport( "dwmapi.dll" )]
        private extern static void DwmEnableBlurBehindWindow( IntPtr hWnd, ref BLURBEHIND pBlurBehind );

        private const int DTT_COMPOSITED = 8192;
        private const int DTT_GLOWSIZE = 2048;
        private const int DTT_TEXTCOLOR = 1;

        private const int DWM_BB_ENABLE = 0X1;
        private const int DWM_BB_BLURREGION = 0X2;
        private const int DWM_BB_TRANSITIONONMAXIMIZED = 0X4;

        [StructLayout( LayoutKind.Sequential )]
        private struct MARGINS
        {
            public int left;
            public int right;
            public int top;
            public int bottom;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct BLURBEHIND
        {
            public int dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        public static bool IsCompositionEnabled
        {
            get
            {
                if ( Environment.OSVersion.Version.Major < 6 )
                {
                    return false;
                }

                bool compositionEnabled = false;
                DwmIsCompositionEnabled( ref compositionEnabled );
                return compositionEnabled;
            }
        }

        public static void SetTransparentBackground( Window w )
        {
            HwndSource source = (HwndSource)PresentationSource.FromVisual( w );
            w.Background = Brushes.Transparent;
            source.CompositionTarget.BackgroundColor = Color.FromArgb( 0, 0, 0, 0 );
        }

        public static void ExtendGlassIntoClientArea( Window w, Thickness glassFrameThickness )
        {
            MARGINS m = new MARGINS()
            {
                left = Convert.ToInt32( glassFrameThickness.Left ),
                right = Convert.ToInt32( glassFrameThickness.Right ),
                top = Convert.ToInt32( glassFrameThickness.Top ),
                bottom = Convert.ToInt32( glassFrameThickness.Bottom )
            };

            HwndSource source = (HwndSource)PresentationSource.FromVisual( w );

            DwmExtendFrameIntoClientArea( source.Handle, ref m );
        }

        public static void EnableBlurBehindWindow( Window w )
        {
            BLURBEHIND b = new BLURBEHIND() { dwFlags = DWM_BB_ENABLE | DWM_BB_TRANSITIONONMAXIMIZED, fEnable = true, hRgnBlur = IntPtr.Zero, fTransitionOnMaximized = true };
            HwndSource source = (HwndSource)PresentationSource.FromVisual( w );

            DwmEnableBlurBehindWindow( source.Handle, ref b );
        }
    }
}
