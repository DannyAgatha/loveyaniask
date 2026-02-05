using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.SnackFood.Events;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class AddAdditionalHpMpEventHandler : IAsyncEventProcessor<AddAdditionalHpMpEvent>
{
    public async Task HandleAsync(AddAdditionalHpMpEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IPlayerEntity character = session.PlayerEntity;

        if (!character.IsAlive())
        {
            return;
        }

        if (e.Hp != 0)
        {
            int maxHpOverflow = character.MaxHp * e.MaxHpPercentage / 100;
            if (character.AdditionalHp + e.Hp > maxHpOverflow)
            {
                if (character.AdditionalHp < maxHpOverflow)
                {
                    character.AdditionalHp = maxHpOverflow;
                }
            }
            else
            {
                character.AdditionalHp += e.Hp;
            }
        }

        if (e.Mp != 0)
        {
            int maxMpOverflow = character.MaxMp * e.MaxMpPercentage / 100;

            if (character.AdditionalMp + e.Mp > maxMpOverflow)
            {
                if (character.AdditionalMp < maxMpOverflow)
                {
                    character.AdditionalMp = maxMpOverflow;
                }
            }
            else
            {
                character.AdditionalMp += e.Mp;
            }
        }

        session.SendGuriPacket(4);
    }
}