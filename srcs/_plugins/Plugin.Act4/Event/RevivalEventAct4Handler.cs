using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace Plugin.Act4.Event;

public class RevivalEventAct4Handler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;
    private readonly PlayerRevivalConfiguration _revivalConfiguration;
    private readonly ISpPartnerConfiguration _spPartner;

    public RevivalEventAct4Handler(IGameLanguageService languageService, GameRevivalConfiguration revivalConfiguration, IBuffFactory buffFactory, IItemsManager itemsManager,
        ISpPartnerConfiguration spPartner)
    {
        _languageService = languageService;
        _buffFactory = buffFactory;
        _itemsManager = itemsManager;
        _spPartner = spPartner;
        _revivalConfiguration = revivalConfiguration.PlayerRevivalConfiguration;
    }

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        IClientSession sender = e.Sender;
        IPlayerEntity character = e.Sender.PlayerEntity;

        if (!sender.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return;
        }

        if (sender.CurrentMapInstance.MapInstanceType == MapInstanceType.Act4Dungeon)
        {
            return;
        }
        
        if (sender.CurrentMapInstance.MapInstanceType == MapInstanceType.Caligor)
        {
            return;
        }

        if (e.Forced == ForcedType.Act4SealRevival)
        {
            if (character.IsAlive())
            {
                return;
            }

            await character.Restore(restoreMates: false);

            foreach (IMateEntity mate in character.MateComponent.TeamMembers())
            {
                character.MapInstance.RemoveMate(mate);
                character.MapInstance.Broadcast(mate.GenerateOut());
            }

            character.IsSeal = true;
            character.Morph = 1564;
            sender.BroadcastCMode();
            sender.BroadcastRevive();
            sender.UpdateVisibility();
            sender.SendBuffsPacket();
            sender.PlayerEntity.UpdateRevival(DateTime.UtcNow + _revivalConfiguration.Act4RevivalDelay, RevivalType.DontPayRevival, ForcedType.Forced);
            return;
        }

        await character.Restore(restoreMates: false);
        character.IsSeal = false;
        character.DisableRevival();

        bool hasPaidPenalization = false;
        if (e.RevivalType == RevivalType.TryPayRevival)
        {
            hasPaidPenalization = await TryPayPenalization(character, _revivalConfiguration.PlayerRevivalPenalization);
        }
        else if (e.Forced == ForcedType.HolyRevival)
        {
            hasPaidPenalization = true;
        }

        if (hasPaidPenalization)
        {
            await sender.EmitEventAsync(new GetDefaultMorphEvent());
            sender.RefreshStat();
            sender.BroadcastTeleportPacket();
            sender.BroadcastInTeamMembers(_languageService, _spPartner);
            sender.RefreshParty(_spPartner);
        }
        else
        {
            character.Hp = character.MaxHp;
            character.Mp = character.MaxMp;
            await sender.Respawn();
            await sender.EmitEventAsync(new GetDefaultMorphEvent());
        }

        sender.BroadcastRevive();
        sender.UpdateVisibility();
        await sender.CheckPartnerBuff();
        sender.SendBuffsPacket();

        if (!character.HasBuff(BuffVnums.RESURRECTION) && !character.HasBuff(BuffVnums.RESURRECTION_CAPSULE) && !hasPaidPenalization && e.RevivalType != RevivalType.DontPayRevival && character.Level > _revivalConfiguration.PlayerRevivalPenalization.MaxLevelWithoutRevivalPenalization)
        {
            await character.AddBuffAsync(_buffFactory.CreateBuff(_revivalConfiguration.PlayerRevivalPenalization.BaseMapRevivalPenalizationDebuff, character));
        }
    }

    private async Task<bool> TryPayPenalization(IPlayerEntity character, PlayerRevivalPenalization playerRevivalPenalization)
    {
        if (character.Level <= playerRevivalPenalization.MaxLevelWithoutRevivalPenalization)
        {
            await character.Restore(restoreMates: false);
            return true;
        }

        int item = playerRevivalPenalization.BaseMapRevivalPenalizationSaver;
        int amount = (short)playerRevivalPenalization.BaseMapRevivalPenalizationSaverAmount;
        string itemName = _languageService.GetItemName(_itemsManager.GetItem(item), character.Session);

        if (!character.HasItem(item, amount))
        {
            string tmp = _languageService.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, character.Session.UserLanguage, amount, itemName);
            character.Session.SendErrorChatMessage(tmp);
            return false;
        }

        await character.Session.RemoveItemFromInventory(item, amount);

        string chatMessage = _languageService.GetLanguageFormat(GameDialogKey.INFORMATION_CHATMESSAGE_REQUIRED_ITEM_EXPENDED, character.Session.UserLanguage, itemName, amount);
        character.Session.SendErrorChatMessage(chatMessage);

        character.Hp = character.MaxHp / 2;
        character.Mp = character.MaxMp / 2;
        return true;
    }
}