#region

using System;
using System.Collections.Generic;
using Windows.Storage;

#endregion

namespace Minista.Helpers
{
    internal class BackgroundUploader
    {
        readonly List<Tuple<string, string>> list = new List<Tuple<string, string>>();

        internal void SetRequestHeader(string v1, string v2)
        {
            list.Add(new Tuple<string, string>(v1, v2));
        }

        internal UploadOperation CreateUpload(Uri instaUri, StorageFile file)
        {
            return new UploadOperation(instaUri, file, this);
        }
    }
}