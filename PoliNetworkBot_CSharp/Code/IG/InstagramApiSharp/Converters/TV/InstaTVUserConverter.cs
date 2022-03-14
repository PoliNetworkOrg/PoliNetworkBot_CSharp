#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaTVChannelConverter : IObjectConverter<InstaTVChannel, InstaTVChannelResponse>
    {
        public InstaTVChannelResponse SourceObject { get; set; }

        public InstaTVChannel Convert()
        {
            if (SourceObject == null)
                throw new ArgumentNullException("SourceObject");

            var channel = new InstaTVChannel
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
                        channel.Items.Add(ConvertersFabric.Instance.GetSingleMediaConverter(item).Convert());
                    }
                    catch
                    {
                    }

            if (SourceObject.UserDetail == null) return channel;
            try
            {
                channel.UserDetail =
                    ConvertersFabric.Instance.GetTVUserConverter(SourceObject.UserDetail).Convert();
            }
            catch
            {
            }

            return channel;
        }
    }
}