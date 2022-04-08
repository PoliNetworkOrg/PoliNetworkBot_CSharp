#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;
using System;

#endregion

namespace InstagramApiSharp.Converters;

internal class
    InstaDiscoverSearchesConverter : IObjectConverter<InstaDiscoverSearches, InstaDiscoverSearchesResponse>
{
    public InstaDiscoverSearchesResponse SourceObject { get; set; }

    public InstaDiscoverSearches Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var searches = new InstaDiscoverSearches
        {
            ClientTime = DateTimeHelper.FromUnixTimeSeconds(SourceObject.ClientTime ?? 0),
            Position = SourceObject.Position,
            User = ConvertersFabric.GetUserConverter(SourceObject.User).Convert()
        };
        return searches;
    }
}