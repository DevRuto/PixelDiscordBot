using System;
using System.Threading.Tasks;
using Discord.Commands;

namespace PixelDiscordBot.Discord.Module
{
    public class TwitchModule : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public Task PingAsync()
        {
            return ReplyAsync("Pong");
        } 
    }
}