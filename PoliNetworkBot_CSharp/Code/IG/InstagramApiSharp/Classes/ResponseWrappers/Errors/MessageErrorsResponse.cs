#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class MessageErrorsResponse
    {
        [JsonProperty("errors")] public List<string> Errors { get; set; }
    }
}