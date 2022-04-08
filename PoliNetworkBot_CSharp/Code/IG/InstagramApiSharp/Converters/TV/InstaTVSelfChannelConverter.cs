#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters;

internal class InstaTVSelfChannelConverter : IObjectConverter<InstaTVSelfChannel, InstaTVSelfChannelResponse>
{
    public InstaTVSelfChannelResponse SourceObject { get; set; }

    public InstaTVSelfChannel Convert()
    {
        if (SourceObject == null)
            throw new ArgumentNullException("SourceObject");

        var channel = new InstaTVSelfChannel
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