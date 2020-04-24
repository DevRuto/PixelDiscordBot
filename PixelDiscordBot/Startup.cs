using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PixelDiscordBot.Discord;
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
            services.AddSingleton<TwitchService>();
            services.AddSingleton<DiscordBot>();

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = new SnakeCaseNamingPolicy(),
                PropertyNameCaseInsensitive = true
            };
            var config = JsonSerializer.Deserialize<Config>(File.ReadAllText("../config.json"), options);
            services.AddSingleton(config);

            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DiscordBot bot)
        {
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
