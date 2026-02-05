using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Configuration;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class ChangeSubClassEventHandler : IAsyncEventProcessor<ChangeSubClassEvent>
{
    private readonly IBattleEntityAlgorithmService _algorithm;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameItemInstanceFactory _gameItemInstance;
    private readonly IGameLanguageService _languageService;
    private readonly IRandomGenerator _randomNumberGenerator;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly IMapManager _mapManager;

    public ChangeSubClassEventHandler(IGameLanguageService languageService, IRandomGenerator randomNumberGenerator,
        IBattleEntityAlgorithmService algorithm, ICharacterAlgorithm characterAlgorithm, IGameItemInstanceFactory gameItemInstance, IReputationConfiguration reputationConfiguration,
        IRankingManager rankingManager, IMapManager mapManager)
    {
        _languageService = languageService;
        _randomNumberGenerator = randomNumberGenerator;
        _algorithm = algorithm;
        _characterAlgorithm = characterAlgorithm;
        _gameItemInstance = gameItemInstance;
        _reputationConfiguration = reputationConfiguration;
        _rankingManager = rankingManager;
        _mapManager = mapManager;
    }

    public async Task HandleAsync(ChangeSubClassEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        
        string? subClassName = Enum.GetName(typeof(SubClassType), e.NewSubClass);
        
        if (subClassName == null)
        {
            return;
        }

        session.SendPacket("npinfo 0");
        session.SendPClearPacket();

        if (e.NewSubClass == (byte)ClassType.Adventurer)
        {
            session.PlayerEntity.HairStyle =
                (byte)session.PlayerEntity.HairStyle > 1 ? 0 : session.PlayerEntity.HairStyle;

            if (session.PlayerEntity.JobLevel > 20)
            {
                session.PlayerEntity.JobLevel = 20;
            }
        }
        
        var baseSkillVnums = new Dictionary<SubClassType, int>
        {
            { SubClassType.OathKeeper, (int)SkillsVnums.OATH_KEEPER_T1_SKILL },
            { SubClassType.CrimsonFury, (int)SkillsVnums.CRIMSON_FURY_T1_SKILL },
            { SubClassType.CelestialPaladin, (int)SkillsVnums.CELESTIAL_PALADIN_T1_SKILL },
            { SubClassType.SilentStalker, (int)SkillsVnums.SILENT_STALKER_T1_SKILL },
            { SubClassType.ArrowLord, (int)SkillsVnums.ARROW_LORD_T1_SKILL },
            { SubClassType.ShadowHunter, (int)SkillsVnums.SHADOW_HUNTER_T1_SKILL },
            { SubClassType.ArcaneSage, (int)SkillsVnums.ARCANE_SAGE_T1_SKILL },
            { SubClassType.Pyromancer, (int)SkillsVnums.PYROMANCER_T1_SKILL },
            { SubClassType.DarkNecromancer, (int)SkillsVnums.DARK_NECROMANCER_T1_SKILL },
            { SubClassType.ZenWarrior, (int)SkillsVnums.ZEN_WARRIOR_T1_SKILL },
            { SubClassType.EmperorsBlade, (int)SkillsVnums.EMPERORS_BLADE_T1_SKILL },
            { SubClassType.StealthShadow, (int)SkillsVnums.STEALTH_SHADOW_T1_SKILL }
        };
        
        if (baseSkillVnums.TryGetValue(session.PlayerEntity.SubClass, out int oldBaseVnum))
        {
            for (int tier = 1; tier <= 5; tier++)
            {
                int skillToRemove = oldBaseVnum + (tier - 1);
                if (session.PlayerEntity.UseSp)
                {
                    if (session.PlayerEntity.SkillsSp.ContainsKey((short)skillToRemove))
                    {
                        session.PlayerEntity.SkillsSp.Remove((short)skillToRemove, out CharacterSkill _);
                    }
                }
                
                session.PlayerEntity.CharacterSkills.TryRemove(skillToRemove, out _);
            }
        }
        
        if (baseSkillVnums.TryGetValue(e.NewSubClass, out int newBaseVnum))
        {
            int newSkillId = newBaseVnum + (e.TierLevel - 1);
            var newSkill = new CharacterSkill { SkillVNum = newSkillId };
            if (session.PlayerEntity.UseSp)
            {
                session.PlayerEntity.SkillsSp[(short)newSkillId] = newSkill;
                session.PlayerEntity.Skills.Add(newSkill);
            }
            session.PlayerEntity.CharacterSkills.TryAdd(newSkillId, newSkill);
        }

        session.SendCondPacket();
        session.PlayerEntity.SubClass= e.NewSubClass;
        session.PlayerEntity.TierLevel = e.TierLevel;
        session.PlayerEntity.TierExperience = e.TierExperience;
        session.PlayerEntity.RefreshMaxHpMp(_algorithm);
        session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
        session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
        session.BroadcastTitleInfo();
        session.RefreshStat();
        session.RefreshLevel(_characterAlgorithm);
        session.SendEqPacket();
        session.BroadcastEffectInRange(EffectType.JobLevelUp);
        
        subClassName = subClassName.AddSpacesToCamelCase();
        session.SendMsg(_languageService.GetLanguageFormat(GameDialogKey.SUCCESSFULLY_SUBCLASS_CHANGED, session.UserLanguage, subClassName), MsgMessageType.Middle);

        session.BroadcastEffectInRange(EffectType.Transform);

        session.BroadcastCMode();
        session.BroadcastIn(_reputationConfiguration, _rankingManager.TopReputation, new ExceptSessionBroadcast(session));
        session.BroadcastGidx(session.PlayerEntity.Family, _languageService);
        session.BroadcastEffectInRange(EffectType.NormalLevelUp);
        session.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);

        session.PlayerEntity.ClearSkillCooldowns();

        foreach (CharacterQuicklistEntryDto remove in session.PlayerEntity.QuicklistComponent.GetQuicklist().Where(x => x.Morph == 0).ToList())
        {
            session.PlayerEntity.QuicklistComponent.RemoveQuicklist(remove.QuicklistTab, remove.QuicklistSlot, 0);
        }
        
        session.RefreshPassiveBCards();
        session.RefreshSkillList();
        session.RefreshQuicklist();
    }
}