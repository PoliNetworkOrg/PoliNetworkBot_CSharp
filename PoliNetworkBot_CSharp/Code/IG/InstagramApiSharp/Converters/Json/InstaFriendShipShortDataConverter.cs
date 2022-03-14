#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.ResponseWrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace InstagramApiSharp.Converters.Json
{
    internal class InstaFriendShipShortDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(InstaFriendshipShortStatusListResponse);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var root = JToken.Load(reader);
            var statusSubContainer = root["friendship_statuses"];
            var list = new InstaFriendshipShortStatusListResponse();
            var extras = statusSubContainer.ToObject<InstaExtraResponse>();

            if (extras is not { Extras: { } } || !extras.Extras.Any())
                return list;

            foreach (var (key, value) in extras.Extras)
                try
                {
                    var f = value.ToObject<InstaFriendshipShortStatusResponse>();
                    if (f == null) continue;

                    f.Pk = long.Parse(key);
                    list.Add(f);
                }
                catch
                {
                }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}