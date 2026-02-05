// NosEmu
// 


using WingsEmu.DTOs.Items;

namespace WingsEmu.Game.Items;

public class GameItem : ItemDTO, IGameItem
{
    public GameItem(ItemDTO item)
    {
        BCards = item.BCards;
        Height = item.Height;
        Width = item.Width;
        MinilandObjectPoint = item.MinilandObjectPoint;
        BasicUpgrade = item.BasicUpgrade;
        CellonLvl = item.CellonLvl;
        Class = item.Class;
        CloseDefence = item.CloseDefence;
        Color = item.Color;
        Concentrate = item.Concentrate;
        CriticalRate = item.CriticalRate;
        CriticalLuckRate = item.CriticalLuckRate;
        DamageMaximum = item.DamageMaximum;
        DamageMinimum = item.DamageMinimum;
        DarkElement = item.DarkElement;
        DarkResistance = item.DarkResistance;
        DefenceDodge = item.DefenceDodge;
        DistanceDefence = item.DistanceDefence;
        DistanceDefenceDodge = item.DistanceDefenceDodge;
        Effect = item.Effect;
        EffectValue = item.EffectValue;
        Element = item.Element;
        ElementRate = item.ElementRate;
        EquipmentSlot = item.EquipmentSlot;
        FireElement = item.FireElement;
        FireResistance = item.FireResistance;
        HitRate = item.HitRate;
        Hp = item.Hp;
        HpRegeneration = item.HpRegeneration;
        IsMinilandActionable = item.IsMinilandActionable;
        IsColorable = item.IsColorable;
        IsConsumable = item.IsConsumable;
        IsDroppable = item.IsDroppable;
        IsHeroic = item.IsHeroic;
        IsLimited = item.IsLimited;
        IsWarehouse = item.IsWarehouse;
        IsSoldable = item.IsSoldable;
        IsTradable = item.IsTradable;
        ShowWarningOnUse = item.ShowWarningOnUse;
        ItemSubType = item.ItemSubType;
        ItemType = item.ItemType;
        ItemValidTime = item.ItemValidTime;
        LevelJobMinimum = item.LevelJobMinimum;
        LevelMinimum = item.LevelMinimum;
        LightElement = item.LightElement;
        LightResistance = item.LightResistance;
        MagicDefence = item.MagicDefence;
        MaxCellon = item.MaxCellon;
        MaxCellonLvl = item.MaxCellonLvl;
        MaxElementRate = item.MaxElementRate;
        MaximumAmmo = item.MaximumAmmo;
        MoreHp = item.MoreHp;
        MoreMp = item.MoreMp;
        Morph = item.Morph;
        Mp = item.Mp;
        MpRegeneration = item.MpRegeneration;
        Name = item.Name;
        Price = item.Price;
        ReputationMinimum = item.ReputationMinimum;
        ReputPrice = item.ReputPrice;
        Sex = item.Sex;
        Speed = item.Speed;
        SpPointsUsage = item.SpPointsUsage;
        Type = item.Type;
        Id = item.Id;
        WaitDelay = item.WaitDelay;
        WaterElement = item.WaterElement;
        WaterResistance = item.WaterResistance;
        IsPartnerSpecialist = item.IsPartnerSpecialist;
        PartnerClass = item.PartnerClass;
        SpMorphId = item.SpMorphId;
        LeftUsages = item.LeftUsages;
        ItemLeftType = item.ItemLeftType;
        ShellType = item.ShellType;
        ShellMinimumLevel = item.ShellMinimumLevel;
        ShellMaximumLevel = item.ShellMaximumLevel;
        Data = item.Data;
    }
}