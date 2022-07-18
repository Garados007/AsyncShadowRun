namespace AsyncShadowRun.Configs;

public class TwitterConfig
{
    public ulong? Channel { get; set; }

    public ulong? Webhook { get; set; }

    public Dictionary<string, System.Text.Json.JsonElement> Configs { get; set; } = new();
}