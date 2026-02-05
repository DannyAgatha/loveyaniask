using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Mates;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalDeathsEventHandler : IAsyncEventProcessor<PlayerDeathEvent>
{
    public async Task HandleAsync(PlayerDeathEvent e, CancellationToken cancellation)
    {
        IPlayerEntity character = e.Sender.PlayerEntity;

        if (character == null || character.IsAlive())
        {
            return;
        }

        switch (e.Killer)
        {
            case IPlayerEntity killerCharacter:
                killerCharacter.LifetimeStats.TotalPlayersKilled++;
                character.LifetimeStats.TotalDeathsByPlayer++;
                break;
            case IMonsterEntity:
                character.LifetimeStats.TotalDeathsByMonster++;
                break;
            case IMateEntity mate:
                mate.Owner.LifetimeStats.TotalPlayersKilled++;
                character.LifetimeStats.TotalDeathsByPlayer++;
                break;
        }
    }
}