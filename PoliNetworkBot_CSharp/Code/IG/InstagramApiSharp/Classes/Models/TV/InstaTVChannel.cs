#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Enums;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.TV;

public class InstaTvChannel
{
    public InstaTVChannelType Type { get; set; }

    public string Title { get; set; }

    public string Id { get; set; }

    public List<InstaMedia> Items { get; set; } = new();

    public bool HasMoreAvailable { get; set; }

    public string MaxId { get; set; }
    //public Seen_State1 seen_state { get; set; }

    public InstaTVUser UserDetail { get; set; }
}

public class InstaTvSelfChannel : InstaTvChannel
{
    public InstaTVUser User { get; set; }
}