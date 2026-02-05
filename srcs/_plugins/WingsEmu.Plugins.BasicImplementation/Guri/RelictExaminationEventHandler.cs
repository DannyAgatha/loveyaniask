using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.BattlePass;
using WingsEmu.DTOs.Items;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class RelictExaminationEventHandler : IGuriHandler
{
    private readonly IDropRarityConfigurationProvider _dropRarityConfigurationProvider;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IRandomGenerator _randomGenerator;

    private readonly RelictConfiguration _relictConfiguration;

    public RelictExaminationEventHandler(RelictConfiguration relictConfiguration, IRandomGenerator randomGenerator, IItemsManager itemsManager, IGameItemInstanceFactory gameItemInstanceFactory,
        IDropRarityConfigurationProvider dropRarityConfigurationProvider)
    {
        _relictConfiguration = relictConfiguration;
        _randomGenerator = randomGenerator;
        _itemsManager = itemsManager;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _dropRarityConfigurationProvider = dropRarityConfigurationProvider;
    }

    public long GuriEffectId => 1502;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        // 10000 => UNKNOWN RELICT
        // 30000 => MYSTERIOUS RELICT
        if (e.Packet.Length != 4)
        {
            return;
        }

        if (session.PlayerEntity.Level < 60)
        {
            session.SendMsg(session.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_RELICT_LOW_LEVEL), MsgMessageType.Middle);
            return;
        }

        if (!int.TryParse(e.Packet[3], out int effect))
        {
            return;
        }

        if (session.PlayerEntity.IsInExchange())
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, $"{session.CharacterName()} is trying use relic item while being in exchange !");
            return;
        }

        int relictVnum = effect == 30000 ? (int)ItemVnums.MYSTERIOUS_RELICT : (int)ItemVnums.UNKNOWN_RELICT;

        InventoryItem relictItem = session.PlayerEntity.GetFirstItemByVnum(relictVnum);

        if (relictItem == null)
        {
            await e.Sender.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Tried to examinate a relict without having one.");
            return;
        }

        RelictConfigurationInfo relictInfo = _relictConfiguration.FirstOrDefault(s => s.RelictVnum == relictVnum);
        if (relictInfo == null)
        {
            return;
        }

        if (session.PlayerEntity.Gold < relictInfo.ExaminationCost)
        {
            return;
        }

        session.PlayerEntity.RemoveGold(relictInfo.ExaminationCost);
        await session.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.SpendXGoldToNpc, relictInfo.ExaminationCost));
        await session.RemoveItemFromInventory(item: relictItem, amount: 1);

        var randomBag = new RandomBag<RelictRollCategory>(_randomGenerator);
        foreach (RelictRollCategory relictRollCategory in relictInfo.Rolls)
        {
            randomBag.AddEntry(relictRollCategory, relictRollCategory.Chance);
        }

        RelictRollCategory rndCategory = randomBag.GetRandom();
        RelictRollItem rndItem = rndCategory.Items.ElementAt(_randomGenerator.RandomNumber(rndCategory.Items.Count));

        IGameItem item = _itemsManager.GetItem(rndItem.ItemVnum);
        sbyte rarity = _dropRarityConfigurationProvider.GetRandomRarity(item.ItemType);

        GameItemInstance itemToAdd = _gameItemInstanceFactory.CreateItem(rndItem.ItemVnum, rndItem.Amount, 0, rarity);

        InventoryItem inventoryItem = await session.AddNewItemToInventory(itemToAdd, true, sendGiftIsFull: true);
        session.SendPdtiPacket(PdtiType.ItemIdentificationSuccessful, rndItem.ItemVnum, (short)rndItem.Amount, inventoryItem.Slot, itemToAdd.Upgrade, rarity);
        session.SendSound(SoundType.CRAFTING_SUCCESS);

        session.RefreshGold();
        session.SendShopEndPacket(ShopEndType.Npc);

        ItemInstanceDTO reward = _gameItemInstanceFactory.CreateDto(itemToAdd);

        await session.EmitEventAsync(new BoxOpenedEvent
        {
            Box = relictItem.ItemInstance,
            Rewards = new List<ItemInstanceDTO>
            {
                reward
            }
        });
    }
}