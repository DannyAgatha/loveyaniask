using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class GetDefaultMorphEventHandler : IAsyncEventProcessor<GetDefaultMorphEvent>
{
    public async Task HandleAsync(GetDefaultMorphEvent e, CancellationToken cancellation)
    {
        IPlayerEntity character = e.Sender.PlayerEntity;

        if (character.IsMorphed)
        {
            character.IsMorphed = false;
            character.Morph = 0;

            e.Sender.BroadcastCMode();
            return;
        }

        if (!character.UseSp && character.Morph != 0)
        {
            character.Morph = 0;
            e.Sender.BroadcastCMode();
            return;
        }

        GameItemInstance sp = character.Specialist;
        if (sp == null)
        {
            return;
        }

        if (sp.HoldingVNum.HasValue)
        {
            character.Morph = sp.HoldingVNum.Value;
        }
        else
        {
            character.Morph = sp.GameItem.Morph;
            character.PreviousMorph = null;
        }
        character.MorphUpgrade = sp.Upgrade;
        character.MorphUpgrade2 = sp.Design;
        e.Sender.BroadcastCMode();
    }
}