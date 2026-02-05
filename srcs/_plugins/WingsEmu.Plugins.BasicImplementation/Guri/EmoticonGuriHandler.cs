using System;
using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class EmoticonGuriHandler : IGuriHandler
{
    public long GuriEffectId => 10;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        if (!e.User.HasValue)
        {
            return;
        }

        int effect;

        if (e.Data >= 973 && e.Data <= 999)
        {
            effect = e.Data + 4099;
        }
        else if (e.Data == 1000)
        {
            effect = e.Data + 4116;
        }
        else if (e.Data >= 9000 && e.Data <= 9028)
        {
            effect = e.Data - 3883;
        }
        else
        {
            return;
        }

        int id = Convert.ToInt32(e.User.Value);

        if (id == session.PlayerEntity.Id)
        {
            session.BroadcastEffect(effect, new EmoticonsBroadcast());
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMate(s => s.Id == id);
        mateEntity?.MapInstance.Broadcast(mateEntity.GenerateEffectPacket(effect), new EmoticonsBroadcast());
    }
}