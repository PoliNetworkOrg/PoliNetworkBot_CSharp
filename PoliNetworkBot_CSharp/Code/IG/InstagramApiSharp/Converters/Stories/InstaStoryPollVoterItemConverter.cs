#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaStoryPollVoterItemConverter : IObjectConverter<InstaStoryVoterItem, InstaStoryVoterItemResponse>
    {
        public InstaStoryVoterItemResponse SourceObject { get; set; }

        public InstaStoryVoterItem Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var voterItem = new InstaStoryVoterItem
            {
                Vote = SourceObject.Vote ?? 0,
                Time = SourceObject.Ts.FromUnixTimeSeconds(),
                User = ConvertersFabric.Instance.GetUserShortFriendshipConverter(SourceObject.User).Convert()
            };

            return voterItem;
        }
    }
}