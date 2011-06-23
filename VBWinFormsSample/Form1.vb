Imports AwesomiumSharp
Imports System.Runtime.InteropServices
Imports System.Drawing.Imaging
Imports Microsoft.VisualBasic

Public Class Form1
    Dim webView As WebView
    Dim bitmap As Bitmap

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Dim config As WebCore.Config = New WebCore.Config()
        config.enablePlugins = True

        WebCore.Initialize(config)
        webView = WebCore.CreateWebview(webViewBitmap.Width, webViewBitmap.Height)
        webView.LoadURL("http://www.google.com")
        webView.Focus()
    End Sub

    Private Sub render()
        Dim rBuffer As RenderBuffer = webView.Render()

        Dim data(webViewBitmap.Width * webViewBitmap.Height) As Integer
        Marshal.Copy(rBuffer.GetBuffer(), data, 0, webViewBitmap.Width * webViewBitmap.Height)

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

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        WebCore.Update()
        If webView.IsDirty Then
            render()
        End If
    End Sub

    Public Sub webViewBitmap_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseDown, webViewBitmap.MouseDown
        webView.InjectMouseDown(MouseButton.Left)
    End Sub

    Public Sub webViewBitmap_MouseMove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseMove, webViewBitmap.MouseMove
        webView.InjectMouseMove(e.X, e.Y)
    End Sub

    Public Sub webViewBitmap_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseUp, webViewBitmap.MouseUp
        webView.InjectMouseUp(MouseButton.Left)
    End Sub

    Public Sub webViewBitmap_MouseWheel(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseWheel
        webView.InjectMouseWheel(e.Delta)
    End Sub

    Public Sub Form1_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyDown
        Dim keyEvent As WebKeyboardEvent = New WebKeyboardEvent
        keyEvent.type = WebKeyType.KeyDown
        keyEvent.virtualKeyCode = e.KeyCode
        webView.InjectKeyboardEvent(keyEvent)
    End Sub

    Private Sub Form1_KeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs) Handles MyBase.KeyPress
        Dim keyEvent As WebKeyboardEvent = New WebKeyboardEvent
        keyEvent.type = WebKeyType.Char
        keyEvent.text = New UShort() {Asc(e.KeyChar), 0, 0, 0}
        webView.InjectKeyboardEvent(keyEvent)
    End Sub

    Public Sub Form1_KeyUp(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles MyBase.KeyUp
        Dim keyEvent As WebKeyboardEvent = New WebKeyboardEvent
        keyEvent.type = WebKeyType.KeyUp
        keyEvent.virtualKeyCode = e.KeyCode
        webView.InjectKeyboardEvent(keyEvent)
    End Sub

    Private Sub Form1_SizeChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.ResizeEnd
        webView.Resize(webViewBitmap.Width, webViewBitmap.Height)
    End Sub
End Class
