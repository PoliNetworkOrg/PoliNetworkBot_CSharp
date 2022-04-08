#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaDirectHashtagConverter : IObjectConverter<InstaDirectHashtag, InstaDirectHashtagResponse>
{
    public InstaDirectHashtagResponse SourceObject { get; set; }

    public InstaDirectHashtag Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");
        var hashtag = new InstaDirectHashtag
        {
            Name = SourceObject.Name,
            MediaCount = SourceObject.MediaCount
        };

        if (SourceObject.Media != null)
            hashtag.Media = ConvertersFabric.GetSingleMediaConverter(SourceObject.Media).Convert();
        return hashtag;
    }
}