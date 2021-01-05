# StarSonata Discord Proxy Bot

Allows you to setup a 2 way mapping between a Star Sonata chat channel and a Discord channel. This has been used internally by Star Revolution X for a while.

## Please do not use it to send messages to All/Help/Trade/LFG - as that'll be punished as spam. I've released it with the intention that people will use it for linking their team chat to team Discord.

## Known Issues

- The bot will not reconnect to the server on losing connection. I'm working on it but no idea when I'll actually get around to it. For now I suggest wrapping the execution in a batch script which will relaunch the bot whenever the previous process dies. Preferably with a few minutes delay.

## How to Use

1) Download and install the .NET Core 2.0 Runtime from here: https://dotnet.microsoft.com/download/dotnet-core/2.0

2) Download the latest release from here: https://github.com/ClxS/StarSonataDiscordProxyBot/releases/

3) Extract the downloaded release.

4) Open config.json in a text editor and fill in the fields.

    **BotDiscordId** is ID for your Discord Bot user: https://maah.gitbooks.io/discord-bots/content/getting-started/fetching-users-and-members.html

    D**iscordToken** is the Discord auth token for you bot: https://www.writebots.com/discord-bot-token/#:~:text=A%20Discord%20Bot%20Token%20is,generate%20a%20Discord%20Bot%20Token.

    **GameUsername** is the in-game username for your bot account.
    
    **GamePassword** is the in-game password for your bot account.
    
    **CharacterName** is the in-game name for the character to use on your bot account.
    
    **CommandChannel** is the ID of the Discord channel which should access admin commands.
    
    **ErrorChannel** is the ID of the Discord channel which should receive errors.
    
    **ChannelMappings** is the bindings between in-game and Discord channels. For example:
```json
"ChannelMappings": {
	"Game": [
		{
			"Game": "Team",
			"Discord": 000
		}
	],
	"Discord": [
		{
			"Discord": 000,
			"Game": "Team"
		}
	]
}
```

5) Open a command prompt in the folder containing your extracted binaries. Run `dotnet SonataDiscordProxyBot.dll`
