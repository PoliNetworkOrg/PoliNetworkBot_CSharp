#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaStoryFriendshipStatusShortConverter : IObjectConverter<InstaStoryFriendshipStatusShort,
        InstaStoryFriendshipStatusShortResponse>
    {
        public InstaStoryFriendshipStatusShortResponse SourceObject { get; set; }

        public InstaStoryFriendshipStatusShort Convert()
        {
            var storyFriendshipStatusShort = new InstaStoryFriendshipStatusShort
            {
                Following = SourceObject.Following,
                OutgoingRequest = SourceObject.OutgoingRequest ?? false,
                Muting = SourceObject.Muting ?? false,
                IsMutingReel = SourceObject.IsMutingReel ?? false
            };
            return storyFriendshipStatusShort;
        }
    }
}