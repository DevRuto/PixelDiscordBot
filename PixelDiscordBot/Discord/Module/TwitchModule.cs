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

        public TwitchModule(DiscordContext db, TwitchService twitch)
        {
            _db = db;
            _twitch = twitch;
        }

        [Command("ping")]
        public Task PingAsync()
        {
            return ReplyAsync("Pong");
        }

        [Command("add")]
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
                await ReplyAsync($"{username} is already added");
                return;
            }
            else
            {
                guild.Streamers.Add(username);
                if (await _db.Streamers.FindAsync(userId) == null)
                {
                    await _db.Streamers.AddAsync(new Streamer
                    {
                        Id = userId,
                        Username = username
                    });
                    await _twitch.Subscribe(userId, username);
                }
                await ReplyAsync($"**{username}** added");
            }

            await _db.SaveChangesAsync();
        }

        [Command("remove")]
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
                    await ReplyAsync($"**{username}** removed");
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
                await ReplyAsync("Tracking following streamers: " + string.Join(", ", guild.Streamers));
            }
        }

        [Command("setchannel")]
        public async Task SetStreamChannel(IChannel channel)
        {
            var guildId = this.Context.Guild.Id;
            var guild = await _db.Guilds.FindAsync(guildId);
            guild.StreamChannelId = channel.Id;
            await _db.SaveChangesAsync();
            await ReplyAsync($"Stream channel set to <#{channel.Id}>");
        }
    }
}