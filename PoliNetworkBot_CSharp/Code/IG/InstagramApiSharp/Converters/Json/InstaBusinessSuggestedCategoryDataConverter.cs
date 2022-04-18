#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models.Business;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace InstagramApiSharp.Converters.Json;

internal class InstaBusinessSuggestedCategoryDataConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(InstaBusinessSuggestedCategoryList);
    }

    public override object ReadJson(JsonReader reader,
        Type objectType,
        object existingValue,
        JsonSerializer serializer)
    {
        var token = JToken.Load(reader);
        var container = token.ToObject<InstaBusinessCategoryContainer>();
        var items = container.Extras.FirstOrDefault().Value["categories"];
        var categories = items.ToObject<InstaBusinessSuggestedCategoryList>();
        return categories;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        serializer.Serialize(writer, value);
    }
}