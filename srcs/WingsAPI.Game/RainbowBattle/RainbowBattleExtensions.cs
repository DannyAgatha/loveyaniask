using System;
using System.Collections.Generic;
using System.Text;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.RainbowBattle;

public static class RainbowBattleExtensions
{
    public static string GenerateRainbowMembers(this RainbowBattleParty rainbowBattle, RainbowBattleTeamType team)
    {
        var packet = new StringBuilder("fbt 0 ");

        IReadOnlyList<IClientSession> members = team == RainbowBattleTeamType.Red ? rainbowBattle.RedTeam : rainbowBattle.BlueTeam;

        foreach (IClientSession member in members)
        {
            packet.Append($" {member.PlayerEntity.Id}");
        }

        return packet.ToString();
    }

    public static string GenerateRainbowBattleWidget(this RainbowBattleParty rainbowBattle, RainbowBattleTeamType team)
    {
        var packet = new StringBuilder("fblst");

        IReadOnlyList<IClientSession> members = team == RainbowBattleTeamType.Red ? rainbowBattle.RedTeam : rainbowBattle.BlueTeam;

        foreach (IClientSession member in members)
        {
            packet.Append(" " +
                $"{member.PlayerEntity.Level}" +
                $".{(member.PlayerEntity.Specialist != null && member.PlayerEntity.UseSp ? member.PlayerEntity.Specialist.GameItem.Morph : -1)}" +
                $".{(byte)member.PlayerEntity.Class}" +
                $".{member.PlayerEntity.RainbowBattleComponent.Kills}" +
                $".{member.PlayerEntity.Name}" +
                $".{(byte)member.PlayerEntity.Gender}" +
                $".{member.PlayerEntity.Id}" +
                $".{member.PlayerEntity.HeroLevel}");
        }

        return packet.ToString();
    }

    public static string GenerateRainbowBattleLive(this RainbowBattleParty rainbowBattle, RainbowBattleTeamType team)
    {
        var packet = new StringBuilder("fbt 3");

        IReadOnlyList<IClientSession> members = team == RainbowBattleTeamType.Red ? rainbowBattle.RedTeam : rainbowBattle.BlueTeam;

        foreach (IClientSession member in members)
        {
            packet.Append($" {member.PlayerEntity.Id}.{member.PlayerEntity.HpPercentage}.{member.PlayerEntity.MpPercentage}");
        }

        return packet.ToString();
    }

    public static string GenerateFlagPacket(this INpcEntity entity) => $"fbt 6 {entity.Id} {(byte)entity.RainbowFlag.FlagType} {(byte)entity.RainbowFlag.FlagTeamType}";

    public static string GenerateRainBowEnter(bool isEnter) => $"fbt 1 {(isEnter ? 1 : 0)}";
    public static string GenerateRainBowExit() => "fbt 2 -1";

    public static string GenerateRainbowScore(this RainbowBattleParty party, RainbowBattleTeamType teamType)
    {
        var packet = new StringBuilder("fbs ");

        int redPoints = party.RedPoints;
        int bluePoints = party.BluePoints;

        switch (teamType)
        {
            case RainbowBattleTeamType.Red:

                int redTeamCount = party.RedTeam.Count;
                int redBigFlags = party.RedFlags.TryGetValue(RainbowBattleFlagType.Big, out byte bigRedCount) ? bigRedCount : 0;
                int redMediumFlags = party.RedFlags.TryGetValue(RainbowBattleFlagType.Medium, out byte mediumRedCount) ? mediumRedCount : 0;
                int redSmallFlags = party.RedFlags.TryGetValue(RainbowBattleFlagType.Small, out byte smallRedCount) ? smallRedCount : 0;

                packet.Append($"1 {redTeamCount} {redPoints} {bluePoints} {redSmallFlags} {redMediumFlags} {redBigFlags} RED");

                break;
            case RainbowBattleTeamType.Blue:

                int blueTeamCount = party.BlueTeam.Count;
                int blueBigFlags = party.BlueFlags.TryGetValue(RainbowBattleFlagType.Big, out byte bigBlueCount) ? bigBlueCount : 0;
                int blueMediumFlags = party.BlueFlags.TryGetValue(RainbowBattleFlagType.Medium, out byte mediumBlueCount) ? mediumBlueCount : 0;
                int blueSmallFlags = party.BlueFlags.TryGetValue(RainbowBattleFlagType.Small, out byte smallBlueCount) ? smallBlueCount : 0;

                packet.Append($"2 {blueTeamCount} {redPoints} {bluePoints} {blueSmallFlags} {blueMediumFlags} {blueBigFlags} BLUE");

                break;
        }

        return packet.ToString();
    }

    public static string GenerateRainbowTime(RainbowTimeType timeType, short? seconds = null)
    {
        switch (timeType)
        {
            case RainbowTimeType.End:
                return "fbt 5 0 0";
            case RainbowTimeType.Start:
                return $"fbt 5 1 {seconds ?? 0}";
            case RainbowTimeType.Enter:
                return "fbt 5 2 0";
            default:
                throw new ArgumentOutOfRangeException(nameof(timeType), timeType, null);
        }
    }

    public static string GenerateRainbowTeamType(this IClientSession session) =>
        $"guri 5 1 {session.PlayerEntity.Id} {(session.PlayerEntity.RainbowBattleComponent.Team == RainbowBattleTeamType.Red ? 1 : 2).ToString()}";

    public static void BroadcastRainbowTeamType(this IClientSession session) => session.CurrentMapInstance.Broadcast(session.GenerateRainbowTeamType());

    public static bool CanJoinToRainbowBattle(this IClientSession session)
    {
        if (session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
        {
            return false;
        }

        if (session.PlayerEntity.IsInGroup())
        {
            return false;
        }

        if (session.PlayerEntity.TimeSpaceComponent.IsInTimeSpaceParty)
        {
            return false;
        }

        if (session.PlayerEntity.IsInRaidParty)
        {
            return false;
        }

        if (session.PlayerEntity.HasShopOpened)
        {
            return false;
        }

        if (session.IsMuted())
        {
            return false;
        }

        return session.CurrentMapInstance != null && session.CurrentMapInstance.HasMapFlag(MapFlags.IS_BASE_MAP);
    }
}