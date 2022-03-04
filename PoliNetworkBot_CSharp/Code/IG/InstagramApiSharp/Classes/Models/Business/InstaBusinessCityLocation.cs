﻿#region

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace InstagramApiSharp.Classes.Models.Business
{
    public class InstaBusinessCityLocation
    {
        [JsonProperty("__typename")] internal string TypeName { get; set; }

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }
    }

    public class InstaBusinessCityLocationList : List<InstaBusinessCityLocation>
    {
    }

    internal class InstaBusinessCityLocationContainer
    {
        [JsonExtensionData] internal IDictionary<string, JToken> Extras { get; set; }
    }
}