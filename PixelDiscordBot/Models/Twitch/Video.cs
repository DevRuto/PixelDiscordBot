using System;

namespace PixelDiscordBot.Models.Twitch
{
    public class Videos
    {
        public Video[] Data { get; set; }
        public Pagination Pagination { get; set; }
    }

    public class Video
    {
        public string Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime PublishedAt { get; set; }
        public string Url { get; set; }
        public string ThumbnailUrl { get; set; }
        public string Viewable { get; set; }
        public int ViewCount { get; set; }
        public string Language { get; set; }
        public string Type { get; set; }
        public string Duration { get; set; }
    }

    public class Pagination
    {
        public string Cursor { get; set; }
    }
}