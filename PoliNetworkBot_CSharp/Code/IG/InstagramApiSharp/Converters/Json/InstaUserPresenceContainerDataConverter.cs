#region

using System;
using InstagramApiSharp.Classes.ResponseWrappers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace InstagramApiSharp.Converters.Json
{
    internal class InstaUserPresenceContainerDataConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(InstaUserPresenceContainerResponse);
        }

        public override object ReadJson(JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            var presence = token.ToObject<InstaUserPresenceContainerResponse>();

            var userPresenceRoot = token?.SelectToken("user_presence");
            var extras = userPresenceRoot.ToObject<InstaExtraResponse>();
            try
            {
                foreach (var (key, value) in extras.Extras)
                    try
                    {
                        var p = value.ToObject<InstaUserPresenceResponse>();
                        p.Pk = long.Parse(key);
                        presence.Items.Add(p);
                    }
                    catch
                    {
                    }
            }
            catch
            {
            }

            return presence;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}