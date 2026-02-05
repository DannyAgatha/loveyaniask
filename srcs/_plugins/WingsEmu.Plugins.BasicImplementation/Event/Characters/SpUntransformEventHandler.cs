using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class SpUntransformEventHandler : IAsyncEventProcessor<SpUntransformEvent>
{
    private readonly ICharacterAlgorithm _characterAlgorithm;
    private readonly IGameLanguageService _languageService;
    private readonly IMeditationManager _meditationManager;
    private readonly ISpWingConfiguration _spWingConfiguration;
    private readonly ISpyOutManager _spyOutManager;
    private readonly ITeleportManager _teleportManager;
    private readonly IScheduler _scheduler;
    private readonly IMapManager _mapManager;

    public SpUntransformEventHandler(IGameLanguageService languageService, ISpWingConfiguration spWingConfiguration,
        ICharacterAlgorithm characterAlgorithm, ISpyOutManager spyOutManager, IMeditationManager meditationManager, ITeleportManager teleportManager, IScheduler scheduler, IMapManager mapManager)
    {
        _languageService = languageService;
        _spWingConfiguration = spWingConfiguration;
        _characterAlgorithm = characterAlgorithm;
        _meditationManager = meditationManager;
        _teleportManager = teleportManager;
        _spyOutManager = spyOutManager;
        _scheduler = scheduler;
        _mapManager = mapManager;
    }

    public async Task HandleAsync(SpUntransformEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        SpWingInfo wingInfo = _spWingConfiguration.GetSpWingInfo(session.PlayerEntity.Specialist.WingsDesign);
        if (wingInfo != null)
        {
            foreach (WingBuff buff in wingInfo.Buffs)
            {
                Buff wingBuff = session.PlayerEntity.BuffComponent.GetBuff(buff.BuffId);
                await session.PlayerEntity.RemoveBuffAsync(buff.IsPermanent, wingBuff);
            }
        }
        
        await session.PlayerEntity.RemoveBuffsOnSpTransformAsync();

        session.PlayerEntity.BCardComponent.ClearEquipmentBCards(EquipmentType.Sp);

        session.PlayerEntity.RemoveAngelElement();
        session.PlayerEntity.ChangeScoutState(ScoutStateType.None);
        session.PlayerEntity.CleanComboState();
        session.SendMsCPacket(1);
        session.PlayerEntity.UseSp = false;
        session.PlayerEntity.LastSkillCombo = null;
        session.RefreshLevel(_characterAlgorithm);
        await session.PlayerEntity.UpdateEnergyBar(-100);
        session.SendRemoveSpFtptPacket();
        int cooldown = 10;
        if (session.PlayerEntity.SkillsSp != null)
        {
            foreach ((int skillVnum, CharacterSkill skill) in session.PlayerEntity.SkillsSp)
            {
                if (session.PlayerEntity.SkillCanBeUsed(skill))
                {
                    continue;
                }

                short time = skill.Skill.Cooldown;
                double temp = (skill.LastUse - DateTime.UtcNow).TotalMilliseconds + time * 100;
                temp /= 2000;
                cooldown = temp > cooldown ? (int)temp : cooldown;
            }
        }

        IMateEntity teamMate = session.PlayerEntity.MateComponent.TeamMembers()
            .FirstOrDefault(x => x.MateType == MateType.Partner);
        if (teamMate != null && teamMate.Skills.Any())
        {
            foreach (IBattleEntitySkill partnerSkill in teamMate.Skills.Where(s => s.Skill.CastId is 38 or 39 && teamMate.Level >= s.Skill.LevelMinimum))
            {
                if (session.PlayerEntity.SkillsSp.TryRemove(partnerSkill.Skill.Id, out CharacterSkill spSkill))
                {
                    var newCharacterSkill = new CharacterSkill
                    {
                        SkillVNum = partnerSkill.Skill.Id
                    };

                    session.PlayerEntity.CharacterSkills[partnerSkill.Skill.Id] = newCharacterSkill;

                    IBattleEntitySkill toRemove = session.PlayerEntity.Skills.FirstOrDefault(x => x.Skill.Id == partnerSkill.Skill.Id);
                    session.PlayerEntity.Skills.Remove(toRemove);
                    session.PlayerEntity.Skills.Add(newCharacterSkill);
                }
            }
        }

        if (_spyOutManager.ContainsSpyOut(session.PlayerEntity.Id))
        {
            session.SendObArPacket();
            _spyOutManager.RemoveSpyOutSkill(session.PlayerEntity.Id);
        }

        session.SendIncreaseRange();
        session.PlayerEntity.ChargeComponent.ResetCharge();
        session.PlayerEntity.BCardComponent.ClearChargeBCard();

        await session.EmitEventAsync(new GetDefaultMorphEvent());
        session.PlayerEntity.SpCooldownEnd = DateTime.UtcNow.AddSeconds(cooldown);
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.DurationOfSideEffect, 4, cooldown);
        session.SendSpCooldownUi(cooldown);
        session.BroadcastCMode();
        session.BroadcastGuri(6, 0, rules: new RangeBroadcast(session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        _meditationManager.RemoveAllMeditation(session.PlayerEntity);
        _teleportManager.RemovePosition(session.PlayerEntity.Id);
        session.PlayerEntity.SkillComponent.SendTeleportPacket = null;

        session.PlayerEntity.IsRemovingSpecialistPoints = false;
        session.PlayerEntity.InitialScpPacketSent = false;
        session.PlayerEntity.Session.SendScpPacket(0);

        if (session.PlayerEntity.IsInRaidParty)
        {
            foreach (IClientSession s in session.PlayerEntity.Raid.Members)
            {
                s.RefreshRaidMemberList(session.PlayerEntity.Raid.IsSpecialRaid());
            }
        }

        session.RefreshSkillList();
        session.PlayerEntity.ClearSkillCooldowns();
        session.PlayerEntity.Skills.Clear();
        foreach (IBattleEntitySkill skill in session.PlayerEntity.GetSkills())
        {
            session.PlayerEntity.Skills.Add(skill);
        }

        session.RefreshQuicklist();
        session.RefreshStatChar();
        session.RefreshEquipment();
        session.RefreshStat(true);
        session.SendCondPacket();
        session.SendDiscordRpcPacket();
    }
}