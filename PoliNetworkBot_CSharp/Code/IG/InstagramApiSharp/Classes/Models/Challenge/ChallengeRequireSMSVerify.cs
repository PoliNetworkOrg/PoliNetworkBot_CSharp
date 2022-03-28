﻿#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes;

public class InstaChallengeRequireSMSVerify
{
    [JsonProperty("step_name")] public string StepName { get; set; }

    [JsonProperty("step_data")] public ChallengeRequireStepDataSMSVerify StepData { get; set; }

    [JsonProperty("user_id")] public long UserId { get; set; }

    [JsonProperty("nonce_code")] public string NonceCode { get; set; }

    [JsonProperty("status")] internal string Status { get; set; }

    [JsonProperty("message")] public string Message { get; set; }
}

public class ChallengeRequireStepDataSMSVerify
{
    [JsonProperty("security_code")] public string SecurityCode { get; set; }

    [JsonProperty("sms_resend_delay")] public int SmsResendDelay { get; set; }

    [JsonProperty("phone_number_preview")] public string PhoneNumberPreview { get; set; }

    [JsonProperty("resend_delay")] public int ResendDelay { get; set; }

    [JsonProperty("contact_point")] public string ContactPoint { get; set; }

    [JsonProperty("form_type")] public string FormType { get; set; }
}