namespace SonataDiscordProxyBot
{
    using System.Collections.Generic;

    public class Mappings
    {
        // Discord > Game Mappings
        public IEnumerable<DiscordGameChannelMapping> Discord;

        // Game > Discord Mappings
        public IEnumerable<DiscordGameChannelMapping> Game;
    }
}
