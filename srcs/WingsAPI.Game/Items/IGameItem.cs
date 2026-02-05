using System.Collections.Generic;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.DTOs.BCards;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace WingsEmu.Game.Items;

public interface IGameItem
{
    List<BCardDTO> BCards { get; }
    int Id { get; }
    byte BasicUpgrade { get; }
    byte CellonLvl { get; }
    byte Class { get; }
    short CloseDefence { get; }
    byte Color { get; }
    short Concentrate { get; }
    sbyte CriticalLuckRate { get; }
    short CriticalRate { get; }
    short DamageMaximum { get; }
    short DamageMinimum { get; }
    byte DarkElement { get; }
    short DarkResistance { get; }
    short DefenceDodge { get; }
    short DistanceDefence { get; }
    short DistanceDefenceDodge { get; }
    short Effect { get; }
    int EffectValue { get; }
    byte Element { get; }
    short ElementRate { get; }
    EquipmentType EquipmentSlot { get; }
    short FireResistance { get; }
    byte Height { get; }
    short HitRate { get; }
    short Hp { get; }
    short HpRegeneration { get; }
    bool IsMinilandActionable { get; }
    bool IsColorable { get; }
    bool IsTimeSpaceRewardBox { get; }
    bool ShowDescriptionOnHover { get; }
    bool Flag3 { get; }
    bool FollowMouseOnUse { get; }
    bool ShowSomethingOnHover { get; }
    bool PlaySoundOnPickup { get; }
    bool Flag7 { get; }
    bool IsLimited { get; }
    bool IsConsumable { get; }
    bool IsDroppable { get; }
    bool IsHeroic { get; }
    bool ShowWarningOnUse { get; }
    bool IsWarehouse { get; }
    bool IsSoldable { get; }
    bool IsTradable { get; }
    byte ItemSubType { get; }
    ItemType ItemType { get; }
    long ItemValidTime { get; }
    byte LevelJobMinimum { get; }
    byte LevelMinimum { get; }
    byte LightElement { get; }
    short LightResistance { get; }
    short MagicDefence { get; }
    byte MaxCellon { get; }
    byte MaxCellonLvl { get; }
    short MaxElementRate { get; }
    byte MaximumAmmo { get; }
    int MinilandObjectPoint { get; }
    short MoreHp { get; }
    short MoreMp { get; }
    short Morph { get; }
    short Mp { get; }
    short MpRegeneration { get; }
    string Name { get; }
    long Price { get; }
    byte ReputationMinimum { get; }
    long ReputPrice { get; }
    byte Sex { get; }
    byte Speed { get; }
    byte SpPointsUsage { get; }
    InventoryType Type { get; }
    short WaitDelay { get; }
    byte WaterElement { get; }
    short WaterResistance { get; }
    byte Width { get; }
    AttackType AttackType { get; }
    bool UseReputationAsPrice { get; }
    byte PartnerClass { get; }
    bool IsPartnerSpecialist { get; }
    byte SpMorphId { get; }
    short ItemLeftType { get; }
    int LeftUsages { get; }
    int IconId { get; }
    short ShellMinimumLevel { get; }
    short ShellMaximumLevel { get; }
    ShellType ShellType { get; }
    int[] Data { get; }
}