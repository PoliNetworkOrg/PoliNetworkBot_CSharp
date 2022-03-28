#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaBrandedContentConverter : IObjectConverter<InstaBrandedContent, InstaBrandedContentResponse>
{
    public InstaBrandedContentResponse SourceObject { get; set; }

    public InstaBrandedContent Convert()
    {
        if (SourceObject == null)
            throw new ArgumentNullException("SourceObject");

        var brandedContent = new InstaBrandedContent
        {
            RequireApproval = SourceObject.RequireApproval
        };
        if (SourceObject.WhitelistedUsers == null || !SourceObject.WhitelistedUsers.Any()) return brandedContent;
        foreach (var item in SourceObject.WhitelistedUsers)
            try
            {
                brandedContent.WhitelistedUsers.Add(ConvertersFabric.GetUserShortConverter(item)
                    .Convert());
            }
            catch
            {
            }

        return brandedContent;
    }
}