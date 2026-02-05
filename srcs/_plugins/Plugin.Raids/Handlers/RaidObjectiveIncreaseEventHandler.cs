using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Plugin.Raids.Const;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RaidObjectiveIncreaseEventHandler : IAsyncEventProcessor<RaidObjectiveIncreaseEvent>
{
    private readonly IGameLanguageService _languageService;

    public RaidObjectiveIncreaseEventHandler(IGameLanguageService languageService) => _languageService = languageService;

    public async Task HandleAsync(RaidObjectiveIncreaseEvent e, CancellationToken cancellation)
    {
        switch (e.RaidTargetType)
        {
            case RaidTargetType.Monster:
                e.RaidSubInstance.CurrentCompletedTargetMonsters++;
                break;
            case RaidTargetType.Button:
                e.RaidSubInstance.CurrentCompletedTargetButtons++;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (e.RaidSubInstance.TargetsCompleted)
        {
            e.RaidSubInstance.MapInstance.Broadcast(x => x.GenerateMsgPacket(
                _languageService.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_TARGETS_COMPLETED, x.UserLanguage), MsgMessageType.Middle));

            await e.RaidSubInstance.TriggerEvents(RaidConstEventKeys.ObjectivesCompleted);
        }
        else
        {
            e.RaidSubInstance.MapInstance.Broadcast(x => x.GenerateMsgPacket(
                _languageService.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_TARGETS_UPDATED, x.UserLanguage), MsgMessageType.Middle));
        }

        foreach (IClientSession member in e.RaidSubInstance.MapInstance.Sessions)
        {
            member.SendRaidmbf();
        }
    }
}