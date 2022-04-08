#region

using Newtonsoft.Json;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaUserTagListResponse
{
    [JsonProperty("in")] public List<InstaUserTagResponse> In { get; set; } = new();
}