#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaDiscoverTopSearchesConverter : IObjectConverter<InstaDiscoverTopSearches, InstaDiscoverTopSearchesResponse>
    {
        public InstaDiscoverTopSearchesResponse SourceObject { get; set; }

        public InstaDiscoverTopSearches Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var recents = new InstaDiscoverTopSearches();
            if (SourceObject.TopResults == null || !SourceObject.TopResults.Any()) return recents;
            foreach (var search in SourceObject.TopResults)
                try
                {
                    recents.TopResults.Add(ConvertersFabric.GetDiscoverSearchesConverter(search)
                        .Convert());
                }
                catch
                {
                }

            return recents;
        }
    }
}