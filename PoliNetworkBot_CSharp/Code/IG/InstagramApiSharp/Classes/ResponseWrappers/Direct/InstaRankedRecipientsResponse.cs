#region

using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers;

public class InstaRankedRecipientsResponse : InstaRecipientsResponse, IInstaRecipientsResponse
{
    [JsonProperty("ranked_recipients")] public RankedRecipientResponse[] RankedRecipients { get; set; }
}