using System;
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
using PixelDiscordBot.Models.Discord;
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

            var db = services.GetRequiredService<DiscordContext>();
            (await db.Streamers.ToListAsync()).ForEach(async streamer => await _twitch.Subscribe(streamer.Id, streamer.Username, true));

            _logger.LogInformation("Streamers loaded");
        }

        private ServiceProvider ConfigureServices()
        {
            return _serviceCollection
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .AddSingleton(this)
                .BuildServiceProvider();
        }

        private static readonly Dictionary<string, List<RestUserMessage>> _messageCache = new Dictionary<string, List<RestUserMessage>>();
        public async Task HandleStreamEvent(string username, Event streamEvents, Guild[] guilds)
        {
            _logger.LogDebug("Logging {username} to {guilds}", username, guilds);
            if (!_messageCache.ContainsKey(username))
                _messageCache.Add(username, new List<RestUserMessage>());

            try
            {
                if (streamEvents.Data.Length == 0)
                {
                    // Offline
                    // await channel.SendMessageAsync("Stream went offline");

                    // Delete message
                    var messages = _messageCache[username];
                    messages.ForEach(async message => await message.DeleteAsync());
                    messages.Clear();

                    var vodGuilds = guilds.Where(guild => guild.EnableVods).ToList();

                    if (vodGuilds.Count == 0) return;
                    var vod = (await _twitch.GetVods(username)).Data.FirstOrDefault();
                    if (vod == null) return;
                    var embed = await CreateVodEmbed(vod);

                    foreach (var guild in vodGuilds)
                    {
                        if (guild.VodChannelId <= 0) continue;
                        var channel = (SocketTextChannel) _client.GetChannel(guild.VodChannelId);
                        await channel.SendMessageAsync(embed: embed);
                    }
                }
                else
                {
                    var streamEvent = streamEvents.Data[0];
                    var messages = _messageCache[username];
                    var gameName = await _twitch.GetGameName(streamEvent.GameId);

                    if (messages.Count == 0)
                    {
                        // Streamer just went live
                        foreach (var guild in guilds)
                        {
                            var channelId = guild.StreamChannelId;
                            var roleId = guild.AnnounceRoleId;
                            if (channelId == 0) continue;
                            var channel = (SocketTextChannel) _client.GetChannel(channelId);
                            string mention = null;
                            if (roleId > 0)
                            {
                                mention = $"<@&{roleId}>";
                            }
                            var message = await channel.SendMessageAsync(text: mention, embed: await CreateStreamEmbed(streamEvent));
                            messages.Add(message);
                        }
                    }
                    else
                    {
                        // Stream title changed OR game changed
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "HandleStreamEvent error");
            }
        }

        public async Task<Embed> CreateStreamEmbed(StreamEvent streamEvent)
            => new EmbedBuilder()
                        .WithTitle($"{streamEvent.UserName} went live")
                        .AddField("Title", streamEvent.Title, false)
                        .AddField("Date", streamEvent.StartedAt.ToLongDateString(), true)
                        .AddField("Game", await _twitch.GetGameName(streamEvent.GameId), true)
                        .WithColor(Color.Red)
                        .WithUrl($"https://twitch.tv/{streamEvent.UserName}")
                        .WithImageUrl(streamEvent.ThumbnailUrl.Replace("{width}", "1280").Replace("{height}", "720"))
                        .WithCurrentTimestamp()
                        .Build();

        public async Task<Embed> CreateVodEmbed(Video vod)
            => new EmbedBuilder()
                        .WithTitle($"{vod.UserName} VOD")
                        .AddField("Title", vod.Title, false)
                        .AddField("Date", vod.CreatedAt.ToLongDateString(), true)
                        .AddField("Duration", vod.Duration, true)
                        .WithColor(Color.Red)
                        .WithUrl(vod.Url)
                        .WithImageUrl(vod.ThumbnailUrl.Replace("%{width}", "1280").Replace("%{height}", "720"))
                        .Build();
    }
}