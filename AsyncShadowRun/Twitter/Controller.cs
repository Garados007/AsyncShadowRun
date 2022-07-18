using Discord;
using Discord.WebSocket;
using Discord.Rest;
using Discord.Webhook;

namespace AsyncShadowRun.Twitter;

public class Controller
{
    public Config Config { get; }

    public DiscordSocketClient Client { get; }

    public RestWebhook? Webhook { get; private set; }

    public DiscordWebhookClient? WebhookClient { get; private set; }

    public List<TwitterUserBase> User { get; } = new();

    public Controller(Config config, DiscordSocketClient client)
    {
        Config = config;
        Client = client;
    }

    public async Task Setup(SocketGuild guild)
    {
        Webhook = await SetupWebhook(guild);
        if (Webhook is not null)
        {
            WebhookClient = new Discord.Webhook.DiscordWebhookClient(Webhook);
            AddUser();
            _ = Task.Run(async () => await ExecuteLoop());
        }
    }

    private async Task<RestWebhook?> SetupWebhook(SocketGuild guild)
    {
        if (Config.Twitter.Channel is ulong twitterId)
        {
            var twitter = guild.GetTextChannel(twitterId);
            if (twitter is null)
                return null;
            if (Config.Twitter.Webhook is ulong webhookId)
            {
                var hook = await twitter.GetWebhookAsync(webhookId);
                if (hook is not null)
                    return hook;
            }
            var webhook = await twitter.CreateWebhookAsync("AsyncShadowRun Twitter Webhook");
            Config.Twitter.Webhook = webhook.Id;
            await Config.Save();
            return webhook;
        }
        else return null;
    }

    private async Task ExecuteLoop()
    {
        while (true)
        {
            var now = DateTime.UtcNow;
            foreach (var user in User.Where(x => x.NextPostTime <= now))
            {
                await user.Execute();
            }
            var min = User.Min(x => x.NextPostTime);
            var delay = min - DateTime.UtcNow;
            if (delay > TimeSpan.Zero)
                await Task.Delay(delay);
        }
    }

    private void AddUser()
    {
        User.Add(new User.Rewe(this));
    }
}
