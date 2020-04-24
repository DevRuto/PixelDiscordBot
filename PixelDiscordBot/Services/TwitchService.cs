using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PixelDiscordBot.Models;

namespace PixelDiscordBot.Services
{
    public class TwitchService
    {
        private Config _config;
        public TwitchService(Config config)
        {
            _config = config;
        }
    
        public async Task<long> GetUserId(string username)
        {
            var url = $"https://api.twitch.tv/helix/users?login={username}";
            var client = new WebClient();
            client.Headers["Client-ID"] = _config.Twitch.ClientId;
            var response = await client.DownloadStringTaskAsync(new Uri(url));
            using var doc = JsonDocument.Parse(response);
            var data = doc.RootElement.GetProperty("data");
            if (data.GetArrayLength() == 0) return -1;
            foreach (var ele in data.EnumerateArray())
            {
                return long.Parse(ele.GetProperty("id").GetString());
            }
            return -1;
        }

        public async Task<string> GetGameName(long gameId)
        {
            var url = $"https://api.twitch.tv/helix/games?id={gameId}";
            var client = new WebClient();
            client.Headers["Client-ID"] = _config.Twitch.ClientId;
            var response = await client.DownloadStringTaskAsync(new Uri(url));
            using var doc = JsonDocument.Parse(response);
            var data = doc.RootElement.GetProperty("data");
            if (data.GetArrayLength() == 0) return null;
            foreach (var ele in data.EnumerateArray())
            {
                return ele.GetProperty("name").GetString();
            }
            return null;
        }
    }
}