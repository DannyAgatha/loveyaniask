using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Groups.Events;
using WingsEmu.Game.Managers;

namespace NosEmu.Plugins.BasicImplementations.Event.Groups;

public class GroupWeedingEventHandler : IAsyncEventProcessor<GroupWeedingEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly ISessionManager _sessionManager;

    public GroupWeedingEventHandler(IBuffFactory buffFactory, ISessionManager sessionManager)
    {
        _buffFactory = buffFactory;
        _sessionManager = sessionManager;
    }

    public async Task HandleAsync(GroupWeedingEvent e, CancellationToken cancellation)
    {
        IPlayerEntity playerEntity = e.Sender.PlayerEntity;
        bool removeBuff = e.RemoveBuff;
        short buff = (short)BuffVnums.WEDDING;

        if (!playerEntity.IsInGroup())
        {
            return;
        }

        IPlayerEntity lover = playerEntity.GetGroup().Members.FirstOrDefault(x => x.IsMarried(playerEntity.Id));
        if (lover == null)
        {
            if (!e.RelatedId.HasValue)
            {
                return;
            }

            lover = _sessionManager.GetSessionByCharacterId(e.RelatedId.Value)?.PlayerEntity;
            if (lover == null)
            {
                return;
            }
        }

        if (!removeBuff)
        {
            if (!playerEntity.BuffComponent.HasBuff(buff))
            {
                Buff playerBuff = _buffFactory.CreateBuff(buff, playerEntity, BuffFlag.BIG | BuffFlag.NO_DURATION);
                await playerEntity.AddBuffAsync(playerBuff);
            }

            if (lover.BuffComponent.HasBuff(buff))
            {
                return;
            }

            Buff marriageBuff = _buffFactory.CreateBuff(buff, lover, BuffFlag.BIG | BuffFlag.NO_DURATION);
            await lover.AddBuffAsync(marriageBuff);
            return;
        }

        Buff weedingPlayerBuff = playerEntity.BuffComponent.GetBuff(buff);
        await playerEntity.RemoveBuffAsync(true, weedingPlayerBuff);

        Buff weedingLoverBuff = lover.BuffComponent.GetBuff(buff);
        await lover.RemoveBuffAsync(true, weedingLoverBuff);
    }
}