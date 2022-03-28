#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaBroadcastCommentConverter : IObjectConverter<InstaBroadcastComment, InstaBroadcastCommentResponse>
    {
        public InstaBroadcastCommentResponse SourceObject { get; set; }

        public InstaBroadcastComment Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var broadcastComment = new InstaBroadcastComment
            {
                MediaId = SourceObject.MediaId,
                ContentType = SourceObject.ContentType,
                CreatedAt = (SourceObject.CreatedAt ?? DateTime.Now.ToUnixTime()).FromUnixTimeSeconds(),
                CreatedAtUtc = (SourceObject.CreatedAtUtc ?? DateTime.UtcNow.ToUnixTime()).FromUnixTimeSeconds(),
                Pk = SourceObject.Pk,
                Text = SourceObject.Text,
                Type = SourceObject.Type,
                BitFlags = SourceObject.BitFlags,
                DidReportAsSpam = SourceObject.DidReportAsSpam,
                InlineComposerDisplayCondition = SourceObject.InlineComposerDisplayCondition,
                UserId = SourceObject.UserId
            };
            if (SourceObject.User != null)
                broadcastComment.User = ConvertersFabric.GetUserShortFriendshipFullConverter(SourceObject.User).Convert();

            return broadcastComment;
        }
    }
}