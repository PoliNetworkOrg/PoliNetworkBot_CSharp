namespace InstagramApiSharp.Classes.Models
{
    public class InstaBaseFeed : IInstaBaseList
    {
        public InstaMediaList Medias { get; set; } = new();
        public string NextMaxId { get; set; }
    }
}