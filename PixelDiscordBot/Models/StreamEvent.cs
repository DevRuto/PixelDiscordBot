namespace PixelDiscordBot.Models
{
    public class StreamEvent
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string GameId { get; set; }
        public long[] CommunityIds { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public int ViewerCount { get; set; }
        public System.DateTime StartedAt { get; set; }
        public string Language { get; set; }
        public string ThumbnailUrl { get; set; }
    }
}