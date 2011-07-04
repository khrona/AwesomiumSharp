using System;

#if USING_MONO
namespace AwesomiumMono
#else
namespace AwesomiumSharp
#endif
{
    public delegate void SelectLocalFilesEventHandler( object sender, SelectLocalFilesEventArgs e );

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
