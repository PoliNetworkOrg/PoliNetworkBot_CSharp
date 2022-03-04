﻿#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaRecentRecipientsResponse : InstaRecipientsResponse, IInstaRecipientsResponse
    {
        [JsonProperty("recent_recipients")] public RankedRecipientResponse[] RankedRecipients { get; set; }
    }
}