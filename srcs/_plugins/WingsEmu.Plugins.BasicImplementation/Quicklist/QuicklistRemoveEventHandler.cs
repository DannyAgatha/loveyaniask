using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.Game._enum;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quicklist;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace NosEmu.Plugins.BasicImplementations.Quicklist;

public class QuicklistRemoveEventHandler : IAsyncEventProcessor<QuicklistRemoveEvent>
{
    public async Task HandleAsync(QuicklistRemoveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        short tab = e.Tab;
        short slot = e.Slot;
        int morphId = session.PlayerEntity.UseSp ? session.PlayerEntity.Specialist?.GameItem.Morph ?? 0 : 0;
        
        if (session.PlayerEntity.Class == ClassType.MartialArtist && session.PlayerEntity.Morph == (byte)MorphType.DraconicFistTransformed ||
            session.PlayerEntity.Morph == (byte)MorphType.DraconicFist)
        {
            morphId = (int)MorphType.DraconicFistTransformed;
        }

        CharacterQuicklistEntryDto quicklist = session.PlayerEntity.QuicklistComponent.GetQuicklistByTabSlotAndMorph(tab, slot, morphId);
        if (quicklist == null)
        {
            return;
        }

        e.Sender.PlayerEntity.QuicklistComponent.RemoveQuicklist(tab, slot, morphId);
        e.Sender.SendEmptyQuicklistSlot(tab, slot);
    }
}