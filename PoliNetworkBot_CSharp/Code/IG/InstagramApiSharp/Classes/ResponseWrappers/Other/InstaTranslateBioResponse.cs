#region

using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaTranslateBioResponse : InstaDefault
    {
        [JsonProperty("translation")] public string Translation { get; set; }
    }
}