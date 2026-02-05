using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class WingsOfFriendshipGuriHandler : IGuriHandler
{
    private readonly IDelayManager _delay;

    private readonly IGameLanguageService _gameLanguageService;
    private readonly ISessionManager _sessionManager;

    public WingsOfFriendshipGuriHandler(IGameLanguageService gameLanguageService, IMapManager mapManager, ISessionManager sessionManager, IDelayManager delay)
    {
        _sessionManager = sessionManager;
        _delay = delay;
        _gameLanguageService = gameLanguageService;
    }

    public long GuriEffectId => 199;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        if (guriPacket.User == null)
        {
            return;
        }

        if (!long.TryParse(guriPacket.User.Value.ToString(), out long charId))
        {
            return;
        }

        if (session.CantPerformActionOnAct4())
        {
            return;
        }

        IClientSession otherSession = _sessionManager.GetSessionByCharacterId(charId);

        bool isMarriedToTarget = session.PlayerEntity.IsMarried(charId);

        if (!session.PlayerEntity.IsFriend(charId) && !isMarriedToTarget)
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.FRIEND_SHOUTMESSAGE_CHARACTER_NOT_FRIEND, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (otherSession == null)
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_USER_NOT_CONNECTED, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        bool hasItem = session.PlayerEntity.HasItem((short)ItemVnums.WING_OF_FRIENDSHIP);
        bool hasLimitedItem = session.PlayerEntity.HasItem((short)ItemVnums.WING_OF_FRIENDSHIP_LIMITED);

        if (!hasItem && !hasLimitedItem && !isMarriedToTarget)
        {
            session.SendChatMessage(_gameLanguageService.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_NO_WINGS, session.UserLanguage), ChatMessageColorType.Red);
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && !session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!otherSession.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && !otherSession.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_USER_NOT_BASEMAP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        // Act 5
        if (session.IsInAct5() && !otherSession.IsInAct5())
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!session.IsInAct5() && otherSession.IsInAct5())
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_USER_NOT_BASEMAP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }
        
        // Act6
        if (session.IsInAct6() && !otherSession.IsInAct6())
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!session.IsInAct6() && otherSession.IsInAct6())
        {
            session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_USER_NOT_BASEMAP, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (guriPacket.Data != 2)
        {
            DateTime waitUntil = await _delay.RegisterAction(session.PlayerEntity, DelayedActionType.WingOfFriendship);
            session.SendDelay((int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.UsingItem, $"guri 199 2 {charId}");
            return;
        }

        if (!await _delay.CanPerformAction(session.PlayerEntity, DelayedActionType.WingOfFriendship))
        {
            return;
        }

        await _delay.CompleteAction(session.PlayerEntity, DelayedActionType.WingOfFriendship);

        switch (otherSession.CurrentMapInstance.MapInstanceType)
        {
            case MapInstanceType.Act4Instance:
                if (session.PlayerEntity.Faction != otherSession.PlayerEntity.Faction)
                {
                    return;
                }

                short mapInstanceX = otherSession.PlayerEntity.PositionY;
                short mapInstanceY = otherSession.PlayerEntity.PositionX;

                session.ChangeMap(otherSession.CurrentMapInstance, mapInstanceX, mapInstanceY);
                break;
            default:
                short mapY = otherSession.PlayerEntity.PositionY;
                short mapX = otherSession.PlayerEntity.PositionX;
                int mapId = otherSession.PlayerEntity.MapInstance.MapId;

                session.ChangeMap(mapId, mapX, mapY);

                break;
        }

        if (!isMarriedToTarget)
        {
            await session.RemoveItemFromInventory(hasLimitedItem ? (short)ItemVnums.WING_OF_FRIENDSHIP_LIMITED : (short)ItemVnums.WING_OF_FRIENDSHIP);
        }
    }
}