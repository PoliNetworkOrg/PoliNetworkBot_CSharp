#region

using InstagramApiSharp.Enums;
using System;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaVisualMediaContainer
{
    public DateTime UrlExpireAt { get; set; }

    public InstaVisualMedia Media { get; set; }

    public int? SeenCount { get; set; }

    public DateTime ReplayExpiringAtUs { get; set; }

    public InstaViewMode ViewMode { get; set; }

    public List<long> SeenUserIds { get; set; } = new();

    public bool IsExpired => Media is { IsExpired: true };
}