// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;

namespace WingsEmu.Game.Skills;

public static class CharacterSkillsExtensions
{
    public static IEnumerable<IBattleEntitySkill> GetSkills(this IPlayerEntity character) => character.UseSp ? character.SkillsSp.Values.ToArray() : character.CharacterSkills.Values.ToArray();

    public static string GenerateSkillCooldownResetPacket(this IClientSession session, int castId) => $"sr {castId}";

    public static string GenerateSkillCooldownResetAfter(this IClientSession session, short castId, short time) => $"sr -10 {castId} {time}";
    
    public static string GenerateSkillCooldownResetAfter(this IClientSession session, short castId, short time, int cd) => $"sr -10 {castId} {time} {cd}";
    public static void SendSkillCooldownResetAfter(this IClientSession session, short castId, short time) => session.SendPacket(session.GenerateSkillCooldownResetAfter(castId, time));
    
    public static void SendSkillCooldownResetAfter(this IClientSession session, short castId, short time, int cd) => session.SendPacket(session.GenerateSkillCooldownResetAfter(castId, time, cd));

    public static void SendSkillCooldownReset(this IClientSession session, int castId) => session.SendPacket(session.GenerateSkillCooldownResetPacket(castId));
}

public class CharacterSkill : CharacterSkillDTO, IBattleEntitySkill
{
    private SkillDTO _skill;
    public DateTime LastUse { get; set; } = DateTime.UtcNow.AddHours(-1);
    public short Rate => 100;
    public SkillDTO Skill => _skill ??= StaticSkillsManager.Instance.GetSkill(SkillVNum);
}