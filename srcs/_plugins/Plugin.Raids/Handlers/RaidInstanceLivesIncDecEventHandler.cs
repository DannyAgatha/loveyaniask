using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Enum;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids;

public class RaidInstanceLivesIncDecEventHandler : IAsyncEventProcessor<RaidInstanceLivesIncDecEvent>
{
    private readonly IAsyncEventPipeline _eventPipeline;

    public RaidInstanceLivesIncDecEventHandler(IAsyncEventPipeline eventPipeline) => _eventPipeline = eventPipeline;

    public async Task HandleAsync(RaidInstanceLivesIncDecEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.Raid.Instance == null)
        {
            return;
        }

        e.Sender.PlayerEntity.Raid.Instance.IncreaseOrDecreaseLives(e.Amount);
        await e.Sender.EmitEventAsync(new RaidRevivedEvent { RestoredLife = e.Amount > 0 });
        if (e.Sender.PlayerEntity.Raid.Instance.Lives < 0)
        {
            await _eventPipeline.ProcessEventAsync(new RaidInstanceFinishEvent(e.Sender.PlayerEntity.Raid, RaidFinishType.NoLivesLeft), cancellation);
        }

        foreach (IClientSession member in e.Sender.PlayerEntity.Raid.Members)
        {
            member.RefreshRaidMemberList(e.Sender.PlayerEntity.Raid.IsSpecialRaid());
            member.SendRaidmbf();
        }
    }
}