#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaTranslateResponse
{
    [JsonProperty("id")] public long Id { get; set; }

    [JsonProperty("translation")] public string Translation { get; set; }
}

public class InstaTranslateContainerResponse : InstaDefault
{
    [JsonProperty("comment_translations")] public List<InstaTranslateResponse> Translations { get; set; }
}