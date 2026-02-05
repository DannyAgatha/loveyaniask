using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WingsAPI.Data.TimeSpace;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Portals;
using WingsEmu.Game.TimeSpaces.Enums;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.TimeSpaces;

public static class TimeSpacePacketExtensions
{
    public static string GenerateRsfn(this IMapInstance mapInstance, bool isStartMap = false, bool isVisit = true)
    {
        int number = isStartMap ? 0 : mapInstance.GetAliveMonsters(x => x.SummonerType == null || x.SummonerType != VisualType.Player).Any() && isVisit ? 3 : isVisit ? 0 : 1;

        return $"rsfn {mapInstance.MapIndexX} {mapInstance.MapIndexY} {number}";
    }

    public static string GenerateMinfo(this IClientSession session)
    {
        TimeSpaceInstance timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace.Instance;
        TimeSpaceObjective timeSpaceObjective = session.PlayerEntity.TimeSpaceComponent.TimeSpace.Instance.TimeSpaceObjective;
        return "minfo " +
            $"{(timeSpaceObjective.KillAllMonsters ? 1 : 0)} " +
            $"{(timeSpaceObjective.GoToExit ? 1 : 0)} " +
            $"{timeSpaceObjective.KillMonsterVnum ?? -1}.{timeSpaceObjective.KilledMonsterAmount}/{timeSpaceObjective.KillMonsterAmount ?? 0} " +
            $"{timeSpaceObjective.CollectItemVnum ?? -1}.{timeSpaceObjective.CollectedItemAmount}/{timeSpaceObjective.CollectItemAmount ?? 0} " +
            $"{(timeSpaceObjective.Conversation == null ? "-1/0" : $"{timeSpaceObjective.ConversationsHad}/{timeSpaceObjective.Conversation}")} " +
            $"{timeSpaceObjective.InteractObjectsVnum ?? -1}.{timeSpaceObjective.InteractedObjectsAmount}/{timeSpaceObjective.InteractObjectsAmount ?? 0} " +
            $"{(timeSpaceObjective.ProtectNPC ? 1 : 0)} " +
            $"{timeSpace.MaxLives} 0";
    }

    public static string GenerateMissionTargetMessage(this IClientSession session, string message) => $"sinfo {message}";

    public static void SendMissionTargetMessage(this IClientSession session, string message) => session.SendPacket(session.GenerateMissionTargetMessage(message));

    public static void SendMinfo(this IClientSession session)
    {
        if (!session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return;
        }

        if (session.PlayerEntity.TimeSpaceComponent.TimeSpace.Instance == null)
        {
            return;
        }

        session.SendPacket(session.GenerateMinfo());
    }

    public static void RefreshTimespaceScoreUi(this IClientSession session)
    {
        session.SendPacket(session.GenerateRnscPacket());
    }

    public static string GenerateRnscPacket(this IClientSession session) => $"rnsc {session.PlayerEntity.TimeSpaceComponent.TimeSpace.Instance.Score}";

    public static string GenerateRsfpPacket(this IClientSession session)
    {
        if (session.CurrentMapInstance.MapInstanceType == MapInstanceType.RaidInstance)
        {
            return "rsfp 0 0";
        }

        if (session.CurrentMapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            return "rsfp 0 -1";
        }

        return $"rsfp {session.CurrentMapInstance.MapIndexX} {session.CurrentMapInstance.MapIndexY}";
    }

    public static string GenerateRsfmPacket(this IClientSession session, TimeSpaceAction action, byte spiralFloor)
    {
        TimeSpaceParty timeSpaceParty = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        byte maxIndexX = timeSpaceParty.Instance.TimeSpaceSubInstances.Values.Max(s => s.MapInstance.MapIndexX);
        byte maxIndexY = timeSpaceParty.Instance.TimeSpaceSubInstances.Values.Max(s => s.MapInstance.MapIndexY);

        return $"rsfm {timeSpaceParty.TimeSpaceId} {(byte)action} {maxIndexX + 1} {maxIndexY + 1} {spiralFloor}";
    }

    public static string GenerateRepayPacket(this IClientSession session, IReadOnlyList<TimeSpaceRewardItem> rewards, TimeSpaceRewardItem selectedDrawItem)
    {
        string repayPacket = "repay";
        foreach (TimeSpaceRewardItem bonusReward in rewards.Where(x => x.Type == TimeSpaceRewardType.BONUS))
        {
            repayPacket += $" {bonusReward.ItemVnum}.{bonusReward.Rarity}.{bonusReward.Amount}";
        }

        for (int i = 0; i < 3 - rewards.Count(x => x.Type == TimeSpaceRewardType.BONUS); i++)
        {
            repayPacket += " -1.0.0";
        }

        foreach (TimeSpaceRewardItem specialReward in rewards.Where(x => x.Type == TimeSpaceRewardType.SPECIAL))
        {
            repayPacket += $" {specialReward.ItemVnum}.{specialReward.Rarity}.{specialReward.Amount}";
        }

        for (int i = 0; i < 2 - rewards.Count(x => x.Type == TimeSpaceRewardType.SPECIAL); i++)
        {
            repayPacket += " -1.0.0";
        }

        repayPacket += selectedDrawItem == null ? " -1.0.0" : $" {selectedDrawItem.ItemVnum}.{selectedDrawItem.Rarity}.{selectedDrawItem.Amount}";
        return repayPacket;
    }

    public static string GenerateScorePacket(this IClientSession session, TimeSpaceFinishType timeSpaceFinishType,
        bool isMonsterPerfect = false, bool isNpcPerfect = false, bool isMapsPerfect = false)
    {
        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        TimeSpaceInstance timeSpaceInstance = session.PlayerEntity.TimeSpaceComponent.TimeSpace.Instance;
        return "score " +
            $"{(int)timeSpaceFinishType} " +
            $"{timeSpaceInstance.Score} " +
            "500 " +
            "500 " +
            "500 " +
            $"{(timeSpace.IsEasyMode ? 0 : timeSpace.TimeSpaceInformation.Rewards.Draw?.Count ?? 0)} " +
            $"{timeSpaceInstance.KilledMonsters} " +
            $"{timeSpaceInstance.EnteredRooms} " +
            $"{timeSpaceInstance.SavedNpcs} " +
            $"{(isMonsterPerfect ? 1 : 0)}{(isMapsPerfect ? 1 : 0)}{(isNpcPerfect ? 1 : 0)} " +
            "1 " +
            "15";
    }

    public static IEnumerable<string> GenerateTimeSpacePortals(this IClientSession session)
    {
        var packets = new List<string>();

        foreach (ITimeSpacePortalEntity timeSpace in session.CurrentMapInstance.TimeSpacePortals)
        {
            byte type = (byte)(session.PlayerEntity.Level < timeSpace.MinLevel ? 0 : 1);

            if (session.PlayerEntity.CompletedTimeSpaces.Contains(timeSpace.TimeSpaceId))
            {
                type = 4;
            }

            if (timeSpace.IsHero)
            {
                type = 8;
            }

            packets.Add($"wp {timeSpace.Position.X} {timeSpace.Position.Y} {timeSpace.TimeSpaceId} {type} {timeSpace.MinLevel} {timeSpace.MaxLevel}");
        }

        return packets;
    }

    public static void SendTimeSpacePortals(this IClientSession session)
    {
        session.SendPackets(session.GenerateTimeSpacePortals());
    }

    public static string GenerateTimeSpaceInfo(this IClientSession session, ITimeSpacePortalEntity portalEntity, TimeSpaceRecordDto timeSpaceRecordDto)
    {
        byte type = (byte)(session.PlayerEntity.Level < portalEntity.MinLevel ? 0 : 1);
        bool completed = session.PlayerEntity.CompletedTimeSpaces.Contains(portalEntity.TimeSpaceId);

        if (completed)
        {
            type = 4;
        }

        string name = session.GetLanguage(portalEntity.Name);
        string desc = session.GetLanguage(portalEntity.Description);

        var drawRewards = new StringBuilder();
        var specialRewards = new StringBuilder();
        var bonusRewards = new StringBuilder();

        byte drawCount = 5;
        byte specialCount = 2;
        byte bonusCount = 3;

        foreach ((short vnum, int amount) in portalEntity.DrawRewards)
        {
            drawCount--;
            drawRewards.Append($"{vnum}.{amount} ");
        }

        for (int i = 0; i < drawCount; i++)
        {
            drawRewards.Append("-1.0 ");
        }

        foreach ((short vnum, int amount) in portalEntity.SpecialRewards)
        {
            specialCount--;
            specialRewards.Append($"{vnum}.{amount} ");
        }

        for (int i = 0; i < specialCount; i++)
        {
            specialRewards.Append("-1.0 ");
        }

        foreach ((short vnum, int amount) in portalEntity.BonusRewards)
        {
            bonusCount--;
            bonusRewards.Append($"{vnum}.{amount} ");
        }

        for (int i = 0; i < bonusCount; i++)
        {
            bonusRewards.Append("-1.0 ");
        }

        return
            $"rbr {portalEntity.TimeSpaceId}.{(portalEntity.IsHero ? 1 : portalEntity.IsHidden || portalEntity.IsSpecial ? 2 : 0)}.0 {type} {(completed ? 15 : 0)} " +
            $"{portalEntity.MinLevel}.{portalEntity.MaxLevel} {portalEntity.SeedsOfPowerRequired} " +
            $"{drawRewards}{specialRewards}{bonusRewards}{(timeSpaceRecordDto == null ? "-1.0" : $"{timeSpaceRecordDto.Record}.{timeSpaceRecordDto.CharacterName}")} 0 " +
            $"{(portalEntity.IsHidden || portalEntity.IsSpecial ? 0 : 1)} {name}\n{desc}";
    }

    public static string GenerateTimeSpaceInfo(this IClientSession session, INpcEntity npcEntity,
        List<(short, int)> drawRewards, List<(short, int)> specialRewards, List<(short, int)> bonusRewards, TimeSpaceRecordDto timeSpaceRecordDto)
    {
        byte type = (byte)(session.PlayerEntity.Level < npcEntity.TimeSpaceInfo.MinLevel ? 0 : 1);
        bool completed = session.PlayerEntity.CompletedTimeSpaces.Contains(npcEntity.TimeSpaceInfo.TsId);

        if (completed)
        {
            type = 4;
        }

        string name = session.GetLanguage(npcEntity.TimeSpaceInfo.Name);
        string desc = session.GetLanguage(npcEntity.TimeSpaceInfo.Description);

        var drawRewardsPacket = new StringBuilder();
        var specialRewardsPacket = new StringBuilder();
        var bonusRewardsPacket = new StringBuilder();

        byte drawCount = 5;
        byte specialCount = 2;
        byte bonusCount = 3;

        foreach ((short vnum, int amount) in drawRewards)
        {
            drawCount--;
            drawRewardsPacket.Append($"{vnum}.{amount} ");
        }

        for (int i = 0; i < drawCount; i++)
        {
            drawRewardsPacket.Append("-1.0 ");
        }

        foreach ((short vnum, int amount) in specialRewards)
        {
            specialCount--;
            specialRewardsPacket.Append($"{vnum}.{amount} ");
        }

        for (int i = 0; i < specialCount; i++)
        {
            specialRewardsPacket.Append("-1.0 ");
        }

        foreach ((short vnum, int amount) in bonusRewards)
        {
            bonusCount--;
            bonusRewardsPacket.Append($"{vnum}.{amount} ");
        }

        for (int i = 0; i < bonusCount; i++)
        {
            bonusRewardsPacket.Append("-1.0 ");
        }

        return
            $"rbr {npcEntity.TimeSpaceInfo.TsId}.{(npcEntity.TimeSpaceInfo.IsHero ? 1 : npcEntity.TimeSpaceInfo.IsHidden || npcEntity.TimeSpaceInfo.IsSpecial ? 2 : 0)}.0 {type} {(completed ? 15 : 0)} " +
            $"{npcEntity.TimeSpaceInfo.MinLevel}.{npcEntity.TimeSpaceInfo.MaxLevel} {npcEntity.TimeSpaceInfo.SeedsOfPowerRequired} " +
            $"{drawRewardsPacket}{specialRewardsPacket}{bonusRewardsPacket}{(timeSpaceRecordDto == null ? "-1.0" : $"{timeSpaceRecordDto.Record}.{timeSpaceRecordDto.CharacterName}")} 0 " +
            $"{(npcEntity.TimeSpaceInfo.IsHidden || npcEntity.TimeSpaceInfo.IsSpecial ? 0 : 1)} {name}\n{desc}";
    }

    public static void SendTimeSpaceInfo(this IClientSession session, ITimeSpacePortalEntity portal, TimeSpaceRecordDto timeSpaceRecordDto)
    {
        session.SendPacket(session.GenerateTimeSpaceInfo(portal, timeSpaceRecordDto));
    }

    public static void SendTimeSpaceInfo(this IClientSession session, INpcEntity npc, TimeSpaceRecordDto timeSpaceRecordDto)
    {
        TimeSpaceFileConfiguration timeSpaceFileConfiguration = npc.TimeSpaceInfo;
        List<(short, int)> drawRewards = new();
        List<(short, int)> specialRewards = new();
        List<(short, int)> bonusRewards = new();
        if (timeSpaceFileConfiguration.Rewards?.Draw != null)
        {
            foreach (TimeSpaceItemConfiguration draw in timeSpaceFileConfiguration.Rewards.Draw)
            {
                drawRewards.Add((draw.ItemVnum, draw.Amount));
            }
        }

        if (timeSpaceFileConfiguration.Rewards?.Special != null)
        {
            foreach (TimeSpaceItemConfiguration special in timeSpaceFileConfiguration.Rewards.Special)
            {
                specialRewards.Add((special.ItemVnum, special.Amount));
            }
        }

        if (timeSpaceFileConfiguration.Rewards?.Bonus != null)
        {
            foreach (TimeSpaceItemConfiguration bonus in timeSpaceFileConfiguration.Rewards.Bonus)
            {
                bonusRewards.Add((bonus.ItemVnum, bonus.Amount));
            }
        }

        session.SendPacket(session.GenerateTimeSpaceInfo(npc, drawRewards, specialRewards, bonusRewards, timeSpaceRecordDto));
    }

    public static string GenerateRsfiPacket(this IClientSession session, ISubActConfiguration subActConfiguration, ITimeSpaceConfiguration timeSpaceConfiguration)
    {
        long lastCompletedTimeSpace = (from tsId in session.PlayerEntity.CompletedTimeSpaces.OrderByDescending(x => x) let getTimeSpace = timeSpaceConfiguration.GetTimeSpaceConfiguration(tsId) where getTimeSpace != null where !getTimeSpace.IsSpecial && !getTimeSpace.IsHidden select tsId).FirstOrDefault();

        SubActsConfiguration getAct = subActConfiguration.GetConfigurationByTimeSpaceId(lastCompletedTimeSpace);

        if (getAct == null)
        {
            return "rsfi 0 0 0 0 0 0";
        }

        int timeSpaceCount = getAct.TimeSpaces.Length;
        int timeSpacesDone = getAct.TimeSpaces.Count(timeSpace => session.PlayerEntity.CompletedTimeSpaces.Contains(timeSpace));

        // Get next act
        if (timeSpaceCount == timeSpacesDone)
        {
            SubActsConfiguration newAct = null;
            int counter = subActConfiguration.GetConfigurations().Count();
            int count = 0;
            while (counter > 0)
            {
                counter--;
                count++;
                newAct = subActConfiguration.GetConfigurationById(getAct.Id + count);
                if (newAct != null)
                {
                    break;
                }
            }

            if (newAct != null)
            {
                getAct = newAct;
                timeSpaceCount = getAct.TimeSpaces.Length;
                timeSpacesDone = getAct.TimeSpaces.Count(timeSpace => session.PlayerEntity.CompletedTimeSpaces.Contains(timeSpace));
            }
        }

        return $"rsfi {getAct.Act} {getAct.SubAct} {timeSpacesDone} {timeSpaceCount} {timeSpacesDone} {timeSpaceCount}";
    }

    public static void SendRsfiPacket(this IClientSession session, ISubActConfiguration subActConfiguration, ITimeSpaceConfiguration timeSpaceConfiguration)
    {
        session.SendPacket(session.GenerateRsfiPacket(subActConfiguration, timeSpaceConfiguration));
    }

    public static string GenerateTimerFreeze(this IClientSession session) => "guri 8 8";

    public static void SendTimerFreeze(this IClientSession session) => session.SendPacket(session.GenerateTimerFreeze());

    public static string GenerateNpcReqPacket(this IClientSession session, int dialog) => $"npc_req 1 {session.PlayerEntity.Id} {dialog}";

    public static void SendNpcReqPacket(this IClientSession session, int dialog) => session.SendPacket(session.GenerateNpcReqPacket(dialog));

    public static int CalculateGoldReward(this TimeSpaceParty timeSpaceParty) => (int)(timeSpaceParty.TimeSpaceInformation.MinLevel *
        (2 + Math.Floor(timeSpaceParty.TimeSpaceInformation.MinLevel / 10.0) / 5) * timeSpaceParty.Instance.KilledMonsters);

    public static long CalculateExperience(this TimeSpaceParty timeSpaceParty) => timeSpaceParty.TimeSpaceInformation.MinLevel * 3 * timeSpaceParty.Instance.KilledMonsters;

    public static int GetTimeSpacePenalty(this IClientSession session)
    {
        TimeSpaceParty timeSpaceParty = session.PlayerEntity.TimeSpaceComponent.TimeSpace;

        int difference = session.PlayerEntity.Level - timeSpaceParty.TimeSpaceInformation.MinLevel;

        switch (difference)
        {
            case <= 0:
                return 0;
            case > 50:
                difference = 50;
                break;
        }

        return difference * 2;
    }

    public static int GetTimeSpaceScorePenalty(this TimeSpaceParty timeSpace)
    {
        int difference = timeSpace.HigherLevel - timeSpace.TimeSpaceInformation.MinLevel;

        switch (difference)
        {
            case <= 0:
                return 0;
            case > 50:
                difference = 50;
                break;
        }

        return difference * 2;
    }

    public static bool CanJoinToTimeSpace(this IClientSession session, long timeSpaceId, ISubActConfiguration subActConfiguration, ITimeSpaceConfiguration timeSpaceConfig)
    {
        TimeSpaceFileConfiguration timeSpaceConfigInfo = timeSpaceConfig.GetTimeSpaceConfiguration(timeSpaceId);
        SubActsConfiguration timeSpaceConfiguration = subActConfiguration.GetConfigurationByTimeSpaceId(timeSpaceId);
        if (timeSpaceConfiguration == null || timeSpaceConfigInfo == null)
        {
            return false;
        }

        bool canJoin = true;
        SubActsConfiguration getPreviousTimeSpaceConfiguration = null;
        int count = 0;
        int counter = subActConfiguration.GetConfigurations().Count();

        while (counter > count)
        {
            count++;
            getPreviousTimeSpaceConfiguration = subActConfiguration.GetConfigurationById(timeSpaceConfiguration.Id - count);
            if (getPreviousTimeSpaceConfiguration?.TimeSpaces == null || !getPreviousTimeSpaceConfiguration.TimeSpaces.Any())
            {
                continue;
            }

            break;
        }

        if (getPreviousTimeSpaceConfiguration?.TimeSpaces != null)
        {
            long getLastTimeSpace = getPreviousTimeSpaceConfiguration.TimeSpaces.LastOrDefault();
            if (!session.PlayerEntity.CompletedTimeSpaces.Contains(getLastTimeSpace))
            {
                canJoin = false;
            }
        }

        if (!timeSpaceConfigInfo.IsHero)
        {
            return canJoin;
        }

        foreach (long timeSpace in timeSpaceConfiguration.TimeSpaces)
        {
            if (timeSpace >= timeSpaceId)
            {
                continue;
            }

            if (session.PlayerEntity.CompletedTimeSpaces.Contains(timeSpace))
            {
                continue;
            }

            canJoin = false;
            break;
        }

        return canJoin;
    }

    public static void SendScorePacket(this IClientSession session, TimeSpaceFinishType timeSpaceFinishType, bool isMonsterPerfect = false, bool isNpcPerfect = false, bool isMapsPerfect = false)
        => session.SendPacket(session.GenerateScorePacket(timeSpaceFinishType, isMonsterPerfect, isNpcPerfect, isMapsPerfect));

    public static void SendRsfpPacket(this IClientSession session) => session.SendPacket(session.GenerateRsfpPacket());

    public static void SendRsfmPacket(this IClientSession session, TimeSpaceAction action, byte spiralFloor = 0)
    {
        TimeSpaceParty timeSpaceParty = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpaceParty?.Instance == null)
        {
            return;
        }

        session.SendPacket(session.GenerateRsfmPacket(action, spiralFloor));
    }

    public static void SendRepayPacket(this IClientSession session, IReadOnlyList<TimeSpaceRewardItem> rewards, TimeSpaceRewardItem selectedDrawItem) =>
        session.SendPacket(session.GenerateRepayPacket(rewards, selectedDrawItem));

    public static bool IsInSpecialOrHiddenTimeSpace(this IClientSession session)
    {
        TimeSpaceParty timeSpace = session.PlayerEntity.TimeSpaceComponent.TimeSpace;
        if (timeSpace == null)
        {
            return false;
        }

        return timeSpace.TimeSpaceInformation.IsSpecial || timeSpace.TimeSpaceInformation.IsHidden;
    }
}