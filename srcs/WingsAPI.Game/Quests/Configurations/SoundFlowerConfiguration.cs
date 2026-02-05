using System.Collections.Generic;

namespace WingsEmu.Game.Quests.Configurations;

public class SoundFlowerConfiguration
{
    public HashSet<int> SoundFlowerQuestVnums { get; set; }
    public HashSet<int> WildSoundFlowerQuestVnums { get; set; }
    public HashSet<int> PossibleBuffs { get; set; }
}