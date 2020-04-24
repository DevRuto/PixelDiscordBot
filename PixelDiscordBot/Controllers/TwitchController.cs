using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.AspNetCore.Mvc;
using PixelDiscordBot.Discord;
using PixelDiscordBot.Models;
using PixelDiscordBot.Services;

namespace PixelDiscordBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TwitchController : Controller
    {
        private TwitchService _service;
        private DiscordSocketClient _discord;

        public TwitchController(TwitchService service, DiscordBot discord)
        {
            _service = service;
            _discord = discord.GetClient();
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

        private static readonly Dictionary<string, List<RestUserMessage>> _messageCache = new Dictionary<string, List<RestUserMessage>>();

        [HttpPost("{username}")]
        public async Task<IActionResult> StreamEventHandler(string username, [FromBody] Event streamEvents)
        {
            if (!_messageCache.ContainsKey(username))
                _messageCache.Add(username, new List<RestUserMessage>());

            ulong channelId = 687151981403439114;
            var channel = (SocketTextChannel)_discord.GetChannel(channelId);
            if (streamEvents.Data.Length == 0)
            {
                // Offline
                await channel.SendMessageAsync("Stream went offline");

                // Delete message
                var messages = _messageCache[username];
                messages.ForEach(async message => await message.DeleteAsync());
                messages.Clear();
            }
            else
            {
                var streamEvent = streamEvents.Data[0];
                var messages = _messageCache[username];

                if (messages.Count == 0)
                {
                    // Streamer just went live
                    var message = await channel.SendMessageAsync($"[{username}] Stream went online - {streamEvent.StartedAt.ToLongDateString()}");
                    messages.Add(message);
                }
                else
                {
                    messages.ForEach(async message =>
                    {
                        await message.ModifyAsync(prop =>
                        {
                            prop.Content = $"[{username}] Stream updated - {streamEvent.Title} - {message.Id}";
                        });
                    });
                }

                // Stream went online OR title changed OR game changed
            }
            return Ok("Hello");
        }
    }
}