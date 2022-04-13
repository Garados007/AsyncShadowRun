namespace AsyncShadowRun.Data;

public class AutoChatMsg
{
    public ulong MessageId { get; set; }

    public string[] RoomNames { get; set; } = Array.Empty<string>();
}