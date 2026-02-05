using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Act4.Event;

public class Act4DungeonMonsterThrowEventHandler : IAsyncEventProcessor<RaidMonsterThrowEvent>
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IRandomGenerator _randomGenerator;

    public Act4DungeonMonsterThrowEventHandler(IRandomGenerator randomGenerator, IGameItemInstanceFactory gameItemInstanceFactory)
    {
        _randomGenerator = randomGenerator;
        _gameItemInstanceFactory = gameItemInstanceFactory;
    }

    public async Task HandleAsync(RaidMonsterThrowEvent e, CancellationToken cancellation)
    {
        const byte minimumDistance = 0;
        const byte maximumDistance = 10;

        int length = e.Drops.Count;

        for (int i = 0; i < e.ItemDropsAmount; i++)
        {
            int number = _randomGenerator.RandomNumber(0, length);
            Drop drop = e.Drops[number];
            ThrowEvent(e.MonsterEntity, drop.ItemVNum, drop.Amount, minimumDistance, maximumDistance);
        }

        for (int i = 0; i < e.GoldDropsAmount; i++)
        {
            int gold = _randomGenerator.RandomNumber(e.GoldDropRange.Minimum, e.GoldDropRange.Maximum + 1);
            ThrowEvent(e.MonsterEntity, (int)ItemVnums.GOLD, gold, minimumDistance, maximumDistance);
        }
    }

    private void ThrowEvent(IBattleEntity battleEntity, int itemVNum, int quantity, byte minimumDistance, byte maximumDistance)
    {
        GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(itemVNum, quantity);

        short rndX = -1;
        short rndY = -1;
        int count = 0;
        while ((rndX == -1 || rndY == -1 || battleEntity.MapInstance.IsBlockedZone(rndX, rndY)) && count < 100)
        {
            rndX = (short)(battleEntity.PositionX + _randomGenerator.RandomNumber(minimumDistance, maximumDistance + 1) * (_randomGenerator.RandomNumber(0, 2) * 2 - 1));
            rndY = (short)(battleEntity.PositionY + _randomGenerator.RandomNumber(minimumDistance, maximumDistance + 1) * (_randomGenerator.RandomNumber(0, 2) * 2 - 1));
            count++;
        }

        var position = new Position(rndX, rndY);

        var item = new MonsterMapItem(position.X, position.Y, newItem, battleEntity.MapInstance);

        battleEntity.MapInstance.AddDrop(item);
        battleEntity.BroadcastThrow(item);
    }
}