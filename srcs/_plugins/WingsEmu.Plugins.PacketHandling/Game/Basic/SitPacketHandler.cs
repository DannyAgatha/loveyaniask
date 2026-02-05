using PhoenixLib.MultiLanguage;
using System.Threading.Tasks;
using WingsAPI.Communication.Sessions.Model;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class SitPacketHandler : GenericGamePacketHandlerBase<SitPacket>
{
    private readonly IMeditationManager _meditationManager;
    private readonly IMapManager _mapManager;
    private readonly IGameLanguageService _languageService;

    public SitPacketHandler(IMeditationManager meditationManager, IMapManager mapManager, IGameLanguageService languageService)
    {
        _meditationManager = meditationManager;
        _mapManager = mapManager;
        _languageService = languageService;
    }

    protected override async Task HandlePacketAsync(IClientSession session, SitPacket packet)
    {
        string mapName = _languageService.GetMapName(_mapManager.GetMapByMapId(session.PlayerEntity.MapId), session);

        if (_meditationManager.HasMeditation(session.PlayerEntity))
        {
            _meditationManager.RemoveAllMeditation(session.PlayerEntity);
        }

        session.SendDiscordRpcPacket(session.PlayerEntity.IsSitting 
            ? $"{session.GetLanguage(GameDialogKey.PLAYING_IN_MAP_RPC)} {mapName}" 
            : $"{session.GetLanguage(GameDialogKey.RESTING_ON_MAP_RPC)} {mapName}"
        );

        if (packet?.Users == null)
        {
            return;
        }

        bool syncWithPlayer = false;
        foreach (SitSubPacket subPacket in packet.Users)
        {
            if (subPacket.VisualType == VisualType.Player)
            {
                await session.RestAsync();
                syncWithPlayer = true;
                continue;
            }

            IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(x => x.Id == subPacket.UserId);
            if (mateEntity == null)
            {
                continue;
            }

            await session.EmitEventAsync(new MateRestEvent
            {
                MateEntity = mateEntity,
                Rest = syncWithPlayer ? session.PlayerEntity.IsSitting : !mateEntity.IsSitting
            });
        }
    }
}