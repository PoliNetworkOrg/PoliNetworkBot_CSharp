﻿#region

using InstagramApiSharp.Classes.Models;
using InstagramApiSharp.Classes.ResponseWrappers;
using System;

#endregion

namespace InstagramApiSharp.Converters.Users
{
    internal class InstaSuggestionsConverter : IObjectConverter<InstaSuggestions, InstaSuggestionUserContainerResponse>
    {
        public InstaSuggestionUserContainerResponse SourceObject { get; set; }

        public InstaSuggestions Convert()
        {
            if (SourceObject == null) throw new ArgumentNullException("Source object");
            var suggest = new InstaSuggestions
            {
                MoreAvailable = SourceObject.MoreAvailable,
                NextMaxId = SourceObject.MaxId ?? string.Empty
            };
            try
            {
                if (SourceObject.SuggestedUsers is { Suggestions: { Count: > 0 } })
                    suggest.SuggestedUsers = ConvertersFabric.Instance
                        .GetSuggestionItemListConverter(SourceObject.SuggestedUsers.Suggestions).Convert();
                if (SourceObject.NewSuggestedUsers is { Suggestions: { Count: > 0 } })
                    suggest.NewSuggestedUsers = ConvertersFabric.Instance
                        .GetSuggestionItemListConverter(SourceObject.NewSuggestedUsers.Suggestions).Convert();
            }
            catch
            {
            }

            return suggest;
        }
    }
}