#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaBlockedUsers : InstaDefault
{
    public List<InstaBlockedUserInfo> BlockedList { get; set; } = new();

    public int? PageSize { get; set; }

    public string? MaxId { get; set; }
}