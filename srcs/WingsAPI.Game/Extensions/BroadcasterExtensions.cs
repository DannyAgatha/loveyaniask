using System;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._i18n;
using WingsEmu.Game.GameEvent;
using WingsEmu.Game.GameEvent.Configuration;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.Extensions;

public static class BroadcasterExtensions
{
    private static IGameLanguageService _languageService => StaticGameLanguageService.Instance;

    public static void BroadcastToFamily(this IBroadcaster broadcaster, string packet, long familyId) => broadcaster.Broadcast(packet, new FamilyBroadcast(familyId));

    public static void BroadcastToFamily(this IBroadcaster broadcaster, long familyId, Func<IClientSession, Task<string>> func)
    {
        broadcaster.BroadcastAsync(func, new FamilyBroadcast(familyId));
    }

    public static void BroadcastToFamilyExceptFamilyHead(this IBroadcaster broadcaster, long familyId, Func<IClientSession, Task<string>> func)
    {
        broadcaster.BroadcastAsync(func, new FamilyBroadcast(familyId));
    }

    public static void BroadcastToGameMaster(this IBroadcaster broadcaster, string message) => broadcaster.Broadcast($"say 0 0 11 {message}", new OnlyGameMasters());

    public static void BroadcastGameEventAsk(this IBroadcaster broadcaster, GameEventType gameType, IGlobalGameEventConfiguration gameEventConfiguration)
    {
        switch (gameType)
        {
            case GameEventType.InstantBattle:
                broadcaster.Broadcast(x => x.GenerateEventAsk(QnamlType.InstantCombat, "guri 506", x.GenerateGameEventMessage(GameDialogKey.INSTANT_COMBAT_NAME)),
                    new InBaseMapBroadcast(), new NotMutedBroadcast());
                break;
        }
    }

    private static string GenerateGameEventMessage(this IClientSession x, GameDialogKey gameEventName, long cost = default)
    {
        if (cost == default)
        {
            return _languageService.GetLanguageFormat(GameDialogKey.GAMEEVENT_DIALOG_ASK_PARTICIPATE, x.UserLanguage,
                _languageService.GetLanguage(gameEventName, x.UserLanguage));
        }

        return _languageService.GetLanguageFormat(GameDialogKey.GAMEEVENT_DIALOG_ASK_PARTICIPATE_WITH_COST, x.UserLanguage,
            _languageService.GetLanguage(gameEventName, x.UserLanguage), cost);
    }

    public static void BroadcastToGameMaster(this IBroadcaster broadcaster, IClientSession player, string message)
        => broadcaster.Broadcast($"say 0 0 11 [GM_ONLY] Player: {player?.PlayerEntity.Name} - {message}", new OnlyGameMasters());
}