using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Characters.Events;
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

public class MateBeadHandler : IItemHandler
{
    private readonly IGameLanguageService _languageService;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly ISpPartnerConfiguration _spPartner;

    public MateBeadHandler(IGameLanguageService languageService, INpcMonsterManager npcMonsterManager, ISpPartnerConfiguration spPartner, IMateEntityFactory mateEntityFactory)
    {
        _languageService = languageService;
        _npcMonsterManager = npcMonsterManager;
        _spPartner = spPartner;
        _mateEntityFactory = mateEntityFactory;
    }

    public ItemType ItemType => ItemType.Box;
    public long[] Effects => new long[] { 0 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IGameItem item = e.Item.ItemInstance.GameItem;

        switch (item.ItemSubType)
        {
            case 7: // Magic Speed Booster
                await CheckMagicSpeedBooster(session, e);
                return;
            // Mate/Partner bead
            case 0:
            case 1:
                break;
            // It's not Mate/Partner bead
            default:
                return;
        }

        if (e.Option == 0)
        {
            session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(byte)e.Item.ItemInstance.GameItem.Type} {e.Item.Slot} 3",
                _languageService.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_OPEN_PET_BEAD, session.UserLanguage));
            return;
        }

        IMonsterData data = _npcMonsterManager.GetNpc((short)e.Item.ItemInstance.GameItem.EffectValue);
        if (data == null)
        {
            Log.Info($"Couldn't find monster with vnum {e.Item.ItemInstance.GameItem.EffectValue}");
            return;
        }

        var heldMonster = new MonsterData(data);

        if (session.CurrentMapInstance != session.PlayerEntity.Miniland)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.ITEM_SHOUTMESSAGE_ONLY_IN_MINILAND, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (session.PlayerEntity.MateComponent.GetMates(x => x.MonsterVNum == heldMonster.MonsterVNum && x.MateType == MateType.Partner).Any() && e.Item.ItemInstance.GameItem.ItemSubType == 1)
        {
            session.SendMsg(_languageService.GetLanguage(GameDialogKey.PARTNER_SHOUTMESSAGE_ALREADY_HAVE_SAME_PARTNER, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!session.PlayerEntity.CanReceiveMate(e.Item.ItemInstance.GameItem.ItemSubType == 1 ? MateType.Partner : MateType.Pet))
        {
            session.SendMsg(
                _languageService.GetLanguage(
                    e.Item.ItemInstance.GameItem.ItemSubType == 1 ? GameDialogKey.INFORMATION_SHOUTMESSAGE_MAX_PARTNER_COUNT : GameDialogKey.INFORMATION_SHOUTMESSAGE_MAX_PET_COUNT,
                    session.UserLanguage),
                MsgMessageType.Middle);
            return;
        }

        IMateEntity mateEntity = _mateEntityFactory.CreateMateEntity(session.PlayerEntity, heldMonster, e.Item.ItemInstance.GameItem.ItemSubType == 1 ? MateType.Partner : MateType.Pet,
            e.Item.ItemInstance.GameItem.LevelMinimum, 0, new List<int>(), e.Item.ItemInstance.IsLimitedMatePearl);
        
        if (mateEntity.MateType is MateType.Pet)
        {
            mateEntity.Stars = heldMonster.Stars;
        }

        await session.EmitEventAsync(new MateInitializeEvent
        {
            MateEntity = mateEntity
        });

        session.CurrentMapInstance.AddMate(mateEntity);
        session.CurrentMapInstance.Broadcast(s => mateEntity.GenerateIn(_languageService, s.UserLanguage, _spPartner));
        session.SendCondMate(mateEntity);
        string mateName = _languageService.GetLanguage(GameDataType.NpcMonster, mateEntity.Name, session.UserLanguage);
        GameDialogKey key = mateEntity.MateType == MateType.Pet ? GameDialogKey.PET_CHATMESSAGE_BEAD_EXTRACT : GameDialogKey.PARTNER_CHATMESSAGE_BEAD_EXTRACT;
        session.SendChatMessage(_languageService.GetLanguageFormat(key, session.UserLanguage, mateName), ChatMessageColorType.Green);

        await session.RemoveItemFromInventory(item: e.Item);

        key = mateEntity.MateType == MateType.Pet ? GameDialogKey.PET_INFO_LEAVE_BEAD : GameDialogKey.PARTNER_INFO_LEAVE_BEAD;
        session.SendInfo(_languageService.GetLanguage(key, session.UserLanguage));
        session.SendScnPackets();
        session.SendScpPackets();
    }

    private async Task CheckMagicSpeedBooster(IClientSession session, InventoryUseItemEvent inventoryUseItemEvent)
    {
        InventoryItem item = inventoryUseItemEvent.Item;
        if (item.ItemInstance.IsBound)
        {
            await session.EmitEventAsync(new SpeedBoosterEvent());
            return;
        }

        if (inventoryUseItemEvent.Option == 0)
        {
            session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(byte)item.ItemInstance.GameItem.Type} {item.Slot} 1",
                _languageService.GetLanguage(GameDialogKey.ITEM_DIALOG_ASK_NOT_TRADABLE, session.UserLanguage));
            return;
        }

        item.ItemInstance.BoundCharacterId = session.PlayerEntity.Id;

        item.ItemInstance.ItemDeleteTime = item.ItemInstance.GameItem.ItemValidTime switch
        {
            -1 => null,
            > 0 => DateTime.UtcNow.AddSeconds(item.ItemInstance.GameItem.ItemValidTime),
            _ => item.ItemInstance.ItemDeleteTime
        };

        session.SendInventoryAddPacket(item);
    }
}