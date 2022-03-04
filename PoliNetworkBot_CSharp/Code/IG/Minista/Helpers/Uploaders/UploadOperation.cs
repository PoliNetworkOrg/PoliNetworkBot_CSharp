#region

using System;
using Windows.Storage;

#endregion

namespace Minista.Helpers
{
    internal class UploadOperation
    {
        internal string Guid;
        private readonly Uri instaUri;
        private StorageFile file;
        private BackgroundUploader backgroundUploader;

        public UploadOperation(Uri instaUri, StorageFile file, BackgroundUploader backgroundUploader)
        {
            this.instaUri = instaUri;
            this.file = file;
            this.backgroundUploader = backgroundUploader;
        }

        internal void Start()
        {
            throw new NotImplementedException();
        }
    }
}