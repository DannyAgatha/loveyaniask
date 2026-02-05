using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Raids.Configuration;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RevivalStartProcedureEventRaidHandler : IAsyncEventProcessor<RevivalStartProcedureEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly RaidConfiguration _raidConfiguration;
    private readonly ISessionManager _sessionManager;

    public RevivalStartProcedureEventRaidHandler(RaidConfiguration raidConfiguration, ISessionManager sessionManager, IGameLanguageService gameLanguage)
    {
        _raidConfiguration = raidConfiguration;
        _sessionManager = sessionManager;
        _gameLanguage = gameLanguage;
    }

    public async Task HandleAsync(RevivalStartProcedureEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive() || e.Sender.CurrentMapInstance.MapInstanceType != MapInstanceType.RaidInstance || e.Sender.PlayerEntity.Raid?.Instance == null)
        {
            return;
        }

        _sessionManager.Broadcast(
            x => { return x.GenerateMsgPacket(_gameLanguage.GetLanguageFormat(GameDialogKey.RAID_SHOUTMESSAGE_PLAYER_DEATH, x.UserLanguage, e.Sender.PlayerEntity.Name), MsgMessageType.Middle); },
            new RaidBroadcast(e.Sender.PlayerEntity.Raid.Id));

        e.Sender.PlayerEntity.AddRaidDeath();
        await e.Sender.EmitEventAsync(new RaidInstanceLivesIncDecEvent(-1));

        if (e.Sender.PlayerEntity.Raid.Finished)
        {
            return;
        }

        if (e.Sender.PlayerEntity.RaidDeaths >= _raidConfiguration.LivesPerCharacter)
        {
            await e.Sender.EmitEventAsync(new RaidPartyLeaveEvent(false, false));
        }
        else
        {
            e.Sender.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.RAID_INFO_PLAYER_DEATH, e.Sender.UserLanguage));
        }

        e.Sender.PlayerEntity.UpdateRevival(DateTime.UtcNow + _raidConfiguration.RaidDeathRevivalDelay, RevivalType.DontPayRevival, ForcedType.Reconnect);
    }
}