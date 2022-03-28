#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaReelShareConverter : IObjectConverter<InstaReelShare, InstaReelShareResponse>
{
    public InstaReelShareResponse SourceObject { get; set; }

    public InstaReelShare Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");

        var reelShare = new InstaReelShare
        {
            IsReelPersisted = SourceObject.IsReelPersisted ?? false,
            ReelOwnerId = SourceObject.ReelOwnerId,
            ReelType = SourceObject.ReelType,
            Text = SourceObject.Text,
            Type = SourceObject.Type
        };
        try
        {
            reelShare.Media = ConvertersFabric.GetStoryItemConverter(SourceObject.Media).Convert();
        }
        catch
        {
        }

        return reelShare;
    }
}