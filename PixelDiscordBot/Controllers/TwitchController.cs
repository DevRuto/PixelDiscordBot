using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PixelDiscordBot.Models;
using PixelDiscordBot.Services;

namespace PixelDiscordBot.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TwitchController : Controller
    {
        private TwitchService _service;

        public TwitchController(TwitchService service)
        {
            _service = service;
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
        public async Task<IActionResult> StreamEventHandler(string username, [FromBody] Event streamEvent)
        {
            if (streamEvent.Data.Length == 0)
            {
                // Offline
            }
            else
            {
                // Stream went online OR title changed OR game changed
            }
            return Ok();
        }
    }
}