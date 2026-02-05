// NosEmu
// 


namespace WingsEmu.Packets.Enums
{
    public enum QuestRewardType : byte
    {
        Gold = 1,
        SecondGold = 2,
        Exp = 3,
        SecondExp = 4,
        JobExp = 5,
        RandomReward = 7,
        AllRewards = 8,
        Reput = 9,
        ThirdGold = 10,
        ThirdExp = 11,
        SecondJobExp = 12,
        Unknow = 13, //never used but it is in the dat file,
        ItemsDependingOnClass = 14,
        HeroExpPercentage = 15,
        SetJobLevel = 16,
        SetLevel = 17,
        SetLevelAndJobLevel = 18
    }
}