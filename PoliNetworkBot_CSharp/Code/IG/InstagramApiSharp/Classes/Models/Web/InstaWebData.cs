#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Web;

public class InstaWebData
{
    public string? MaxId { get; set; }

    public List<InstaWebDataItem> Items { get; set; } = new();
}