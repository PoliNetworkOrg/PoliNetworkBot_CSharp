#region

using System;
using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Media;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Direct;

public class InstaVisualMedia
{
    public long MediaId { get; set; }

    public string InstaIdentifier { get; set; }

    public InstaMediaType MediaType { get; set; }

    public List<InstaImage> Images { get; set; } = new();

    public List<InstaVideo> Videos { get; set; } = new();

    public string TrackingToken { get; set; }

    public int Width { get; set; }

    public int Height { get; set; }

    public DateTime UrlExpireAt { get; set; }

    public bool IsExpired => string.IsNullOrEmpty(InstaIdentifier);
}