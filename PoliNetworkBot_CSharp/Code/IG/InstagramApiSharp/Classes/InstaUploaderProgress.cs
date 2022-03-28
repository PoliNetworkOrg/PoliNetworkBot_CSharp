﻿#region

using InstagramApiSharp.Enums;

#endregion

namespace InstagramApiSharp.Classes;

public class InstaUploaderProgress
{
    public InstaUploadState UploadState { get; internal set; }

    //public long FileSize { get; internal set; }
    //public long UploadedBytes { get; internal set; }
    public string UploadId { get; internal set; }

    public string Caption { get; internal set; }
    public string Name { get; internal set; } = "Uploading single file";
}