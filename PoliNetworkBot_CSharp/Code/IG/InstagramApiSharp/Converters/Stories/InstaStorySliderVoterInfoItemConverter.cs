#region

using System;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Converters;
using InstagramApiSharp.Helpers;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Story;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Converters.Stories;

internal class InstaStorySliderVoterInfoItemConverter : IObjectConverter<InstaStorySliderVoterInfoItem,
    InstaStorySliderVoterInfoItemResponse>
{
    public InstaStorySliderVoterInfoItemResponse SourceObject { get; set; }

    public InstaStorySliderVoterInfoItem Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");

        var voterInfoItem = new InstaStorySliderVoterInfoItem
        {
            LatestSliderVoteTime =
                (SourceObject.LatestSliderVoteTime ?? DateTime.Now.ToUnixTime()).FromUnixTimeSeconds(),
            MaxId = SourceObject.MaxId,
            MoreAvailable = SourceObject.MoreAvailable,
            SliderId = SourceObject.SliderId
        };

        if (!(SourceObject.Voters?.Count > 0)) return voterInfoItem;
        foreach (var voter in SourceObject.Voters)
            voterInfoItem.Voters.Add(ConvertersFabric.GetStoryPollVoterItemConverter(voter).Convert());

        return voterInfoItem;
    }
}