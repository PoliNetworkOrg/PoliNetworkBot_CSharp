#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaSuggestionUserContainerResponse
    {
        [JsonProperty("more_available")] public bool MoreAvailable { get; set; }

        [JsonProperty("max_id")] public string MaxId { get; set; }

        [JsonProperty("suggested_users")] public InstaSuggestionResponse SuggestedUsers { get; set; } = new();

        [JsonProperty("new_suggested_users")] public InstaSuggestionResponse NewSuggestedUsers { get; set; } = new();

        [JsonProperty("status")] public string Status { get; set; }
    }

    public class InstaSuggestionResponse
    {
        [JsonProperty("suggestions")] public InstaSuggestionItemListResponse Suggestions { get; set; } = new();
    }
}