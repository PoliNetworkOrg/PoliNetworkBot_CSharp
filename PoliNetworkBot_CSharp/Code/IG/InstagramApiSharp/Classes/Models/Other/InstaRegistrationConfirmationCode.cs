#region

using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes
{
    public class InstaRegistrationConfirmationCode : InstaDefaultResponse
    {
        [JsonProperty("signup_code")] public string SignupCode { get; set; }
    }
}