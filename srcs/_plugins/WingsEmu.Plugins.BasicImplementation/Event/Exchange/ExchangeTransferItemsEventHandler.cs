using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Exchange;
using WingsEmu.Game.Exchange.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Exchange;

public class ExchangeTransferItemsEventHandler : IAsyncEventProcessor<ExchangeTransferItemsEvent>
{
    private readonly IBankReputationConfiguration _bankReputationConfiguration;
    private readonly IGameItemInstanceFactory _gameItemInstance;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly IServerManager _serverManager;
    private readonly IAccountService _accountService;

    private readonly Lock senderLock = new();
    private readonly Lock targetLock = new();

    public ExchangeTransferItemsEventHandler(IServerManager serverManager, IGameLanguageService gameLanguage, IGameItemInstanceFactory gameItemInstance,
        IReputationConfiguration reputationConfiguration, IBankReputationConfiguration bankReputationConfiguration, IRankingManager rankingManager, IAccountService accountService)
    {
        _serverManager = serverManager;
        _gameLanguage = gameLanguage;
        _gameItemInstance = gameItemInstance;
        _reputationConfiguration = reputationConfiguration;
        _bankReputationConfiguration = bankReputationConfiguration;
        _rankingManager = rankingManager;
        _accountService = accountService;
    }

    public async Task HandleAsync(ExchangeTransferItemsEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IClientSession target = e.Target;
        bool transformGold = true;

        long maxGold = _serverManager.MaxGold;
        long maxBankGold = _serverManager.MaxBankGold;

        int senderGold = e.SenderGold;
        long senderBankGold = e.SenderBankGold;

        int targetGold = e.TargetGold;
        long targetBankGold = e.TargetBankGold;

        if (senderGold + target.PlayerEntity.Gold > maxGold)
        {
            transformGold = false;
        }

        if (targetGold + session.PlayerEntity.Gold > maxGold)
        {
            transformGold = false;
        }

        if (senderBankGold + target.Account.BankMoney > maxBankGold)
        {
            transformGold = false;
        }

        if (targetBankGold + session.Account.BankMoney > maxBankGold)
        {
            transformGold = false;
        }

        if (senderBankGold != 0)
        {
            if (!session.HasEnoughGold(session.PlayerEntity.GetBankPenalty(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation)))
            {
                await session.CloseExchange();
                session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, session.UserLanguage), MsgMessageType.Middle);
                return;
            }
        }

        if (targetBankGold != 0)
        {
            if (!target.HasEnoughGold(target.PlayerEntity.GetBankPenalty(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation)))
            {
                await target.CloseExchange();
                target.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD, target.UserLanguage), MsgMessageType.Middle);
                return;
            }
        }

        if (!transformGold)
        {
            await session.CloseExchange();
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_MAX_GOLD, session.UserLanguage), MsgMessageType.Middle);
            target.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INFORMATION_MESSAGE_MAX_GOLD, target.UserLanguage), MsgMessageType.Middle);
            return;
        }

        IReadOnlyList<ExchangeItem> senderItems = e.SenderItems;
        IReadOnlyList<ExchangeItem> targetItems = e.TargetItems;

        bool targetCanReceiveSenderItems = CanReceive(target.PlayerEntity, senderItems);

        if (!targetCanReceiveSenderItems)
        {
            await session.CloseExchange();
            target.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, target.UserLanguage), MsgMessageType.Middle);
            return;
        }

        bool senderCanReceiveTargetItems = CanReceive(session.PlayerEntity, targetItems);

        if (!senderCanReceiveTargetItems)
        {
            await session.CloseExchange();
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE, target.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (senderBankGold != 0)
        {
            session.PlayerEntity.Gold -= session.PlayerEntity.GetBankPenalty(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
        }

        if (targetBankGold != 0)
        {
            target.PlayerEntity.Gold -= target.PlayerEntity.GetBankPenalty(_reputationConfiguration, _bankReputationConfiguration, _rankingManager.TopReputation);
        }

        session.PlayerEntity.Gold -= senderGold;
        target.PlayerEntity.Gold += senderGold;

        target.PlayerEntity.Gold -= targetGold;
        session.PlayerEntity.Gold += targetGold;

        session.Account.BankMoney -= senderBankGold;
        target.Account.BankMoney += senderBankGold;

        target.Account.BankMoney -= targetBankGold;
        session.Account.BankMoney += targetBankGold;

        session.RefreshGold();
        target.RefreshGold();

        List<(GameItemInstance, int)> senderInventoryItems = [];
        List<(GameItemInstance, int)> targetInventoryItems = [];

        Task senderTask = ProcessExchangeItemsAsync(session, target, e.SenderItems, senderInventoryItems, senderLock, true);
        Task targetTask = ProcessExchangeItemsAsync(target, session, e.TargetItems, targetInventoryItems, targetLock, false);

        await Task.WhenAll(senderTask, targetTask);

        await session.EmitEventAsync(new ExchangeCompletedEvent
        {
            Target = target,
            SenderGold = senderGold,
            SenderBankGold = senderBankGold,
            SenderItems = senderInventoryItems.Select(s => (_gameItemInstance.CreateDto(s.Item1), s.Item2)).ToList(),
            TargetItems = targetInventoryItems.Select(s => (_gameItemInstance.CreateDto(s.Item1), s.Item2)).ToList(),
            TargetGold = targetGold,
            TargetBankGold = targetBankGold
        });

        await session.CloseExchange(ExcCloseType.Successful);
    }

    private async Task ProcessExchangeItemsAsync(IClientSession sourceSession, IClientSession targetSession, IEnumerable<ExchangeItem> exchangeItems, List<(GameItemInstance, int)> inventoryItems, Lock sourceLock, bool isSender)
    {
        foreach (ExchangeItem item in exchangeItems)
        {

            lock (sourceLock)
            {
                InventoryItem inventoryItem = sourceSession.PlayerEntity.GetItemBySlotAndType(item.Slot, item.Type);

                if (inventoryItem.ItemInstance.Type != ItemInstanceType.NORMAL_ITEM)
                {
                    GameItemInstance deepCopy = _gameItemInstance.DuplicateItem(inventoryItem.ItemInstance);
                    deepCopy.Amount = 1;
                    deepCopy.PityCounter.Clear();

                    bool asGift = !targetSession.PlayerEntity.HasSpaceFor(deepCopy.ItemVNum);
                    targetSession.AddNewItemToInventory(deepCopy, sendGiftIsFull: asGift).Wait();
                }
                else
                {
                    GameItemInstance newItem = _gameItemInstance.CreateItem(inventoryItem.ItemInstance.ItemVNum, item.Amount);
                    bool asGift = !targetSession.PlayerEntity.HasSpaceFor(newItem.ItemVNum, item.Amount);
                    targetSession.AddNewItemToInventory(newItem, sendGiftIsFull: asGift).Wait();
                }

                inventoryItems.Add((inventoryItem.ItemInstance, item.Amount));
                sourceSession.RemoveItemFromInventory(inventoryItem.ItemInstance.ItemVNum, item.Amount, item: inventoryItem).Wait();
            }
        }
    }

    private bool CanReceive(IPlayerEntity playerEntity, IReadOnlyList<ExchangeItem> items)
    {
        var dictionary = new Dictionary<InventoryType, short>();
        int counter = 0;

        if (!items.Any())
        {
            return true;
        }

        foreach (ExchangeItem item in items)
        {
            InventoryType type = item.Type;
            short slots = playerEntity.GetInventorySlots(false, type);

            for (short i = 0; i < slots; i++)
            {
                if (type != InventoryType.Etc && type != InventoryType.Main && type != InventoryType.Equipment)
                {
                    return false;
                }

                if (dictionary.TryGetValue(type, out short slot))
                {
                    if (i == slot)
                    {
                        continue;
                    }
                }

                InventoryItem freeSlot = playerEntity.GetItemBySlotAndType(i, type);
                if (freeSlot?.ItemInstance != null)
                {
                    continue;
                }

                counter++;
                dictionary[item.Type] = i;
                break;
            }
        }

        return counter != 0 && counter == items.Count;
    }
}