using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.TimeSpace;

public class GetPartnerHandler : INpcDialogAsyncHandler
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;

    public GetPartnerHandler(INpcMonsterManager npcMonsterManager, IMateEntityFactory mateEntityFactory, IGameItemInstanceFactory gameItemInstanceFactory, IGameLanguageService gameLanguage)
    {
        _npcMonsterManager = npcMonsterManager;
        _mateEntityFactory = mateEntityFactory;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _gameLanguage = gameLanguage;
    }

    public NpcRunType[] NpcRunTypes => new[] { NpcRunType.GET_PARTNER };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {
        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        List<INpcEntity> partners = session.PlayerEntity.TimeSpaceComponent.Partners;

        if (timeSpace.Instance.StartTimeFreeze.HasValue)
        {
            await session.EmitEventAsync(new TimeSpaceAddTimeToTimerEvent
            {
                Time = DateTime.UtcNow - timeSpace.Instance.StartTimeFreeze.Value
            });

            timeSpace.Instance.StartTimeFreeze = null;
        }

        if (!timeSpace.Instance.TimeSpaceSubInstances.TryGetValue(session.CurrentMapInstance.Id, out TimeSpaceSubInstance timeSpaceSubInstance))
        {
            return;
        }

        await session.EmitEventAsync(new TimeSpaceStartTaskEvent
        {
            TimeSpaceSubInstance = timeSpaceSubInstance
        });

        if (timeSpaceSubInstance.Task != null && timeSpaceSubInstance.Task.StartDialog.HasValue)
        {
            timeSpaceSubInstance.Task.StartDialog = null;
            if (timeSpaceSubInstance.Task.StartDialogIsObjective)
            {
                timeSpaceSubInstance.Task.StartDialogIsObjective = false;
                timeSpace.Instance.TimeSpaceObjective.ConversationsHad++;
                await session.EmitEventAsync(new TimeSpaceRefreshObjectiveProgressEvent());
            }
        }

        if (timeSpace.Instance.ObtainablePartnerVnum == null)
        {
            return;
        }

        INpcEntity timeSpacePartner = partners.FirstOrDefault(x => x.MonsterVNum == timeSpace.Instance.ObtainablePartnerVnum.Value);
        if (!partners.Any() || timeSpacePartner == null)
        {
            return;
        }

        IMonsterData partner = _npcMonsterManager.GetNpc(timeSpace.Instance.ObtainablePartnerVnum.Value);
        if (partner == null)
        {
            return;
        }

        if (session.PlayerEntity.MateComponent.GetMates(x => x.MonsterVNum == partner.MonsterVNum && x.MateType == MateType.Partner).Any())
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.PARTNER_SHOUTMESSAGE_ALREADY_HAVE_SAME_PARTNER), ChatMessageColorType.Yellow);
            return;
        }

        if (!session.PlayerEntity.CanReceiveMate(MateType.Partner))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_MAX_PARTNER_COUNT), ChatMessageColorType.Yellow);
            return;
        }

        session.PlayerEntity.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);
        timeSpacePartner.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);
        timeSpacePartner.MapInstance.Broadcast(timeSpacePartner.GenerateOut());
        timeSpacePartner.MapInstance.RemoveNpc(timeSpacePartner);

        session.PlayerEntity.TimeSpaceComponent.Partners.Remove(timeSpacePartner);

        byte partnerLevel = session.PlayerEntity.Level < partner.BaseLevel ? session.PlayerEntity.Level : partner.BaseLevel;

        IMateEntity mateEntity = _mateEntityFactory.CreateMateEntity(session.PlayerEntity, new MonsterData(partner), MateType.Partner, partnerLevel, partner.BaseHeroLevel, new List<int>());
        await session.EmitEventAsync(new MateInitializeEvent
        {
            MateEntity = mateEntity
        });

        CreatePartnerEquipment(session, mateEntity);

        if (session.PlayerEntity.MateComponent.GetTeamMember(x => x.MateType == MateType.Partner) != null)
        {
            return;
        }

        await session.EmitEventAsync(new MateJoinTeamEvent
        {
            MateEntity = mateEntity,
            IsNewCreated = true
        });
    }

    private void CreatePartnerEquipment(IClientSession session, IMateEntity mateEntity)
    {
        if (mateEntity.Level < 15)
        {
            return;
        }

        int weapon = 0;
        int armor = 0;

        switch (mateEntity.AttackType)
        {
            case AttackType.Melee:

                switch (mateEntity.Level)
                {
                    case >= 20 and <= 30:
                        weapon = 19; // Short Sword
                        armor = 162; // Durable Armour
                        break;
                    case > 30 and <= 48:
                        weapon = 21; // Short Sword
                        armor = 97; // Leather Armour
                        break;
                    default:
                        weapon = 139; // Katzbalger
                        armor = 102; // Armour of Morale
                        break;
                }

                break;
            case AttackType.Ranged:

                switch (mateEntity.Level)
                {
                    case >= 20 and <= 30:
                        weapon = 33; // Small Bow
                        armor = 108; // Lambskin Tunic
                        break;
                    case > 30 and <= 49:
                        weapon = 143; // Flame Bow
                        armor = 169; // Tunic of Nature
                        break;
                    default:
                        weapon = 146; // Wild Bow
                        armor = 116; // Peccary Tunic
                        break;
                }

                break;
            case AttackType.Magical:

                switch (mateEntity.Level)
                {
                    case >= 20 and <= 30:
                        weapon = 47; // Red Bead Wand
                        armor = 121; // Cotton Robe
                        break;
                    case > 30 and <= 49:
                        weapon = 150; // Magic Wand of Flame
                        armor = 175; // Soft Robe
                        break;
                    default:
                        weapon = 153; // Magic Spell Wand
                        armor = 129; // Silk Robe
                        break;
                }

                break;
        }

        GameItemInstance weaponItem = _gameItemInstanceFactory.CreateItem(weapon, 1, 5, 5);

        switch (weaponItem.GameItem.EquipmentSlot)
        {
            case EquipmentType.SecondaryWeapon:
            case EquipmentType.MainWeapon:
                switch (weaponItem.GameItem.Class)
                {
                    case (int)ItemClassType.Swordsman:
                        weaponItem.ItemVNum = weaponItem.GameItem.EquipmentSlot == (int)EquipmentType.MainWeapon ? (int)ItemVnums.PARTNER_WEAPON_MELEE : (int)ItemVnums.PARTNER_WEAPON_RANGED;
                        break;
                    case (int)ItemClassType.Archer:
                        weaponItem.ItemVNum = weaponItem.GameItem.EquipmentSlot == (int)EquipmentType.MainWeapon ? (int)ItemVnums.PARTNER_WEAPON_RANGED : (int)ItemVnums.PARTNER_WEAPON_MELEE;
                        break;
                    case (int)ItemClassType.Mage:
                        weaponItem.ItemVNum = weaponItem.GameItem.EquipmentSlot == (int)EquipmentType.MainWeapon ? (int)ItemVnums.PARTNER_WEAPON_MAGIC : (int)ItemVnums.PARTNER_WEAPON_RANGED;
                        break;
                    case (int)ItemClassType.MartialArtist:
                        weaponItem.ItemVNum = (int)ItemVnums.PARTNER_WEAPON_MELEE;
                        break;
                    default:
                        session.SendShopEndPacket(ShopEndType.Npc);
                        return;
                }

                break;
        }

        weaponItem.EquipmentOptions?.Clear();
        weaponItem.OriginalItemVnum = weapon;
        weaponItem.BoundCharacterId = null;

        GameItemInstance armorItem = _gameItemInstanceFactory.CreateItem(armor, 1, 5, 5);
        switch (armorItem.GameItem.Class)
        {
            case (int)ItemClassType.Swordsman:
            case (int)ItemClassType.MartialArtist:
                armorItem.ItemVNum = (int)ItemVnums.PARTNER_ARMOR_MELEE;
                break;
            case (int)ItemClassType.Archer:
                armorItem.ItemVNum = (int)ItemVnums.PARTNER_ARMOR_RANGED;
                break;
            case (int)ItemClassType.Mage:
                armorItem.ItemVNum = (int)ItemVnums.PARTNER_ARMOR_MAGIC;
                break;
        }

        armorItem.EquipmentOptions?.Clear();
        armorItem.OriginalItemVnum = armor;
        armorItem.BoundCharacterId = null;

        session.PlayerEntity.PartnerEquipItem(weaponItem, mateEntity.PetSlot);
        session.PlayerEntity.PartnerEquipItem(armorItem, mateEntity.PetSlot);
        session.SendScpPackets();
        session.SendScnPackets();
        mateEntity.RefreshEquipmentValues(weaponItem, false);
        mateEntity.RefreshEquipmentValues(armorItem, false);
        session.SendPetInfo(mateEntity, _gameLanguage);
        session.SendCondMate(mateEntity);
    }
}