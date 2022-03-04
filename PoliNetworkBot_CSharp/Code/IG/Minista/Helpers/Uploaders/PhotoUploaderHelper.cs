#region

using System;
using System.Collections.Generic;
using System.Threading;
using Windows.Storage;
using InstagramApiSharp.Classes.Models;

#endregion

namespace Minista.Helpers
{
    internal class PhotoUploaderHelper
    {
        private CancellationTokenSource cts;


        private StorageFile NotifyFile;
        private List<InstaUserTagUpload> UserTags = new();

        public PhotoUploaderHelper()
        {
            Name = Guid.NewGuid().ToString();
        }

        public string Name { get; }
    }
}