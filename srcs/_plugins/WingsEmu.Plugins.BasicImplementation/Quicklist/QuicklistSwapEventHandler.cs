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

public class QuicklistSwapEventHandler : IAsyncEventProcessor<QuicklistSwapEvent>
{
    public async Task HandleAsync(QuicklistSwapEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;

        short tab = e.Tab;
        short fromSlot = e.FromSlot;
        short toSlot = e.ToSlot;
        int morphId = session.PlayerEntity.UseSp ? session.PlayerEntity.Specialist?.GameItem.Morph ?? 0 : 0;

        if (session.PlayerEntity.Class == ClassType.MartialArtist && session.PlayerEntity.Morph == (byte)MorphType.DraconicFistTransformed || 
            session.PlayerEntity.Morph == (byte)MorphType.DraconicFist)
        {
            morphId = (int)MorphType.DraconicFistTransformed;
        }
        
        CharacterQuicklistEntryDto from = session.PlayerEntity.QuicklistComponent.GetQuicklistByTabSlotAndMorph(tab, fromSlot, morphId);
        if (from == null)
        {
            // incorrect packet
            return;
        }

        session.PlayerEntity.QuicklistComponent.RemoveQuicklist(tab, fromSlot, morphId);
        from.QuicklistSlot = toSlot;

        CharacterQuicklistEntryDto to = session.PlayerEntity.QuicklistComponent.GetQuicklistByTabSlotAndMorph(tab, toSlot, morphId);
        if (to != null)
        {
            session.PlayerEntity.QuicklistComponent.RemoveQuicklist(tab, toSlot, morphId);
            to.QuicklistSlot = fromSlot;
            session.PlayerEntity.QuicklistComponent.AddQuicklist(to);
            session.SendQuicklistSlot(to);
        }
        else
        {
            session.SendEmptyQuicklistSlot(tab, fromSlot);
        }

        session.PlayerEntity.QuicklistComponent.AddQuicklist(from);
        session.SendQuicklistSlot(from);
    }
}