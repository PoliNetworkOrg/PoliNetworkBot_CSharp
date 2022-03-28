#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaHashtagSearchConverter : IObjectConverter<InstaHashtagSearch, InstaHashtagSearchResponse>
    {
        public InstaHashtagSearchResponse SourceObject { get; set; }

        public InstaHashtagSearch Convert()
        {
            if (SourceObject == null)
                throw new ArgumentNullException("Source object");

            var tags = new InstaHashtagSearch
            {
                MoreAvailable = SourceObject.MoreAvailable.GetValueOrDefault(false),
                RankToken = SourceObject.RankToken
            };

            tags.AddRange(SourceObject.Tags.Select(tag =>
                ConvertersFabric.GetHashTagConverter(tag).Convert()));

            return tags;
        }
    }
}