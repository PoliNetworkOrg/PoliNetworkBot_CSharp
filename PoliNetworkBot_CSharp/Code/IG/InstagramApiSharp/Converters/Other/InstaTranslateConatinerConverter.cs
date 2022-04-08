﻿#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;
using System.Linq;

#endregion

namespace InstagramApiSharp.Converters;

internal class
    InstaTranslateContainerConverter : IObjectConverter<InstaTranslateList, InstaTranslateContainerResponse>
{
    public InstaTranslateContainerResponse SourceObject { get; set; }

    public InstaTranslateList Convert()
    {
        if (SourceObject == null) throw new ArgumentNullException("SourceObject");

        var list = new InstaTranslateList();
        if (SourceObject.Translations != null && SourceObject.Translations.Any())
            list.AddRange(SourceObject.Translations.Select(item =>
                ConvertersFabric.GetSingleTranslateConverter(item).Convert()));

        return list;
    }
}