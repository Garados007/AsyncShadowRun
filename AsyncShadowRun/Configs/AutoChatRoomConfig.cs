namespace AsyncShadowRun.Configs;

public class AutoChatRoomConfig
{
    public ulong? Category { get; set; }

    public ulong? AutoRoleRoom { get; set; }

    public List<Data.AutoChatRoom> Rooms { get; set; } = new List<Data.AutoChatRoom>();

    public Dictionary<ulong, Data.AutoChatMsg> RoomMsg { get; set; }
        = new Dictionary<ulong, Data.AutoChatMsg>();
}
