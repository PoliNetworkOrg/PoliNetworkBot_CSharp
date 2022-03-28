﻿#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.Models;

internal class InstaLoggedInChallengeDataInfoContainer
{
    [JsonProperty("step_name")] public string StepName { get; set; }

    [JsonProperty("step_data")] public InstaLoggedInChallengeDataInfo StepData { get; set; }

    [JsonProperty("status")] public string Status { get; set; }
}

public class InstaLoggedInChallengeDataInfo
{
    [JsonProperty("choice")] public string Choice { get; set; }

    [JsonProperty("country")] public string Country { get; set; }

    [JsonProperty("enrollment_time")] public long? EnrollmentTime { get; set; }

    [JsonProperty("enrollment_date")] public string EnrollmentDate { get; set; }

    [JsonProperty("latitude")] public float Latitude { get; set; }

    [JsonProperty("longitude")] public float Longitude { get; set; }

    [JsonProperty("city")] public string City { get; set; }

    [JsonProperty("platform")] public string Platform { get; set; }

    [JsonProperty("user_agent")] public string UserAgent { get; set; }
}