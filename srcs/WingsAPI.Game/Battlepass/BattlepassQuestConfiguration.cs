using System;
using System.Collections.Generic;
using WingsAPI.Packets.Enums.BattlePass;

namespace WingsEmu.Game.BattlePass;

public class BattlePassQuestConfiguration
{
    public List<BattlePassQuest> Quests { get; set; }
}

public class BattlePassQuest
{
    public long Id { get; set; }

    public MissionType MissionType { get; set; }

    public FrequencyType FrequencyType { get; set; }

    public long FirstData { get; set; }

    public long MaxObjectiveValue { get; set; }

    public RewardsType RewardsType { get; set; }

    public byte RewardAmount { get; set; }

    public DateTime Start { get; set; }
}