#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers.Web;

public class InstaWebContainerResponse
{
    [JsonProperty("country_code")] public string CountryCode { get; set; }

    [JsonProperty("language_code")] public string LanguageCode { get; set; }

    [JsonProperty("locale")] public string Locale { get; set; }

    [JsonProperty("config")] public InstaWebConfigResponse Config { get; set; }

    [JsonProperty("entry_data")] public InstaWebEntryDataResponse Entry { get; set; }
}

public class InstaWebConfigResponse
{
    [JsonProperty("viewer")] public InstaUserShortResponse Viewer { get; set; }
}

public class InstaWebEntryDataResponse
{
    [JsonProperty("SettingsPages")] public List<InstaWebSettingsPageResponse> SettingsPages { get; set; } = new();
}

public class InstaWebSettingsPageResponse
{
    [JsonProperty("is_blocked")] public bool? IsBlocked { get; set; }

    [JsonProperty("page_name")] public string PageName { get; set; }

    [JsonProperty("date_joined")] public InstaWebDataResponse DateJoined { get; set; }

    [JsonProperty("switched_to_business")] public InstaWebDataResponse SwitchedToBusiness { get; set; }

    [JsonProperty("data")] public InstaWebDataListResponse Data { get; set; }
}

public class InstaWebDataResponse
{
    [JsonProperty("link")] public object Link { get; set; }

    [JsonProperty("data")] public InstaWebDataItemResponse Data { get; set; }

    [JsonProperty("cursor")] public string Cursor { get; set; }
}

public class InstaWebDataListResponse
{
    [JsonProperty("link")] public object Link { get; set; }

    [JsonProperty("data")] public List<InstaWebDataItemResponse> Data { get; set; } = new();

    [JsonProperty("cursor")] public string Cursor { get; set; }
}

public class InstaWebDataItemResponse
{
    [JsonProperty("text")] public string Text { get; set; }

    [JsonProperty("timestamp")] public long? Timestamp { get; set; }
}