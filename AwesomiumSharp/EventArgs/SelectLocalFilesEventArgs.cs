using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    /// <summary>
    /// Represents the method that will handle the <see cref="WebView.SelectLocalFiles"/> and 
    /// <see cref="Windows.Controls.WebControl.SelectLocalFiles"/> events.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">An <see cref="SelectLocalFilesEventArgs"/> that contains the event data.</param>
    public delegate void SelectLocalFilesEventHandler( object sender, SelectLocalFilesEventArgs e );

    /// <summary>
    /// Provides data for the <see cref="WebView.SelectLocalFiles"/> and <see cref="Windows.Controls.WebControl.SelectLocalFiles"/> events.
    /// </summary>
    public class SelectLocalFilesEventArgs : EventArgs
    {
        public SelectLocalFilesEventArgs( bool selectMultipleFiles, string title, string defaultPaths )
        {
            this.selectMultipleFiles = selectMultipleFiles;
            this.title = title;
            this.defaultPaths = defaultPaths;
        }

        private bool selectMultipleFiles;
        public bool SelectMultipleFiles
        {
            get
            {
                return selectMultipleFiles;
            }
        }
        private string title;
        public string Title
        {
            get
            {
                return title;
            }
        }
        private string defaultPaths;
        public string DefaultPaths
        {
            get
            {
                return defaultPaths;
            }
        }

        private string[] selectedFiles;
        public string[] SelectedFiles
        {
            get
            {
                return selectedFiles;
            }
            set
            {
                if ( selectedFiles == value )
                    return;

                selectedFiles = value;
            }
        }
    }
}
