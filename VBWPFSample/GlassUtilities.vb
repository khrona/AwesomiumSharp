Imports System.Runtime.InteropServices
Imports System.Windows.Interop

Friend NotInheritable Class GlassUtilities

    <DllImport("dwmapi.dll")> _
    Private Shared Sub DwmIsCompositionEnabled(ByRef pfEnabled As Boolean)
    End Sub
    <DllImport("dwmapi.dll")> _
    Private Shared Sub DwmExtendFrameIntoClientArea(ByVal hWnd As IntPtr, ByRef pMargins As MARGINS)
    End Sub
    <DllImport("dwmapi.dll")> _
    Private Shared Sub DwmEnableBlurBehindWindow(ByVal hWnd As IntPtr, ByRef pBlurBehind As BLURBEHIND)
    End Sub

    Private Const DTT_COMPOSITED As Integer = 8192
    Private Const DTT_GLOWSIZE As Integer = 2048
    Private Const DTT_TEXTCOLOR As Integer = 1

    Private Const DWM_BB_ENABLE As Integer = &H1
    Private Const DWM_BB_BLURREGION As Integer = &H2
    Private Const DWM_BB_TRANSITIONONMAXIMIZED As Integer = &H4

    <StructLayout(LayoutKind.Sequential)> _
    Private Structure MARGINS
        Public left, right, top, bottom As Integer
    End Structure

    <StructLayout(LayoutKind.Sequential)> _
    Private Structure BLURBEHIND
        Public dwFlags As Integer
        Public fEnable As Boolean
        Public hRgnBlur As IntPtr
        Public fTransitionOnMaximized As Boolean
    End Structure

    Private Sub New()
    End Sub

    Public Shared ReadOnly Property IsCompositionEnabled() As Boolean
        Get
            If Environment.OSVersion.Version.Major < 6 Then
                Return False
            End If

            Dim compositionEnabled As Boolean = False
            DwmIsCompositionEnabled(compositionEnabled)
            Return compositionEnabled
        End Get
    End Property

    Public Shared Sub SetTransparentBackground(ByVal w As Window)
        Dim source As HwndSource = PresentationSource.FromVisual(w)
        w.Background = Brushes.Transparent
        source.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0)
    End Sub

    Public Shared Sub ExtendGlassIntoClientArea(ByVal w As Window, glassFrameThickness As Thickness)
        Dim m As New MARGINS() With {
            .left = CInt(glassFrameThickness.Left),
            .right = CInt(glassFrameThickness.Right),
            .top = CInt(glassFrameThickness.Top),
            .bottom = CInt(glassFrameThickness.Bottom)}
        Dim source As HwndSource = PresentationSource.FromVisual(w)

        DwmExtendFrameIntoClientArea(source.Handle, m)
    End Sub

    Public Shared Sub EnableBlurBehindWindow(ByVal w As Window)
        Dim b As New BLURBEHIND() With {.dwFlags = DWM_BB_ENABLE Or DWM_BB_TRANSITIONONMAXIMIZED, .fEnable = True, .hRgnBlur = IntPtr.Zero, .fTransitionOnMaximized = True}
        Dim source As HwndSource = PresentationSource.FromVisual(w)

        DwmEnableBlurBehindWindow(source.Handle, b)
    End Sub

End Class