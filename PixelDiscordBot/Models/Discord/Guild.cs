using System.Collections.Generic;

namespace PixelDiscordBot.Models.Discord
{
    public class Guild
    {
        public ulong Id { get; set; }
        public ulong StreamChannelId { get; set; }
        public ulong AnnounceRoleId { get; set; }

        public List<string> Streamers { get; set; }
    }
}