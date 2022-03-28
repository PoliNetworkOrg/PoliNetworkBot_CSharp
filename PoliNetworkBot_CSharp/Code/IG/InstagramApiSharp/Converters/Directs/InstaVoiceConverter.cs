﻿#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaVoiceConverter : IObjectConverter<InstaVoice, InstaVoiceResponse>
    {
        public InstaVoiceResponse SourceObject { get; set; }

        public InstaVoice Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");

            var voice = new InstaVoice
            {
                Audio = ConvertersFabric.GetAudioConverter(SourceObject.Audio).Convert(),
                Id = SourceObject.Id,
                MediaType = SourceObject.MediaType,
                OrganicTrackingToken = SourceObject.OrganicTrackingToken,
                ProductType = SourceObject.ProductType
            };
            if (SourceObject.User != null)
                voice.FriendshipStatus = ConvertersFabric.GetFriendShipStatusConverter(SourceObject.User.FriendshipStatus).Convert();

            return voice;
        }
    }
}