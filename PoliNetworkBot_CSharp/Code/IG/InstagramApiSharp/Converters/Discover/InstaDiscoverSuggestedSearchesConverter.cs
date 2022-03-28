#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaDiscoverSuggestedSearchesConverter :
    IObjectConverter<InstaDiscoverSuggestedSearches, InstaDiscoverSuggestedSearchesResponse>
{
    public InstaDiscoverSuggestedSearchesResponse SourceObject { get; set; }

    public InstaDiscoverSuggestedSearches Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var suggested = new InstaDiscoverSuggestedSearches
        {
            RankToken = SourceObject.RankToken
        };
        if (SourceObject.Suggested == null || !SourceObject.Suggested.Any()) return suggested;
        foreach (var search in SourceObject.Suggested)
            try
            {
                suggested.Suggested.Add(
                    ConvertersFabric.GetDiscoverSearchesConverter(search).Convert());
            }
            catch
            {
            }

        return suggested;
    }
}