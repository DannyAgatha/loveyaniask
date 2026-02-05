// NosEmu
// 


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Plugins.Essentials.Skills;

[Name("Admin-SkillsCheat")]
[Description("Module related to Administrator commands.")]
[RequireAuthority(AuthorityType.GA)]
public class AdministratorCheatModule_Skills : SaltyModuleBase
{
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISkillsManager _skillsManager;

    public AdministratorCheatModule_Skills(ISkillsManager skillsManager, IGameLanguageService gameLanguage, ICharacterAlgorithm characterAlgorithm)
    {
        _skillsManager = skillsManager;
        _gameLanguage = gameLanguage;
        _characterAlgorithm = characterAlgorithm;
    }

    [Command("unlockallclassskills")]
    public async Task<SaltyCommandResult> UnlockAllSkills()
    {
        IClientSession session = Context.Player;
        IEnumerable<SkillDTO> skills = _skillsManager.GetSkills().Where(x => (ClassType)x.Class == session.PlayerEntity.Class && x.SkillType == SkillType.NormalPlayerSkill);

        foreach (SkillDTO skill in skills)
        {
            if (session.PlayerEntity.CharacterSkills.ContainsKey(skill.Id))
            {
                continue;
            }

            var newSkill = new CharacterSkill
            {
                SkillVNum = skill.Id
            };

            session.PlayerEntity.CharacterSkills.TryAdd(skill.Id, newSkill);
            session.PlayerEntity.Skills.Add(newSkill);
        }

        session.RefreshSkillList();
        session.RefreshQuicklist();
        session.RefreshLevel(_characterAlgorithm);

        return new SaltyCommandResult(true);
    }

    [Command("unlockallpassives", "allbasicskills")]
    public async Task<SaltyCommandResult> UnlockAllBasicSkills()
    {
        IEnumerable<SkillDTO> skills = _skillsManager.GetSkills().Where(s => s.IsPassiveSkill());

        var passiveSkills = Context.Player.PlayerEntity.CharacterSkills
            .Where(s => s.Value.Skill.IsPassiveSkill())
            .Select(s => s.Value.SkillVNum)
            .ToHashSet();

        foreach (SkillDTO skill in skills)
        {
            if (passiveSkills.Contains(skill.Id))
            {
                continue;
            }

            Context.Player.PlayerEntity.CharacterSkills.TryAdd(skill.Id, new CharacterSkill
            {
                SkillVNum = skill.Id
            });
        }

        Context.Player.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_LEARNED, Context.Player.UserLanguage), 0);
        Context.Player.RefreshSkillList();
        Context.Player.RefreshQuicklist();
        Context.Player.RefreshPassiveBCards();
        return new SaltyCommandResult(true, "All passive skills unlocked !");
    }

    [Command("clear-buff", "remove-card", "remove-buff")]
    [Description("remove buff on map")]
    public async Task<SaltyCommandResult> ClearMapFromBuff(int cardId)
    {
        var tmp = Context.Player.CurrentMapInstance.Sessions.ToList();

        foreach (IClientSession player in tmp)
        {
            foreach (IMateEntity pet in player.PlayerEntity.MateComponent.GetMates())
            {
                Buff petBuff = pet.BuffComponent.GetBuff(cardId);
                if (petBuff == null)
                {
                    continue;
                }

                await pet.RemoveBuffAsync(false, petBuff);
            }

            Buff buff = player.PlayerEntity.BuffComponent.GetBuff(cardId);
            if (buff == null)
            {
                continue;
            }

            await player.PlayerEntity.RemoveBuffAsync(false, buff);
        }

        foreach (INpcEntity npc in Context.Player.CurrentMapInstance.GetAliveNpcs())
        {
            Buff buff = npc.BuffComponent.GetBuff(cardId);
            if (buff == null)
            {
                continue;
            }

            await npc.RemoveBuffAsync(false, buff);
        }

        return new SaltyCommandResult(true, "All buff");
    }
}