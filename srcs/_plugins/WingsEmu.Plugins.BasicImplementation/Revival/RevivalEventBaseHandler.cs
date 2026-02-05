using PhoenixLib.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Revival;

public class RevivalEventBaseHandler : IAsyncEventProcessor<RevivalReviveEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly IItemsManager _itemsManager;
    private readonly IGameLanguageService _languageService;
    private readonly PlayerRevivalConfiguration _revivalConfiguration;
    private readonly IRevivalManager _revivalManager;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;
    private readonly ICharacterAlgorithm _characterAlgorithm;

    public RevivalEventBaseHandler(GameRevivalConfiguration gameRevivalConfiguration,
        IBuffFactory buffFactory, IGameLanguageService languageService, IItemsManager itemsManager,
        IRevivalManager revivalManager, ISpPartnerConfiguration spPartnerConfiguration, ICharacterAlgorithm characterAlgorithm)
    {
        _revivalConfiguration = gameRevivalConfiguration.PlayerRevivalConfiguration;
        _buffFactory = buffFactory;
        _languageService = languageService;
        _itemsManager = itemsManager;
        _revivalManager = revivalManager;
        _spPartnerConfiguration = spPartnerConfiguration;
        _characterAlgorithm = characterAlgorithm;
    }

    public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
    {
        IClientSession sender = e.Sender;
        IPlayerEntity character = sender?.PlayerEntity;

        if (character?.MapInstance == null)
        {
            return;
        }

        if (character.IsAlive())
        {
            return;
        }

        if (!sender.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP))
        {
            return;
        }

        if (sender.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
        {
            return;
        }

        bool isInNosVille = character.MapId == (int)MapIds.NOSVILLE;
        character.Hp = isInNosVille ? character.MaxHp / 2 : 1;
        character.Mp = isInNosVille ? character.MaxMp / 2 : 1;

        if (sender.CurrentMapInstance.HasMapFlag(MapFlags.HAS_CHAMPION_EXPERIENCE_ENABLED))
        {
            // 3% HXP lost
            character.HeroXp -= (character.GetHeroXp(_characterAlgorithm) * 3 / 100);
            if (character.HeroXp < 0)
            {
                character.HeroXp = 0;
            }
        }

        bool hasPaidPenalization = false;
        if (e.RevivalType == RevivalType.TryPayRevival && e.Forced != ForcedType.HolyRevival)
        {
            hasPaidPenalization = await TryPayPenalization(character, _revivalConfiguration.PlayerRevivalPenalization);
        }

        if (e.Forced == ForcedType.HolyRevival)
        {
            hasPaidPenalization = true;
        }

        if (hasPaidPenalization)
        {
            sender.RefreshStat();
            sender.BroadcastTeleportPacket();
            sender.BroadcastInTeamMembers(_languageService, _spPartnerConfiguration);
            sender.RefreshParty(_spPartnerConfiguration);
        }
        else if (e.Forced != ForcedType.HolyRevival)
        {
            await sender.Respawn();
        }

        if (e.Forced == ForcedType.HolyRevival)
        {
            e.Sender.PlayerEntity.Hp = e.Sender.PlayerEntity.MaxHp;
            e.Sender.PlayerEntity.Mp = e.Sender.PlayerEntity.MaxMp;
            e.Sender.RefreshStat();
        }

        sender.BroadcastRevive();
        sender.UpdateVisibility();
        await sender.CheckPartnerBuff();
        sender.SendBuffsPacket();

        if (!character.HasBuff(BuffVnums.RESURRECTION) && !character.HasBuff(BuffVnums.RESURRECTION_CAPSULE) && character.Level > _revivalConfiguration.PlayerRevivalPenalization.MaxLevelWithoutRevivalPenalization && e.Forced != ForcedType.HolyRevival)
        {
            await character.AddBuffAsync(_buffFactory.CreateBuff(_revivalConfiguration.PlayerRevivalPenalization.BaseMapRevivalPenalizationDebuff, character));
        }
    }

    public bool BasicUnregistering(long id, ForcedType forced, Guid expectedGuid)
    {
        switch (forced)
        {
            case ForcedType.Forced:
                if (!_revivalManager.UnregisterRevival(id, expectedGuid))
                {
                    return false;
                }

                break;
            case ForcedType.NoForced:
                if (!_revivalManager.UnregisterRevival(id))
                {
                    return false;
                }

                break;
            default:
                _revivalManager.TryUnregisterRevival(id);
                break;
        }

        return true;
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
            character.Session.SendErrorChatMessage(_languageService.GetLanguageFormat(GameDialogKey.INVENTORY_SHOUTMESSAGE_NOT_ENOUGH_ITEMS, character.Session.UserLanguage, amount, itemName));
            return false;
        }

        await character.Session.RemoveItemFromInventory(item, amount);

        character.Session.SendSuccessChatMessage(_languageService.GetLanguageFormat(GameDialogKey.INFORMATION_CHATMESSAGE_REQUIRED_ITEM_EXPENDED, character.Session.UserLanguage, itemName, amount));

        character.Hp = character.MaxHp / 2;
        character.Mp = character.MaxMp / 2;
        return true;
    }
}