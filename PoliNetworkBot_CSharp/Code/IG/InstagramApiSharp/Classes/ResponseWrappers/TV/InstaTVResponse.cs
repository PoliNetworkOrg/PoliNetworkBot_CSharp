#region

using System.Collections.Generic;
using Newtonsoft.Json;

#endregion

namespace InstagramApiSharp.Classes.ResponseWrappers
{
    public class InstaTVResponse
    {
        [JsonProperty("channels")] public List<InstaTVChannelResponse> Channels { get; set; }

        [JsonProperty("my_channel")] public InstaTVSelfChannelResponse MyChannel { get; set; }

        [JsonProperty("status")] internal string Status { get; set; }
        //public Badging badging { get; set; }
        //public Composer composer { get; set; }
    }
}