using System.Linq;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game.Extensions;
using PhoenixLib.Events;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Data.Character;
using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Pity;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class RollItemBoxEventHandler : IAsyncEventProcessor<RollItemBoxEvent>
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemBoxManager _itemBoxManager;
    private readonly IItemsManager _itemManager;
    private readonly IRandomGenerator _random;
    private readonly ISessionManager _sessionManager;
    private readonly PityConfiguration _pityConfiguration;

    public RollItemBoxEventHandler(IRandomGenerator random, IGameLanguageService gameLanguage, IItemsManager itemManager, ISessionManager sessionManager,
        IGameItemInstanceFactory gameItemInstanceFactory, IItemBoxManager itemBoxManager, PityConfiguration pityConfiguration)
    {
        _random = random;
        _gameLanguage = gameLanguage;
        _itemManager = itemManager;
        _sessionManager = sessionManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _itemBoxManager = itemBoxManager;
        _pityConfiguration = pityConfiguration;
    }

    public async Task HandleAsync(RollItemBoxEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        InventoryItem item = e.Item;

        if (session.PlayerEntity.IsInExchange() || session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsShopping
            || session.PlayerEntity.IsWarehouseOpen || session.PlayerEntity.IsPartnerWarehouseOpen || session.PlayerEntity.IsFamilyWarehouseOpen)
        {
            return;
        }
        
        // ItemBoxManager
        ItemBoxDto itemBox = _itemBoxManager.GetItemBoxByItemVnumAndDesign(item.ItemInstance.ItemVNum);
        if (itemBox == null)
        {
            return;
        }

        if (itemBox.ItemBoxType == ItemBoxType.BUNDLE)
        {
            foreach (ItemBoxItemDto rollItem in itemBox.Items)
            {
                GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(
                    rollItem.ItemGeneratedVNum,
                    rollItem.ItemGeneratedAmount,
                    rollItem.ItemGeneratedUpgrade,
                    rollItem.ItemGeneratedRarity);
                
                await session.AddNewItemToInventory(newItem, true, sendGiftIsFull: true);
                if (itemBox.ShowsRaidBoxPanelOnOpen)
                {
                    session.SendRdiPacket(rollItem.ItemGeneratedVNum, rollItem.ItemGeneratedAmount);
                }
            }

            await session.RemoveItemFromInventory(item: item);
            return;
        }

        IReadOnlyCollection<ItemBoxItemDto> rewards = GetRandomRewards(session, itemBox, item.ItemInstance);
        List<ItemInstanceDTO> obtainedItems = new();
        foreach (ItemBoxItemDto itemBoxItem in rewards)
        {
            IGameItem createdGameItem = _itemManager.GetItem(itemBoxItem.ItemGeneratedVNum);

            if (createdGameItem == null)
            {
                continue;
            }

            GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(
                itemBoxItem.ItemGeneratedVNum,
                itemBoxItem.ItemGeneratedAmount,
                itemBoxItem.ItemGeneratedUpgrade,
                itemBoxItem.ItemGeneratedRarity);

        obtainedItems.Add(newItem);
            await session.AddNewItemToInventory(newItem, true, sendGiftIsFull: true);

            if (itemBox.ShowsRaidBoxPanelOnOpen)
            {
                session.SendRdiPacket(itemBoxItem.ItemGeneratedVNum, itemBoxItem.ItemGeneratedAmount);
            }
        }

        await session.RemoveItemFromInventory(item: item);
        await session.EmitEventAsync(new BoxOpenedEvent
        {
            Box = e.Item.ItemInstance,
            Rewards = obtainedItems
        });
    }

    private IReadOnlyCollection<ItemBoxItemDto> GetRandomRewards(IClientSession session, ItemBoxDto box, GameItemInstance boxItem)
    {
        var rewards = new List<ItemBoxItemDto>();

        int minimumRoll = box.MinimumRewards ?? 1;
        int maximumRoll = box.MaximumRewards ?? minimumRoll;
        int rolls = _random.RandomNumber(minimumRoll, maximumRoll + 1);

        // Group by category (probability)
        var possibleRewards = new Dictionary<short, List<ItemBoxItemDto>>();
        foreach (ItemBoxItemDto item in box.Items)
        {
            if (!possibleRewards.ContainsKey(item.Probability))
            {
                possibleRewards[item.Probability] = [];
            }

            possibleRewards[item.Probability].Add(item);
        }

        var randomBag = new RandomBag<List<ItemBoxItemDto>>(_random);
        foreach ((short categoryChance, List<ItemBoxItemDto> items) in possibleRewards)
        {
            randomBag.AddEntry(items, categoryChance);
        }

        PityInfo pityInfo = _pityConfiguration.PityInfo.FirstOrDefault(p => p.PityType == PityType.RandomBox);
        bool isPityItem = pityInfo?.PityData.Any(p => p.ItemVnum == boxItem.ItemVNum) == true;

        ItemBoxItemDto lowestChanceItem = GetLowestChanceItem(box);
        bool gotLowestChanceItem = false;

        for (int i = 0; i < rolls; i++)
        {
            List<ItemBoxItemDto> randomCategory = randomBag.GetRandom();
            ItemBoxItemDto rolledItem = randomCategory.ElementAt(_random.RandomNumber(randomCategory.Count));
            rewards.Add(rolledItem);

            if (isPityItem && rolledItem.ItemGeneratedVNum == lowestChanceItem.ItemGeneratedVNum)
            {
                gotLowestChanceItem = true;
            }
        }

        if (gotLowestChanceItem)
        {
            boxItem.PityCounter[(int)PityType.RandomBox] = 0;
            CharacterPityDto playerPity = session.PlayerEntity.PityDto.FirstOrDefault(p => p.ItemVnum == boxItem.ItemVNum);
            if (playerPity != null)
            {
                playerPity.PityCounter = 0;
            }

            session.SendChatMessage(session.GetLanguage(GameDialogKey.PITY_CHATMESSAGE_SUCCESS), ChatMessageColorType.Green);
        }
        else
        {
            CharacterPityDto playerPity;
            switch (isPityItem)
            {
                case true when boxItem.IsPityBox(_pityConfiguration, session.PlayerEntity):
                    rewards.Clear();
                    rewards.Add(lowestChanceItem);
                    boxItem.PityCounter[(int)PityType.RandomBox] = 0;
                    playerPity = session.PlayerEntity.PityDto.FirstOrDefault(p => p.ItemVnum == boxItem.ItemVNum);
                    if (playerPity != null)
                    {
                        playerPity.PityCounter = 0;
                    }

                    session.SendChatMessage(session.GetLanguage(GameDialogKey.PITY_CHATMESSAGE_SUCCESS), ChatMessageColorType.Green);
                    break;
                case true:
                {
                    boxItem.PityCounter[(int)PityType.RandomBox]++;
                    playerPity = session.PlayerEntity.PityDto.FirstOrDefault(p => p.ItemVnum == boxItem.ItemVNum);
                    if (playerPity != null)
                    {
                        playerPity.PityCounter = boxItem.PityCounter[(int)PityType.RandomBox];
                    }

                    (int, int) maxFailCounter = boxItem.BoxPityMaxFailCounter(_pityConfiguration, session.PlayerEntity);
                    session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.PITY_CHATMESSAGE_FAIL, maxFailCounter.Item1, maxFailCounter.Item2), ChatMessageColorType.Green);
                    break;
                }
            }
        }

        return rewards;
    }

    private ItemBoxItemDto GetLowestChanceItem(ItemBoxDto box)
    {
        var lowestChanceItems = box.Items
            .Where(item => item.Probability == box.Items.Min(i => i.Probability))
            .ToList();

        if (lowestChanceItems.Count == 1)
        {
            return lowestChanceItems.First();
        }
        
        int randomIndex = _random.RandomNumber(0, lowestChanceItems.Count);
        return lowestChanceItems[randomIndex];
    }
}
