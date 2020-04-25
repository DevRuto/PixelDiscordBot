using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelDiscordBot.Models;
using PixelDiscordBot.Models.Twitch;
using PixelDiscordBot.Services;

namespace PixelDiscordBot.Discord
{
    public class DiscordBot
    {
        private Config _config;
        private ILogger _logger;
        private IServiceCollection _serviceCollection;
        private DiscordSocketClient _client;
        private TwitchService _twitch;
        private DiscordContext _db;

        public DiscordBot(Config config, ILogger<DiscordBot> logger, IServiceCollection serviceCollection, TwitchService twitch)
        {
            _config = config;
            _logger = logger;
            _serviceCollection = serviceCollection;
            _twitch = twitch;
        }

        public DiscordSocketClient GetClient() => _client;

        public async Task Start()
        {
            var services = ConfigureServices();
            _client = services.GetRequiredService<DiscordSocketClient>();

            await _client.LoginAsync(TokenType.Bot, _config.Discord.Token);
            await _client.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            _logger.LogInformation("Discord Bot started");
        }

        private ServiceProvider ConfigureServices()
        {
            return _serviceCollection
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }

        private static readonly Dictionary<string, List<RestUserMessage>> _messageCache = new Dictionary<string, List<RestUserMessage>>();
        public async Task HandleStreamEvent(string username, Event streamEvents, params ulong[] streamChannelIds)
        {
            if (!_messageCache.ContainsKey(username))
                _messageCache.Add(username, new List<RestUserMessage>());

            if (streamEvents.Data.Length == 0)
            {
                // Offline
                // await channel.SendMessageAsync("Stream went offline");

                // Delete message
                var messages = _messageCache[username];
                messages.ForEach(async message => await message.DeleteAsync());
                messages.Clear();
            }
            else
            {
                var streamEvent = streamEvents.Data[0];
                var messages = _messageCache[username];
                var gameName = await _twitch.GetGameName(streamEvent.GameId);

                if (messages.Count == 0)
                {
                    // Streamer just went live
                    foreach (var channelId in streamChannelIds)
                    {
                        if (channelId == 0) continue;
                        var channel = (SocketTextChannel) _client.GetChannel(channelId);
                        var message = await channel.SendMessageAsync(embed: await CreateStreamEmbed(streamEvent));
                        messages.Add(message);
                    }
                }
                else
                {
                    // Stream went online OR title changed OR game changed
                    var embed = await CreateStreamEmbed(streamEvent);
                    messages.ForEach(async message =>
                    {
                        await message.ModifyAsync(prop =>
                        {
                            prop.Embed = embed;
                        });
                    });
                }
            }
        }

        private async Task<Embed> CreateStreamEmbed(StreamEvent streamEvent)
            => new EmbedBuilder()
                        .WithTitle($"{streamEvent.UserName} went live")
                        .AddField("Title", streamEvent.Title, false)
                        .AddField("Date", streamEvent.StartedAt.ToLongDateString(), true)
                        .AddField("Game", await _twitch.GetGameName(streamEvent.GameId), true)
                        .AddField("Viewers", streamEvent.ViewerCount, true)
                        .WithColor(Color.Red)
                        .WithUrl($"https://twitch.tv/{streamEvent.UserName}")
                        .WithImageUrl(streamEvent.ThumbnailUrl.Replace("{width}", "1280").Replace("{height}", "720"))
                        .WithCurrentTimestamp()
                        .Build();
    }
}