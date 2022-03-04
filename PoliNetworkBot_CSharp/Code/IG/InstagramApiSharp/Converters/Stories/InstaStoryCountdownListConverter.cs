﻿#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaStoryCountdownListConverter : IObjectConverter<InstaStoryCountdownList, InstaStoryCountdownListResponse>
    {
        public InstaStoryCountdownListResponse SourceObject { get; set; }

        public InstaStoryCountdownList Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var storyCountdownList = new InstaStoryCountdownList
            {
                MoreAvailable = SourceObject.MoreAvailable ?? false,
                MaxId = SourceObject.MaxId
            };

            if (SourceObject.Items?.Count > 0)
                foreach (var countdown in SourceObject.Items)
                    storyCountdownList.Items.Add(ConvertersFabric.Instance
                        .GetStoryCountdownStickerItemConverter(countdown).Convert());

            return storyCountdownList;
        }
    }
}