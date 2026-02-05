using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RaidListUnregisterEventHandler : IAsyncEventProcessor<RaidListUnregisterEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRaidManager _raidManager;

    public RaidListUnregisterEventHandler(IRaidManager raidManager, IGameLanguageService gameLanguage)
    {
        _raidManager = raidManager;
        _gameLanguage = gameLanguage;
    }

    public async Task HandleAsync(RaidListUnregisterEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (!session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            return;
        }

        RaidParty raid = session.PlayerEntity.Raid;
        if (!_raidManager.ContainsRaidInRaidPublishList(raid))
        {
            return;
        }

        _raidManager.UnregisterRaidFromRaidPublishList(raid);
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_UNREGISTERED, session.UserLanguage), MsgMessageType.Middle);
    }
}