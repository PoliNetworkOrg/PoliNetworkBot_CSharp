#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.Models
{
    public class InstaAccountConfirmEmail
    {
        [JsonProperty("is_email_legit")] public bool IsEmailLegit { get; set; }

        [JsonProperty("title")] public string Title { get; set; }

        [JsonProperty("body")] public string Body { get; set; }

        [JsonProperty("status")] internal string Status { get; set; }
    }
}