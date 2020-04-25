using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using PixelDiscordBot.Discord;
using PixelDiscordBot.Models;
using PixelDiscordBot.Models.Twitch;
using PixelDiscordBot.Services;

namespace PixelDiscordBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TwitchController : Controller
    {
        private TwitchService _service;
        private DiscordBot _discord;
        private DiscordContext _db;

        public TwitchController(TwitchService service, DiscordBot discord, DiscordContext ctx)
        {
            _service = service;
            _discord = discord;
            _db = ctx;
        }

        [HttpGet("debug")]
        public async Task<IActionResult> Test()
        {
            return Ok(new object[]
            {
                await _db.Streamers.ToListAsync(),
                await _db.Guilds.ToListAsync()
            });
        }

        /// hub.challenge handler
        [HttpGet("{username}")]
        public IActionResult SubscriptionHandler(
            string username,
            [FromQuery(Name = "hub.challenge")] string challengeToken
        )
        {
            return Ok(challengeToken);
        }

        [HttpPost("{username}")]
        public async Task<IActionResult> StreamEventHandler(string username, [FromBody] Event streamEvents)
        {
            var guilds = await _db.Guilds.ToListAsync();
            var channelIds = guilds
                                .Where(guild => guild.Streamers.Contains(username))
                                .Select(guild => guild.StreamChannelId)
                                .ToArray();
            await _discord.HandleStreamEvent(username, streamEvents, channelIds);
            
            return Ok();
        }
    }

}