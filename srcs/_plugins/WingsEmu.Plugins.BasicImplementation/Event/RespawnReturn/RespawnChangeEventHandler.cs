using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RespawnReturn.Event;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.RespawnReturn;

public class RespawnChangeEventHandler : IAsyncEventProcessor<RespawnChangeEvent>
{
    private readonly IGameLanguageService _gameLanguage;

    public RespawnChangeEventHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public async Task HandleAsync(RespawnChangeEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        switch ((MapIds)e.MapId)
        {
            case MapIds.NOSVILLE:
                session.PlayerEntity.HomeComponent.ChangeRespawn(RespawnType.NOSVILLE_SPAWN);
                break;
            case MapIds.KREM:
                session.PlayerEntity.HomeComponent.ChangeRespawn(RespawnType.KREM_SPAWN);
                break;
            case MapIds.ALVEUS:
                session.PlayerEntity.HomeComponent.ChangeRespawn(RespawnType.ALVEUS_SPAWN);
                break;
            case MapIds.MORTAZ_DESERT_PORT:
                session.PlayerEntity.HomeComponent.ChangeAct5Respawn(Act5RespawnType.MORTAZ_DESERT_PORT);
                break;
            case MapIds.AKAMUR_CAMP:
                session.PlayerEntity.HomeComponent.ChangeAct5Respawn(Act5RespawnType.AKAMUR_CAMP);
                break;
            case MapIds.DESERT_EAGLY_CITY:
                session.PlayerEntity.HomeComponent.ChangeAct5Respawn(Act5RespawnType.DESERT_EAGLE_CITY);
                break;
            case MapIds.CYLLOAN:
                session.PlayerEntity.HomeComponent.ChangeRespawn(RespawnType.CYLLOAN_SPAWN);
                break;
            default:
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Player wanted change spawn point, but map doesn't have Teleporter NPC to change spawn.");
                return;
        }

        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.RESPAWNLOCATION_SHOUTMESSAGE_CHANGED, session.UserLanguage), MsgMessageType.Middle);
    }
}