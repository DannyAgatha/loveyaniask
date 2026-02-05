using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Miniland;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class InviteJoinMinilandEventHandler : IAsyncEventProcessor<InviteJoinMinilandEvent>
{
    private readonly IGameLanguageService _gameLanguageService;
    private readonly SerializableGameServer _gameServer;
    private readonly IItemsManager _itemsManager;
    private readonly IMinilandManager _miniland;
    private readonly ISessionManager _sessionManager;

    public InviteJoinMinilandEventHandler(ISessionManager sessionManager, IMinilandManager miniland, IItemsManager itemsManager, SerializableGameServer gameServer,
        IGameLanguageService gameLanguageService)
    {
        _sessionManager = sessionManager;
        _miniland = miniland;
        _itemsManager = itemsManager;
        _gameServer = gameServer;
        _gameLanguageService = gameLanguageService;
    }

    public async Task HandleAsync(InviteJoinMinilandEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IClientSession target = _sessionManager.GetSessionByCharacterName(e.Target);
        bool isFirstStep = e.IsFirstStep;
        bool isByFriend = e.IsByFriend;
        bool isOnAct4 = _gameServer.ChannelType == GameChannelType.ACT_4;

        if (isOnAct4)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP), MsgMessageType.Middle);
            return;
        }

        if (session.CurrentMapInstance.Id != session.PlayerEntity.Miniland.Id && !isByFriend)
        {
            return;
        }

        if (target == null)
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.INFORMATION_INFO_PLAYER_OFFLINE));
            return;
        }

        if (session.PlayerEntity.Id == target.PlayerEntity.Id)
        {
            return;
        }

        /*
         * IsFirstStep -> session is who sent the invitation
         * !IsFirstStep -> target is who sent the invitation, so session is who accepted it
         */

        if (target.PlayerEntity.MinilandInviteBlocked)
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.MINILAND_INFO_INVITE_LOCK));
            return;
        }

        if (!isFirstStep)
        {
            switch (target.PlayerEntity.MinilandState)
            {
                case MinilandState.PRIVATE when !session.PlayerEntity.IsFriend(target.PlayerEntity.Id) && !session.PlayerEntity.IsMarried(target.PlayerEntity.Id) && !session.IsGameMaster():
                    session.SendInfo(session.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_CLOSED));
                    return;
                case MinilandState.LOCK:
                    if (session.IsGameMaster())
                    {
                        break;
                    }

                    session.SendInfo(session.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_CLOSED));
                    return;
            }
        }

        if (isByFriend)
        {
            if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP), MsgMessageType.Middle);
                return;
            }

            if (!session.PlayerEntity.IsFriend(target.PlayerEntity.Id) && !session.PlayerEntity.IsMarried(target.PlayerEntity.Id))
            {
                return;
            }

            if (!session.PlayerEntity.IsAlive())
            {
                return;
            }

            if (target.PlayerEntity.MinilandState == MinilandState.LOCK && !session.IsGameMaster())
            {
                session.SendInfo(session.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_CLOSED));
                return;
            }

            session.ChangeMap(target.PlayerEntity.Miniland);
            return;
        }

        if (!session.PlayerEntity.HasItem((short)ItemVnums.SEED_OF_POWER))
        {
            string itemName = _itemsManager.GetItem((short)ItemVnums.SEED_OF_POWER).GetItemName(_gameLanguageService, session.UserLanguage);
            session.SendInfo(session.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, 1, itemName));
            return;
        }

        if (isFirstStep)
        {
            _miniland.SaveMinilandInvite(session.PlayerEntity.Id, target.PlayerEntity.Id);
            target.SendDialog($"mjoin 0 {session.PlayerEntity.Id} 1", $"mjoin 0 {session.PlayerEntity.Id} 0",
                target.GetLanguageFormat(GameDialogKey.MINILAND_DIALOG_ASK_INVITE, session.PlayerEntity.Name));
            return;
        }

        if (!_miniland.ContainsMinilandInvite(session.PlayerEntity.Id))
        {
            return;
        }

        if (!_miniland.ContainsTargetInvite(session.PlayerEntity.Id, target.PlayerEntity.Id))
        {
            return;
        }

        _miniland.RemoveMinilandInvite(session.PlayerEntity.Id, target.PlayerEntity.Id);

        if (!target.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            target.SendMsg(target.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MUST_BE_IN_CLASSIC_MAP), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.MinilandState == MinilandState.LOCK && !target.IsGameMaster())
        {
            target.SendInfo(target.GetLanguage(GameDialogKey.MINILAND_SHOUTMESSAGE_CLOSED));
            return;
        }

        await session.RemoveItemFromInventory((short)ItemVnums.SEED_OF_POWER);
        target.ChangeMap(session.PlayerEntity.Miniland);
    }
}