#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaTV
{
    public List<InstaTVChannel> Channels { get; set; } = new();

    public InstaTVSelfChannel MyChannel { get; set; }

    internal string Status { get; set; }
}