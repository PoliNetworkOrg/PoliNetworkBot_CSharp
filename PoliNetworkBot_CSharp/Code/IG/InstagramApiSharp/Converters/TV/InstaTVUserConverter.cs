#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using PoliNetworkBot_CSharp.Code.IG.InstagramApiSharp.Classes.Models.TV;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaTVChannelConverter : IObjectConverter<InstaTvChannel, InstaTVChannelResponse>
{
    public InstaTVChannelResponse SourceObject { get; set; }

    public InstaTvChannel Convert()
    {
        if (SourceObject == null)
            throw new ArgumentNullException("SourceObject");

        var channel = new InstaTvChannel
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

        if (SourceObject.UserDetail == null) return channel;
        try
        {
            channel.UserDetail =
                ConvertersFabric.GetTvUserConverter(SourceObject.UserDetail).Convert();
        }
        catch
        {
        }

        return channel;
    }
}