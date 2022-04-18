#region

using System.Collections.Generic;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.TV;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaTV
{
    public List<InstaTvChannel> Channels { get; set; } = new();

    public InstaTvSelfChannel MyChannel { get; set; }

    internal string Status { get; set; }
}