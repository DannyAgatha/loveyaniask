using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Items.Event;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Items;

public class SpecialistSkinPotionEventHandler : IAsyncEventProcessor<SpecialistSkinPotionEvent>
{
    private readonly SpecialistSkinConfiguration _specialistSkinConfiguration;

    public SpecialistSkinPotionEventHandler(SpecialistSkinConfiguration specialistSkinConfiguration) => _specialistSkinConfiguration = specialistSkinConfiguration;

    public async Task HandleAsync(SpecialistSkinPotionEvent e, CancellationToken cancellation)
    {
        InventoryItem item = e.Item;
        IClientSession session = e.Sender;
        GameItemInstance specialist = session.PlayerEntity.Specialist;

        SpecialistSkin specialistSkin = _specialistSkinConfiguration.SpecialistSkins.FirstOrDefault(s => s.SpVnum == specialist.ItemVNum);

        if (specialistSkin == null)
        {
            return;
        }

        if (specialist.HoldingVNum.HasValue && specialist.HoldingVNum == specialistSkin.SpSkinMorph)
        {
            return;
        }

        specialist.HoldingVNum = specialistSkin.SpSkinMorph;
        await session.RemoveItemFromInventory(item: item);
    }
}