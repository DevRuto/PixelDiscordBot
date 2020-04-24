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
        private IServiceCollection _serviceCollection;
        private DiscordSocketClient _client;

        public DiscordBot(Config config, ILogger<DiscordBot> logger, IServiceCollection serviceCollection)
        {
            _config = config;
            _logger = logger;
            _serviceCollection = serviceCollection;
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
    }
}