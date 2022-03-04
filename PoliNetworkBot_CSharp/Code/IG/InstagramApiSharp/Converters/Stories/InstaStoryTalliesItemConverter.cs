#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaStoryTalliesItemConverter : IObjectConverter<InstaStoryTalliesItem, InstaStoryTalliesItemResponse>
    {
        public InstaStoryTalliesItemResponse SourceObject { get; set; }

        public InstaStoryTalliesItem Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var tallies = new InstaStoryTalliesItem
            {
                Count = SourceObject.Count,
                FontSize = SourceObject.FontSize,
                Text = SourceObject.Text
            };
            return tallies;
        }
    }
}