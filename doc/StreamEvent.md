# How to subscribe/unsubscribe to stream notifications?

## Basic flow:
1. POST to `https://api.twitch.tv/helix/webhooks/hub`

### **POST** to https://api.twitch.tv/helix/webhooks/hub

JSON Body
```JSON
{
    "hub.callback": "http://localhost:3000/twitch",
    "hub.mode": "subscribe",
    "hub.topic": "https://api.twitch.tv/helix/streams?user_id=5678",
    "hub.lease_seconds": 864000,
    "hub.secret": "my secret"
}
```
### Notes
* `hub.topic` is the event to subscribe to (specific streams in this case)
    * Change `user_id` query value to the id of the user you wish to get stream notifications for
* `hub.mode` should be `subscribe` or `unsubscribe`, whichever your intent is
* `hub.lease_seconds` is the length in seconds for how long the subscription will last, max of 10 days (864000 seconds)
* `hub.callback` is the URL that twitch will **POST** to with the following JSON body
    * Each user should have their own callback URL since the offline event doesn't directly indicate the user going offline
    * **IMPORTANT** - Make sure you respond with a 2xx HTTP code
> If stream went offline
```JSON
{
   "data": []
}
```
> If stream went online OR title changes OR game changes (dummy data below)
```JSON
{
    "data": [
        {
            "id": "0123456789",
            "user_id": "5678",
            "user_name": "rutokz",
            "game_id": "21779",
            "community_ids": [],
            "type": "live",
            "title": "Best Stream Ever",
            "viewer_count": 500,
            "started_at": "2017-12-01T10:09:45Z",
            "language": "en",
            "thumbnail_url": "http://twitch/thumbnail"
        }
    ]
}
```

## Confirming Subscription
> https://dev.twitch.tv/docs/api/webhooks-guide#subscriptions

After your **POST**, you will receive a **GET** request to `hub.callback` with a `hub.challenge` query parameter.
You should send a simple `text/plain` response with the **value** of `hub.challenge`