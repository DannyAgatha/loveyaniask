using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.Quests;

namespace WingsEmu.Game.Configurations;

public interface IPeriodicQuestsConfiguration
{
    bool IsDailyQuest(QuestDto quest);
    IReadOnlyCollection<PeriodicQuestSet> GetDailyQuests();
    PeriodicQuestSet GetPeriodicQuestSetByQuestId(int questId);
}

public class PeriodicQuestsConfiguration : IPeriodicQuestsConfiguration
{
    // References the starting quest of a questline
    public HashSet<PeriodicQuestSet> DailyQuests { get; set; } = new();
    public bool IsDailyQuest(QuestDto quest) => DailyQuests?.Any(s => s?.QuestVnums?.Contains(quest.Id) ?? false) ?? false;
    public IReadOnlyCollection<PeriodicQuestSet> GetDailyQuests() => DailyQuests;
    public PeriodicQuestSet GetPeriodicQuestSetByQuestId(int questId) => DailyQuests.FirstOrDefault(s => s.QuestVnums.Contains(questId));
}

public class PeriodicQuestSet
{
    public HashSet<int> QuestVnums { get; set; } = new();
    public int Id { get; set; }
    public bool? PerNoswingsAccount { get; set; } = false;
}