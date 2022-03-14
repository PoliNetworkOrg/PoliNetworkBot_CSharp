#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers.Web;
using InstagramApiSharp.Helpers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaWebAccountInfoConverter : IObjectConverter<InstaWebAccountInfo, InstaWebSettingsPageResponse>
    {
        public InstaWebSettingsPageResponse SourceObject { get; set; }

        public InstaWebAccountInfo Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var info = new InstaWebAccountInfo
            {
                JoinedDate = SourceObject.DateJoined?.Data?.Timestamp != null ? SourceObject.DateJoined?.Data?.Timestamp.Value.FromUnixTimeSeconds() : DateTime.MinValue,
                SwitchedToBusinessDate = SourceObject.SwitchedToBusiness?.Data?.Timestamp != null ? SourceObject.SwitchedToBusiness?.Data?.Timestamp.Value.FromUnixTimeSeconds() : DateTime.MinValue
            };

            return info;
        }
    }
}