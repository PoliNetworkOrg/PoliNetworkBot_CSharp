#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;
using System;

#endregion

namespace InstagramApiSharp.Converters;

internal class
    InstaStoryPollVotersListConverter : IObjectConverter<InstaStoryPollVotersList, InstaStoryPollVotersListResponse>
{
    public InstaStoryPollVotersListResponse SourceObject { get; set; }

    public InstaStoryPollVotersList Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");

        var votersList = new InstaStoryPollVotersList
        {
            LatestPollVoteTime = (SourceObject.LatestPollVoteTime ?? 0).FromUnixTimeSeconds(),
            MaxId = SourceObject.MaxId,
            MoreAvailable = SourceObject.MoreAvailable,
            PollId = SourceObject.PollId
        };

        if (!(SourceObject.Voters?.Count > 0)) return votersList;
        foreach (var voter in SourceObject.Voters)
            votersList.Voters.Add(ConvertersFabric.GetStoryPollVoterItemConverter(voter).Convert());

        return votersList;
    }
}