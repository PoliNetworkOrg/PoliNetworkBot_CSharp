#region

using InstagramApiSharp.Classes.ResponseWrappers;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes;

public class InstaChallengeRequireVerifyCode
{
    [JsonIgnore] public bool IsLoggedIn => LoggedInUser != null || Status.ToLower() == "ok";

    [JsonProperty("logged_in_user")] public InstaUserShortResponse LoggedInUser { get; set; }

    [JsonProperty("message")] public string? Message { get; set; }

    [JsonProperty("status")] internal string Status { get; set; }

    [JsonProperty("action")] internal string Action { get; set; }
}