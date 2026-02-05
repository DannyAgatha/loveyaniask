using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Equipment.Box;

/// <summary>
///     This handler is used by player related bead (bead with custom item in or without item)
/// </summary>
public class UserBeadHandler : IItemUsageByVnumHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _languageService;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly ISpPartnerConfiguration _spPartner;

    public UserBeadHandler(IGameLanguageService languageService, INpcMonsterManager npcMonsterManager, ISpPartnerConfiguration spPartner, IGameItemInstanceFactory gameItemInstanceFactory,
        IMateEntityFactory mateEntityFactory)
    {
        _languageService = languageService;
        _npcMonsterManager = npcMonsterManager;
        _spPartner = spPartner;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _mateEntityFactory = mateEntityFactory;
    }

    public long[] Vnums => new[] { (long)ItemVnums.PET_BEAD, (long)ItemVnums.PARTNER_BEAD, (long)ItemVnums.MOUNT_BEAD, (long)ItemVnums.FAIRY_BEAD, (long)ItemVnums.TRAINER_PET_BEAD, (long)ItemVnums.TRAINER_PET_BEAD_LIMITED };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        int beadVnum = e.Item.ItemInstance.ItemVNum;

        InventoryItem box = session.PlayerEntity.GetItemBySlotAndType(e.Item.Slot, InventoryType.Equipment);

        if (box == null)
        {
            return;
        }

        if (box.ItemInstance.Type != ItemInstanceType.BoxInstance)
        {
            return;
        }

        GameItemInstance boxInstanceItem = box.ItemInstance;

        switch (beadVnum)
        {
            case (int)ItemVnums.FAIRY_BEAD when boxInstanceItem.HoldingVNum == 0 || boxInstanceItem.HoldingVNum == null:
                session.SendGuriPacket(26, 0, e.Item.Slot);
                return;
            case (int)ItemVnums.FAIRY_BEAD:
            {
                if (!session.PlayerEntity.HasSpaceFor(boxInstanceItem.HoldingVNum.Value))
                {
                    session.SendMsg(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE), MsgMessageType.Middle);
                    session.SendChatMessage(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE), ChatMessageColorType.Yellow);
                    return;
                }

                GameItemInstance newFairy = _gameItemInstanceFactory.CreateItem(boxInstanceItem.HoldingVNum.Value);

                await session.AddNewItemToInventory(newFairy, true, ChatMessageColorType.Green, true);
                newFairy.ElementRate = boxInstanceItem.ElementRate;
                newFairy.BoundCharacterId = session.PlayerEntity.Id;
                newFairy.Xp = boxInstanceItem.Xp;
                await session.RemoveItemFromInventory(item: e.Item);
                return;
            }
            case (int)ItemVnums.MOUNT_BEAD when boxInstanceItem.HoldingVNum == 0 || boxInstanceItem.HoldingVNum == null:
                session.SendGuriPacket(24, 0, e.Item.Slot);
                return;
            case (int)ItemVnums.MOUNT_BEAD:
            {
                if (!session.PlayerEntity.HasSpaceFor(boxInstanceItem.HoldingVNum.Value))
                {
                    session.SendMsg(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE), MsgMessageType.Middle);
                    session.SendChatMessage(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_PLACE), ChatMessageColorType.Yellow);
                    return;
                }

                GameItemInstance newMount = _gameItemInstanceFactory.CreateItem(boxInstanceItem.HoldingVNum.Value);
                await session.AddNewItemToInventory(newMount, true, ChatMessageColorType.Green, true);
                await session.RemoveItemFromInventory(item: box);
                return;
            }
            case (int)ItemVnums.PET_BEAD:
            case (int)ItemVnums.PARTNER_BEAD:
            case (int)ItemVnums.TRAINER_PET_BEAD:
            case (int)ItemVnums.TRAINER_PET_BEAD_LIMITED:
            {
                if (e.Option == 0)
                {
                    if (boxInstanceItem.HoldingVNum != null && boxInstanceItem.HoldingVNum != 0)
                    {
                        session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(int)box.InventoryType} {e.Item.Slot} 1",
                            _languageService.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_OPEN_PET_BEAD, session.UserLanguage));
                        return;
                    }

                    if (e.Packet.Length < 4)
                    {
                        return;
                    }

                    if (!int.TryParse(e.Packet[3], out int mateId))
                    {
                        return;
                    }

                    if (!session.PlayerEntity.MateComponent.GetMates(x => x.Id == mateId && x.MateType == (beadVnum == (int)ItemVnums.PET_BEAD ? MateType.Pet : MateType.Partner)).Any())
                    {
                        return;
                    }

                    session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(int)box.InventoryType} {e.Item.Slot} {mateId}",
                        _languageService.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_PET_STORE, session.UserLanguage));
                    return;
                }

                if (boxInstanceItem.HoldingVNum == 0 || boxInstanceItem.HoldingVNum == null)
                {
                    if (e.Packet.Length < 6)
                    {
                        return;
                    }

                    if (!int.TryParse(e.Packet[6], out int petId))
                    {
                        return;
                    }

                    IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(s => s.Id == petId && s.MateType == (beadVnum is (int)ItemVnums.PET_BEAD or (int)ItemVnums.TRAINER_PET_BEAD or (int)ItemVnums.TRAINER_PET_BEAD_LIMITED ? MateType.Pet : MateType.Partner));
                    if (mateEntity == null)
                    {
                        return;
                    }

                    if (!mateEntity.IsAlive())
                    {
                        return;
                    }

                    if (mateEntity.IsTeamMember)
                    {
                        return;
                    }

                    if (mateEntity.IsLimited)
                    {
                        session.SendMsg(session.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_IS_LIMITED), MsgMessageType.Middle);
                        return;
                    }

                    if (mateEntity.MapInstance.Id != session.PlayerEntity.Miniland.Id)
                    {
                        session.SendMsg(_languageService.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_ONLY_IN_MINILAND, session.UserLanguage), MsgMessageType.Middle);
                        return;
                    }
                    
                    IMonsterData datas = _npcMonsterManager.GetNpc(mateEntity.MonsterVNum);

                    if (datas == null)
                    {
                        return;
                    }
                        
                    if (mateEntity.Stars != 6 && beadVnum is (int)ItemVnums.TRAINER_PET_BEAD or (int)ItemVnums.TRAINER_PET_BEAD_LIMITED)
                    {
                        return;
                    }

                    boxInstanceItem.MateType = mateEntity.MateType;
                    boxInstanceItem.HoldingVNum = mateEntity.NpcMonsterVNum;
                    boxInstanceItem.SpLevel = mateEntity.Level;
                    boxInstanceItem.SpDamage = mateEntity.Attack;
                    boxInstanceItem.SpDefence = mateEntity.Defence;
                    boxInstanceItem.Xp = mateEntity.Experience;
                    boxInstanceItem.Stars = mateEntity.Stars;

                    await session.EmitEventAsync(new MateRemoveEvent
                    {
                        MateEntity = mateEntity
                    });

                    GameDialogKey gameKey = mateEntity.MateType == MateType.Pet ? GameDialogKey.PET_INFO_STORED : GameDialogKey.PARTNER_INFO_STORED;
                    session.SendInfo(_languageService.GetLanguage(gameKey, session.UserLanguage));
                    return;
                }

                if (boxInstanceItem.HoldingVNum == 0 || boxInstanceItem.HoldingVNum == null)
                {
                    return;
                }

                IMonsterData data = _npcMonsterManager.GetNpc(boxInstanceItem.HoldingVNum.Value);
                if (data == null)
                {
                    return;
                }

                var heldMonster = new MonsterData(data);

                if (session.CurrentMapInstance != session.PlayerEntity.Miniland)
                {
                    session.SendMsg(_languageService.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_ONLY_IN_MINILAND, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (session.PlayerEntity.MateComponent.GetMates(x => x.MonsterVNum == heldMonster.MonsterVNum && x.MateType == MateType.Partner).Any() && boxInstanceItem.MateType == MateType.Partner)
                {
                    session.SendMsg(_languageService.GetLanguage(GameDialogKey.PARTNER_SHOUTMESSAGE_ALREADY_HAVE_SAME_PARTNER, session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                if (!boxInstanceItem.MateType.HasValue)
                {
                    return;
                }

                if (!session.PlayerEntity.CanReceiveMate(boxInstanceItem.MateType.Value))
                {
                    session.SendMsg(
                        _languageService.GetLanguage(
                            boxInstanceItem.MateType == MateType.Pet ? GameDialogKey.INFORMATION_SHOUTMESSAGE_MAX_PET_COUNT : GameDialogKey.INFORMATION_SHOUTMESSAGE_MAX_PARTNER_COUNT,
                            session.UserLanguage), MsgMessageType.Middle);
                    return;
                }

                IMateEntity mate = _mateEntityFactory.CreateMateEntity(session.PlayerEntity, heldMonster, boxInstanceItem.MateType.Value,
                    boxInstanceItem.SpLevel == 0 ? (byte)1 : boxInstanceItem.SpLevel, 1, new List<int>());
                mate.Attack = boxInstanceItem.SpDamage;
                mate.Defence = boxInstanceItem.SpDefence;
                mate.Experience = boxInstanceItem.Xp;
                mate.Stars = boxInstanceItem.Stars;

                await session.EmitEventAsync(new MateInitializeEvent
                {
                    MateEntity = mate
                });

                session.CurrentMapInstance.AddMate(mate);
                session.CurrentMapInstance.Broadcast(s => mate.GenerateIn(_languageService, s.UserLanguage, _spPartner));
                session.SendCondMate(mate);
                string mateName = mate.MateName == mate.Name ? _languageService.GetLanguage(GameDataType.NpcMonster, mate.Name, session.UserLanguage) : mate.MateName;
                GameDialogKey key = mate.MateType == MateType.Pet ? GameDialogKey.PET_CHATMESSAGE_BEAD_EXTRACT : GameDialogKey.PARTNER_CHATMESSAGE_BEAD_EXTRACT;
                session.SendChatMessage(_languageService.GetLanguageFormat(key, session.UserLanguage, mateName), ChatMessageColorType.Green);

                await session.RemoveItemFromInventory(item: e.Item);
                key = mate.MateType == MateType.Pet ? GameDialogKey.PET_INFO_LEAVE_BEAD : GameDialogKey.PARTNER_INFO_LEAVE_BEAD;
                session.SendInfo(_languageService.GetLanguage(key, session.UserLanguage));
                session.SendScnPackets();
                session.SendScpPackets();
                break;
            }
        }
    }
}