Imports System.IO
Imports System.Net
Imports AwesomiumSharp
Imports System.Threading.Tasks

Class MainWindow

#Region " Fields "
    Private Const JS_FAVICON As String = "(function(){links = document.getElementsByTagName('link'); wHref=window.location.protocol + '//' + window.location.hostname + '/favicon.ico'; for(i=0; i<links.length; i++){s=links[i].rel; if(s.indexOf('icon') != -1){ wHref = links[i].href }; }; return wHref; })();"

    Private m_HomePage As Uri = New Uri("http://www.google.com")
    Private m_Icon As ImageSource
#End Region


#Region " Constructors "
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_Icon = New ImageSourceConverter().ConvertFromString("pack://application:,,,/VBWPFSample;component/Awesomium.ico")
    End Sub
#End Region


#Region " Overrides "
    Protected Overrides Sub OnSourceInitialized(e As EventArgs)
        MyBase.OnSourceInitialized(e)

        If GlassUtilities.IsCompositionEnabled Then
            GlassUtilities.SetTransparentBackground(Me)
            GlassUtilities.ExtendGlassIntoClientArea(Me, New Thickness(0, 35, 0, 30))
        End If
    End Sub

    Protected Overrides Sub OnClosed(e As EventArgs)
        Browser.Close()
        WebCore.Shutdown()

        MyBase.OnClosed(e)
    End Sub
#End Region

#Region " Methods "
    Private Sub UpdateFavicon()
        Dim val As JSValue = Browser.ExecuteJavascriptWithResult(JS_FAVICON)

        If (val IsNot Nothing) AndAlso (val.Type = JSValueType.String) Then
            Task.Factory.StartNew(AddressOf GetFavicon, val.ToString()).ContinueWith(
                Sub(t)
                    If t.Exception Is Nothing Then
                        Me.Icon = If(t.Result, m_Icon)
                    End If
                End Sub,
                TaskScheduler.FromCurrentSynchronizationContext())
        End If
    End Sub

    Private Sub RestoreFavicon()
        Dim oldImage As BitmapImage = TryCast(Me.Icon, BitmapImage)

        Me.Icon = m_Icon

        If oldImage IsNot Nothing Then
            oldImage.StreamSource.Close()
        End If

        GC.Collect()
    End Sub

    Private Shared Function GetFavicon(href As Object) As BitmapImage
        Using client As New WebClient()
            Dim data As Byte() = client.DownloadData(CStr(href))
            Dim stream As New MemoryStream(data)
            Dim bitmap As New BitmapImage()

            With bitmap
                .BeginInit()
                .StreamSource = stream
                .EndInit()
                .Freeze()
            End With

            Return bitmap
        End Using
    End Function
#End Region

#Region " Event Handlers "
    Private Sub MainWindow_Loaded(sender As Object, e As RoutedEventArgs) Handles Me.Loaded
        If WebCore.IsRunning Then
            WebCore.HomeURL = "http://www.google.com"
        End If
    End Sub

    Private Sub AddressBox_GotKeyboardFocus(sender As Object, e As KeyboardFocusChangedEventArgs) Handles AddressBox.GotKeyboardFocus
        AddressBox.SelectAll()
    End Sub

    Private Sub AddressBox_PreviewMouseLeftButtonDown(sender As Object, e As MouseButtonEventArgs) Handles AddressBox.PreviewMouseLeftButtonDown
        If Not AddressBox.IsKeyboardFocusWithin Then
            AddressBox.Focus()
            e.Handled = True
        End If
    End Sub

    Private Sub AddressBox_KeyDown(sender As Object, e As KeyEventArgs) Handles AddressBox.KeyDown
        If e.Key = Key.Return Then
            Dim bind As BindingExpression = BindingOperations.GetBindingExpression(AddressBox, TextBox.TextProperty)

            If bind IsNot Nothing Then
                bind.UpdateSource()
            End If
        End If
    End Sub

    Private Sub Browser_BeginLoading(sender As Object, e As BeginLoadingEventArgs) Handles Browser.BeginLoading
        RestoreFavicon()
    End Sub

    Private Sub Browser_DomReady(sender As Object, e As EventArgs) Handles Browser.DomReady
        UpdateFavicon()
    End Sub

    Private Sub Browser_OpenExternalLink(sender As Object, e As OpenExternalLinkEventArgs) Handles Browser.OpenExternalLink
        ' Open in the same window.
        Browser.LoadURL(e.Url)
    End Sub
#End Region

End Class
