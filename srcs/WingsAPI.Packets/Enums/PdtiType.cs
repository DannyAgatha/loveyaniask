namespace WingsEmu.Packets.Enums
{
    public enum PdtiType : byte
    {
        SkillIsTrained = 1,
        ItemIsStrengthened = 2,
        ItemIsSucceedsInBeeting = 3,
        ItIsCraftedAsNormalQualityAccessory = 6,
        ItIsCraftedAsHigherQualityAccessory = 7,
        ItIsCraftedAsHighestQualityAccessory = 8,
        ConvertedIntoPartnerEquip = 9,
        ResistancesAreFused = 10,
        ItemHasBeenProduced = 11,
        ItemIsChanged = 12,
        ItemIsObtained = 13,
        ItemIsCollected = 14,
        Acquisition = 15, // ?
        ItemIdentificationSuccessful = 16,
        Fishing = 17
    }
}