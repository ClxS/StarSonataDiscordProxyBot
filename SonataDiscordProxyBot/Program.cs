namespace SonataDiscordProxyBot
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Discord;

    using DiscordApi;

    using Newtonsoft.Json;

    using SonataDiscordProxyBot.ExtensionMethods;

    using StarSonataApi;
    using StarSonataApi.Messages.Incoming;
    using StarSonataApi.Objects;

    public class Program
    {
        private readonly SemaphoreSlim loginSemaphore = new SemaphoreSlim(1);

        private DateTime? lastError;

        private DateTime startTime;

        private enum EAppState
        {
            WaitingForLogin,

            ManuallyStopped,

            Ready,
        }

        private EAppState AppState { get; set; } = EAppState.WaitingForLogin;

        private EAppState PreStoppedState { get; set; }

        public static void Main(string[] args)
        {
            new Program().MainAsync().GetAwaiter().GetResult();
        }

        public async Task MainAsync()
        {
            this.startTime = DateTime.UtcNow;
            var settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText("config.json"));

            var discordApi = new DiscordApi();
            await discordApi.StartAsync(settings.DiscordToken).ConfigureAwait(false);

            var ssApi = new StarSonataApi();
            ssApi.SetServerUrl(settings.ServerUrl);
            ssApi.Initialise();
            this.TryGameLoginAsync(ssApi, discordApi, settings).Forget();

            discordApi.WhenMessageRecieved.Subscribe(
                msg =>
                {
                    if (msg.Author.Id == settings.BotDiscordId)
                    {
                        return;
                    }

                    if (msg.Channel.Id == settings.CommandChannel)
                    {
                        this.HandleCommands(settings, discordApi, ssApi, msg.Text);
                        return;
                    }

                    if (this.AppState != EAppState.Ready)
                    {
                        return;
                    }

                    var channelMapping =
                        settings.ChannelMappings.Discord.FirstOrDefault(c => c.Discord == msg.Channel.Id);
                    if (channelMapping == null)
                    {
                        return;
                    }

                    try
                    {
                        // ssApi.SendImpersonationChatAsync(channelMapping.Game, msg.Author.Name, msg.Text);
                        ssApi.SendChatAsync($"<{msg.Author.Name}>: {msg.Text}", (MessageChannel)Enum.Parse(typeof(MessageChannel), channelMapping.Game));
                    }
                    catch (Exception)
                    {
                        lock (this)
                        {
                            this.lastError = DateTime.Now;
                        }

                        discordApi.SendMessageAsync(
                            settings.ErrorChannel,
                            "Error: Failed to send a message. Likely disconnected").Forget();

                        this.TryGameLoginAsync(ssApi, discordApi, settings).Forget();
                    }
                });

            ssApi.WhenMessageReceived.Where(m => !string.IsNullOrEmpty((m as TextMessage)?.Message?.Username))
                 .Subscribe(
                     m =>
                     {
                         var msg = (TextMessage)m;
                         var channelMapping =
                             settings.ChannelMappings.Game.FirstOrDefault(
                                 c => c.Game == msg.Message.Channel.ToString());
                         if (channelMapping == null)
                         {
                             return;
                         }

                         try
                         {
                             discordApi.SendMessageAsync(
                                 channelMapping.Discord,
                                 msg.Message.Username,
                                 msg.Message.Message).Forget();
                         }
                         catch (Exception e)
                         {
                             Console.Error.WriteLine("Error sending discord message. " + e.Message);
                         }
                     });

            await Task.Delay(-1).ConfigureAwait(false);
        }

        private void HandleCommands(Settings settings, DiscordApi discordApi, StarSonataApi ssApi, string commandText)
        {
            if (commandText.Equals("!wtfkill", StringComparison.OrdinalIgnoreCase))
            {
                discordApi.SendMessageAsync(settings.CommandChannel, "`:( Okay`", true).Forget();
                Thread.Sleep(1000);
                Environment.Exit(-1);
            }

            if (commandText.Equals("!kill", StringComparison.OrdinalIgnoreCase))
            {
                discordApi.MessagingDisabled = true;
                this.PreStoppedState = this.AppState;
                this.AppState = EAppState.ManuallyStopped;
                discordApi.SendMessageAsync(settings.CommandChannel, "`Processing stopped`", true).Forget();
            }

            if (commandText.Equals("!continue", StringComparison.OrdinalIgnoreCase)
                && this.AppState == EAppState.ManuallyStopped)
            {
                discordApi.MessagingDisabled = false;
                this.AppState = this.PreStoppedState;
                discordApi.SendMessageAsync(settings.CommandChannel, "`Processing started`", true).Forget();
                if (this.AppState == EAppState.WaitingForLogin)
                {
                    this.TryGameLoginAsync(ssApi, discordApi, settings).Forget();
                }
            }

            if (commandText.Equals("!help"))
            {
                var eb = new EmbedBuilder();
                eb.WithTitle("Commands");
                eb.AddInlineField("!status", "Returns the current bot status.");
                eb.AddInlineField("!kill", "Stop processing. Use this if the bot is messing up.");
                eb.AddInlineField("!continue", "Continue processing. Use this to continue after a !kill command.");
                eb.AddInlineField(
                    "!wtfkill",
                    "Will kill the bots process. Only use this if !kill doesn't work no one is around who can fix it.");
                discordApi.EmbedObjectAsync(settings.CommandChannel, eb.Build(), true).Forget();
            }

            if (commandText.Equals("!status"))
            {
                var eb = new EmbedBuilder();
                eb.WithTitle("Bot Status");
                eb.AddInlineField("Status", this.AppState.ToString());
                eb.AddInlineField("Uptime", (DateTime.Now - this.startTime).ToPrettyFormat());
                lock (this)
                {
                    if (this.lastError.HasValue)
                    {
                        eb.AddInlineField(
                            "Last Error",
                            (DateTime.Now - this.lastError.Value).ToPrettyFormat() + " ago");
                    }
                }

                discordApi.EmbedObjectAsync(settings.CommandChannel, eb.Build(), true).Forget();
            }
        }

        private Task TryGameLoginAsync(StarSonataApi ssApi, DiscordApi discordApi, Settings settings)
        {
            return Task.Run(
                async () =>
                {
                    var connected = false;
                    var semaphoreEntered = false;
                    try
                    {
                        semaphoreEntered = await this.loginSemaphore.WaitAsync(0).ConfigureAwait(false);
                        if (semaphoreEntered)
                        {
                            for (var i = 0; i < 10; ++i)
                            {
                                if (this.AppState == EAppState.ManuallyStopped)
                                {
                                    return;
                                }

                                this.AppState = EAppState.WaitingForLogin;

                                try
                                {
                                    ssApi.TryLoginAsync(settings.GameUsername, settings.GamePassword).Wait();
                                }
                                catch (Exception)
                                {
                                    // ignored
                                }

                                if (!ssApi.IsConnected)
                                {
                                    lock (this)
                                    {
                                        this.lastError = DateTime.Now;
                                    }

                                    discordApi.SendMessageAsync(
                                                  settings.ErrorChannel,
                                                  $"Error: Login attempt {i + 1} of 10 failed. Trying again in 30 seconds.")
                                              .Forget();
                                    Task.Delay(TimeSpan.FromSeconds(1)).Wait();
                                }
                                else
                                {
                                    connected = true;
                                    if (this.AppState == EAppState.ManuallyStopped)
                                    {
                                        this.PreStoppedState = EAppState.Ready;
                                    }
                                    else
                                    {
                                        this.AppState = EAppState.Ready;
                                    }

                                    break;
                                }
                            }

                            if (!connected)
                            {
                                lock (this)
                                {
                                    this.lastError = DateTime.Now;
                                }

                                await discordApi.SendMessageAsync(
                                                    settings.ErrorChannel,
                                                    "Error: Was unable to establish a connection. Will retry in 5 minutes")
                                                .ConfigureAwait(false);
                                Task.Delay(TimeSpan.FromMinutes(0.1)).ContinueWith(
                                        t => { this.TryGameLoginAsync(ssApi, discordApi, settings).Forget(); })
                                    .Forget();
                            }
                        }
                    }
                    finally
                    {
                        if (semaphoreEntered)
                        {
                            this.loginSemaphore.Release();
                        }
                    }
                });
        }
    }
}
