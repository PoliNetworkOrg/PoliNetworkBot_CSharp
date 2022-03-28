﻿#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaBrandedContent
{
    public bool RequireApproval { get; set; }

    public List<InstaUserShort> WhitelistedUsers { get; set; } = new();
}