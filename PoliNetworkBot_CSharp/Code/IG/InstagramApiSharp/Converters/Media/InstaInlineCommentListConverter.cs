#region

using System.Linq;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Converters;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Comment;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Converters.Media;

internal class InstaInlineCommentListConverter
    : IObjectConverter<InstaInlineCommentList, InstaInlineCommentListResponse>
{
    public InstaInlineCommentListResponse SourceObject { get; set; }

    public InstaInlineCommentList Convert()
    {
        if (SourceObject == null)
            return null;
        var inline = new InstaInlineCommentList
        {
            ChildCommentCount = SourceObject.ChildCommentCount,
            HasMoreHeadChildComments = SourceObject.HasMoreHeadChildComments,
            HasMoreTailChildComments = SourceObject.HasMoreTailChildComments,
            NumTailChildComments = SourceObject.NumTailChildComments,
            NextMaxId = SourceObject.NextMaxId,
            NextMinId = SourceObject.NextMinId
        };
        if (SourceObject.ParentComment != null)
            try
            {
                inline.ParentComment = ConvertersFabric.GetCommentConverter(SourceObject.ParentComment)
                    .Convert();
            }
            catch
            {
            }

        if (SourceObject.ChildComments == null || !SourceObject.ChildComments.Any()) return inline;
        foreach (var cmt in SourceObject.ChildComments)
            try
            {
                inline.ChildComments.Add(ConvertersFabric.GetCommentConverter(cmt).Convert());
            }
            catch
            {
            }

        //if (!string.IsNullOrEmpty(SourceObject.NextMinId))
        //{
        //    try
        //    {
        //        var convertedNextId = JsonConvert.DeserializeObject<InstaInlineCommentNextIdResponse>(SourceObject.NextMinId);
        //        inline.NextMinId = convertedNextId.BifilterToken;
        //    }
        //    catch { }
        //}
        //if (!string.IsNullOrEmpty(SourceObject.NextMaxId))
        //{
        //    try
        //    {
        //        var convertedNextId = JsonConvert.DeserializeObject<InstaInlineCommentNextIdResponse>(SourceObject.NextMaxId);
        //        inline.NextMaxId = convertedNextId.ServerCursor;
        //    }
        //    catch { }
        //}
        return inline;
    }
}