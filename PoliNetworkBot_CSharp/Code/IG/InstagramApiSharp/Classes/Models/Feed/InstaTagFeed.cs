#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Feed;

public class InstaTagFeed : InstaFeed
{
    public List<InstaMedia> RankedMedias { get; set; } = new();
}