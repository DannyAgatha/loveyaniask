// NosEmu
// 


using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardDarkCloneSummonHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IRandomGenerator _random;

    public BCardDarkCloneSummonHandler(IRandomGenerator random, IAsyncEventPipeline eventPipeline, INpcMonsterManager npcMonsterManager)
    {
        _random = random;
        _eventPipeline = eventPipeline;
        _npcMonsterManager = npcMonsterManager;
    }

    public BCardType HandledType => BCardType.DarkCloneSummon;

    public async void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstData;
        int secondData = ctx.BCard.SecondData;

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.DarkCloneSummon.SummonDarkCloneChance:
                if (_random.RandomNumber() > firstData)
                {
                    return;
                }

                var monsters = new List<ToSummon>();
                for (int i = 0; i < secondData; i++)
                {
                    int vnum = 2112 + i;

                    IMonsterData monsterData = _npcMonsterManager.GetNpc(vnum);
                    if (monsterData == null)
                    {
                        continue;
                    }

                    short x = sender.Position.X;
                    short y = sender.Position.Y;

                    x += (short)_random.RandomNumber(-1, 2);
                    y += (short)_random.RandomNumber(-1, 2);
                    monsters.Add(new ToSummon
                    {
                        VNum = monsterData.MonsterVNum,
                        SpawnCell = new Position(x, y),
                        IsMoving = monsterData.CanWalk,
                        IsHostile = monsterData.RawHostility != (int)HostilityType.NOT_HOSTILE
                    });
                }

                await _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(sender.MapInstance, monsters, sender));
                break;
        }
    }
}