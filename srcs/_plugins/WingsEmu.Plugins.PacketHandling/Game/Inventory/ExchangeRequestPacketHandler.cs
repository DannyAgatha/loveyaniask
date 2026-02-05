using System;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Exchange.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Relations;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class ExchangeRequestPacketHandler : GenericGamePacketHandlerBase<ExchangeRequestPacket>
{
    private readonly IInvitationManager _invitation;
    private readonly IGameLanguageService _language;
    private readonly IServerManager _serverManager;
    private readonly IMapManager _mapManager;

    public ExchangeRequestPacketHandler(IServerManager serverManager, IGameLanguageService language, IInvitationManager invitation, IMapManager mapManager)
    {
        _invitation = invitation;
        _serverManager = serverManager;
        _language = language;
        _mapManager = mapManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, ExchangeRequestPacket exchangeRequestPacket)
    {
        long targetId = exchangeRequestPacket.CharacterId;
        RequestExchangeType requestType = exchangeRequestPacket.RequestType;
        string mapName = _language.GetMapName(_mapManager.GetMapByMapId(session.PlayerEntity.MapId), session);
        
        if (!session.HasCurrentMapInstance || !session.HasSelectedCharacter)
        {
            return;
        }
        
        if (session.PlayerEntity.MapInstance.IsAct6PvpInstance)
        {
            return;
        }

        if (session.IsActionForbidden())
        {
            return;
        }

        if (!session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && session.CurrentMapInstance.MapInstanceType != MapInstanceType.Miniland)
        {
            return;
        }

        IClientSession target;
        switch (requestType)
        {
            case RequestExchangeType.Requested:
                if (session.PlayerEntity.Id == targetId)
                {
                    return;
                }

                target = session.CurrentMapInstance.GetCharacterById(targetId)?.Session;

                if (target == null)
                {
                    return;
                }

                if (target.IsActionForbidden())
                {
                    return;
                }

                if (session.CurrentMapInstance.Id != target.CurrentMapInstance.Id)
                {
                    return;
                }

                if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
                {
                    if (target.PlayerEntity.Faction != session.PlayerEntity.Faction)
                    {
                        return;
                    }
                }

                if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.UnderWaterShowdown)
                {
                    return;
                }
                
                if (target.PlayerEntity.HasRaidStarted)
                {
                    session.SendMsg(_language.GetLanguage(GameDialogKey.TRADE_SHOUTMESSAGE_NOT_ALLOWED_IN_RAID, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (session.PlayerEntity.HasRaidStarted)
                {
                    session.SendMsg(_language.GetLanguage(GameDialogKey.TRADE_SHOUTMESSAGE_NOT_ALLOWED_WITH_RAID_MEMBER, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (session.PlayerEntity.IsBlocking(targetId))
                {
                    session.SendInfo(_language.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKING, session.UserLanguage));
                    return;
                }

                if (target.PlayerEntity.IsBlocking(session.PlayerEntity.Id))
                {
                    session.SendInfo(_language.GetLanguage(GameDialogKey.BLACKLIST_INFO_BLOCKED, session.UserLanguage));
                    return;
                }

                if (session.PlayerEntity.IsInExchange())
                {
                    session.SendModal(_language.GetLanguage(GameDialogKey.TRADE_MODAL_ALREADY_IN_EXCHANGE, session.UserLanguage), ModalType.Cancel);
                    return;
                }

                if (target.PlayerEntity.IsInExchange())
                {
                    session.SendModal(_language.GetLanguage(GameDialogKey.TRADE_MODAL_ALREADY_IN_EXCHANGE, session.UserLanguage), ModalType.Cancel);
                    return;
                }

                if (target.PlayerEntity.LastSkillUse.AddSeconds(10) > DateTime.UtcNow || target.PlayerEntity.LastDefence.AddSeconds(10) > DateTime.UtcNow)
                {
                    session.SendInfo(_language.GetLanguageFormat(GameDialogKey.INFORMATION_INFO_PLAYER_IN_BATTLE, session.UserLanguage, target.PlayerEntity.Name));
                    return;
                }

                if (session.PlayerEntity.LastSkillUse.AddSeconds(10) > DateTime.UtcNow || session.PlayerEntity.LastDefence.AddSeconds(10) > DateTime.UtcNow)
                {
                    session.SendInfo(_language.GetLanguage(GameDialogKey.TRADE_INFO_PLAYER_IN_BATTLE, session.UserLanguage));
                    return;
                }

                if (session.PlayerEntity.HasShopOpened || target.PlayerEntity.HasShopOpened)
                {
                    session.SendMsg(_language.GetLanguage(GameDialogKey.TRADE_SHOUTMESSAGE_HAS_SHOP_OPEN, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (target.PlayerEntity.ExchangeBlocked)
                {
                    session.SendChatMessage(_language.GetLanguage(GameDialogKey.TRADE_CHATMESSAGE_BLOCKED, session.UserLanguage), ChatMessageColorType.Red);
                    return;
                }

                session.SendModal(_language.GetLanguageFormat(GameDialogKey.TRADE_DIALOG_ASK_FOR_TRADE, session.UserLanguage, target.PlayerEntity.Name), ModalType.Cancel);
                await session.EmitEventAsync(new InvitationEvent(targetId, InvitationType.Exchange));
                break;

            case RequestExchangeType.Confirmed:
                PlayerExchange exchange = session.PlayerEntity.GetExchange();
                if (exchange == null)
                {
                    return;
                }

                target = session.CurrentMapInstance.GetCharacterById(exchange.TargetId)?.Session;

                PlayerExchange targetExchange = target?.PlayerEntity.GetExchange();
                if (targetExchange == null)
                {
                    return;
                }

                if (session.PlayerEntity.IsCraftingItem)
                {
                    await session.CloseExchange();
                    session.SendShopEndPacket(ShopEndType.Player);
                    return;
                }

                if (session.PlayerEntity.HasRaidStarted)
                {
                    session.SendMsg(_language.GetLanguage(GameDialogKey.TRADE_SHOUTMESSAGE_NOT_ALLOWED_IN_RAID, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (target.PlayerEntity.HasRaidStarted)
                {
                    session.SendMsg(_language.GetLanguage(GameDialogKey.TRADE_SHOUTMESSAGE_NOT_ALLOWED_WITH_RAID_MEMBER, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (session.IsDisposing || target.IsDisposing)
                {
                    await session.CloseExchange();
                    return;
                }

                if (!exchange.RegisteredItems || !targetExchange.RegisteredItems)
                {
                    return;
                }

                if (!exchange.AcceptedTrade)
                {
                    exchange.AcceptedTrade = true;
                }

                if (exchange.AcceptedTrade && !targetExchange.AcceptedTrade)
                {
                    session.SendInfo(_language.GetLanguageFormat(GameDialogKey.TRADE_INFO_WAITING_FOR_TARGET, session.UserLanguage, target.PlayerEntity.Name));
                    return;
                }

                await session.EmitEventAsync(new ExchangeTransferItemsEvent
                {
                    Target = target,
                    SenderGold = exchange.Gold,
                    SenderBankGold = exchange.BankGold,
                    SenderItems = exchange.Items,
                    TargetItems = targetExchange.Items,
                    TargetGold = targetExchange.Gold,
                    TargetBankGold = targetExchange.BankGold
                });

                break;

            case RequestExchangeType.Cancelled:
                if (!session.PlayerEntity.IsInExchange())
                {
                    return;
                }

                PlayerExchange playerExchange = session.PlayerEntity.GetExchange();
                if (playerExchange == null)
                {
                    return;
                }

                target = session.CurrentMapInstance.GetCharacterById(playerExchange.TargetId)?.Session;
                if (target == null)
                {
                    return;
                }
                
                await session.CloseExchange();
                
                session.SendDiscordRpcPacket($"{session.GetLanguage(GameDialogKey.PLAYING_IN_MAP_RPC)} {mapName}");
                target.SendDiscordRpcPacket($"{session.GetLanguage(GameDialogKey.PLAYING_IN_MAP_RPC)} {mapName}");
                break;

            case RequestExchangeType.AcceptExchangeInvitation:
                if (session.PlayerEntity.Id == targetId)
                {
                    return;
                }

                target = session.CurrentMapInstance.GetCharacterById(targetId)?.Session;
                if (target == null)
                {
                    return;
                }

                if (session.CurrentMapInstance.Id != target.CurrentMapInstance.Id)
                {
                    return;
                }

                if (!_invitation.ContainsPendingInvitation(targetId, session.PlayerEntity.Id, InvitationType.Exchange))
                {
                    return;
                }

                if (session.PlayerEntity.IsInExchange())
                {
                    return;
                }

                if (target.PlayerEntity.IsInExchange())
                {
                    session.SendModal(_language.GetLanguage(GameDialogKey.TRADE_MODAL_ALREADY_IN_EXCHANGE, session.UserLanguage), ModalType.Cancel);
                    return;
                }

                _invitation.RemovePendingInvitation(targetId, session.PlayerEntity.Id, InvitationType.Exchange);
                
                session.SendDiscordRpcPacket($"{session.GetLanguage(GameDialogKey.EXCHANGING_RPC)} {target.PlayerEntity.Name}");
                target.SendDiscordRpcPacket($"{session.GetLanguage(GameDialogKey.EXCHANGING_RPC)} {session.PlayerEntity.Name}");
                
                await session.EmitEventAsync(new ExchangeJoinEvent
                {
                    Target = target
                });
                break;

            case RequestExchangeType.Declined:
                if (session.PlayerEntity.Id == targetId)
                {
                    return;
                }

                target = session.CurrentMapInstance.GetCharacterById(targetId)?.Session;

                if (target == null)
                {
                    return;
                }

                if (session.CurrentMapInstance.Id != target.CurrentMapInstance.Id)
                {
                    return;
                }

                if (!_invitation.ContainsPendingInvitation(targetId, session.PlayerEntity.Id, InvitationType.Exchange))
                {
                    return;
                }

                _invitation.RemovePendingInvitation(targetId, session.PlayerEntity.Id, InvitationType.Exchange);
                target.SendChatMessage(_language.GetLanguageFormat(GameDialogKey.TRADE_CHATMESSAGE_REFUSED, target.UserLanguage, session.PlayerEntity.Name), ChatMessageColorType.Yellow);
                session.SendChatMessage(_language.GetLanguage(GameDialogKey.TRADE_CHATMESSAGE_YOU_REFUSED, session.UserLanguage), ChatMessageColorType.Yellow);
                break;
        }
    }
}