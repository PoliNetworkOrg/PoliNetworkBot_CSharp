﻿#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaTVConverter : IObjectConverter<InstaTV, InstaTVResponse>
    {
        public InstaTVResponse SourceObject { get; set; }

        public InstaTV Convert()
        {
            if (SourceObject == null)
                throw new ArgumentNullException("SourceObject");

            var tv = new InstaTV
            {
                Status = SourceObject.Status
            };
            if (SourceObject.MyChannel != null)
                try
                {
                    tv.MyChannel = ConvertersFabric.GetTvSelfChannelConverter(SourceObject.MyChannel)
                        .Convert();
                }
                catch
                {
                }

            if (SourceObject.Channels == null || !SourceObject.Channels.Any()) return tv;
            foreach (var channel in SourceObject.Channels)
                try
                {
                    tv.Channels.Add(ConvertersFabric.GetTvChannelConverter(channel).Convert());
                }
                catch
                {
                }

            return tv;
        }
    }
}