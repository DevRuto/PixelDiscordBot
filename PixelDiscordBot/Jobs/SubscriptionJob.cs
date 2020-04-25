using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Cronos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PixelDiscordBot.Models;
using PixelDiscordBot.Services;

namespace PixelDiscordBot.Jobs
{
    // https://codeburst.io/schedule-cron-jobs-using-hostedservice-in-asp-net-core-e17c47ba06
    public class SubscriptionJob : IHostedService, IDisposable
    {
        private System.Timers.Timer _timer;
        private ILogger<SubscriptionJob> _logger;
        private TwitchService _twitch;

        public SubscriptionJob(ILogger<SubscriptionJob> logger, TwitchService twitch)
        {
            _logger = logger;
            _twitch = twitch;
        }
        
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[JOB] Starting Subscription job");
            await ScheduleJob(cancellationToken);
        }

        public async Task ScheduleJob(CancellationToken cancellationToken)
        {
            var cronExp = CronExpression.Parse("0 0 * * *");
            var next = cronExp.GetNextOccurrence(DateTimeOffset.Now, TimeZoneInfo.Local);
            if (next.HasValue)
            {
                var delay = next.Value - DateTimeOffset.Now;
                _timer = new System.Timers.Timer(delay.TotalMilliseconds);
                _timer.Elapsed += async (sender, args) =>
                {
                    _timer.Dispose();
                    _timer = null;

                    if (!cancellationToken.IsCancellationRequested)
                        await Job(cancellationToken);
                    
                    if (!cancellationToken.IsCancellationRequested)
                        await ScheduleJob(cancellationToken);
                };
                _timer.Start();
            }
            await Task.CompletedTask;
        }

        public async Task Job(CancellationToken token)
        {
            _logger.LogInformation("[JOB] Running renewal job");
            await _twitch.RenewSubscriptions();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[JOB] Stopping Subscription job");
            _timer?.Stop();
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _logger.LogInformation("[JOB] Disposing Subscription job");
            _timer?.Dispose();
        }
    }
}