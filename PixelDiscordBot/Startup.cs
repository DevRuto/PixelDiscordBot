using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PixelDiscordBot.Discord;
using PixelDiscordBot.Jobs;
using PixelDiscordBot.Models;
using PixelDiscordBot.Serialization;
using PixelDiscordBot.Services;

namespace PixelDiscordBot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                PropertyNameCaseInsensitive = true
            };
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText("./Data/config.json"), jsonOptions);
            services.AddSingleton(config);

            services.AddDbContext<DiscordContext>(options =>
            {
                // options.UseInMemoryDatabase("Discord");
                options.UseSqlite("Data Source=./Data/discordbot.sqlite3");
            });

            services.AddSingleton<IServiceCollection>(services);

            services.AddSingleton<TwitchService>();
            services.AddSingleton<DiscordBot>();

            services.AddHostedService<SubscriptionJob>();

            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = jsonOptions.PropertyNamingPolicy;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                    options.JsonSerializerOptions.Converters.Add(new LongConverter());
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DiscordBot bot, DiscordContext ctx)
        {
            ctx.Database.EnsureCreated();
            var queries = new string[] {
                "ALTER TABLE Guild ADD COLUMN AnnounceRoleId INTEGER DEFAULT 0 NOT NULL",
                "ALTER TABLE Guild ADD COLUMN EnableVods INTEGER DEFAULT 0 NOT NULL",
                "ALTER TABLE Guild ADD COLUMN VodChannelId INTEGER DEFAULT 0 NOT NULL",
            };
            foreach (var query in queries)
            {
                try
                {
                    ctx.Database.ExecuteSqlRaw(query);
                } catch {}
            }
            bot.Start();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
