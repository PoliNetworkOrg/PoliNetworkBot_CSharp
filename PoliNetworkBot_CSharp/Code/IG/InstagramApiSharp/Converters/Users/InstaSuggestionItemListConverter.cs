#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class
    InstaSuggestionItemListConverter : IObjectConverter<InstaSuggestionItemList, InstaSuggestionItemListResponse>
{
    public InstaSuggestionItemListResponse SourceObject { get; set; }

    public InstaSuggestionItemList Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var suggest = new InstaSuggestionItemList();

        if (SourceObject is not { Count: > 0 }) return suggest;
        foreach (var item in SourceObject)
            try
            {
                var convertedItem = ConvertersFabric.GetSuggestionItemConverter(item).Convert();
                suggest.Add(convertedItem);
            }
            catch
            {
            }

        return suggest;
    }
}