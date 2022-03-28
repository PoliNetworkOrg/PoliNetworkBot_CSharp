#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaStoryPollStickerItemConverter : IObjectConverter<InstaStoryPollStickerItem,
            InstaStoryPollStickerItemResponse>
    {
        public InstaStoryPollStickerItemResponse SourceObject { get; set; }

        public InstaStoryPollStickerItem Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var pollSticker = new InstaStoryPollStickerItem
            {
                Finished = SourceObject.Finished,
                Id = SourceObject.Id,
                IsSharedResult = SourceObject.IsSharedResult,
                PollId = SourceObject.PollId,
                Question = SourceObject.Question,
                ViewerCanVote = SourceObject.ViewerCanVote,
                ViewerVote = SourceObject.ViewerVote ?? 0
            };

            if (!(SourceObject.Tallies?.Count > 0)) return pollSticker;
            foreach (var tallies in SourceObject.Tallies)
                try
                {
                    pollSticker.Tallies.Add(ConvertersFabric.GetStoryTalliesItemConverter(tallies)
                        .Convert());
                }
                catch
                {
                }

            return pollSticker;
        }
    }
}