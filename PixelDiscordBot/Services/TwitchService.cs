using System;
using System.Collections.Generic;
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
    
        public async Task<ulong> GetUserId(string username)
        {
            var url = $"https://api.twitch.tv/helix/users?login={username}";
            var client = new WebClient();
            client.Headers["Client-ID"] = _config.Twitch.ClientId;
            var response = await client.DownloadStringTaskAsync(new Uri(url));
            using var doc = JsonDocument.Parse(response);
            var data = doc.RootElement.GetProperty("data");
            if (data.GetArrayLength() == 0) return 0;
            foreach (var ele in data.EnumerateArray())
            {
                return ulong.Parse(ele.GetProperty("id").GetString());
            }
            return 0;
        }

        private static readonly Dictionary<long, string> _nameCache = new Dictionary<long, string>();

        public async Task<string> GetGameName(long gameId)
        {
            if (_nameCache.ContainsKey(gameId))
                return _nameCache[gameId];
            var url = $"https://api.twitch.tv/helix/games?id={gameId}";
            var client = new WebClient();
            client.Headers["Client-ID"] = _config.Twitch.ClientId;
            var response = await client.DownloadStringTaskAsync(new Uri(url));
            using var doc = JsonDocument.Parse(response);
            var data = doc.RootElement.GetProperty("data");
            if (data.GetArrayLength() == 0) return null;
            foreach (var ele in data.EnumerateArray())
            {
                var name = ele.GetProperty("name").GetString();
                _nameCache.Add(gameId, name);
                return name;
            }
            return "Unknown";
        }
    }
}