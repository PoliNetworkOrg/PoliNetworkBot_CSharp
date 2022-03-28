#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaTagFeed : InstaFeed
{
    public List<InstaMedia> RankedMedias { get; set; } = new();
}