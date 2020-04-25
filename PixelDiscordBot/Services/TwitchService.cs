using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PixelDiscordBot.Models;

namespace PixelDiscordBot.Services
{
    public class TwitchService
    {
        private Config _config;
        private ILogger<TwitchService> _logger;
        private static readonly Dictionary<long, string> _nameCache = new Dictionary<long, string>();

        public TwitchService(Config config, ILogger<TwitchService> logger)
        {
            _config = config;
            _logger = logger;
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

        public async Task Subscribe(ulong userId, string username)
        {
            _logger.LogInformation($"[TWITCH] Subscribing to {username}");
            try
            {
                using (var client = new HttpClient())
                {
                    var json = $@"{{ ""hub.callback"": ""{_config.CallbackUrl}/{username}"", ""hub.mode"": ""subscribe"", ""hub.topic"": ""https://api.twitch.tv/helix/streams?user_id={userId}"", ""hub.lease_seconds"": 86400, ""hub.secret"": """" }}";
                    var response = await client.PostAsync(
                        "https://api.twitch.tv/helix/webhooks/hub",
                        new StringContent(json, Encoding.UTF8, "application/json")
                    );
                    _logger.LogInformation($"[TWITCH] {username} subscribe Result: {response.StatusCode}");
                }
                // var client = new WebClient();
                // client.Headers["Client-ID"] = _config.Twitch.ClientId;
                // client.Headers[HttpRequestHeader.ContentType] = "application/json";
                // var body = @"
                // {
                //     ""hub.callback"": ""{callbackurl}"",
                //     ""hub.mode"": ""subscribe"",
                //     ""hub.topic"": ""https://api.twitch.tv/helix/streams?user_id={userid}"",
                //     ""hub.lease_seconds"": 86400,
                //     ""hub.secret"": ""my secret""
                // }";
                // body = body.Replace("{userid}", userId.ToString());
                // body = body.Replace("{callbackurl}", $"{_config.CallbackUrl}/{username}");
                // await client.UploadStringTaskAsync("https://api.twitch.tv/helix/webhooks/hub", body);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Welp");
            }
        }
    }
}