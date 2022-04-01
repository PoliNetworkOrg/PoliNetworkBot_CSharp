#region

using InstagramApiSharp.Classes.ResponseWrappers.Business;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.Business;

#endregion

namespace InstagramApiSharp.Converters.Business;

internal class InstaMediaShortConverter : IObjectConverter<InstaMediaShort, InstaMediaShortResponse>
{
    public InstaMediaShortResponse SourceObject { get; set; }

    public InstaMediaShort Convert()
    {
        var media = new InstaMediaShort();
        if (!string.IsNullOrEmpty(SourceObject.InstagramMediaType))
            try
            {
            }
            catch
            {
            }

        if (SourceObject.Image is { Uri: { } })
        {
        }

        if (SourceObject.InlineInsightsNode == null) return media;
        try
        {
        }
        catch
        {
        }

        return media;
    }
}