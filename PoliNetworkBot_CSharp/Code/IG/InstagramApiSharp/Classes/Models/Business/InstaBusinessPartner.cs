#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.Models.Business
{
    internal class InstaBusinessPartnerContainer : InstaDefault
    {
        [JsonProperty("partners")] public InstaBusinessPartner[] Partners { get; set; }
    }

    public class InstaBusinessPartnersList : List<InstaBusinessPartner>
    {
    }

    public class InstaBusinessPartner
    {
        [JsonProperty("app_id")] public string AppId { get; set; }

        [JsonProperty("label")] public string Label { get; set; }

        [JsonProperty("partner_name")] public string PartnerName { get; set; }
    }
}