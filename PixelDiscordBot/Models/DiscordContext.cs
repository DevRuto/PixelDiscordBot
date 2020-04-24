using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PixelDiscordBot.Models.Discord;

namespace PixelDiscordBot.Models
{
    public class DiscordContext : DbContext
    {
        public DbSet<Guild> Guilds { get; set; }
        public DbSet<Streamer> Streamers { get; set; }

        public DiscordContext(DbContextOptions<DiscordContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Guild>()
                .Property(g => g.Streamers)
                .HasConversion(v => string.Join(',', v), v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
            modelBuilder.Entity<Guild>().ToTable("Guild");
            modelBuilder.Entity<Streamer>().ToTable("Streamer");
        }
    }
}