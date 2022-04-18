#region

using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Broadcast;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaStoryTray
{
    public long Id { get; set; }

    public InstaTopLive TopLive { get; set; } = new();

    public bool IsPortrait { get; set; }

    public List<InstaStory> Tray { get; set; } = new();
}