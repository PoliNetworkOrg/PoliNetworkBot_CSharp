#region

using System.Collections.Generic;

#endregion

namespace InstagramApiSharp.Classes.Models;

public class InstaFeed : IInstaBaseList
{
    public int MediaItemsCount => Medias.Count;
    public int StoriesItemsCount => Stories.Count;

    public List<InstaMedia> Medias { get; set; } = new();
    public List<InstaStory> Stories { get; set; } = new();

    public List<InstaSuggestionItem> SuggestedUserItems { get; set; } = new();
    public string NextMaxId { get; set; }
}