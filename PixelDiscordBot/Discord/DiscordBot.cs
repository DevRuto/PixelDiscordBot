using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PixelDiscordBot.Models;

namespace PixelDiscordBot.Discord
{
    public class DiscordBot
    {
        private Config _config;
        private ILogger _logger;

        public DiscordBot(Config config, ILogger<DiscordBot> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task Start()
        {
            var services = ConfigureServices();
            var client = services.GetRequiredService<DiscordSocketClient>();

            await client.LoginAsync(TokenType.Bot, _config.Discord.Token);
            await client.StartAsync();

            await services.GetRequiredService<CommandHandlingService>().InitializeAsync();

            _logger.LogInformation("Discord Bot started");
        }

        private ServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                .AddSingleton<HttpClient>()
                .BuildServiceProvider();
        }
    }
}