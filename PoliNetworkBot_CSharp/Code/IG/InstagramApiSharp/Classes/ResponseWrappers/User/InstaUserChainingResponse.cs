#region

using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaUserChainingContainerResponse : InstaDefault
    {
        [JsonProperty("is_backup")] public bool IsBackup { get; set; }

        [JsonProperty("users")] public List<InstaUserChainingResponse> Users { get; set; }
    }

    public class InstaUserChainingResponse : InstaUserShortResponse
    {
        [JsonProperty("chaining_info")] public InstaUserChainingInfoResponse ChainingInfo { get; set; }

        [JsonProperty("profile_chaining_secondary_label")]
        public string ProfileChainingSecondaryLabel { get; set; }
    }

    public class InstaUserChainingInfoResponse
    {
        public string Sources { get; set; }
    }
}