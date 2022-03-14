#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

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
                ConvertersFabric.Instance.GetHashTagConverter(tag).Convert()));

            return tags;
        }
    }
}