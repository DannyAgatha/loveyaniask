using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle.Event;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class RainbowBattleCaptureFlagGuriHandler : IGuriHandler
{
    public long GuriEffectId => 504;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        long npcId = e.Data;

        if (session.CurrentMapInstance is not { MapInstanceType: MapInstanceType.RainbowBattle })
        {
            return;
        }

        if (!session.PlayerEntity.RainbowBattleComponent.IsInRainbowBattle)
        {
            return;
        }

        INpcEntity npcEntity = session.CurrentMapInstance.GetNpcById(npcId);
        if (npcEntity == null)
        {
            return;
        }

        await session.EmitEventAsync(new RainbowBattleCaptureFlagEvent
        {
            NpcEntity = npcEntity,
            IsConfirm = true
        });
    }
}