// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Raids;

public static class RaidsPacketExtensions
{
    public static string GenerateRaidName(this IClientSession session, IGameLanguageService gameLanguageService, RaidType raidType)
    {
        string raidKey = $"RAID_NAME_{raidType.ToString().ToUpper()}";
        if (!System.Enum.TryParse(raidKey, out GameDialogKey key))
        {
            return raidKey;
        }

        return gameLanguageService.GetLanguage(key, session.UserLanguage);
    }

    public static string GenerateRdList(this IClientSession session, bool isSpecialRaid)
    {
        StringBuilder stringBuilder = isSpecialRaid == true ? new StringBuilder("rdlstf -10") : new StringBuilder("rdlst");
        stringBuilder.Append($" {session.PlayerEntity.Raid.MinimumLevel} 0");
        stringBuilder.Append($" {(byte)session.PlayerEntity.Raid.Type}");
        foreach (IClientSession targetSession in session.PlayerEntity.Raid.Members)
        {
            stringBuilder.Append(
                $" {targetSession.PlayerEntity.Level}.{(targetSession.PlayerEntity.Specialist != null && targetSession.PlayerEntity.UseSp ? targetSession.PlayerEntity.Specialist.GameItem.Morph : -1)}" +
                $".{(byte)targetSession.PlayerEntity.Class}.{targetSession.PlayerEntity.RaidDeaths}." +
                $"{targetSession.PlayerEntity.Name}.{(byte)targetSession.PlayerEntity.Gender}.{targetSession.PlayerEntity.Id}.{targetSession.PlayerEntity.HeroLevel}");
        }

        return stringBuilder.ToString();
    }

    public static string GenerateRl(this IClientSession session, byte type, IRaidManager raidManager)
    {
        IReadOnlyCollection<RaidParty> registeredRaids = raidManager.RaidPublishList;
        if (!registeredRaids.Any())
        {
            return $"rl {type}";
        }

        string header = $"rl {type} ";
        foreach (RaidParty raid in registeredRaids)
        {
            IClientSession leader = raid.Members.First();
            header += $" {(byte)raid.Type}.{raid.MinimumLevel}.{raid.MaximumLevel}.{leader.PlayerEntity.Name}.{leader.PlayerEntity.Level}." +
                $"{(leader.PlayerEntity.Morph == 0 ? -1 : leader.PlayerEntity.Morph)}.{(byte)leader.PlayerEntity.Class}.{(byte)leader.PlayerEntity.Gender}.{raid.Members.Count}.{leader.PlayerEntity.HeroLevel}";
        }

        return header;
    }

    public static string GenerateRaidPacket(this IClientSession session, RaidPacketType packetType, bool isLeaving = false)
    {
        var stringBuilder = new StringBuilder("raid");
        switch (packetType)
        {
            case RaidPacketType.LIST_MEMBERS:
                stringBuilder.Append(" 0");

                if (session.PlayerEntity.Raid?.Members != null)
                {
                    foreach (IClientSession targetSession in session.PlayerEntity.Raid.Members.OrderByDescending(s => s.PlayerEntity.Level))
                    {
                        stringBuilder.Append(" ");
                        stringBuilder.Append(targetSession.PlayerEntity.Id);
                    }
                }

                break;
            case RaidPacketType.LEAVE:
                return isLeaving ? "raid 1 0" : "raid 1 1";
            case RaidPacketType.LEADER_RELATED:
                return "raid 2 " + (isLeaving
                    ? "-1"
                    : $"{(session.PlayerEntity.Raid?.Members != null && session.PlayerEntity.Raid.Members.Any() ? session.PlayerEntity.Raid.Members.First().PlayerEntity.Id : 0)}");
            case RaidPacketType.REFRESH_MEMBERS_HP_MP:
                stringBuilder.Append(" 3");

                if (session.PlayerEntity.Raid?.Members != null)
                {
                    foreach (IClientSession targetSession in session.PlayerEntity.Raid.Members.OrderByDescending(s => s.PlayerEntity.Level))
                    {
                        stringBuilder.Append($" {targetSession.PlayerEntity.Id}.{targetSession.PlayerEntity.GetHpPercentage()}.{targetSession.PlayerEntity.GetMpPercentage()}");
                    }
                }

                break;
            case RaidPacketType.AFTER_INSTANCE_START_BUT_BEFORE_REFRESH_MEMBERS:
                return "raid 4";
            case RaidPacketType.INSTANCE_START:
                return "raid 5 1";
        }

        return stringBuilder.ToString();
    }

    public static string GenerateRaidBossPacket(this IMonsterEntity entity, bool secondBoss)
        => $"rboss {(secondBoss ? 4 : 3)} {entity.Id} {entity.Hp} {entity.FakeMaxHp ?? entity.MaxHp} {entity.MonsterVNum}";

    public static string GenerateEmptyRaidBoss(this IClientSession session) => "rboss -1 -1 0 0";

    public static void SendEmptyRaidBoss(this IClientSession session)
    {
        session.SendPacket(session.GenerateEmptyRaidBoss());
    }

    public static string GenerateRaidUiPacket(this IClientSession session, RaidType raidType, RaidWindowType windowType) =>
        $"raidbf 0 {(byte)windowType} {(raidType == RaidType.Laurena ? 40 : 25)}";

    public static string GenerateThrowPacket(this IBattleEntity entity, MonsterMapItem mapItem)
        => $"throw {mapItem.ItemVNum} {mapItem.TransportId} {entity.PositionX} {entity.PositionY} {mapItem.PositionX} {mapItem.PositionY} {mapItem.Amount}";

    public static string GeneraterRaidmbf(this IClientSession session)
    {
        RaidSubInstance raidSubInstance = session.PlayerEntity.Raid.Instance.RaidSubInstances[session.PlayerEntity.MapInstanceId];

        return "raidmbf " +
            $"{raidSubInstance.CurrentTargetMonsters.ToString()} " + // initialMonstersToKill
            $"{(raidSubInstance.CurrentTargetMonsters - raidSubInstance.CurrentCompletedTargetMonsters).ToString()} " + // currentMonsterToKill
            $"{raidSubInstance.CurrentTargetButtons.ToString()} " + // initialButtonsToUse
            $"{(raidSubInstance.CurrentTargetButtons - raidSubInstance.CurrentCompletedTargetButtons).ToString()} " + // currentButtonsToUse
            $"{session.PlayerEntity.Raid.Instance.Lives.ToString()} " +
            $"{session.PlayerEntity.Raid.Instance.MaxLives.ToString()} " +
            $"{(session.PlayerEntity.Raid.Type == RaidType.Laurena ? 40 : 25).ToString()}";
    }
    public static void SendRaidPacket(this IClientSession session, RaidPacketType type, bool isLeaving = false)
    {
        session.SendPacket(session.GenerateRaidPacket(type, isLeaving));
    }
    
    public static void RefreshRaidMemberList(this IClientSession session, bool isSpecialRaid)
    {
        if (!session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        session.SendPacket(session.GenerateRdList(isSpecialRaid));
    }

    public static void SendRlPacket(this IClientSession session, byte type, IRaidManager raidManager) => session.SendPacket(session.GenerateRl(type, raidManager));

    public static void TrySendRaidBossPackets(this IClientSession session)
    {
        if (!session.PlayerEntity.IsInRaidParty)
        {
            return;
        }

        if (session.CurrentMapInstance == null)
        {
            return;
        }

        if (session.PlayerEntity.Raid?.Instance == null)
        {
            return;
        }

        if (!session.PlayerEntity.Raid.Instance.RaidSubInstances.TryGetValue(session.CurrentMapInstance.Id, out RaidSubInstance subInstance))
        {
            return;
        }

        bool secondBoss = false;
        foreach (IMonsterEntity boss in subInstance.BossMonsters)
        {
            session.SendPacket(boss.GenerateRaidBossPacket(secondBoss));
            secondBoss = true;
        }
    }

    public static void TrySendRaidBossDeadPackets(this IClientSession session)
    {
        if (!session.PlayerEntity.Raid.Instance.RaidSubInstances.ContainsKey(session.CurrentMapInstance.Id))
        {
            return;
        }

        RaidSubInstance subInstance = session.PlayerEntity.Raid.Instance.RaidSubInstances[session.CurrentMapInstance.Id];

        bool secondBoss = false;
        foreach (IMonsterEntity boss in subInstance.DeadBossMonsters)
        {
            session.SendPacket(boss.GenerateRaidBossPacket(secondBoss));
            secondBoss = true;
        }
    }

    public static void SendRaidUiPacket(this IClientSession session, RaidType raidType, RaidWindowType raidWindowType) =>
        session.SendPacket(session.GenerateRaidUiPacket(raidType, raidWindowType));

    public static void SendRaidmbf(this IClientSession session)
    {
        if (session.PlayerEntity.Raid?.Instance == null)
        {
            return;
        }

        if (!session.PlayerEntity.Raid.Instance.RaidSubInstances.ContainsKey(session.PlayerEntity.MapInstanceId))
        {
            return;
        }

        session.SendPacket(session.GeneraterRaidmbf());
    }

    public static void BroadcastThrow(this IBattleEntity entity, MonsterMapItem mapItem) => entity.MapInstance.Broadcast(entity.GenerateThrowPacket(mapItem));

    public static bool IsRaidTypeRestricted(this IClientSession session, RaidType raidType)
    {
        switch (raidType)
        {
            case RaidType.Glacerus:
            case RaidType.LordDraco:
                return true;
            default:
                return false;
        }
    }

    public static bool CanPlayerJoinToRestrictedRaid(this IClientSession session, RaidType raidType)
    {
        return raidType switch
        {
            RaidType.LordDraco => session.PlayerEntity.RaidRestrictionDto.LordDraco > 0,
            RaidType.Glacerus => session.PlayerEntity.RaidRestrictionDto.Glacerus > 0,
            _ => true
        };
    }
    
    public static bool IsFrozenByGlacerus(this IBattleEntity battleEntity) => battleEntity.BuffComponent.HasBuff((short)BuffVnums.ETERNAL_ICE);
    public static async Task RemoveEternalIce(this IBattleEntity entity)
    {
        await entity.RemoveBuffAsync((short)BuffVnums.ETERNAL_ICE, true);
    }

    public static bool IsPlayerWearingRaidAmulet(this IClientSession session, RaidType raidType)
    {
        if (session.PlayerEntity.Amulet == null)
        {
            return false;
        }

        return raidType switch
        {
            RaidType.LordDraco => session.PlayerEntity.Amulet.ItemVNum == (short)ItemVnums.DRACO_AMULET,
            RaidType.Glacerus => session.PlayerEntity.Amulet.ItemVNum == (short)ItemVnums.GLACERUS_AMULET,
            _ => true
        };
    }
}