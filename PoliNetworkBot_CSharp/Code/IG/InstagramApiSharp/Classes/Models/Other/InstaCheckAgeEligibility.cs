#region

using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes
{
    public class InstaCheckAgeEligibility : InstaDefaultResponse
    {
        [JsonProperty("eligible_to_register")] public bool? EligibleToRegister { get; set; }

        [JsonProperty("parental_consent_required")]
        public bool? ParentalConsentRequired { get; set; }
    }
}