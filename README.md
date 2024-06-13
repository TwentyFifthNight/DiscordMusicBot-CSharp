# Discord Music Bot – CSharp

## Overview
 Music Bot for Discord using DSharpPlus, Lavalink4NET and Lavalink

## Requirements
 * .NET 8.0 runtime
 * [JDK 17.0.2](https://jdk.java.net/archive/)
 * [Lavalink 4.0.5](https://github.com/lavalink-devs/Lavalink/releases)
   

## Configuration
 1. To set up Lavalink, follow the instructions from the official [Lavalink website](https://lavalink.dev/getting-started/index.html).
 2. Go to the [Discord Developer page](https://discord.com/developers/applications/) and create a new app. Go to the Bot tab and press Reset Token button. Press the Copy button and save the token for later.
 3. Download the contents of this repository.
 4. Rename example.appsettings.json to appsettings.json and paste your discord token into the token field. Configure the remaining fields in the appsettiings.json file. Their values should be in the corresponding fields in the Lavalink configuration file.
 5. Build Bot application using Visual Studio or Command Prompt.
 6. Run Lavalink.
 7. Run the Bot application.

## Inviting Bot to your Discord server
 To invite bot go to the [Discord Developer page](https://discord.com/developers/applications/), select your bot and  go to the OAuth2 tab. Scroll down and check the **applications.commands** and **bot** checkboxes. Bot permissions should have appeard. Check **Read Messages/View Channels**, **Send Messages**, **Manage Messages**, **Embed Links**, **Read Message History**, **Add Reactions**, **Connect** and **Speak** permissons checkboxes. Copy generated URL and paste it into your browser. Choose server and click **Authorize** button.

## Lavalink Settings
 An example application.yml file can be found at this [link](https://github.com/lavalink-devs/Lavalink/blob/master/LavalinkServer/application.yml.example/). You can also copy my application.yml options. Remember to change the **port**, **address** and **password** to match the values in the **appsettings.json file**. 
```
server:
  port: 2333
  address: 0.0.0.0
  http2:
    enabled: false
plugins:
  youtube:
    enabled: true
    clients: ["MUSIC", "WEB"]
lavalink:
  plugins:
    - dependency: "dev.lavalink.youtube:youtube-plugin:1.1.0"
      snapshot: false
  server:
    password: "youshallnotpass"
    sources:
      youtube: false
      bandcamp: false
      soundcloud: true
      twitch: false
      vimeo: false
      nico: false
      http: false
      local: false
    filters:
      volume: true
      equalizer: true
      karaoke: true
      timescale: true
      tremolo: true
      vibrato: true
      distortion: true
      rotation: true
      channelMix: true
      lowPass: true
    bufferDurationMs: 1200
    frameBufferDurationMs: 10000
    opusEncodingQuality: 8
    resamplingQuality: LOW
    trackStuckThresholdMs: 10000
    useSeekGhosting: true
    youtubePlaylistLoadLimit: 6
    playerUpdateInterval: 5
    youtubeSearchEnabled: true
    soundcloudSearchEnabled: true
    gc-warnings: true
metrics:
  prometheus:
    enabled: false
    endpoint: /metrics

sentry:
  dsn: ""
  environment: ""

logging:
  file:
    path: ./logs/

  level:
    root: INFO
    lavalink: INFO

  request:
    enabled: true
    includeClientInfo: true
    includeHeaders: false
    includeQueryString: true
    includePayload: true
    maxPayloadLength: 10000


  logback:
    rollingpolicy:
      max-file-size: 1GB
      max-history: 30
```
