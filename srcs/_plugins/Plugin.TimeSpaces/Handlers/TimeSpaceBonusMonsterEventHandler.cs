using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game.Entities;
using WingsEmu.Game.TimeSpaces;
using WingsEmu.Game.TimeSpaces.Events;
using WingsEmu.Packets.Enums;

namespace Plugin.TimeSpaces.Handlers;

public class TimeSpaceBonusMonsterEventHandler : IAsyncEventProcessor<TimeSpaceBonusMonsterEvent>
{
    private readonly IRandomGenerator _randomGenerator;

    public TimeSpaceBonusMonsterEventHandler(IRandomGenerator randomGenerator) => _randomGenerator = randomGenerator;

    public async Task HandleAsync(TimeSpaceBonusMonsterEvent e, CancellationToken cancellation)
    {
        IMonsterEntity[] monsters = e.MonsterEntities.Where(x => x.SummonerType is not VisualType.Player).ToArray();
        TimeSpaceSubInstance timeSpaceSubInstance = e.TimeSpaceSubInstance;
        if (!monsters.Any())
        {
            return;
        }

        IMonsterEntity anotherMonster = monsters.FirstOrDefault(x => x.IsBonus);
        if (anotherMonster != null)
        {
            return;
        }

        int randomNumber = _randomGenerator.RandomNumber(monsters.Length);
        IMonsterEntity mob = monsters[randomNumber];
        if (mob == null)
        {
            return;
        }

        mob.IsBonus = true;
        timeSpaceSubInstance.MonsterBonusId = mob.Id;
    }
}