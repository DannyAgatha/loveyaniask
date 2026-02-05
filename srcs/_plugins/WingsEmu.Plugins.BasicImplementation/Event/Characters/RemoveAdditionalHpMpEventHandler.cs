using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class RemoveAdditionalHpMpEventHandler : IAsyncEventProcessor<RemoveAdditionalHpMpEvent>
{
    public async Task HandleAsync(RemoveAdditionalHpMpEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IPlayerEntity character = session.PlayerEntity;

        if (e.Hp > 0)
        {
            character.AdditionalHp = character.AdditionalHp - e.Hp > 0 ? character.AdditionalHp - e.Hp : 0;
        }

        if (e.Mp > 0)
        {
            character.AdditionalMp = character.AdditionalMp - e.Mp > 0 ? character.AdditionalMp - e.Mp : 0;
        }

        session.SendGuriPacket(4);
    }
}