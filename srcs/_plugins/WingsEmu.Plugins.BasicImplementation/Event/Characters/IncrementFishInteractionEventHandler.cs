using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class IncrementFishInteractionEventHandler : IAsyncEventProcessor<IncrementFishInteractionEvent>
{
    public Task HandleAsync(IncrementFishInteractionEvent e, CancellationToken cancellation)
    {
        IPlayerEntity character = e.Sender.PlayerEntity;

        character.FishInteraction++;

        if (character.FishInteraction > 32000)
        {
            character.FishInteraction = 1;
        }

        return Task.CompletedTask;
    }
}