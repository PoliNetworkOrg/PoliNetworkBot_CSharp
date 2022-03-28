﻿#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using InstagramApiSharp.Enums;
using System;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaCurrentUserConverter : IObjectConverter<InstaCurrentUser, InstaCurrentUserResponse>
    {
        public InstaCurrentUserResponse SourceObject { get; set; }

        public InstaCurrentUser Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var shortConverter = ConvertersFabric.GetUserShortConverter(SourceObject);
            var user = new InstaCurrentUser(shortConverter.Convert())
            {
                HasAnonymousProfilePicture = SourceObject.HasAnonymousProfilePicture,
                Biography = SourceObject.Biography,
                Birthday = SourceObject.Birthday,
                CountryCode = SourceObject.CountryCode,
                NationalNumber = SourceObject.NationalNumber,
                Email = SourceObject.Email,
                ExternalUrl = SourceObject.ExternalURL,
                ShowConversionEditEntry = SourceObject.ShowConversationEditEntry,
                Gender = (InstaGenderType)SourceObject.Gender,
                PhoneNumber = SourceObject.PhoneNumber
            };

            if (SourceObject.HDProfilePicVersions is { Length: > 0 })
                foreach (var imageResponse in SourceObject.HDProfilePicVersions)
                {
                    var converter = ConvertersFabric.Instance.GetImageConverter(imageResponse);
                    user.HdProfileImages.Add(converter.Convert());
                }

            if (SourceObject.HDProfilePicture == null) return user;
            {
                var converter = ConvertersFabric.Instance.GetImageConverter(SourceObject.HDProfilePicture);
                user.HdProfilePicture = converter.Convert();
            }

            return user;
        }
    }
}