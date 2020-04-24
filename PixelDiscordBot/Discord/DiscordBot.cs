using System.Threading.Tasks;
using PixelDiscordBot.Models;

namespace PixelDiscordBot.Discord
{
    public class DiscordBot
    {
        private Config _config;

        public DiscordBot(Config config)
        {
            _config = config;
        }

        public async Task Start()
        {

        }
    }
}