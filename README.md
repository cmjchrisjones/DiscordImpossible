# DiscordImpossible

## Purpose

A proof of concept to schedule a message for automatic deletion based on a time period given

## Disclaimers

DISCLAIMER: This is a proof of concept app, it may not be the most performant or may have bugs - its use is purely educational - any use is at your own risk.

DISCLAIMER: There is a `clearChannelMessages` boolean propery set at the top of [program.cs](./DiscordImpossible/Program.cs) which if set to true, will clear all previous messages from the same channel used for announcing the bot (`announceChannel`).

I strongly urge you to create a new Discord "test/playground" server to dabble with this project - it is NOT production ready!

No liability is taken by me if you have failed to read the above disclaimers!

## Tech Stack

Built as a .NET 6 Console Application

## Create your bot

Head over to the [Discord Developer Portal](https://discord.com/developers/applications), click on the `New Applications`, give it a name, and hit `Create`

Then click `Bot` in the left hand menu, then the `Add Bot` button

## Add Bot Token to Environment Variable

We now need to add the bot token to an environment variable named `DiscordImpossibleBotToken`, or you can replace this line in `Program.cs` for it to be hardcoded.

```csharp
// Using Environment variable
static string token = Environment.GetEnvironmentVariable("DiscordImpossibleBotToken");
// Using hardcoded
static string token = "<your token>";
```

You'll also want to update the `announceChannel` variable with the channel ID you want to announce the bot in. 

There is a `clearChannelMessages` boolean propery set at the top of [program.cs](./DiscordImpossible/Program.cs) which if set to true, will clear all previous messages from the same channel used for announcing the bot (`announceChannel`).

## Permissions

The bot will require the following permissions:
    
    - Send Messages
    - Send Messages in Threads
    - Add Reactions
    - Manage Messages

You can use the Discord Permissions calculator website to generate a URL which, after adding your client ID in, will generate a link which when clicked on, will take you to a page and ask which server you want to install the bot onto. For quick reference, [this link](https://discordapi.com/permissions.html#274877917248) will have the relevant permissions already selected.

To get the Client ID, head over to the Discord developer portal, Click on `OAuth` and copy the value under `Client ID` 

## Using the bot

### ___Please make sure you have read all of the disclaimers at the top of this document - they're important - I promise, it'll only take 30 seconds and it might save you a nightmare!___

At the time of writing, the bot only supports two commands.

| Command | Action |
|---------|--------|
|`!!!`| issued by itself, will report the current status of the inflight queue. The inflight queue is where messages are tracked with an expiry time, when that time comes, the thread that is keeping track of the items in the queue will remove it. You'll see the message and the bots auto respond message disappear close to its scheduled time.|
|`!! {time in seconds}` <br />EG<br /> `!! 5 This is a test message`|The double bang (!!) means you want the message to 'self destruct', the time is the number of seconds you want the message to live for (maximum 3600 (an hour)).

## Known issues/bugs

### As I've already mentioned, this is a prrof of concept and therefore not production ready, hear are some of the things which require more work on:

- If the app crashes, then any messages in the queue for destruction won't be destroyed. This is due to us tracking these items in memory, this would better be used a data store of some description.
- Input time just takes in a number after the initial `!!` command, if its not a number it won't be scheduled for deletion, but no feedback to the user to confirm this
- Maybe there should be an upper time limit on messages (an hour maybe?)
- There maybe other unknown bugs which might cause the application to crash (most of any errors occuring shouldn't as the whole app is wrapped in a try/catch block but its a demo, so error logging/handling needs a ton of work)


Resources/Links used

| Name | Link |
|------|------|
|DSharpPlus|https://github.com/DSharpPlus/DSharpPlus|
|Guide to discord bot| https://www.writebots.com/how-to-make-a-discord-bot|
|Discord Permissions Generator|https://discordapi.com/permissions.html| 


## Why the name

It was a play on words from the Mission Impossible film franchise, where messages Tom Cruise receives will self destruct in 5 seconds!

