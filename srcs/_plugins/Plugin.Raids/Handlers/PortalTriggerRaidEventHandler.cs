using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class PortalTriggerRaidEventHandler : IAsyncEventProcessor<PortalTriggerEvent>
{
    private readonly IGameLanguageService _gameLanguage;

    public PortalTriggerRaidEventHandler(IGameLanguageService gameLanguage) => _gameLanguage = gameLanguage;

    public async Task HandleAsync(PortalTriggerEvent e, CancellationToken cancellation)
    {
        if (e.Portal.Type != PortalType.Raid)
        {
            return;
        }

        if (!e.Portal.RaidType.HasValue)
        {
            return;
        }

        IClientSession session = e.Sender;

        if (!session.PlayerEntity.IsInRaidParty)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.RAID_CHATMESSAGE_START_NOT_IN_RAID_PARTY, session.UserLanguage), ChatMessageColorType.PlayerSay);
            return;
        }

        if (!session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.RAID_CHATMESSAGE_START_IS_NOT_LEADER, session.UserLanguage), ChatMessageColorType.PlayerSay);
            return;
        }

        if (session.PlayerEntity.Raid.Type != (RaidType)e.Portal.RaidType.Value)
        {
            session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.RAID_CHATMESSAGE_START_RAID_TYPE_IS_WRONG, session.UserLanguage), ChatMessageColorType.PlayerSay);
            return;
        }

        string raidName = session.GenerateRaidName(_gameLanguage, session.PlayerEntity.Raid.Type);
        session.SendQnaPacket("mkraid", _gameLanguage.GetLanguageFormat(GameDialogKey.RAID_DIALOG_ASK_START_RAID, session.UserLanguage, raidName));
    }
}