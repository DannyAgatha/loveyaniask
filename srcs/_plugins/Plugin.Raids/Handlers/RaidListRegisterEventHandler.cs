using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Raids;

public class RaidListRegisterEventHandler : IAsyncEventProcessor<RaidListRegisterEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRaidManager _raidManager;
    private readonly ISessionManager _sessionManager;

    public RaidListRegisterEventHandler(IRaidManager raidManager, IGameLanguageService gameLanguage, ISessionManager sessionManager)
    {
        _raidManager = raidManager;
        _gameLanguage = gameLanguage;
        _sessionManager = sessionManager;
    }

    public async Task HandleAsync(RaidListRegisterEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (!session.PlayerEntity.IsInRaidParty || !session.PlayerEntity.IsRaidLeader(session.PlayerEntity.Id))
        {
            return;
        }

        RaidParty raid = session.PlayerEntity.Raid;
        if (_raidManager.ContainsRaidInRaidPublishList(raid) || raid.Started)
        {
            return;
        }

        await _sessionManager.BroadcastAsync(async x =>
        {
            string getRaidName = x.GenerateRaidName(_gameLanguage, raid.Type);
            return x.GenerateEventAsk(QnamlType.Raid, "rl",
                _gameLanguage.GetLanguageFormat(GameDialogKey.RAID_BROADCAST_LOOKING_FOR_TEAM_MEMBERS, x.UserLanguage, session.PlayerEntity.Name, getRaidName));
        }, new LevelBroadcast(raid.MinimumLevel), new ExceptSessionBroadcast(session), new InBaseMapBroadcast());

        _raidManager.RegisterRaidInRaidPublishList(raid);
        session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.RAID_SHOUTMESSAGE_REGISTERED, session.UserLanguage), MsgMessageType.Middle);
    }
}