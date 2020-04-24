namespace PixelDiscordBot.Models
{
    public class Config
    {
        public TwitchConfig Twitch { get; set; }
        public DiscordConfig Discord { get; set; }
        public string CallbackUrl { get; set; }
    
        public class TwitchConfig
        {
            public string ClientId { get; set; }
            public string ClientSecret { get; set; }
        }

        public class DiscordConfig
        {
            public string Token { get; set; }
        }
    }
}