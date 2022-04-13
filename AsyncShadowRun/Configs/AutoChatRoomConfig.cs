namespace AsyncShadowRun.Configs;

public class AutoChatRoomConfig
{
    public ulong? Category { get; set; }

    public List<Data.AutoChatRoom> Rooms { get; set; } = new List<Data.AutoChatRoom>();
}
