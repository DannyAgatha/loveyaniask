using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateStayInsideMinilandEventHandler : IAsyncEventProcessor<MateStayInsideMinilandEvent>
{
    private readonly IMateBuffConfigsContainer _mateBuffConfigsContainer;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public MateStayInsideMinilandEventHandler(IRandomGenerator randomGenerator, IMateBuffConfigsContainer mateBuffConfigsContainer, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _randomGenerator = randomGenerator;
        _mateBuffConfigsContainer = mateBuffConfigsContainer;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public async Task HandleAsync(MateStayInsideMinilandEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IMateEntity mateEntity = e.MateEntity;

        if (mateEntity == null)
        {
            return;
        }

        if (session.PlayerEntity.Miniland == null)
        {
            return;
        }

        if (!mateEntity.IsAlive())
        {
            mateEntity.Hp = 1;
        }

        if (session.PlayerEntity.Miniland.IsBlockedZone(mateEntity.PositionX, mateEntity.PositionY))
        {
            Position cell = mateEntity.NewMinilandMapCell(_randomGenerator);
            mateEntity.MinilandX = cell.X;
            mateEntity.MinilandY = cell.Y;
        }

        if (session.PlayerEntity.Miniland.IsBlockedZone(mateEntity.MinilandX, mateEntity.MinilandY))
        {
            Position cell = mateEntity.NewMinilandMapCell(_randomGenerator);
            mateEntity.MinilandX = cell.X;
            mateEntity.MinilandY = cell.Y;
        }

        mateEntity.IsTeamMember = false;
        mateEntity.ChangePosition(new Position(mateEntity.MinilandX, mateEntity.MinilandY));

        if (!e.IsOnCharacterEnter)
        {
            await mateEntity.RemovePartnerSp();
            mateEntity.RefreshPartnerSkills();
            await mateEntity.RemoveAllBuffsAsync(true);
            session.RemovePetBuffs(mateEntity, _mateBuffConfigsContainer);
        }

        switch (mateEntity.MateType)
        {
            case MateType.Pet:
                if (mateEntity.HasDhaPremium)
                {
                    session.PlayerEntity.CharacterSkills.TryRemove((short)SkillsVnums.DHA_PREMIUM, out CharacterSkill value);
                    session.PlayerEntity.Skills.Remove(value);
                    session.RefreshSkillList();
                    session.RefreshQuicklist();
                }
                break;
            case MateType.Partner when mateEntity.Skills is not null:
                foreach (IBattleEntitySkill skill in mateEntity.Skills.Where(s => s.Skill.CastId is 38 or 39 && mateEntity.Level >= s.Skill.LevelMinimum))
                {
                    if (session.PlayerEntity.UseSp)
                    {
                        session.PlayerEntity.SkillsSp.TryRemove(skill.Skill.Id, out _);
                    }
                    else
                    {
                        session.PlayerEntity.CharacterSkills.TryRemove(skill.Skill.Id, out _);
                    }
                    IBattleEntitySkill toRemove = session.PlayerEntity.Skills.FirstOrDefault(x => x.Skill.Id == skill.Skill.Id);
                    if (toRemove != null)
                    {
                        session.PlayerEntity.Skills.Remove(toRemove);
                    }
                }

                session.RefreshSkillList();
                session.RefreshQuicklist();
                break;

        }

        session.RefreshParty(_spPartnerConfiguration);
    }
}