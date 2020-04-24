using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PixelDiscordBot.Serialization
{
    public class LongConverter : JsonConverter<ulong>
    {
        public override ulong Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert == typeof(ulong));
            return ulong.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, ulong value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}