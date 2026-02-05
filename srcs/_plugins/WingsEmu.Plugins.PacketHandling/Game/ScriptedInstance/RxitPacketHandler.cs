using System.Threading.Tasks;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.ScriptedInstance;

public class RxitPacketHandler : GenericGamePacketHandlerBase<RxitPacket>
{
    private readonly IGameLanguageService _language;
    private readonly IMapManager _mapManager;
    private readonly ISessionManager _sessionManager;

    public RxitPacketHandler(IGameLanguageService language, IMapManager mapManager, ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
        _mapManager = mapManager;
        _language = language;
    }

    protected override async Task HandlePacketAsync(IClientSession session, RxitPacket packet)
    {
        if (packet?.State != 1)
        {
            return;
        }

        switch (session.CurrentMapInstance?.MapInstanceType)
        {
            case MapInstanceType.TimeSpaceInstance:
                await session.EmitEventAsync(new TimeSpaceLeavePartyEvent
                {
                    RemoveLive = true,
                    CheckForSeeds = true
                });
                break;
            case MapInstanceType.RaidInstance when session.PlayerEntity.IsInRaidParty:
                if (session.PlayerEntity.Raid is { Finished: true })
                {
                    return;
                }

                await session.EmitEventAsync(new RaidPartyLeaveEvent(false));
                break;
        }
    }
}