#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaCommentListConverter : IObjectConverter<InstaCommentList, InstaCommentListResponse>
    {
        public InstaCommentListResponse SourceObject { get; set; }

        public InstaCommentList Convert()
        {
            var commentList = new InstaCommentList
            {
                Caption = SourceObject.Caption != null
                    ? ConvertersFabric.GetCaptionConverter(SourceObject.Caption).Convert()
                    : null,
                CanViewMorePreviewComments = SourceObject.CanViewMorePreviewComments,
                CaptionIsEdited = SourceObject.CaptionIsEdited,
                CommentsCount = SourceObject.CommentsCount,
                MoreCommentsAvailable = SourceObject.MoreCommentsAvailable,
                InitiateAtTop = SourceObject.InitiateAtTop,
                InsertNewCommentToTop = SourceObject.InsertNewCommentToTop,
                MediaHeaderDisplay = SourceObject.MediaHeaderDisplay,
                ThreadingEnabled = SourceObject.ThreadingEnabled,
                LikesEnabled = SourceObject.LikesEnabled,
                MoreHeadLoadAvailable = SourceObject.MoreHeadLoadAvailable,
                NextMaxId = SourceObject.NextMaxId,
                NextMinId = SourceObject.NextMinId
            };
            if (SourceObject.Comments == null || !(SourceObject?.Comments?.Count > 0)) return commentList;
            foreach (var converter in SourceObject.Comments.Select(commentResponse =>
                         ConvertersFabric.GetCommentConverter(commentResponse)))
                commentList.Comments.Add(converter.Convert());

            if (SourceObject.PreviewComments == null || !SourceObject.PreviewComments.Any()) return commentList;
            foreach (var cmt in SourceObject.PreviewComments)
                try
                {
                    commentList.PreviewComments.Add(ConvertersFabric.GetCommentConverter(cmt).Convert());
                }
                catch
                {
                }

            return commentList;
        }
    }
}