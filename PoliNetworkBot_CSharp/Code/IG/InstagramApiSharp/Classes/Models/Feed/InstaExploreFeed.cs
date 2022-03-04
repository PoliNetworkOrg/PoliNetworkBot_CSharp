namespace InstagramApiSharp.Classes.Models
{
    public class InstaExploreFeed : InstaBaseFeed
    {
        public InstaStoryTray StoryTray { get; set; } = new();
        public InstaChannel Channel { get; set; } = new();
        public string MaxId { get; set; }
        public string RankToken { get; set; }
        public bool MoreAvailable { get; set; }
        public int ResultsCount { get; set; }
        public bool AutoLoadMoreEnabled { get; set; }
    }
}