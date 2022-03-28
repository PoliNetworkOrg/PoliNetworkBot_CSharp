﻿#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaChannelConverter : IObjectConverter<InstaChannel, InstaChannelResponse>
    {
        public InstaChannelResponse SourceObject { get; set; }

        public InstaChannel Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var channel = new InstaChannel
            {
                ChannelId = SourceObject.ChannelId,
                ChannelType = SourceObject.ChannelType,
                Context = SourceObject.Context,
                Header = SourceObject.Header,
                Title = SourceObject.Title
            };
            if (SourceObject.Media != null)
                channel.Media = ConvertersFabric.GetSingleMediaConverter(SourceObject.Media).Convert();
            return channel;
        }
    }
}