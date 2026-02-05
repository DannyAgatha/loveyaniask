// NosEmu
// 


using System.Threading.Tasks;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Characters.Events;

public static class PlayerStrangeBehaviorExtensions
{
    public static async Task NotifyStrangeBehavior(this IClientSession session, StrangeBehaviorSeverity severity, string reason)
    {
        await session.EmitEventAsync(new StrangeBehaviorEvent(severity, reason));
    }
}

public class StrangeBehaviorEvent : PlayerEvent
{
    public StrangeBehaviorEvent(StrangeBehaviorSeverity severity, string reason)
    {
        Severity = severity;
        Reason = reason;
    }

    public StrangeBehaviorSeverity Severity { get; }
    public string Reason { get; }
}