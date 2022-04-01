#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Converters;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Converters.Stories;

internal class InstaReelMentionConverter : IObjectConverter<InstaReelMention, InstaReelMentionResponse>
{
    public InstaReelMentionResponse SourceObject { get; init; }

    public InstaReelMention Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var mention = new InstaReelMention
        {
            Height = SourceObject.Height,
            IsPinned = System.Convert.ToBoolean(SourceObject.IsPinned),
            IsHidden = System.Convert.ToBoolean(SourceObject.IsHidden),
            Rotation = SourceObject.Rotation,
            Width = SourceObject.Width,
            X = SourceObject.X,
            Y = SourceObject.Y,
            Z = SourceObject.Z
        };
        if (SourceObject.Hashtag != null)
            mention.Hashtag = ConvertersFabric.GetHashTagConverter(SourceObject.Hashtag).Convert();
        if (SourceObject.User != null)
            mention.User = ConvertersFabric.GetUserShortConverter(SourceObject.User).Convert();
        return mention;
    }
}