namespace SonataDiscordProxyBot
{
    public class Settings
    {
        public ulong BotDiscordId { get; set; }

        public Mappings ChannelMappings { get; set; }

        public ulong CommandChannel { get; set; }

        public string DiscordToken { get; set; }

        public ulong ErrorChannel { get; set; }

        public string GamePassword { get; set; }

        public string GameUsername { get; set; }

        public string CharacterName { get; set; }

        public string ServerUrl { get; set; }
    }
}
