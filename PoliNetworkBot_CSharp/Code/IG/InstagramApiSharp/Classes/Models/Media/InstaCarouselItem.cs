#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaCarouselItem
{
    public string InstaIdentifier { get; set; }

    public InstaMediaType MediaType { get; set; }

    public List<InstaImage> Images { get; set; } = new();

    public List<InstaVideo> Videos { get; set; } = new();

    public int Width { get; set; }

    public int Height { get; set; }

    public string Pk { get; set; }

    public string CarouselParentId { get; set; }

    public List<InstaUserTag> UserTags { get; set; } = new();
}