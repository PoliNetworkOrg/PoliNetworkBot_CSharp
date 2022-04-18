#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Media;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Direct;

public class InstaInboxMedia
{
    public List<InstaImage> Images { get; set; } = new();
    public long OriginalWidth { get; set; }
    public long OriginalHeight { get; set; }
    public InstaMediaType MediaType { get; set; }
    public List<InstaVideo> Videos { get; set; } = new();
}