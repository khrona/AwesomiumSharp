Imports AwesomiumSharp
Imports System.Runtime.InteropServices
Imports System.Drawing.Imaging
Imports Microsoft.VisualBasic

Public Class Form1
    Private WithEvents m_WebView As WebView
    Private bitmap As Bitmap

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        Dim config As WebCoreConfig = New WebCoreConfig() With {.EnablePlugins = True, .AutoUpdate = True}

        WebCore.Initialize(config)
        m_WebView = WebCore.CreateWebview(webViewBitmap.Width, webViewBitmap.Height)
        m_WebView.LoadURL("http://www.google.com")
        m_WebView.Focus()
    End Sub

    Private Sub Render()
        Dim rBuffer As RenderBuffer = m_WebView.Render()
        Debug.Print(String.Format("VB: {0}", m_WebView.IsDirty))

        'Dim data(webViewBitmap.Width * webViewBitmap.Height) As Integer
        'Marshal.Copy(rBuffer.Buffer, data, 0, webViewBitmap.Width * webViewBitmap.Height)

        If bitmap Is Nothing Then
            bitmap = New Bitmap(webViewBitmap.Width, webViewBitmap.Height, PixelFormat.Format32bppArgb)
        ElseIf Not bitmap.Width.Equals(webViewBitmap.Width) Or Not bitmap.Height.Equals(webViewBitmap.Height) Then
            bitmap.Dispose()
            bitmap = New Bitmap(webViewBitmap.Width, webViewBitmap.Height, PixelFormat.Format32bppArgb)
        End If

        Dim bits As BitmapData = bitmap.LockBits(New Rectangle(0, 0, webViewBitmap.Width, webViewBitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat)

        Dim ptr As IntPtr = bits.Scan0.ToInt32()
        rBuffer.CopyTo(ptr, bits.Stride, 4, False)

        bitmap.UnlockBits(bits)

        webViewBitmap.Image = bitmap
    End Sub

    'Private Sub Timer1_Tick(ByVal sender As Object, ByVal e As EventArgs) Handles Timer1.Tick
    '    If WebCore.IsInitialized Then
    '        WebCore.Update()

    '        If m_WebView.IsDirty Then
    '            Render()
    '        End If
    '    End If
    'End Sub

    Public Sub webViewBitmap_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseDown, webViewBitmap.MouseDown
        m_WebView.InjectMouseDown(MouseButton.Left)
    End Sub

    Public Sub webViewBitmap_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseMove, webViewBitmap.MouseMove
        m_WebView.InjectMouseMove(e.X, e.Y)
    End Sub

    Public Sub webViewBitmap_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseUp, webViewBitmap.MouseUp
        m_WebView.InjectMouseUp(MouseButton.Left)
    End Sub

    Public Sub webViewBitmap_MouseWheel(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseWheel
        m_WebView.InjectMouseWheel(e.Delta)
    End Sub

    Public Sub Form1_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
        Dim keyEvent As WebKeyboardEvent = New WebKeyboardEvent() With {.Type = WebKeyType.KeyDown, .VirtualKeyCode = e.KeyCode}
        m_WebView.InjectKeyboardEvent(keyEvent)
    End Sub

    Private Sub Form1_KeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs) Handles MyBase.KeyPress
        Dim keyEvent As WebKeyboardEvent = New WebKeyboardEvent() With {.Type = WebKeyType.Char, .Text = New UShort() {Asc(e.KeyChar), 0, 0, 0}}
        m_WebView.InjectKeyboardEvent(keyEvent)
    End Sub

    Public Sub Form1_KeyUp(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyUp
        Dim keyEvent As WebKeyboardEvent = New WebKeyboardEvent() With {.Type = WebKeyType.KeyUp, .VirtualKeyCode = e.KeyCode}
        m_WebView.InjectKeyboardEvent(keyEvent)
    End Sub

    Private Sub Form1_SizeChanged(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.ResizeEnd
        m_WebView.Resize(webViewBitmap.Width, webViewBitmap.Height)
    End Sub

    Private Sub m_WebView_IsDirtyChanged(sender As Object, e As EventArgs) Handles m_WebView.IsDirtyChanged
        If m_WebView.IsDirty Then
            Render()
        End If
    End Sub
End Class
