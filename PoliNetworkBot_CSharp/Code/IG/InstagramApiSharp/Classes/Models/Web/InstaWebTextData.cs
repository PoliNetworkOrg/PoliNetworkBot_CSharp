#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaWebTextData
{
    public string MaxId { get; set; }

    public List<string> Items { get; set; } = new();
}