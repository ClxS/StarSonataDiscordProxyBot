# StarSonata Discord Proxy Bot

Allows you to setup a 2 way mapping between a Star Sonata chat channel and a Discord channel. This has been used internally by Star Revolution X for a while.

## Known Issues

- The bot will not reconnect to the server on losing connection. I'm working on it but no idea when I'll actually get around to it. For now I suggest wrapping the execution in a batch script which will relaunch the bot whenever the previous process dies. Preferably with a few minutes delay.
- Some people report issues resolving System.Reactive. Either try: Restarting Visual Studio; or if that fails, remove and re-add the System.Reactive nuget package.
