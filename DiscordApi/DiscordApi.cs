namespace DiscordApi
{
    using System;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Discord;
    using Discord.WebSocket;

    public class DiscordApi
    {
        private readonly Subject<Message> messageRecievedSubject;

        private DiscordSocketClient client;

        public DiscordApi()
        {
            this.messageRecievedSubject = new Subject<Message>();
            this.WhenMessageRecieved = this.messageRecievedSubject.AsObservable();
        }

        public bool MessagingDisabled { get; set; } = false;

        public IObservable<Message> WhenMessageRecieved { get; }

        public async Task EmbedObjectAsync(ulong channel, Embed eb, bool bypassLock = false)
        {
            if (this.MessagingDisabled && !bypassLock)
            {
                return;
            }

            var discordChannel =
                this.client.Guilds.SelectMany(g => g.TextChannels).FirstOrDefault(c => c.Id == channel);
            if (discordChannel == null)
            {
                await Console.Error.WriteLineAsync($"Discord channel could not be found: {channel}")
                             .ConfigureAwait(false);
                return;
            }

            await discordChannel.SendMessageAsync(string.Empty, false, eb).ConfigureAwait(false);
        }

        public async Task SendMessageAsync(ulong channel, string message, bool bypassLock = false)
        {
            if (this.MessagingDisabled && !bypassLock)
            {
                return;
            }

            var discordChannel =
                this.client.Guilds.SelectMany(g => g.TextChannels).FirstOrDefault(c => c.Id == channel);
            if (discordChannel == null)
            {
                await Console.Error.WriteLineAsync($"Discord channel could not be found: {channel}")
                             .ConfigureAwait(false);
                return;
            }

            await discordChannel.SendMessageAsync(message).ConfigureAwait(false);
        }

        public Task SendMessageAsync(ulong channel, string username, string message, bool bypassLock = false)
        {
            return this.SendMessageAsync(channel, $"`{username}`  {message}", bypassLock);
        }

        public async Task StartAsync(string token)
        {
            this.client = new DiscordSocketClient();
            this.client.Log += this.LogAsync;
            this.client.MessageReceived += this.MessageReceivedAsync;
            this.client.GuildAvailable += this.GuildAvailableAsync;

            await this.client.LoginAsync(TokenType.Bot, token).ConfigureAwait(false);
            await this.client.StartAsync().ConfigureAwait(false);
        }

        private string DecomposeContent(SocketMessage sockMsg)
        {
            var content = sockMsg.Content;
            content = this.ReplaceUsers(sockMsg, content);
            content = this.ReplaceGroups(sockMsg, content);
            content = this.ReplaceChannels(sockMsg, content);

            return content;
        }

        private Task GuildAvailableAsync(SocketGuild arg)
        {
            return Task.CompletedTask;
        }

        private Task LogAsync(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task MessageReceivedAsync(SocketMessage sockMsg)
        {
            var content = this.DecomposeContent(sockMsg);
            var message = new Message
                          {
                              Author = new Author { Id = sockMsg.Author.Id, Name = sockMsg.Author.Username },
                              Channel = new Channel { Id = sockMsg.Channel.Id, Name = sockMsg.Channel.Name },
                              Text = content
                          };
            this.messageRecievedSubject.OnNext(message);
            return Task.CompletedTask;
        }

        private string ReplaceChannels(SocketMessage sockMsg, string content)
        {
            var channelMentions = Regex.Matches(content, "<#\\d*?>");
            foreach (Match match in channelMentions)
            {
                var idStr = match.Value.Replace("<#", string.Empty).Replace(">", string.Empty);
                var id = ulong.Parse(idStr);
                var channel = sockMsg.MentionedChannels.FirstOrDefault(u => u.Id == id);
                if (channel != null)
                {
                    content = content.Replace(match.Value, $"#{channel.Name}");
                }
            }

            return content;
        }

        private string ReplaceGroups(SocketMessage sockMsg, string content)
        {
            var roleMentions = Regex.Matches(content, "<@&\\d*?>");
            foreach (Match match in roleMentions)
            {
                var idStr = match.Value.Replace("<@&", string.Empty).Replace(">", string.Empty);
                var id = ulong.Parse(idStr);
                var role = sockMsg.MentionedRoles.FirstOrDefault(u => u.Id == id);
                if (role != null)
                {
                    content = content.Replace(match.Value, $"@{role.Name}");
                }
            }

            return content;
        }

        private string ReplaceUsers(SocketMessage sockMsg, string content)
        {
            var userMentions = Regex.Matches(content, "<@\\d*?>");
            foreach (Match match in userMentions)
            {
                var idStr = match.Value.Replace("<@", string.Empty).Replace(">", string.Empty);
                var id = ulong.Parse(idStr);
                var user = sockMsg.MentionedUsers.FirstOrDefault(u => u.Id == id);
                if (user != null)
                {
                    content = content.Replace(match.Value, $"@{user.Username}");
                }
            }

            return content;
        }
    }
}
