using System.Text.Json;
using Humanizer;

namespace PixelDiscordBot.Serialization
{
    public class SnakeCaseNamingPolicy : JsonNamingPolicy
    {
        public override string ConvertName(string name)
        {
            return name.Underscore();
        }        
    }
}