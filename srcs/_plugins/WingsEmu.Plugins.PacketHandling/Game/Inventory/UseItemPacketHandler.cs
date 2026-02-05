using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Quests;
using WingsEmu.DTOs.ServerDatas;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;
using NosEmu.Plugins.BasicImplementations.Vehicles;

namespace WingsEmu.Plugins.PacketHandling.Game.Inventory;

public class UseItemPacketHandler : GenericGamePacketHandlerBase<UseItemPacket>
{
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly ICostumeScrollConfiguration _costumeScrollConfiguration;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemBoxManager _itemBoxManager;
    private readonly IVehicleConfigurationProvider _vehicleConfiguration;

    public UseItemPacketHandler(IGameLanguageService gameLanguage, IItemBoxManager itemBoxManager, IVehicleConfigurationProvider vehicleConfiguration,
        ICostumeScrollConfiguration costumeScrollConfiguration, IBCardEffectHandlerContainer bCardEffectHandlerContainer)
    {
        _gameLanguage = gameLanguage;
        _itemBoxManager = itemBoxManager;
        _vehicleConfiguration = vehicleConfiguration;
        _costumeScrollConfiguration = costumeScrollConfiguration;
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
    }

    protected override async Task HandlePacketAsync(IClientSession session, UseItemPacket useItemPacket)
    {
        if (session.PlayerEntity.IsInExchange())
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, "Tried to use item while being in exchange");
            return;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.NORMAL, "Tried to use item while having shop open");
            return;
        }

        if (session.PlayerEntity.IsSeal)
        {
            return;
        }

        if (session.PlayerEntity.CheatComponent.IsInvisible)
        {
            return;
        }

        if (!session.PlayerEntity.IsAlive())
        {
            return;
        }

        if ((byte)useItemPacket.Type >= 9)
        {
            return;
        }

        InventoryItem inv = session.PlayerEntity.GetItemBySlotAndType(useItemPacket.Slot, useItemPacket.Type);
        string[] split = useItemPacket.OriginalContent.Split(' ', '^');

        if (inv == null)
        {
            return;
        }

        /*
         * You can use only potions and snack being on vehicle / be morphed and:
         * Speed Booster / Limited and vehicle
         * Costume scroll
         */
        if (inv.ItemInstance.Type == ItemInstanceType.NORMAL_ITEM)
        {
            if (!CanUseItemInVehicle(session, inv))
            {
                session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_CHATMESSAGE_ON_MOUNT), ChatMessageColorType.Yellow);
                return;
            }

            if (session.PlayerEntity.IsMorphed)
            {
                IGameItem gameItem = inv.ItemInstance.GameItem;
                IReadOnlyList<short> morphs = _costumeScrollConfiguration.GetScrollMorphs((short)gameItem.Id);
                if (gameItem.ItemType is not ItemType.Potion and ItemType.Snack && (morphs == null || !morphs.Any()))
                {
                    return;
                }
            }
        }

        if (inv.ItemInstance.GameItem.ItemType == ItemType.Special)
        {
            IGameItem gameItem = inv.ItemInstance.GameItem;
            foreach (BCardDTO c in gameItem.BCards)
            {
                _bCardEffectHandlerContainer.Execute(session.PlayerEntity, session.PlayerEntity, c);
            }

            ItemBoxDto itemBox = _itemBoxManager.GetItemBoxByItemVnumAndDesign(inv.ItemInstance.ItemVNum);

            if (itemBox != null && itemBox.Items.Count > 0)
            {
                if (split.Length == 9 || gameItem.ItemSubType == 3)
                {
                    session.SendQnaPacket($"guri 4999 8023 {inv.Slot}", _gameLanguage.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_OPEN_BOX, session.UserLanguage));
                    return;
                }
            }
        }

        CheckQuests(session, inv.ItemInstance);
        switch (inv.ItemInstance.Type)
        {
            case ItemInstanceType.SpecialistInstance when !inv.ItemInstance.GameItem.IsPartnerSpecialist:
                await session.EmitEventAsync(new InventoryEquipItemEvent(inv.Slot, true, InventoryType.Specialist));
                return;
            case ItemInstanceType.WearableInstance when inv.ItemInstance.GameItem.ItemType == ItemType.Fashion && inv.InventoryType == InventoryType.Costume:
                await session.EmitEventAsync(new InventoryEquipItemEvent(inv.Slot, true, InventoryType.Costume));
                return;
            default:
                await session.EmitEventAsync(new InventoryUseItemEvent
                {
                    Item = inv,
                    Option = split[1].ElementAt(0) == '#' ? (byte)255 : (byte)0,
                    Packet = split
                });
                break;
        }
    }

    private bool CanUseItemInVehicle(IClientSession session, InventoryItem item)
    {
        if (!session.PlayerEntity.IsOnVehicle)
        {
            return true;
        }

        if (item.ItemInstance.GameItem.ItemType is ItemType.Potion or ItemType.Snack)
        {
            return true;
        }

        if (item.ItemInstance.GameItem.Id is (short)ItemVnums.SPEED_BOOSTER or (short)ItemVnums.SPEED_BOOSTER_LIMITED)
        {
            return true;
        }

        if (item.ItemInstance.GameItem.ItemType is ItemType.Box && item.ItemInstance.GameItem.ItemSubType == 7)
        {
            return true;
        }

        return _vehicleConfiguration.GetByVehicleVnum(item.ItemInstance.GameItem.Id) != null;
    }

    private void CheckQuests(IClientSession session, GameItemInstance inv)
    {
        IEnumerable<CharacterQuest> characterQuests = session.PlayerEntity.GetCurrentQuestsByType(QuestType.USE_ITEM_ON_TARGET);
        foreach (CharacterQuest quest in characterQuests)
        {
            IReadOnlyCollection<QuestObjectiveDto> objectives = quest.Quest.Objectives;
            foreach (QuestObjectiveDto objective in objectives)
            {
                if (inv.ItemVNum != objective.Data0)
                {
                    continue;
                }


                IEntity entity = session.CurrentMapInstance.GetBattleEntity(session.PlayerEntity.LastEntity.Item1, session.PlayerEntity.LastEntity.Item2);
                if (entity == null)
                {
                    continue;
                }

                CharacterQuestObjectiveDto questObjectiveDto = quest.ObjectiveAmount[objective.ObjectiveIndex];
                if (entity.IsMonster())
                {
                    var monster = (IMonsterEntity)entity;
                    if (monster.MonsterVNum != objective.Data1)
                    {
                        continue;
                    }

                    if (questObjectiveDto.CurrentAmount == 0)
                    {
                        questObjectiveDto.CurrentAmount++;
                        session.EmitEventAsync(new QuestObjectiveUpdatedEvent
                        {
                            CharacterQuest = quest
                        });
                        session.RemoveItemFromInventory(inv.ItemVNum);
                    }
                }

                else if (entity.IsNpc())
                {
                    var npc = (INpcEntity)entity;
                    if (npc.NpcVNum != objective.Data1)
                    {
                        continue;
                    }

                    if (questObjectiveDto.CurrentAmount == 0)
                    {
                        questObjectiveDto.CurrentAmount++;
                        session.EmitEventAsync(new QuestObjectiveUpdatedEvent
                        {
                            CharacterQuest = quest
                        });
                        session.RemoveItemFromInventory(inv.ItemVNum);
                    }
                }

                if (session.PlayerEntity.IsQuestCompleted(quest))
                {
                    session.EmitEvent(new QuestCompletedEvent(quest));
                }
            }
        }
    }
}