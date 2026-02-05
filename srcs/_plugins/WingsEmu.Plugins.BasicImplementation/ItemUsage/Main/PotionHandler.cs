using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Data.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.Enums.Chat;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.SnackFood;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main;

public class PotionHandler : IItemHandler
{
    private readonly IBCardEffectHandlerContainer _bCardEffectHandlerContainer;
    private readonly SnackFoodConfiguration _configuration;

    private readonly SerializableGameServer _gameServer;

    public PotionHandler(SerializableGameServer gameServer, IBCardEffectHandlerContainer bCardEffectHandlerContainer, SnackFoodConfiguration configuration)
    {
        _gameServer = gameServer;
        _bCardEffectHandlerContainer = bCardEffectHandlerContainer;
        _configuration = configuration;
    }

    public ItemType ItemType => ItemType.Potion;
    public long[] Effects => new long[] { 0 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        IPlayerEntity character = session.PlayerEntity;
        GameItemInstance itemInstance = e.Item.ItemInstance;
        DateTime now = DateTime.UtcNow;

        if (character.RainbowBattleComponent.IsInRainbowBattle || session.CurrentMapInstance is { MapInstanceType: MapInstanceType.ArenaInstance })
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
            return;
        }
        
        if (character.BCardComponent.HasBCard(BCardType.TimeCircleSkills, (byte)AdditionalTypes.TimeCircleSkills.ItemCannotBeUsed))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.ITEM_MESSAGE_CANT_USE), ChatMessageColorType.Yellow);
            return;
        }
        
        if (character.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
        {
            session.SendMsgi(MessageType.Default, Game18NConstString.ThatIsNotCurrentlyPossible);
            session.SendSayi(ChatMessageColorType.Red, Game18NConstString.ThatIsNotCurrentlyPossible);
            return;
        }
        
        if (session.CurrentMapInstance.MapVnum is (short)MapIds.ACT4_TUNDRA or (short)MapIds.WESTERN_FROST_LAND or (short)MapIds.EASTERN_FROST_LAND or (short)MapIds.ACT4_CALIGOR or (short)MapIds.NORTH_ICE_DESERT or (short)MapIds.NORTH_ICE_DESERT_2 or (short)MapIds.UNKNOWN_LAND)
        {
            return;
        }

        if ((now - character.LastPotion).TotalMilliseconds < 500)
        {
            return;
        }

        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.Alzanor)
        {
            return;
        }

        if (!character.IsAlive())
        {
            return;
        }

        if (character.Hp == character.MaxHp && character.Mp == character.MaxMp)
        {
            if (session.PlayerEntity.MateComponent.TeamMembers(m => m.IsAlive()).All(mate => mate.Hp == mate.MaxHp && mate.Mp == mate.MaxMp))
            {
                return;
            }
        }

        bool isOnAct4 = _gameServer.ChannelType == GameChannelType.ACT_4 && session.CurrentMapInstance.MapInstanceType != MapInstanceType.Act4Dungeon && session.CurrentMapInstance.MapInstanceType != MapInstanceType.RaidInstance;

        if (e.Item.ItemInstance.GameItem.Hp > 0)
        {
            await HealHp(session, e.Item.ItemInstance.GameItem.Hp);
        }

        if (e.Item.ItemInstance.GameItem.Mp > 0)
        {
            HealMp(session, e.Item.ItemInstance.GameItem.Mp);
        }

        switch (e.Item.ItemInstance.GameItem.Id)
        {
            case (int)ItemVnums.FULL_HP_POTION:
                if (isOnAct4)
                {
                    return;
                }

                await HealHp(session);
                break;
            case (int)ItemVnums.FULL_MP_POTION:
                if (isOnAct4)
                {
                    return;
                }

                HealMp(session);
                break;
            case (int)ItemVnums.FULL_HP_MP_POTION:
            case (int)ItemVnums.FULL_HP_MP_POTION_LIMIT:
                if (isOnAct4)
                {
                    return;
                }

                await HealHp(session);
                HealMp(session);
                break;
        }

        foreach (BCardDTO bCard in e.Item.ItemInstance.GameItem.BCards)
        {
            _bCardEffectHandlerContainer.Execute(character, character, bCard);
        }

        character.LastPotion = DateTime.UtcNow;
        await session.RemoveItemFromInventory(item: e.Item);
        session.RefreshStat();
    }

    private async Task HealHp(IClientSession session, int health = 0)
    {
        IPlayerEntity character = session.PlayerEntity;
        int potionHp;

        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
        {
            if (!mate.IsAlive())
            {
                continue;
            }

            int mateMaxHp = mate.MaxHp;
            potionHp = health;
            if (health == 0)
            {
                potionHp = mateMaxHp;
            }
            else
            {
                int toAdd = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.IncreaseRecoveryItems, character.Level).firstData;
                toAdd += character.GetMaxArmorShellValue(ShellEffectType.IncreasedRecoveryItemSpeed);
                toAdd += character.Family?.UpgradeValues.GetOrDefault(FamilyUpgradeType.INCREASE_POTION_REGEN).Item1 ?? 0;
                int toRemove = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DecreaseRecoveryItems, character.Level).firstData;

                double finalHeal = (100 + (toAdd - toRemove)) * 0.01;

                potionHp = (int)(potionHp * finalHeal);
            }

            if (health == 0)
            {
                int mateHpHeal = mate.Hp + potionHp > mateMaxHp ? mateMaxHp - mate.Hp : potionHp;
                mate.Hp += mateHpHeal;
                mate.BroadcastHeal(mateHpHeal);
            }
            else
            {
                await mate.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = mate,
                    HpHeal = potionHp
                });
            }

            session.SendMateLife(mate);
        }

        int maxHp = character.MaxHp;
        potionHp = health;
        if (health == 0)
        {
            potionHp = maxHp;
        }
        else
        {
            int toAdd = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.IncreaseRecoveryItems, character.Level).firstData;
            toAdd += character.GetMaxArmorShellValue(ShellEffectType.IncreasedRecoveryItemSpeed);
            toAdd += character.Family?.UpgradeValues.GetOrDefault(FamilyUpgradeType.INCREASE_POTION_REGEN).Item1 ?? 0;
            int toRemove = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DecreaseRecoveryItems, character.Level).firstData;

            double finalHeal = (100 + (toAdd - toRemove)) * 0.01;

            potionHp = (int)(potionHp * finalHeal);
        }

        if (health == 0)
        {
            int hpHeal = session.PlayerEntity.Hp + potionHp > maxHp ? maxHp - session.PlayerEntity.Hp : potionHp;
            session.PlayerEntity.BroadcastHeal(hpHeal);
            session.PlayerEntity.Hp += hpHeal;
        }
        else
        {
            await session.PlayerEntity.EmitEventAsync(new BattleEntityHealEvent
            {
                Entity = session.PlayerEntity,
                HpHeal = potionHp
            });
        }
    }

    private void HealMp(IClientSession session, int mana = 0)
    {
        IPlayerEntity character = session.PlayerEntity;
        int potionMp;

        foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
        {
            if (!mate.IsAlive())
            {
                continue;
            }

            int mateMaxMp = mate.MaxMp;
            potionMp = mana;
            if (mana == 0)
            {
                potionMp = mateMaxMp;
            }
            else
            {
                int toAdd = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.IncreaseRecoveryItems, character.Level).firstData;
                toAdd += character.GetMaxArmorShellValue(ShellEffectType.IncreasedRecoveryItemSpeed);
                toAdd += character.Family?.UpgradeValues.GetOrDefault(FamilyUpgradeType.INCREASE_POTION_REGEN).Item1 ?? 0;
                int toRemove = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DecreaseRecoveryItems, character.Level).firstData;

                double finalHeal = (100 + (toAdd - toRemove)) * 0.01;

                potionMp = (int)(potionMp * finalHeal);
            }

            int mateMpHeal = mate.Mp + potionMp > mateMaxMp ? mateMaxMp - mate.Mp : potionMp;
            mate.Mp += mateMpHeal;
            session.SendMateLife(mate);
        }

        int maxMp = character.MaxMp;
        potionMp = mana;
        if (mana == 0)
        {
            potionMp = maxMp;
        }
        else
        {
            int toAdd = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.IncreaseRecoveryItems, character.Level).firstData;
            toAdd += character.GetMaxArmorShellValue(ShellEffectType.IncreasedRecoveryItemSpeed);
            toAdd += character.Family?.UpgradeValues.GetOrDefault(FamilyUpgradeType.INCREASE_POTION_REGEN).Item1 ?? 0;
            int toRemove = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DecreaseRecoveryItems, character.Level).firstData;

            double finalHeal = (100 + (toAdd - toRemove)) * 0.01;

            potionMp = (int)(potionMp * finalHeal);
        }

        int mpHeal = session.PlayerEntity.Mp + potionMp > maxMp ? maxMp - session.PlayerEntity.Mp : potionMp;
        session.PlayerEntity.Mp += mpHeal;
    }
}