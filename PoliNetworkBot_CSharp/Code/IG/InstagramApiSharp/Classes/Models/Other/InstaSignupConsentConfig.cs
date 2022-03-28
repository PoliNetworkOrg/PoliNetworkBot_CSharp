#region

using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes;

public class InstaSignupConsentConfig : InstaDefaultResponse
{
    [JsonProperty("age_required")] public bool? AgeRequired { get; set; }

    [JsonProperty("gdpr_required")] public bool? GdprRequired { get; set; }

    [JsonProperty("tos_acceptance_not_required")]
    public bool? TosAcceptanceNotRequired { get; set; }
}