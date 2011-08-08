Imports AwesomiumSharp
Imports AwesomiumSharp.Windows.Forms

Public Class Form1

#Region " Fields "
    Private WithEvents m_WebView As WebView

    Private rBuffer As RenderBuffer
    Private frameBuffer As Bitmap
    Private needsResize As Boolean
#End Region


#Region " Methods "
    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        If (m_WebView IsNot Nothing) AndAlso m_WebView.IsEnabled AndAlso m_WebView.IsDirty Then
            rBuffer = m_WebView.Render()
        End If

        If rBuffer IsNot Nothing Then
            Utilities.DrawBuffer(rBuffer, e.Graphics, Me.BackColor, frameBuffer)
        Else
            MyBase.OnPaint(e)
        End If
    End Sub

    Protected Overrides Sub OnResize(e As EventArgs)
        MyBase.OnResize(e)

        If (m_WebView Is Nothing) OrElse (Not m_WebView.IsEnabled) Then Return

        If (Me.ClientSize.Width <> 0) AndAlso (Me.ClientSize.Height <> 0) Then
            needsResize = True
        End If
    End Sub

    Protected Overrides Sub OnActivated(e As EventArgs)
        MyBase.OnActivated(e)

        If Not m_WebView.IsEnabled Then Return
        m_WebView.Focus()
    End Sub

    Protected Overrides Sub OnDeactivate(e As EventArgs)
        MyBase.OnDeactivate(e)

        If Not m_WebView.IsEnabled Then Return
        m_WebView.Unfocus()
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)

        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectMouseDown(MouseButton.Left)
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)

        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectMouseMove(e.X, e.Y)
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)

        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectMouseUp(MouseButton.Left)
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        MyBase.OnMouseWheel(e)

        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectMouseWheel(e.Delta)
    End Sub

    Protected Overrides Sub OnKeyDown(e As KeyEventArgs)
        MyBase.OnKeyDown(e)

        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectKeyboardEvent(Utilities.GetKeyboardEvent(WebKeyType.KeyDown, e))
    End Sub

    Protected Overrides Sub OnKeyUp(e As KeyEventArgs)
        MyBase.OnKeyUp(e)

        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectKeyboardEvent(Utilities.GetKeyboardEvent(WebKeyType.KeyUp, e))
    End Sub

    Protected Overrides Sub OnKeyPress(e As KeyPressEventArgs)
        MyBase.OnKeyPress(e)

        If Not m_WebView.IsEnabled Then Return
        m_WebView.InjectKeyboardEvent(Utilities.GetKeyboardEvent(e))
    End Sub

    Protected Overrides Sub OnFormClosed(e As FormClosedEventArgs)
        If m_WebView.IsEnabled Then
            m_WebView.Close()
            WebCore.Shutdown()
        End If

        MyBase.OnFormClosed(e)
    End Sub
#End Region

#Region " Event Handlers "
    Private Sub Form1_Load(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        Dim config As WebCoreConfig = New WebCoreConfig() With {.EnablePlugins = True}
        WebCore.Initialize(config)

        m_WebView = WebCore.CreateWebView(Me.ClientSize.Width, Me.ClientSize.Height)
        m_WebView.LoadURL("http://www.google.com")
        m_WebView.Focus()
    End Sub

    Private Sub m_WebView_CursorChanged(sender As Object, e As ChangeCursorEventArgs) Handles m_WebView.CursorChanged
        Cursor = Utilities.GetCursor(e.CursorType)
    End Sub

    Private Sub m_WebView_IsDirtyChanged(sender As Object, e As EventArgs) Handles m_WebView.IsDirtyChanged
        If needsResize AndAlso (Not m_WebView.IsDisposed) Then
            If (Not m_WebView.IsResizing) Then
                m_WebView.Resize(Me.ClientSize.Width, Me.ClientSize.Height)
                needsResize = False
            End If
        End If

        If m_WebView.IsDirty Then
            Me.Invalidate()
        End If
    End Sub

    Private Sub m_WebView_SelectLocalFiles(sender As Object, e As SelectLocalFilesEventArgs) Handles m_WebView.SelectLocalFiles
        Using dialog As New OpenFileDialog() With {
            .InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            .CheckFileExists = True,
            .Multiselect = e.SelectMultipleFiles}

            If dialog.ShowDialog(Me) = DialogResult.OK Then
                If dialog.FileNames.Length > 0 Then
                    e.SelectedFiles = dialog.FileNames
                End If
            End If
        End Using
    End Sub
#End Region

End Class
