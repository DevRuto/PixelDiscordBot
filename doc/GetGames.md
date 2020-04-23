# Get Games
> We'll use this to get the name of the game when given a game ID

> We should cache this in the application, no need to repeated calls for the same game

## **GET** https://api.twitch.tv/helix/games
> Query Parameters
* `id` - ID of the game

## Response
```JSON
{
    "data": [
        {
            "id": "32399",
            "name": "Counter-Strike: Global Offensive",
            "box_art_url": "https://static-cdn.jtvnw.net/ttv-boxart/./Counter-Strike:%20Global%20Offensive-{width}x{height}.jpg"
        }
    ]
}
```