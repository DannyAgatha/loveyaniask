using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateLeaveTeamEventHandler : IAsyncEventProcessor<MateLeaveTeamEvent>
{
    private readonly IDelayManager _delayManager;
    private readonly IGameLanguageService _langService;
    private readonly IMateBuffConfigsContainer _mateBuffConfigsContainer;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public MateLeaveTeamEventHandler(IGameLanguageService gameLanguageService, IDelayManager delayManager,
        ISpPartnerConfiguration spPartnerConfiguration, IRandomGenerator randomGenerator, IMateBuffConfigsContainer mateBuffConfigsContainer)
    {
        _langService = gameLanguageService;
        _delayManager = delayManager;
        _spPartnerConfiguration = spPartnerConfiguration;
        _randomGenerator = randomGenerator;
        _mateBuffConfigsContainer = mateBuffConfigsContainer;
    }

    public async Task HandleAsync(MateLeaveTeamEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IMateEntity mateEntity = e.MateEntity;

        if (mateEntity == null)
        {
            return;
        }

        if (mateEntity.Owner.Id != e.Sender.PlayerEntity.Id)
        {
            return;
        }

        bool isInMiniland = e.MateEntity.MapInstance?.Id == e.MateEntity.Owner.Miniland.Id;

        if (isInMiniland)
        {
            await session.EmitEventAsync(new MateStayInsideMinilandEvent
            {
                MateEntity = mateEntity
            });
            return;
        }

        session.PlayerEntity.MapInstance?.RemoveMate(mateEntity);
        mateEntity.BroadcastMateOut();
        Position cell = mateEntity.NewMinilandMapCell(_randomGenerator);
        mateEntity.MinilandX = cell.X;
        mateEntity.MinilandY = cell.Y;
        mateEntity.ChangePosition(cell);

        mateEntity.IsTeamMember = false;

        if (session.PlayerEntity.Miniland.GetBattleEntity(VisualType.Npc, mateEntity.Id) == null)
        {
            session.PlayerEntity.Miniland.AddMate(mateEntity);
        }

        await mateEntity.RemovePartnerSp();
        mateEntity.RefreshPartnerSkills();
        await mateEntity.RemoveAllBuffsAsync(true);
        session.RemovePetBuffs(mateEntity, _mateBuffConfigsContainer);
        session.SendEmptyMateSkillPacket();

        session.SendScpPackets();
        session.SendScnPackets();

        session.RefreshParty(_spPartnerConfiguration);

        if (mateEntity.MateType == MateType.Partner && mateEntity.Skills is not null)
        {
            foreach (IBattleEntitySkill skill in mateEntity.Skills.Where(s => s.Skill?.CastId is 38 or 39 && mateEntity.Level >= s.Skill.LevelMinimum))
            {
                if (session?.PlayerEntity.SkillsSp == null)
                {
                    continue;
                }

                session.PlayerEntity.SkillsSp.TryRemove(skill.Skill.Id, out CharacterSkill value);
                session.PlayerEntity.Skills.Remove(value);

                session.PlayerEntity.CharacterSkills.TryRemove(skill.Skill.Id, out value);
                session.PlayerEntity.Skills.Remove(value);
            }
    
            session?.RefreshSkillList();
            session?.RefreshQuicklist();
        }

        if (session != null && !session.PlayerEntity.Miniland.Sessions.Any())
        {
            return;
        }

        e.MateEntity.MapInstance?.Broadcast(s => e.MateEntity.GenerateIn(_langService, s.UserLanguage, _spPartnerConfiguration));
    }
}