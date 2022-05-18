#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Converters;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Hashtags;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Converters.Hashtags;

internal class InstaHashtagMediaConverter : IObjectConverter<InstaSectionMedia, InstaSectionMediaListResponse>
{
    public InstaSectionMediaListResponse SourceObject { get; set; }

    public InstaSectionMedia Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("Source object");

        var media = new InstaSectionMedia
        {
            AutoLoadMoreEnabled = SourceObject.AutoLoadMoreEnabled ?? false,
            MoreAvailable = SourceObject.MoreAvailable,
            NextMaxId = SourceObject.NextMaxId,
            NextMediaIds = SourceObject.NextMediaIds,
            NextPage = SourceObject.NextPage ?? 0
        };
        if (SourceObject.Sections != null)
            foreach (var section in SourceObject.Sections)
                try
                {
                    foreach (var item in section.LayoutContent.Medias)
                        try
                        {
                            media.Medias.Add(
                                ConvertersFabric.GetSingleMediaConverter(item.Media).Convert());
                        }
                        catch
                        {
                        }
                }
                catch
                {
                }

        if (!(SourceObject.PersistentSections?.Count > 0)) return media;
        {
            try
            {
                foreach (var related in SourceObject.PersistentSections
                             .Where(section => section.LayoutContent?.Related?.Count > 0)
                             .SelectMany(section => section.LayoutContent.Related))
                    try
                    {
                        media.RelatedHashtags.Add(ConvertersFabric.GetRelatedHashtagConverter(related).Convert());
                    }
                    catch
                    {
                    }
            }
            catch
            {
            }
        }

        return media;
    }
}