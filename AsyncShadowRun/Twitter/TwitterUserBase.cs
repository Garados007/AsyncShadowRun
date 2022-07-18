using System.Text.Json;

namespace AsyncShadowRun.Twitter;

public abstract class TwitterUserBase
{
    public Controller Controller { get; }

    public abstract DateTime NextPostTime { get; }

    public abstract Guid Id { get; }

    public abstract string Name { get; }

    public abstract string AvatarUrl { get; }

    public TwitterUserBase(Controller controller)
    {
        Controller = controller;
    }

    protected T? GetConfig<T>()
        where T : notnull
    {
        if (!Controller.Config.Twitter.Configs.TryGetValue(Id.ToString(), out JsonElement json))
            return default;
        
        return JsonSerializer.Deserialize<T>(json);
    }

    protected async Task SetConfig<T>(T value)
        where T : notnull
    {
        Controller.Config.Twitter.Configs[Id.ToString()] =
            JsonSerializer.SerializeToElement(value);
        
        await Controller.Config.Save();
    }

    protected async Task Post(string message)
    {
        if (Controller.WebhookClient != null)
            await Controller.WebhookClient.SendMessageAsync(
                text: message,
                username: Name,
                avatarUrl: AvatarUrl
            );
    }

    public abstract Task Execute();
}