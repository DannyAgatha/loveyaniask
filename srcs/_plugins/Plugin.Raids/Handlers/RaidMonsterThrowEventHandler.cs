using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Items;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;
using WingsEmu.Game.Raids.Events;

namespace Plugin.Raids;

public class RaidMonsterThrowEventHandler : IAsyncEventProcessor<RaidMonsterThrowEvent>
{
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IRaidModeConfiguration _raidModeConfiguration;
    
    public RaidMonsterThrowEventHandler(IRandomGenerator randomGenerator, IGameItemInstanceFactory gameItemInstanceFactory, IRaidModeConfiguration raidModeConfiguration)
    {
        _randomGenerator = randomGenerator;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _raidModeConfiguration = raidModeConfiguration;
    }

    public async Task HandleAsync(RaidMonsterThrowEvent e, CancellationToken cancellation)
    {
        const byte minimumDistance = 0;
        const byte maximumDistance = 10;
        IClientSession session = e.MonsterEntity.MapInstance.Sessions.FirstOrDefault(s => s.PlayerEntity.IsInRaidParty);
        RaidParty raidParty = session?.PlayerEntity.Raid;
        
        if (raidParty != null)
        {
            RaidModeType raidMode = _raidModeConfiguration.GetModeType(raidParty.Type, raidParty.ModeType);
        
            int length = e.Drops.Count;

            for (int i = 0; i < e.ItemDropsAmount; i++)
            {
                int number = _randomGenerator.RandomNumber(0, length);
                Drop drop = e.Drops[number];
                ThrowEvent(e.MonsterEntity, drop.ItemVNum, drop.Amount * raidMode.RewardsMultiplier.ItemsAmount, minimumDistance, maximumDistance);
            }

            for (int i = 0; i < e.GoldDropsAmount; i++)
            {
                int gold = _randomGenerator.RandomNumber(e.GoldDropRange.Minimum * raidMode.RewardsMultiplier.Gold, 
                    e.GoldDropRange.Maximum * raidMode.RewardsMultiplier.Gold + 1);
                ThrowEvent(e.MonsterEntity, (int)ItemVnums.GOLD, gold, minimumDistance, maximumDistance);
            }
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