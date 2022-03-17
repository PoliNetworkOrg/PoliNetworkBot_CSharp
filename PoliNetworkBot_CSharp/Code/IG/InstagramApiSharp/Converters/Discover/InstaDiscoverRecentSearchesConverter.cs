#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaDiscoverRecentSearchesConverter : IObjectConverter<InstaDiscoverRecentSearches,
            InstaDiscoverRecentSearchesResponse>
    {
        public InstaDiscoverRecentSearchesResponse SourceObject { get; set; }

        public InstaDiscoverRecentSearches Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var recents = new InstaDiscoverRecentSearches();
            if (SourceObject.Recent == null || !SourceObject.Recent.Any()) return recents;
            foreach (var search in SourceObject.Recent)
                try
                {
                    recents.Recent.Add(ConvertersFabric.Instance.GetDiscoverSearchesConverter(search).Convert());
                }
                catch
                {
                }

            return recents;
        }
    }
}