﻿#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters
{
    internal class
        InstaUserPresenceListConverter : IObjectConverter<InstaUserPresenceList, InstaUserPresenceContainerResponse>
    {
        public InstaUserPresenceContainerResponse SourceObject { get; set; }

        public InstaUserPresenceList Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var list = new InstaUserPresenceList();
            if (SourceObject.Items == null || !SourceObject.Items.Any()) return list;
            foreach (var item in SourceObject.Items)
                try
                {
                    list.Add(ConvertersFabric.GetSingleUserPresenceConverter(item).Convert());
                }
                catch
                {
                }

            return list;
        }
    }
}