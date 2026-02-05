using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.InitialConfiguration;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using YamlDotNet.Core;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class ChangeClassEventHandler : IAsyncEventProcessor<ChangeClassEvent>
{
    private readonly IBattleEntityAlgorithmService _algorithm;
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameItemInstanceFactory _gameItemInstance;
    private readonly IGameLanguageService _languageService;
    private readonly IRandomGenerator _randomNumberGenerator;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly IMapManager _mapManager;
    private readonly InitialClassConfiguration _initialClassConfiguration;
    private readonly ISkillsManager _skillsManager;

    public ChangeClassEventHandler(IGameLanguageService languageService, IRandomGenerator randomNumberGenerator,
        IBattleEntityAlgorithmService algorithm, ICharacterAlgorithm characterAlgorithm, IGameItemInstanceFactory gameItemInstance, IReputationConfiguration reputationConfiguration,
        IRankingManager rankingManager, IMapManager mapManager, InitialClassConfiguration initialClassConfiguration, ISkillsManager skillsManager)
    {
        _languageService = languageService;
        _randomNumberGenerator = randomNumberGenerator;
        _algorithm = algorithm;
        _characterAlgorithm = characterAlgorithm;
        _gameItemInstance = gameItemInstance;
        _reputationConfiguration = reputationConfiguration;
        _rankingManager = rankingManager;
        _mapManager = mapManager;
        _initialClassConfiguration = initialClassConfiguration;
        _skillsManager = skillsManager;
    }

    public async Task HandleAsync(ChangeClassEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        if (e.ShouldResetJobLevel)
        {
            session.PlayerEntity.JobLevel = 80;
            session.PlayerEntity.JobLevelXp = 0;
        }

        session.SendPacket("npinfo 0");
        session.SendPClearPacket();

        if (e.NewClass == (byte)ClassType.Adventurer)
        {
            session.PlayerEntity.HairStyle =
                (byte)session.PlayerEntity.HairStyle > 1 ? 0 : session.PlayerEntity.HairStyle;

            if (session.PlayerEntity.JobLevel > 80)
            {
                session.PlayerEntity.JobLevel = 80;
            }
        }

        session.SendCondPacket();
        session.PlayerEntity.Class = e.NewClass;
        session.PlayerEntity.RefreshMaxHpMp(_algorithm);
        session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
        session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
        session.BroadcastTitleInfo();
        session.RefreshStat();
        session.RefreshLevel(_characterAlgorithm);
        session.SendEqPacket();
        session.BroadcastEffectInRange(EffectType.JobLevelUp);
        session.SendMsg(_languageService.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_CLASS_CHANGED, session.UserLanguage), MsgMessageType.Middle);
        session.BroadcastEffectInRange(EffectType.Transform);
        if (e.ShouldObtainNewFaction && !session.PlayerEntity.IsInFamily())
        {
            await session.EmitEventAsync(new ChangeFactionEvent
            {
                NewFaction = (FactionType)_randomNumberGenerator.RandomNumber(1, 3)
            });
        }

        session.BroadcastCMode();
        session.BroadcastIn(_reputationConfiguration, _rankingManager.TopReputation, new ExceptSessionBroadcast(session));
        session.BroadcastGidx(session.PlayerEntity.Family, _languageService);
        session.BroadcastEffectInRange(EffectType.NormalLevelUp);
        session.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);
        session.PlayerEntity.Skills.Clear();
        List<CharacterSkill> passivesToRemove = new();
        foreach (CharacterSkill skill in session.PlayerEntity.CharacterSkills.Values)
        {
            if (skill.Skill.IsPassiveSkill())
            {
                int skillMinimumLevel = 0;
                if (skill.Skill.MinimumSwordmanLevel == 0 && skill.Skill.MinimumArcherLevel == 0 && skill.Skill.MinimumMagicianLevel == 0)
                {
                    skillMinimumLevel = skill.Skill.MinimumAdventurerLevel;
                }
                else
                {
                    skillMinimumLevel = session.PlayerEntity.Class switch
                    {
                        ClassType.Adventurer => skill.Skill.MinimumAdventurerLevel,
                        ClassType.Swordman => skill.Skill.MinimumSwordmanLevel,
                        ClassType.Archer => skill.Skill.MinimumArcherLevel,
                        ClassType.Magician => skill.Skill.MinimumMagicianLevel,
                        _ => skillMinimumLevel
                    };
                }

                if (skillMinimumLevel == 0)
                {
                    passivesToRemove.Add(skill);
                }

                continue;
            }

            session.PlayerEntity.CharacterSkills.TryRemove(skill.SkillVNum, out CharacterSkill value);
            if (session.PlayerEntity.SkillComponent.SkillUpgrades.TryGetValue((short)skill.SkillVNum, out HashSet<IBattleEntitySkill> upgrades))
            {
                upgrades.Clear();
            }
        }

        foreach (CharacterSkill passive in passivesToRemove)
        {
            session.PlayerEntity.CharacterSkills.TryRemove(passive.Skill.Id, out _);
        }

        CharacterSkill newSkill;
        var skillsToAdd = new List<IBattleEntitySkill>();

        if (_initialClassConfiguration != null)
        {
            ClassEquipmentConfiguration classConfig = _initialClassConfiguration.ClassEquipments
                .FirstOrDefault(ce => ce.ClassType == session.PlayerEntity.Class);

            if (classConfig != null)
            {
                foreach (SkillConfiguration skillConfig in classConfig.DefaultSkills)
                {
                    newSkill = new CharacterSkill
                    {
                        SkillVNum = skillConfig.SkillVnum
                    };
                    skillsToAdd.Add(newSkill);
                    session.PlayerEntity.CharacterSkills[skillConfig.SkillVnum] = newSkill;
                }
            }
        }
        else
        {
            newSkill = new CharacterSkill
            {
                SkillVNum = (short)(200 + 20 * (byte)session.PlayerEntity.Class)
            };
            skillsToAdd.Add(newSkill);
            session.PlayerEntity.CharacterSkills[(short)(200 + 20 * (byte)session.PlayerEntity.Class)] = newSkill;

            newSkill = new CharacterSkill
            {
                SkillVNum = (short)(201 + 20 * (byte)session.PlayerEntity.Class)
            };
            skillsToAdd.Add(newSkill);
            session.PlayerEntity.CharacterSkills[(short)(201 + 20 * (byte)session.PlayerEntity.Class)] = newSkill;

            int skillCatch = session.PlayerEntity.Class switch
            {
                ClassType.Adventurer => 209,
                ClassType.Swordman => 235,
                ClassType.Archer => 236,
                ClassType.Magician => 237,
                _ => 0
            };

            if (skillCatch > 0)
            {
                newSkill = new CharacterSkill
                {
                    SkillVNum = skillCatch
                };
                skillsToAdd.Add(newSkill);
                session.PlayerEntity.CharacterSkills[skillCatch] = newSkill;
            }
        }

        session.PlayerEntity.ClearSkillCooldowns();

        foreach (CharacterQuicklistEntryDto remove in session.PlayerEntity.QuicklistComponent.GetQuicklist().Where(x => x.Morph == 0).ToList())
        {
            session.PlayerEntity.QuicklistComponent.RemoveQuicklist(remove.QuicklistTab, remove.QuicklistSlot, 0);
        }

        // Add default passive skills from configuration
        ClassEquipmentConfiguration passiveSkillConfiguration = _initialClassConfiguration.ClassEquipments
            .FirstOrDefault(ce => ce.ClassType == session.PlayerEntity.Class);

        if (passiveSkillConfiguration != null)
        {
            foreach (SkillConfiguration skillConfig in passiveSkillConfiguration.PassiveSkills)
            {
                SkillDTO skill = _skillsManager.GetSkill(skillConfig.SkillVnum);
                if (!skill.IsPassiveSkill())
                {
                    continue;
                }

                var newSkill1 = new CharacterSkill
                {
                    SkillVNum = skillConfig.SkillVnum
                };

                if (!session.PlayerEntity.CharacterSkills.TryAdd(skillConfig.SkillVnum, newSkill1))
                {
                    continue;
                }

                session.PlayerEntity.Skills.Add(newSkill1);
            }
        }

        session.PlayerEntity.Skills.AddRange(skillsToAdd);
        session.RefreshPassiveBCards();
        session.RefreshSkillList();
        session.RefreshQuicklist();

        if (!e.ShouldObtainBasicItems)
        {
            return;
        }

        ClassEquipmentConfiguration classEquipment = _initialClassConfiguration.ClassEquipments
            .FirstOrDefault(ce => ce.ClassType == e.NewClass);

        if (classEquipment != null)
        {
            foreach (EquipmentConfiguration equipment in classEquipment.Equipments)
            {
                GameItemInstance newItem = _gameItemInstance.CreateItem((short)equipment.ItemVnum, equipment.Amount, equipment.Upgrade, equipment.Rare);
                newItem.EquipmentOptions ??= [];
                newItem.EquipmentOptions.Clear();
                if (equipment.Options != null)
                {
                    newItem.EquipmentOptions.AddRange(equipment.Options);
                }
                newItem.BoundCharacterId = session.PlayerEntity.Id;
                InventoryItem item = await session.AddNewItemToInventory(newItem);

                if (equipment.Equipped)
                {
                    await session.EmitEventAsync(new InventoryEquipItemEvent(item.Slot));
                }
            }

            foreach (SpecialistConfiguration specialist in classEquipment.Specialists)
            {
                GameItemInstance newCard = _gameItemInstance.CreateSpecialistCard(
                    specialist.CardVnum,
                    (byte)specialist.SpLevel,
                    specialist.Upgrade,
                    specialist.SpStoneUpgrade,
                    specialist.SpDamage,
                    specialist.SpDefence,
                    specialist.SpElement,
                    specialist.SpHp,
                    specialist.SpFire,
                    specialist.SpWater,
                    specialist.SpLight,
                    specialist.SpDark
                );
                InventoryItem item = await session.AddNewItemToInventory(newCard);

                if (specialist.Equipped)
                {
                    await session.EmitEventAsync(new InventoryEquipItemEvent(item.Slot));
                }
            }
        }
    }
}