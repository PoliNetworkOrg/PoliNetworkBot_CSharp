﻿#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaBroadcastLiveHeartBeatViewerCountResponse
    {
        [JsonProperty("viewer_count")] public float ViewerCount { get; set; }

        [JsonProperty("broadcast_status")] public string BroadcastStatus { get; set; }

        [JsonProperty("cobroadcaster_ids")] public object[] CobroadcasterIds { get; set; }

        [JsonProperty("offset_to_video_start")]
        public int OffsetToVideoStart { get; set; }

        [JsonProperty("total_unique_viewer_count")]
        public int TotalUniqueViewerCount { get; set; }

        [JsonProperty("is_top_live_eligible")] public int IsTopLiveEligible { get; set; }

        [JsonProperty("status")] public string Status { get; set; }
    }
}