using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using PixelDiscordBot.Models;
using PixelDiscordBot.Models.Discord;
using PixelDiscordBot.Services;

namespace PixelDiscordBot.Discord.Module
{
    [Group("twitch")]
    public class TwitchModule : ModuleBase<SocketCommandContext>
    {
        private readonly DiscordContext _db;
        private readonly TwitchService _twitch;
        private readonly DiscordBot _bot;

        public TwitchModule(DiscordContext db, TwitchService twitch, DiscordBot bot)
        {
            _db = db;
            _twitch = twitch;
            _bot = bot;
        }

        [Command("ping")]
        public Task PingAsync()
        {
            return ReplyAsync("Pong");
        }

        [Command("add")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AddStreamer(string username)
        {
            var guildId = this.Context.Guild.Id;
            var guild = await _db.Guilds.FindAsync(guildId);
            if (guild == null)
            {
                guild = new Guild
                {
                    Id = guildId,
                    StreamChannelId = 0,
                    AnnounceRoleId = 0,
                    Streamers = new List<string>()
                };
                await _db.Guilds.AddAsync(guild);
                await _db.SaveChangesAsync();
            }

            if (guild.StreamChannelId == 0)
            {
                await ReplyAsync("Please set the stream output channel `twitch setchannel #streams` before adding streamers");
                return;
            }

            var userId = await _twitch.GetUserId(username);

            if (guild.Streamers.Contains(username))
            {
                await ReplyAsync($"{Format.Sanitize(username)} is already added");
                return;
            }
            else
            {
                guild.Streamers.Add(username);
                _db.Guilds.Update(guild);
                if (await _db.Streamers.FindAsync(userId) == null)
                {
                    await _db.Streamers.AddAsync(new Streamer
                    {
                        Id = userId,
                        Username = username
                    });
                    await _twitch.Subscribe(userId, username);
                }
                await ReplyAsync($"**{Format.Sanitize(username)}** added");
            }

            await _db.SaveChangesAsync();
        }

        [Command("remove")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveStreamer(string username)
        {
            var guildId = this.Context.Guild.Id;
            var guild = await _db.Guilds.FindAsync(guildId);
            if (guild == null || guild.Streamers.Count == 0)
            {
                await ReplyAsync("No streamers being tracked");
            }
            else
            {
                if (guild.Streamers.Contains(username))
                {
                    guild.Streamers.Remove(username);
                    await ReplyAsync($"**{Format.Sanitize(username)}** removed");
                }
            }
            await _db.SaveChangesAsync();
        }

        [Command("list")]
        public async Task ListStreamers()
        {
            var guildId = this.Context.Guild.Id;
            var guild = await _db.Guilds.FindAsync(guildId);
            if (guild == null || guild.Streamers.Count == 0)
            {
                await ReplyAsync("No streamers being tracked");
            }
            else
            {
                await ReplyAsync("Tracking following streamers: " + Format.Sanitize(string.Join(", ", guild.Streamers)));
            }
        }

        [Command("setchannel")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetStreamChannel(IChannel channel)
        {
            var guildId = this.Context.Guild.Id;
            var guild = await _db.Guilds.FindAsync(guildId);
            if (guild == null)
            {
                guild = new Guild
                {
                    Id = guildId,
                    StreamChannelId = 0,
                    AnnounceRoleId = 0,
                    Streamers = new List<string>()
                };
                await _db.Guilds.AddAsync(guild);
                await _db.SaveChangesAsync();
            }
            guild.StreamChannelId = channel.Id;
            await _db.SaveChangesAsync();
            await ReplyAsync($"Stream channel set to <#{channel.Id}>");
        }

        [Command("setannouncer")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task SetAnnouncerRole(IRole role)
        {
            var guildId = this.Context.Guild.Id;
            var guild = await _db.Guilds.FindAsync(guildId);
            if (guild == null)
            {
                guild = new Guild
                {
                    Id = guildId,
                    StreamChannelId = 0,
                    AnnounceRoleId = 0,
                    Streamers = new List<string>()
                };
                await _db.Guilds.AddAsync(guild);
                await _db.SaveChangesAsync();
            }
            if (!role.IsMentionable)
            {
                await ReplyAsync("WARNING: Role is not mentionable");
            }
            guild.AnnounceRoleId = role.Id;
            await _db.SaveChangesAsync();
            await ReplyAsync($"Announce Role set to: {role.Name}");
        }

        [Command("clearannouncer")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ClearAnnouncerRole()
        {
            var guildId = this.Context.Guild.Id;
            var guild = await _db.Guilds.FindAsync(guildId);
            if (guild == null)
            {
                guild = new Guild
                {
                    Id = guildId,
                    StreamChannelId = 0,
                    AnnounceRoleId = 0,
                    Streamers = new List<string>()
                };
                await _db.Guilds.AddAsync(guild);
                await _db.SaveChangesAsync();
            }
            guild.AnnounceRoleId = 0;
            await _db.SaveChangesAsync();
            await ReplyAsync("Announce Role cleared");
        }

        [Command("testembed")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task TestEmbed()
        {
            var streamEvent = new Models.Twitch.StreamEvent
            {
                Id = "1",
                UserId = "133549408",
                UserName = "rutokz",
                GameId = "32399",
                Type = "live",
                Title = "Test Stream Embed",
                ViewerCount = 99999,
                StartedAt = DateTime.Now,
                Language = "en",
                ThumbnailUrl = "https://static-cdn.jtvnw.net/ttv-boxart/Counter-Strike:%20Global%20Offensive-1280x720.jpg"
            };
            await ReplyAsync("Test", embed: await _bot.CreateStreamEmbed(streamEvent));
        }
    }
}