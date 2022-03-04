﻿#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaDiscoverSearchResultConverter : IObjectConverter<InstaDiscoverSearchResult,
            InstaDiscoverSearchResultResponse>
    {
        public InstaDiscoverSearchResultResponse SourceObject { get; set; }

        public InstaDiscoverSearchResult Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var result = new InstaDiscoverSearchResult
            {
                HasMoreAvailable = SourceObject.HasMore ?? false,
                RankToken = SourceObject.RankToken,
                NumResults = SourceObject.NumResults ?? 0
            };
            if (SourceObject.Users != null && SourceObject.Users.Any())
                foreach (var user in SourceObject.Users)
                    try
                    {
                        result.Users.Add(ConvertersFabric.Instance.GetUserConverter(user).Convert());
                    }
                    catch
                    {
                    }

            return result;
        }
    }
}