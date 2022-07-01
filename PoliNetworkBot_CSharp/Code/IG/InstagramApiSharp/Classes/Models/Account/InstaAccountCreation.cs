#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes;

public class InstaAccountCreation : InstaDefaultResponse
{
    [JsonProperty("account_created")] public bool AccountCreated { get; set; }

    [JsonProperty("created_user")] public InstaUserShortResponse CreatedUser { get; set; }
}

internal class InstaAccountCreationResponse : InstaAccountCreation
{
    [JsonProperty("error_type")] public string ErrorType { get; set; }

    [JsonProperty("errors")] public InstaAccountCreationErrors Errors { get; set; }
}

public class InstaAccountCreationErrors
{
    [JsonProperty("username")] public string?[] Username { get; set; }
}