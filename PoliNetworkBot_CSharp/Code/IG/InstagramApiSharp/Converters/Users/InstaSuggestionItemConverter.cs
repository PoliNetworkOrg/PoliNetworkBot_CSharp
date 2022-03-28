#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaSuggestionItemConverter : IObjectConverter<InstaSuggestionItem, InstaSuggestionItemResponse>
{
    public InstaSuggestionItemResponse SourceObject { get; set; }

    public InstaSuggestionItem Convert()
    {
        var suggestion = new InstaSuggestionItem
        {
            Caption = SourceObject.Caption ?? string.Empty,
            IsNewSuggestion = SourceObject.IsNewSuggestion,
            SocialContext = SourceObject.SocialContext ?? string.Empty,
            User = ConvertersFabric.GetUserShortConverter(SourceObject.User).Convert(),
            Algorithm = SourceObject.Algorithm ?? string.Empty,
            Icon = SourceObject.Icon ?? string.Empty,
            Value = SourceObject.Value ?? 0,
            Uuid = SourceObject.Uuid
        };
        try
        {
            if (SourceObject.LargeUrls is { Length: > 0 })
                foreach (var url in SourceObject.LargeUrls)
                    suggestion.LargeUrls.Add(url);
            if (SourceObject.MediaIds is { Length: > 0 })
                foreach (var url in SourceObject.MediaIds)
                    suggestion.MediaIds.Add(url);
            if (SourceObject.ThumbnailUrls is { Length: > 0 })
                foreach (var url in SourceObject.ThumbnailUrls)
                    suggestion.ThumbnailUrls.Add(url);
            if (SourceObject.MediaInfos is { Count: > 0 })
                foreach (var item in SourceObject.MediaInfos)
                    try
                    {
                        var converted = ConvertersFabric.GetSingleMediaConverter(item).Convert();
                        suggestion.MediaInfos.Add(converted);
                    }
                    catch
                    {
                    }
        }
        catch
        {
        }

        return suggestion;
    }
}