Imports AwesomiumSharp

Class MainWindow

#Region " Fields "
    Private m_HomePage As Uri = New Uri("http://www.google.com")
#End Region


#Region " Overrides "
    Protected Overrides Sub OnSourceInitialized(e As System.EventArgs)
        MyBase.OnSourceInitialized(e)

        If GlassUtilities.IsCompositionEnabled Then
            GlassUtilities.SetTransparentBackground(Me)
            GlassUtilities.ExtendGlassIntoClientArea(Me, New Thickness(0, 35, 0, 30))
        End If
    End Sub
#End Region

#Region " Event Handlers "
    Private Sub AddressBox_KeyDown(sender As Object, e As KeyEventArgs) Handles AddressBox.KeyDown
        If e.Key = Key.Return Then
            Dim bind As BindingExpression = BindingOperations.GetBindingExpression(AddressBox, TextBox.TextProperty)

            If bind IsNot Nothing Then
                bind.UpdateSource()
            End If
        End If
    End Sub

    Private Sub Browser_OpenExternalLink(sender As Object, e As OpenExternalLinkEventArgs) Handles Browser.OpenExternalLink
        ' Open in the same window.
        Browser.LoadURL(e.Url)
    End Sub

    Private Sub BrowseHomeCommandBinding_CanExecute(sender As Object, e As CanExecuteRoutedEventArgs) Handles BrowseHomeCommandBinding.CanExecute
        e.CanExecute = If(Browser IsNot Nothing, Browser.Source <> m_HomePage, False)
    End Sub

    Private Sub BrowseHomeCommandBinding_Executed(sender As Object, e As ExecutedRoutedEventArgs) Handles BrowseHomeCommandBinding.Executed
        Browser.Source = m_HomePage

        ' Test manually disabling...
        'Browser.IsEnabled = Not Browser.IsEnabled
        ' ...OR even destroying.
        'Browser.Close()
    End Sub
#End Region

End Class
