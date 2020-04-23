# OAuth

Reference: https://discuss.dev.twitch.tv/t/requiring-oauth-for-helix-twitch-api-endpoints/23916

Starting May 1st, 2020, All Helix endpoints will need Client ID matching with an OAuth token in the headers

Example from the post:

```bash
curl -H 'Authorization: Bearer cfabdegwdoklmawdzdo98xt2fo512y' \
-H Client-ID: uo6dggojyb8d6soh92zknwmi5ej1q2' \
-X GET 'https://api.twitch.tv/helix/users?id=44322889'
```

Out of the various ways to obtain an OAuth token, we will use the [OAuth Client Credentials Flow](https://dev.twitch.tv/docs/authentication/getting-tokens-oauth/#oauth-client-credentials-flow)

### **POST** to `https://id.twitch.tv/oauth2/token` with the following **required query** parameters
* `client_id`
* `client_secret`
* `grant_type` - must be `client_credentials`

Example from Twitch API Doc
```
POST https://id.twitch.tv/oauth2/token?client_id=uo6dggojyb8d6soh92zknwmi5ej1q2&client_secret=nyo51xcdrerl8z9m56w9w6wg&grant_type=client_credentials`
```

## JSON Reponse (example from Twitch API)
```JSON
{
  "access_token": "<user access token>",
  "refresh_token": "",
  "expires_in": <number of seconds until the token expires>,
  "scope": ["<your previously listed scope(s)>"],
  "token_type": "bearer"
}
```

### Notes
* We'll be using `access_token`, which is our OAuth token
* We will need to get a new token when the token expires, indicated by `expires_in` in seconds