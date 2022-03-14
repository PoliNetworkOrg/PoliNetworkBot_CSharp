#region

using System;
using System.Linq;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaStoryTrayConverter : IObjectConverter<InstaStoryTray, InstaStoryTrayResponse>
    {
        public InstaStoryTrayResponse SourceObject { get; set; }

        public InstaStoryTray Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var storyTray = new InstaStoryTray
            {
                Id = SourceObject.Id,
                IsPortrait = SourceObject.IsPortrait,
                TopLive = ConvertersFabric.Instance.GetTopLiveConverter(SourceObject.TopLive).Convert()
            };

            if (SourceObject.Tray == null) return storyTray;
            foreach (var story in SourceObject.Tray.Select(item =>
                         ConvertersFabric.Instance.GetStoryConverter(item).Convert())) storyTray.Tray.Add(story);

            return storyTray;
        }
    }
}