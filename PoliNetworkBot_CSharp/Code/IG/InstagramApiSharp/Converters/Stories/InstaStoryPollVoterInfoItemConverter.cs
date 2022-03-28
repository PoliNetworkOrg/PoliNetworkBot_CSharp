#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaStoryPollVoterInfoItemConverter : IObjectConverter<InstaStoryPollVoterInfoItem,
            InstaStoryPollVoterInfoItemResponse>
    {
        public InstaStoryPollVoterInfoItemResponse SourceObject { get; set; }

        public InstaStoryPollVoterInfoItem Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var voterInfoItem = new InstaStoryPollVoterInfoItem
            {
                LatestPollVoteTime =
                    (SourceObject.LatestPollVoteTime ?? DateTime.Now.ToUnixTime()).FromUnixTimeSeconds(),
                MaxId = SourceObject.MaxId,
                MoreAvailable = SourceObject.MoreAvailable,
                PollId = SourceObject.PollId
            };

            if (!(SourceObject.Voters?.Count > 0)) return voterInfoItem;
            foreach (var voter in SourceObject.Voters)
                voterInfoItem.Voters.Add(ConvertersFabric.GetStoryPollVoterItemConverter(voter).Convert());

            return voterInfoItem;
        }
    }
}