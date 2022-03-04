﻿#region

using System.Collections.Generic;
using InstagramApiSharp.Classes.Models;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaStoryPollVotersListContainerResponse : InstaDefault
    {
        [JsonProperty("voter_info")] public InstaStoryPollVotersListResponse VoterInfo { get; set; }
    }

    public class InstaStoryPollVotersListResponse
    {
        [JsonProperty("poll_id")] public long PollId { get; set; }

        [JsonProperty("voters")] public List<InstaStoryVoterItemResponse> Voters { get; set; } = new();

        [JsonProperty("max_id")] public string MaxId { get; set; }

        [JsonProperty("more_available")] public bool MoreAvailable { get; set; }

        [JsonProperty("latest_poll_vote_time")]
        public long? LatestPollVoteTime { get; set; }
    }
}