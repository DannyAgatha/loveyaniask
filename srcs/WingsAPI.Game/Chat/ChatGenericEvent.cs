// NosEmu
// 


using WingsEmu.Game._packetHandling;
using WingsEmu.Game._playerActionLogs;

namespace WingsEmu.Game.Chat;

public class ChatGenericEvent : PlayerEvent
{
    public string Message { get; init; }
    public ChatType ChatType { get; init; }
    public long? TargetCharacterId { get; init; }
}