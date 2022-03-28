#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaTVSearchResultConverter : IObjectConverter<InstaTVSearchResult, InstaTVSearchResultResponse>
    {
        public InstaTVSearchResultResponse SourceObject { get; set; }

        public InstaTVSearchResult Convert()
        {
            if (SourceObject == null)
                throw new ArgumentNullException("SourceObject");

            var search = new InstaTVSearchResult
            {
                Type = SourceObject.Type
            };

            if (SourceObject.Channel != null)
                try
                {
                    search.Channel = ConvertersFabric.GetTvChannelConverter(SourceObject.Channel).Convert();
                }
                catch
                {
                }

            if (SourceObject.User == null) return search;
            try
            {
                search.User = ConvertersFabric.GetUserShortFriendshipConverter(SourceObject.User)
                    .Convert();
            }
            catch
            {
            }

            return search;
        }
    }
}