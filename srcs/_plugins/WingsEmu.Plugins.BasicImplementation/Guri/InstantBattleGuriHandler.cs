using System;
using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class InstantBattleGuriHandler : IGuriHandler
{
    private const GameEventType Type = GameEventType.InstantBattle;

    private readonly IGameEventRegistrationManager _gameEventRegistrationManager;

    public InstantBattleGuriHandler(IGameEventRegistrationManager gameEventRegistrationManager) => _gameEventRegistrationManager = gameEventRegistrationManager;

    public long GuriEffectId => 506;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        if (!_gameEventRegistrationManager.IsGameEventRegistrationOpen(Type, DateTime.UtcNow))
        {
            return;
        }

        if (session.IsMuted())
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.MUTE_SHOUTMESSAGE_YOU_ARE_MUTED), MsgMessageType.Middle);
            return;
        }

        _gameEventRegistrationManager.SetCharacterGameEventInclination(session.PlayerEntity.Id, Type);
    }
}