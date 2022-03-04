﻿#region

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

            var info = new InstaWebAccountInfo();
            if (SourceObject.DateJoined?.Data?.Timestamp != null)
                info.JoinedDate = SourceObject.DateJoined?.Data?.Timestamp.Value.FromUnixTimeSeconds();
            else
                info.JoinedDate = DateTime.MinValue;

            if (SourceObject.SwitchedToBusiness?.Data?.Timestamp != null)
                info.SwitchedToBusinessDate =
                    SourceObject.SwitchedToBusiness?.Data?.Timestamp.Value.FromUnixTimeSeconds();
            else
                info.SwitchedToBusinessDate = DateTime.MinValue;

            return info;
        }
    }
}