using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._packetHandling;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateDeathEventHandler : IAsyncEventProcessor<MateDeathEvent>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IItemsManager _itemManager;
    private readonly MateRevivalConfiguration _mateRevivalConfiguration;
    private readonly GameMinMaxConfiguration _minMaxConfiguration;
    private readonly IRevivalManager _revivalManager;
    private readonly IBuffFactory _buffFactory;
    private readonly IMateBuffConfigsContainer _mateBuffConfigsContainer;

    public MateDeathEventHandler(IItemsManager itemManager, IGameLanguageService gameLanguage, GameRevivalConfiguration gameRevivalConfiguration, IRevivalManager revivalManager, IBuffFactory buffFactory, IMateBuffConfigsContainer mateBuffConfigsContainer,
        GameMinMaxConfiguration minMaxConfiguration)
    {
        _gameLanguage = gameLanguage;
        _itemManager = itemManager;
        _mateRevivalConfiguration = gameRevivalConfiguration.MateRevivalConfiguration;
        _revivalManager = revivalManager;
        _minMaxConfiguration = minMaxConfiguration;
        _buffFactory = buffFactory;
        _mateBuffConfigsContainer = mateBuffConfigsContainer;
    }

    public async Task HandleAsync(MateDeathEvent e, CancellationToken cancellation)
    {
        if (e.MateEntity.IsAlive())
        {
            return;
        }
        
        if (e.MateEntity.NpcMonsterVNum is (int)MonsterVnum.SUN_WOLF)
        {
            e.Sender.PlayerEntity.IsSunWolfDead = true;
            await e.Sender.EmitEventAsync(new MateRemoveEvent
            {
                MateEntity = e.MateEntity
            });
            return;
        }

        Guid id = _revivalManager.RegisterRevival(e.MateEntity.Id);

        if (id == default)
        {
            return;
        }

        e.MateEntity.LastDeath = DateTime.UtcNow;
        e.MateEntity.Killer = e.Killer;
        await e.MateEntity.RemoveAllBuffsAsync(true);

        await MateFateDictator(e, id);

        e.Sender.SendPetInfo(e.MateEntity, _gameLanguage);
        e.Sender.SendMateLife(e.MateEntity);
    }

    private async Task MateFateDictator(MateDeathEvent e, Guid expectedGuid)
    {
        if (e.MateEntity.MapInstance?.Id == e.Sender.PlayerEntity.Miniland?.Id)
        {
            await e.Sender.EmitEventAsync(new MateReviveEvent(e.MateEntity, false, expectedGuid));
            return;
        }

        if (e.MateEntity.MateType == MateType.Pet ? e.Sender.PlayerEntity.IsPetAutoRelive : e.Sender.PlayerEntity.IsPartnerAutoRelive)
        {
            if (await MateTrySaveByGuardian(e.MateEntity))
            {
                e.MateEntity.AddLoyalty(_mateRevivalConfiguration.LoyaltyDeathPenalizationAmount, _minMaxConfiguration, _gameLanguage);
                _revivalManager.TryUnregisterRevival(e.MateEntity.Id);
                e.MateEntity.SpawnMateByGuardian = DateTime.UtcNow;
                await e.MateEntity.Owner.Session.AddPetBuff(e.MateEntity, _mateBuffConfigsContainer, _buffFactory);
                return;
            }

            e.MateEntity.RemoveLoyalty(_mateRevivalConfiguration.LoyaltyDeathPenalizationAmount, _minMaxConfiguration, _gameLanguage);

            if (MateTryDelayedRevival(e))
            {
                e.MateEntity.UpdateRevival(DateTime.UtcNow + _mateRevivalConfiguration.DelayedRevivalDelay, true);
                GameDialogKey gameDialogKey = e.MateEntity.MateType == MateType.Pet ? GameDialogKey.PET_SHOUTMESSAGE_WILL_BE_BACK : GameDialogKey.PARTNER_SHOUTMESSAGE_WILL_BE_BACK;
                e.Sender.SendMsg(_gameLanguage.GetLanguageFormat(gameDialogKey, e.Sender.UserLanguage, e.MateEntity.MateType), MsgMessageType.Middle);
                return;
            }

            e.Sender.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, e.Sender.UserLanguage,
                    _mateRevivalConfiguration.DelayedRevivalPenalizationSaverAmount,
                    _gameLanguage.GetItemName(_itemManager.GetItem(_mateRevivalConfiguration.DelayedRevivalPenalizationSaver), e.Sender)),
                MsgMessageType.Middle);
        }

        GameDialogKey key = e.MateEntity.MateType == MateType.Pet ? GameDialogKey.PET_SHOUTMESSAGE_WENT_BACK_TO_MINILAND : GameDialogKey.PARTNER_SHOUTMESSAGE_WENT_BACK_TO_MINILAND;
        await e.Sender.EmitEventAsync(new MateBackToMinilandEvent(e.MateEntity, expectedGuid));
        e.Sender.SendMsg(_gameLanguage.GetLanguage(key, e.Sender.UserLanguage), MsgMessageType.Middle);
    }

    private bool MateTryDelayedRevival(PlayerEvent e)
    {
        if (!e.Sender.PlayerEntity.HasItem(_mateRevivalConfiguration.DelayedRevivalPenalizationSaver, (short)_mateRevivalConfiguration.DelayedRevivalPenalizationSaverAmount))
        {
            return false;
        }

        e.Sender.RemoveItemFromInventory(_mateRevivalConfiguration.DelayedRevivalPenalizationSaver, (short)_mateRevivalConfiguration.DelayedRevivalPenalizationSaverAmount);
        return true;
    }

    private async Task<bool> MateTrySaveByGuardian(IMateEntity mateEntity)
    {
        IPlayerEntity owner = mateEntity.Owner;
        if (mateEntity.MateType == MateType.Pet ? !owner.IsPetAutoRelive : !owner.IsPartnerAutoRelive)
        {
            return false;
        }

        if (mateEntity.MapInstance.MapInstanceType == MapInstanceType.RainbowBattle)
        {
            return false;
        }

        if (mateEntity.MapInstance.MapInstanceType == MapInstanceType.Icebreaker)
        {
            return false;
        }

        bool shouldSave = false;

        List<int> itemNeeded = mateEntity.MateType == MateType.Pet
            ? _mateRevivalConfiguration.MateInstantRevivalPenalizationSaver
            : _mateRevivalConfiguration.PartnerInstantRevivalPenalizationSaver;

        foreach (int item in itemNeeded)
        {
            InventoryItem getItem = owner.GetFirstItemByVnum(item);
            if (getItem == null)
            {
                continue;
            }

            await owner.Session.RemoveItemFromInventory(item: getItem);
            mateEntity.SavedGuardianVnum = item;
            shouldSave = true;
            break;
        }

        if (!shouldSave)
        {
            return false;
        }

        GameDialogKey gameDialogKey = mateEntity.MateType == MateType.Pet ? GameDialogKey.PET_SHOUTMESSGE_SAVED_BY_SAVER : GameDialogKey.PARTNER_SHOUTMESSAGE_SAVED_BY_SAVER;
        owner.Session.SendMsg(owner.Session.GetLanguage(gameDialogKey), MsgMessageType.Middle);

        return true;
    }
}