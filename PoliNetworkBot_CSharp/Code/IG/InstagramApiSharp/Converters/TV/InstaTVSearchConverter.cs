#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaTVSearchConverter : IObjectConverter<InstaTVSearch, InstaTVSearchResponse>
{
    public InstaTVSearchResponse SourceObject { get; set; }

    public InstaTVSearch Convert()
    {
        if (SourceObject == null)
            throw new ArgumentNullException("SourceObject");

        var search = new InstaTVSearch
        {
            NumResults = SourceObject.NumResults ?? 0,
            Status = SourceObject.Status,
            RankToken = SourceObject.RankToken
        };

        if (SourceObject.Results == null || !SourceObject.Results.Any()) return search;
        foreach (var result in SourceObject.Results)
            try
            {
                search.Results.Add(ConvertersFabric.GetTvSearchResultConverter(result).Convert());
            }
            catch
            {
            }

        return search;
    }
}