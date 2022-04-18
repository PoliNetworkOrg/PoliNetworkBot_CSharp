#region

using System;
using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.API;

#endregion

namespace Minista.Helpers;

internal class BackgroundUploader
{
    public readonly List<Tuple<string, string>> list = new();

    internal void SetRequestHeader(string v1, string v2)
    {
        list.Add(new Tuple<string, string>(v1, v2));
    }

    internal UploadOperation CreateUpload(Uri instaUri, StorageFile file, InstaApi instaApi)
    {
        return new UploadOperation(instaUri, file, this, instaApi);
    }
}