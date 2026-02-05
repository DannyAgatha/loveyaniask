using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.CharacterLifetimeStats;

public class TotalMonstersKilledEventHandler : IAsyncEventProcessor<MonsterDeathEvent>
{
    public async Task HandleAsync(MonsterDeathEvent e, CancellationToken cancellation)
    {
        switch (e.Killer)
        {
            case IPlayerEntity player:
                player.LifetimeStats.TotalMonstersKilled++;
                
                if (player.PrivateMapInstanceInfo is not null)
                {
                    player.PrivateMapInstanceInfo.MonstersKilled++;
                }
                
                break;

            case IMateEntity mate:
                mate.Owner.LifetimeStats.TotalMonstersKilled++;
                
                if (mate.Owner.PrivateMapInstanceInfo is not null)
                {
                    mate.Owner.PrivateMapInstanceInfo.MonstersKilled++;
                }
                
                break;
            case IMonsterEntity monster:
                if (!monster.SummonerId.HasValue || monster.IsMateTrainer || monster.IsSparringMonster)
                {
                    break;
                }

                if (monster.SummonerType != VisualType.Player)
                {
                    break;
                }

                IClientSession summoner = monster.MapInstance.GetCharacterById(monster.SummonerId.Value)?.Session;
                if (summoner == null)
                {
                    break;
                }

                summoner.PlayerEntity.LifetimeStats.TotalMonstersKilled++;
                
                if (summoner.PlayerEntity.PrivateMapInstanceInfo is not null)
                {
                    summoner.PlayerEntity.PrivateMapInstanceInfo.MonstersKilled++;
                }
                
                break;
        }
    }
}