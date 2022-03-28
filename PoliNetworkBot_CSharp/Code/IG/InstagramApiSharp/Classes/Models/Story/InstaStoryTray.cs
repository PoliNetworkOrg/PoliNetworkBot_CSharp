#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaStoryTray
{
    public long Id { get; set; }

    public InstaTopLive TopLive { get; set; } = new();

    public bool IsPortrait { get; set; }

    public List<InstaStory> Tray { get; set; } = new();
}