using System;
using System.Threading.Tasks;
using WingsEmu.Game._enum;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class DanceGuriHandler : IGuriHandler
{
    public long GuriEffectId => 5;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        int value = Convert.ToInt32(e.Value);

        switch (value)
        {
            case -2:
                if (e.Packet[5] != "100")
                {
                    return;
                }

                session.PlayerEntity.LastUpgradePet = DateTime.Now;
                break;
            case 5:
                if (e.Packet.Length < 5)
                {
                    return;
                }

                int isProcessing = Convert.ToInt32(e.Packet[5]);

                if (isProcessing == -1)
                {
                    session.PlayerEntity.LastUnfreezedPlayer = DateTime.MinValue;
                }

                break;
            case 245:
                DateTime now = DateTime.UtcNow;
                if (session.PlayerEntity.LastRainbowArrowEffect.AddSeconds(2) > now)
                {
                    return;
                }

                session.PlayerEntity.LastRainbowArrowEffect = now;
                await session.PlayerEntity.RemoveInvisibility();

                session.BroadcastEffect(EffectType.YellowArrow);
                break;
        }
    }
}