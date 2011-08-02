Imports AwesomiumSharp
Imports System.Drawing.Imaging
Imports AwesomiumSharp.Windows.Forms

Public Class Form1
    Private WithEvents m_WebView As WebView
    Private bitmap As Bitmap

    Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        Dim config As WebCoreConfig = New WebCoreConfig() With {.EnablePlugins = True}

        WebCore.Initialize(config)
        m_WebView = WebCore.CreateWebView(webViewBitmap.Width, webViewBitmap.Height)
        m_WebView.LoadURL("http://www.google.com")
        m_WebView.Focus()
    End Sub

    Private Sub Render()
        If Not m_WebView.IsEnabled Then Return

        Dim rBuffer As RenderBuffer = m_WebView.Render()

        'Dim data(webViewBitmap.Width * webViewBitmap.Height) As Integer
        'Marshal.Copy(rBuffer.Buffer, data, 0, webViewBitmap.Width * webViewBitmap.Height)

        If bitmap Is Nothing Then
            bitmap = New Bitmap(webViewBitmap.Width, webViewBitmap.Height, PixelFormat.Format32bppArgb)

        ElseIf (Not bitmap.Width.Equals(webViewBitmap.Width)) OrElse (Not bitmap.Height.Equals(webViewBitmap.Height)) Then
            bitmap.Dispose()
            bitmap = New Bitmap(webViewBitmap.Width, webViewBitmap.Height, PixelFormat.Format32bppArgb)

        End If

        Dim bits As BitmapData = bitmap.LockBits(
            New Rectangle(0, 0, webViewBitmap.Width, webViewBitmap.Height),
            ImageLockMode.ReadWrite,
            bitmap.PixelFormat)

        Dim ptr As IntPtr = bits.Scan0.ToInt32()

        rBuffer.CopyTo(ptr, bits.Stride, 4)

        bitmap.UnlockBits(bits)

        webViewBitmap.Image = bitmap
    End Sub

    Public Sub webViewBitmap_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseDown, webViewBitmap.MouseDown
        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectMouseDown(MouseButton.Left)
    End Sub

    Public Sub webViewBitmap_MouseMove(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseMove, webViewBitmap.MouseMove
        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectMouseMove(e.X, e.Y)
    End Sub

    Public Sub webViewBitmap_MouseUp(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseUp, webViewBitmap.MouseUp
        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectMouseUp(MouseButton.Left)
    End Sub

    Public Sub webViewBitmap_MouseWheel(ByVal sender As Object, ByVal e As MouseEventArgs) Handles MyBase.MouseWheel
        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectMouseWheel(e.Delta)
    End Sub

    Public Sub Form1_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyDown
        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectKeyboardEvent(Utilities.GetKeyboardEvent(WebKeyType.KeyDown, e))
    End Sub

    Private Sub Form1_KeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs) Handles MyBase.KeyPress
        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectKeyboardEvent(Utilities.GetKeyboardEvent(e))
    End Sub

    Public Sub Form1_KeyUp(ByVal sender As Object, ByVal e As KeyEventArgs) Handles MyBase.KeyUp
        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectKeyboardEvent(Utilities.GetKeyboardEvent(WebKeyType.KeyUp, e))
    End Sub

    Private Sub Form1_SizeChanged(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.ResizeEnd
        If Not m_WebView.IsEnabled Then Return
        m_WebView.Resize(webViewBitmap.Width, webViewBitmap.Height)
    End Sub

    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles Me.FormClosed
        If Not m_WebView.IsEnabled Then Return
        m_WebView.Close()
    End Sub

    Private Sub m_WebView_CursorChanged(sender As Object, e As ChangeCursorEventArgs) Handles m_WebView.CursorChanged
        Cursor = Utilities.GetCursor(e.CursorType)
    End Sub

    Private Sub m_WebView_IsDirtyChanged(sender As Object, e As EventArgs) Handles m_WebView.IsDirtyChanged
        If m_WebView.IsDirty Then
            Render()
        End If
    End Sub

    Private Sub m_WebView_SelectLocalFiles(sender As Object, e As SelectLocalFilesEventArgs) Handles m_WebView.SelectLocalFiles
        Using dialog As New OpenFileDialog() With {
            .InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            .CheckFileExists = True,
            .Multiselect = e.SelectMultipleFiles}

            If dialog.ShowDialog(Me) = System.Windows.Forms.DialogResult.OK Then
                If dialog.FileNames.Length > 0 Then
                    e.SelectedFiles = dialog.FileNames
                End If
            End If
        End Using
    End Sub

End Class
