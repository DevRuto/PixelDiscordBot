# Get User
> We'll use this to get the id of the user when given a username

> Username's user id won't change so we should cache this (maybe per instance)

## **GET** https://api.twitch.tv/helix/users
> Query Parameters
* `id` - ID of the game

## Response
```JSON
{
    "data": [
        {
            "id": "133549408",
            "login": "rutokz",
            "display_name": "RutoKZ",
            "type": "",
            "broadcaster_type": "affiliate",
            "description": "",
            "profile_image_url": "https://static-cdn.jtvnw.net/jtv_user_pictures/425cbb02-f9df-492b-865f-95cd92586e3c-profile_image-300x300.png",
            "offline_image_url": "",
            "view_count": 1417
        }
    ]
}
```