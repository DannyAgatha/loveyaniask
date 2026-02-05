using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Exchange.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using WingsAPI.Game.Extensions.AccountExtensions;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Characters.Events;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class ExcListPacketHandler : GenericGamePacketHandlerBase<ExcListPacket>
{
    private readonly IBankReputationConfiguration _bankReputationConfiguration;
    private readonly IGameLanguageService _language;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly ISessionManager _sessionManager;
    private readonly IAccountService _accountService;

    public ExcListPacketHandler(
        IGameLanguageService language,
        ISessionManager sessionManager,
        IReputationConfiguration reputationConfiguration,
        IBankReputationConfiguration bankReputationConfiguration,
        IRankingManager rankingManager,
        IAccountService accountService)
    {
        _sessionManager = sessionManager;
        _reputationConfiguration = reputationConfiguration;
        _bankReputationConfiguration = bankReputationConfiguration;
        _rankingManager = rankingManager;
        _language = language;
        _accountService = accountService;
    }

    protected override async Task HandlePacketAsync(IClientSession session, ExcListPacket packet)
    {
        if (packet == null)
        {
            return;
        }

        if (session.IsActionForbidden())
        {
            return;
        }

        if (!session.PlayerEntity.IsInExchange())
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(packet.PacketData))
        {
            return;
        }
            
        string[] packetSplit = packet.PacketData.Split(' ', 32);
        if (packetSplit.Length < 2)
        {
            return;
        }

        if (!int.TryParse(packetSplit[0], out int gold))
        {
            return;
        }

        if (!long.TryParse(packetSplit[1], out long bankGold))
        {
            return;
        }
            
        if (bankGold < 0 || bankGold > session.Account.BankMoney / 1000)
        {
            return;
        }

        if (gold < 0 || gold > session.PlayerEntity.Gold)
        {
            return;
        }

        PlayerExchange playerExchange = session.PlayerEntity.GetExchange();
        if (playerExchange == null)
        {
            return;
        }

        IClientSession exchangeTarget = _sessionManager.GetSessionByCharacterId(playerExchange.TargetId);
        if (exchangeTarget == null)
        {
            return;
        }

        if (playerExchange.RegisteredItems)
        {
            return;
        }
            
        long penalty = session.PlayerEntity.GetBankPenalty(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
        if (bankGold != 0 && !session.HasEnoughGold(penalty + gold))
        {
            await session.CloseExchange();
            session.SendMsg(_language.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.HasShopOpened || exchangeTarget.PlayerEntity.HasShopOpened)
        {
            await session.CloseExchange();
            return;
        }

        var seenSlots = new HashSet<(InventoryType Inv, short Slot)>();
        var itemList = new List<ExchangeItem>();
        var packetListBuilder = new StringBuilder();

        for (int i = 2, j = 0; i < packetSplit.Length && j < 10; i += 3, j++)
        {
            if (i + 2 >= packetSplit.Length ||
                !byte.TryParse(packetSplit[i], out byte invRaw) ||
                !Enum.IsDefined(typeof(InventoryType), invRaw) ||
                !short.TryParse(packetSplit[i + 1], out short slot) ||
                !short.TryParse(packetSplit[i + 2], out short itemAmount))
            {
                await session.CloseExchange();
                return;
            }

            var inventoryType = (InventoryType)invRaw;
                
            int maxSlots = session.PlayerEntity.GetInventorySlots(false, inventoryType);
            if (slot < 0 || slot >= maxSlots)
            {
                await session.CloseExchange();
                return;
            }

            InventoryItem item = session.PlayerEntity.GetItemBySlotAndType(slot, inventoryType);
            if (item == null ||
                !item.ItemInstance.GameItem.IsTradable ||
                item.ItemInstance.IsBound)
            {
                await session.CloseExchange();
                session.SendMsgi(MessageType.Default, Game18NConstString.SomeItemsCannotBeTraded);
                return;
            }

            bool isEquipment = item.ItemInstance.Type != ItemInstanceType.NORMAL_ITEM;

            itemList.Add(new ExchangeItem
            {
                Amount = itemAmount,
                Type = inventoryType,
                Slot = slot,
                ItemInstanceId = item.ItemInstance.TransportId,
                Rarity = isEquipment ? item.ItemInstance.Rarity : null,
                Upgrade = isEquipment ? item.ItemInstance.Upgrade : null,
                Runes = isEquipment ? item.ItemInstance.GetCarvedRunesInformation(true) : null
            });

            if (!isEquipment)
            {
                packetListBuilder.Append($"{j}.{(byte)inventoryType}.{item.ItemInstance.ItemVNum}.{itemAmount}.0 ");
            }
            else
            {
                packetListBuilder.Append($"{j}.{(byte)inventoryType}.{item.ItemInstance.ItemVNum}.{item.ItemInstance.Rarity}.{item.ItemInstance.Upgrade}.{item.ItemInstance.GetCarvedRunesInformation(true)} ");
            }
        }

        string packetList = packetListBuilder.ToString();

        await session.EmitEventAsync(new ExchangeRegisterEvent
        {
            InventoryItems = itemList,
            BankGold = bankGold * 1000,
            Gold = gold,
            Packets = packetList
        });
    }
}