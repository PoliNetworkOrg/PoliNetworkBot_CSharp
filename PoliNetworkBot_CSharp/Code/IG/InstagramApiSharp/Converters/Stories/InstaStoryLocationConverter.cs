#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaStoryLocationConverter : IObjectConverter<InstaStoryLocation, InstaStoryLocationResponse>
    {
        public InstaStoryLocationResponse SourceObject { get; set; }

        public InstaStoryLocation Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var storyLocation = new InstaStoryLocation
            {
                Height = SourceObject.Height,
                IsHidden = SourceObject.IsHidden,
                IsPinned = SourceObject.IsPinned,
                Rotation = SourceObject.Rotation,
                Width = SourceObject.Width,
                X = SourceObject.X,
                Y = SourceObject.Y,
                Z = SourceObject.Z,
                Location = ConvertersFabric.GetPlaceShortConverter(SourceObject.Location).Convert()
            };

            return storyLocation;
        }
    }
}