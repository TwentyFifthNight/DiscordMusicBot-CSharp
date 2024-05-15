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
 4. Rename example.appsettings.json to appsettings.json and paste your discord token into the token field. Configure other fields in the appsettiings.json based on the contents of your application.yml file used in Lavalink.
 5. Build Bot application using Visual Studio or Command Prompt.
 6. Run Lavalink.
 7. Run the Bot application.

## Inviting Bot to your Discord server
 To invite bot go to the [Discord Developer page](https://discord.com/developers/applications/), select your bot and  go to the OAuth2 tab. Scroll down and check the **applications.commands** and **bot** checkboxes. Bot permissions should have appeard. Check **Read Messages/View Channels**, **Send Messages**, **Manage Messages**, **Embed Links**, **Read Message History**, **Add Reactions**, **Connect** and **Speak** permissons checkboxes. Copy generated URL and paste it into your browser. Choose server and click **Authorize** button.

 ## Issues
  If your bot won't play music and you can see "Video returned by YouTube isn't what was requested" exception in your Lavalink, you need to download the patched version of Lavalink from the [Lavalink 
support Discord](https://lavalink.dev/#need-help).
