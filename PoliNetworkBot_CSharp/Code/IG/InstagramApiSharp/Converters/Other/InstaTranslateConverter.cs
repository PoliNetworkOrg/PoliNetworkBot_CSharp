﻿#region

using System;
using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaTranslateConverter : IObjectConverter<InstaTranslate, InstaTranslateResponse>
    {
        public InstaTranslateResponse SourceObject { get; set; }

        public InstaTranslate Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("SourceObject");

            var translate = new InstaTranslate
            {
                Id = SourceObject.Id,
                Translation = SourceObject.Translation
            };
            return translate;
        }
    }
}