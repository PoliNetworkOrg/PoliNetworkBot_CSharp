#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaWebData
{
    public string MaxId { get; set; }

    public List<InstaWebDataItem> Items { get; set; } = new();
}