#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Helpers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaBroadcastSendCommentConverter : IObjectConverter<InstaBroadcastSendComment,
            InstaBroadcastSendCommentResponse>
    {
        public InstaBroadcastSendCommentResponse SourceObject { get; set; }

        public InstaBroadcastSendComment Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var broadcastSendComment = new InstaBroadcastSendComment
            {
                MediaId = SourceObject.MediaId,
                ContentType = SourceObject.ContentType,
                CreatedAt = (SourceObject.CreatedAt ?? DateTime.Now.ToUnixTime()).FromUnixTimeSeconds(),
                CreatedAtUtc = (SourceObject.CreatedAtUtc ?? DateTime.UtcNow.ToUnixTime()).FromUnixTimeSeconds(),
                Pk = SourceObject.Pk,
                Text = SourceObject.Text,
                Type = SourceObject.Type
            };
            if (SourceObject.User != null)
                broadcastSendComment.User = ConvertersFabric.GetUserShortFriendshipFullConverter(SourceObject.User).Convert();

            return broadcastSendComment;
        }
    }
}