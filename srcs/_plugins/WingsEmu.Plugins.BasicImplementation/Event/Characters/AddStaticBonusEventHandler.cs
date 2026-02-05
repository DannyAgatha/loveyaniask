using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class AddStaticBonusEventHandler : IAsyncEventProcessor<AddStaticBonusEvent>
{
    public async Task HandleAsync(AddStaticBonusEvent e, CancellationToken cancellation)
    {
        e.Sender.PlayerEntity.AddStaticBonus(e.StaticBonusDto);
        e.Sender.SendStaticBonuses();
    }
}