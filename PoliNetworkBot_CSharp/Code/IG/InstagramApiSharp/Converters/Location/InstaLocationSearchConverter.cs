﻿#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class InstaLocationSearchConverter : IObjectConverter<InstaLocationShortList, InstaLocationSearchResponse>
    {
        public InstaLocationSearchResponse SourceObject { get; set; }

        public InstaLocationShortList Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var locations = new InstaLocationShortList();
            locations.AddRange(SourceObject.Locations.Select(location =>
                ConvertersFabric.GetLocationShortConverter(location).Convert()));
            return locations;
        }
    }
}