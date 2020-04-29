# Pixel Discord Bot

> Ideally this bot should be self-hosted, so by supplying your own Twitch token for the bot, you shouldn't worry about hitting the ~10k limit for twitch

The project has quite some inconsistencies but for now the goal was just to make it work.

Maybe one day this project will be rewritten with nicer code but for now, woot it works

## Twitch protocol: [doc](./TWITCH.md)

# Config

1. Enter the `PixelDiscordBot` directory
2. Rename or copy `config.sample.json` to `config.json`
3. Fill in the values for `config.json`
4. Run the command: `dotnet publish -c Release -o Release`
5. Go to the `Release` directory
6. Run `dotnet PixelDiscordBot.dll --urls="http://localhost:8090"`

# Twitch Commands

* `!twitch setchannel #streams` - Sets the stream announcement channel to #streams
* `!twitch list` - list streamers
* `!twitch add <streamer>` - add streamer
* `!twitch remove <streamer>` - remove streamer

# Docker

> You need to follow the Config step 1-3 and link the Data directory in the `-v` arg in the docker command

```
docker build -t pixelbot .`
`docker run -it -p 5000:5000 -v $(pwd)/Data:/app/Data --rm --name pixeldiscordbot pixelbot
```
