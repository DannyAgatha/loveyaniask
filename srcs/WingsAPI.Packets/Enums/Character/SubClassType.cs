namespace WingsEmu.Packets.Enums.Character;

public enum SubClassType : byte
{
    NotDefined = 0,
    
    // Swordsman
    OathKeeper = 1, // Oathkeeper - PvE: Enhanced for resilience and defense in controlled environments.
    CrimsonFury = 2, // Crimson Fury - PvP: Designed for rapidly inflicting critical damage in short confrontations.
    CelestialPaladin = 3, // Celestial Paladin - PvP-PvE: A balance between defense and magical support, suitable for both group and solo combat.
    
    // Archer
    SilentStalker = 4, // Silent Stalker - PvP: Stealth and evasion skills, perfect for ambushes.
    ArrowLord = 5, // Arrow Lord - PvE: Enhanced long-range damage capabilities to confront enemies and bosses.
    ShadowHunter = 6, // Shadow Hunter - PvP-PvE: Versatile to adapt to various situations, useful in skirmishes and quests.
    
    // Mage
    ArcaneSage = 7, // Arcane Sage - PvE: Efficient mana usage and powerful spells for extended combat.
    Pyromancer = 8, // Pyromancer - PvP-PvE: A balance between explosive damage and area control, effective in multiple contexts.
    DarkNecromancer = 9, // Dark Necromancer - PvP: Specialized in attrition and enemy manipulation.
    
    // Martial Artist
    ZenWarrior = 10, // Zen Warrior - PvP-PvE: Regeneration and endurance skills, adapted for varied combat.
    EmperorsBlade = 11, // Emperor's Blade - PvE: Precise and powerful attacks, ideal for quickly taking down enemies.
    StealthShadow = 12 // Stealth Shadow - PvP: Evasion and surprise attacks, excellent for disorienting opponents.
}