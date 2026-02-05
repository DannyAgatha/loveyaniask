using System;
using System.Collections.Generic;
using WingsEmu.DTOs.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Items;

public class GameItemInstance : ItemInstanceDTO
{
    private IGameItem _gameItem;

    private long _transportId;

    public GameItemInstance()
    {
    }

    public GameItemInstance(int vNum, int amount, byte upgrade, short rarity, short design)
    {
        ItemVNum = vNum;
        Amount = amount;
        Upgrade = upgrade;
        Rarity = rarity;
        Design = design;
    }

    private static IItemsManager ItemManager => new Lazy<IItemsManager>(() => StaticItemsManager.Instance).Value;

    public long TransportId
    {
        get
        {
            if (_transportId == 0)
            {
                // create transportId thru factory
                _transportId = TransportFactory.Instance.GenerateTransportId();
            }

            return _transportId;
        }
    }

    public int DamageMaximum => WeaponMaxDamageAdditionalValue;
    public int DamageMinimum => WeaponMinDamageAdditionalValue;
    public int CloseDefence => ArmorMeleeAdditionalValue;
    public int DefenceDodge => ArmorDodgeAdditionalValue;
    public int DistanceDefenceDodge => ArmorDodgeAdditionalValue;
    public int HitRate => WeaponHitRateAdditionalValue;
    public int DistanceDefence => ArmorRangeAdditionalValue;
    public int MagicDefence => ArmorMagicAdditionalValue;

    public bool IsBound => BoundCharacterId.HasValue && GameItem.ItemType != ItemType.Armor && GameItem.ItemType != ItemType.Weapon;

    public IGameItem GameItem => !OriginalItemVnum.HasValue
        ? _gameItem ??= ItemManager.GetItem(ItemVNum)
        : _gameItem != null && _gameItem.Id == OriginalItemVnum.Value
            ? _gameItem
            : _gameItem = ItemManager.GetItem(OriginalItemVnum.Value);

    public List<PartnerSkill> PartnerSkills { get; set; }
}