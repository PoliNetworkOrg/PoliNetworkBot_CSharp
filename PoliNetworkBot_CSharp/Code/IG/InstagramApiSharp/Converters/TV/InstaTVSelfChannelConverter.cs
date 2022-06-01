#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Converters;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.TV;

#endregion

namespace PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Converters.TV;

internal class InstaTvSelfChannelConverter : IObjectConverter<InstaTvSelfChannel, InstaTVSelfChannelResponse>
{
    public InstaTVSelfChannelResponse SourceObject { get; set; }

    public InstaTvSelfChannel Convert()
    {
        if (SourceObject == null)
            throw new ArgumentNullException("SourceObject");

        var channel = new InstaTvSelfChannel
        {
            HasMoreAvailable = SourceObject.HasMoreAvailable,
            Id = SourceObject.Id,
            MaxId = SourceObject.MaxId,
            Title = SourceObject.Title,
            Type = SourceObject.Type
        };

        if (SourceObject.Items != null && SourceObject.Items.Any())
            foreach (var item in SourceObject.Items)
                try
                {
                    channel.Items.Add(ConvertersFabric.GetSingleMediaConverter(item).Convert());
                }
                catch
                {
                }

        if (SourceObject.User == null) return channel;
        try
        {
            channel.User = ConvertersFabric.GetTvUserConverter(SourceObject.User).Convert();
        }
        catch
        {
        }

        return channel;
    }
}