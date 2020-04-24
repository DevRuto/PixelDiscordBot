namespace PixelDiscordBot.Models
{
    public class StreamEvent
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string UserName { get; set; }
        public long GameId { get; set; }
        public long[] CommunityIds { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public long ViewerCount { get; set; }
        public System.DateTime StartedAt { get; set; }
        public string Language { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}