using System.Threading.Tasks;
using WingsEmu.Game._NpcDialog;
using WingsEmu.Game._NpcDialog.Event;
using WingsEmu.Game.Act5;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.NpcDialogs.SP5_SP6;

public class Act5ItemCraftingHandler : INpcDialogAsyncHandler
{
    public NpcRunType[] NpcRunTypes => new[]
    {
        NpcRunType.GRENIGAS_SEAL_PRODUCTION, NpcRunType.ICE_FLOWERS_10_PRODUCTION,
        NpcRunType.MAGIC_CAMEL_BOX_PRODUCTION, NpcRunType.MAGIC_CAMEL_PRODUCTION,
        NpcRunType.DRACO_CLAW_SP5_PERF_STON_PRODUCTION, NpcRunType.DRACO_CLAW_SP5_PRODUCTION,
        NpcRunType.DRACO_SEAL_PRODUCTION, NpcRunType.GLACERUS_MANE_SP6_PERF_STONE_PRODUCTION,
        NpcRunType.GLACERUS_MANE_SP6_PRODUCTION, NpcRunType.GLACERUS_SEAL_PRODUCTION
    };

    public async Task Execute(IClientSession session, NpcDialogEvent e)
    {

        if (session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsInExchange()) return;

        await session.EmitEventAsync(new Act5OpenNpcRunEvent
        {
            NpcRunType = e.NpcRunType,
            IsConfirm = e.Confirmation.HasValue
        });
    }
}