// NosEmu
// 


using System;
using System.Threading.Tasks;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Characters.Events;

public static class CharacterEventsExtensions
{
    public static async Task CharacterDisconnect(this IClientSession session)
    {
        await session.EmitEventAsync(new CharacterDisconnectedEvent
        {
            DisconnectionTime = DateTime.UtcNow
        });
    }

    public static async Task RestAsync(this IClientSession session, bool restTeamMemberMates = false, bool force = false)
    {
        await session.EmitEventAsync(new PlayerRestEvent
        {
            RestTeamMemberMates = restTeamMemberMates,
            Force = force
        });
    }
}

public class CharacterPreLoadEvent : PlayerEvent
{
    public CharacterPreLoadEvent(byte slot) => Slot = slot;

    public byte Slot { get; }
}

public class CharacterLoadEvent : PlayerEvent
{
}

public class CharacterDisconnectedEvent : PlayerEvent
{
    public DateTime DisconnectionTime { get; set; }
}

/// <summary>
///     Forces the save of A Character
///     Will be removed when we fully manage saveable data through microservices
/// </summary>
public class SessionSaveEvent : PlayerEvent
{
}