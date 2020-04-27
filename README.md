# Pixel Discord Bot

> Ideally this bot should be self-hosted, so by supplying your own Twitch token for the bot, you shouldn't worry about hitting the ~10k limit for twitch

The project has quite some inconsistencies but for now the goal was just to make it work.

Maybe one day this project will be rewritten with nicer code but for now, woot it works

## Twitch protocol: [doc](./TWITCH.md)

# Docker
```
docker build -t pixelbot .`
`docker run -it -p 5000:5000 -v $(pwd)/Data:/app/Data --rm --name pixeldiscordbot pixelbot
```