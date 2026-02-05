using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardHideHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly ISpyOutManager _spyOutManager;

    public BCardHideHandler(IAsyncEventPipeline eventPipeline, ISpyOutManager spyOutManager, INpcMonsterManager npcMonsterManager)
    {
        _eventPipeline = eventPipeline;
        _spyOutManager = spyOutManager;
        _npcMonsterManager = npcMonsterManager;
    }

    public BCardType HandledType => BCardType.FalconSkill;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        byte subType = ctx.BCard.SubType;

        switch (subType)
        {
            case (byte)AdditionalTypes.FalconSkill.Hide:
                if (target is not IPlayerEntity character)
                {
                    return;
                }

                character.CharacterInvisible(true);
                break;
            case (byte)AdditionalTypes.FalconSkill.Ambush:
                if (target is not IPlayerEntity characterAmbush)
                {
                    return;
                }

                characterAmbush.CharacterInvisible();

                break;
            case (byte)AdditionalTypes.FalconSkill.CausingChanceLocation:
                var summons = new List<ToSummon>();
                short x = ctx.Position.X;
                short y = ctx.Position.Y;

                if (x == 0 || y == 0)
                {
                    x = sender.PositionX;
                    y = sender.PositionY;
                }

                IMonsterData monsterData = _npcMonsterManager.GetNpc(ctx.BCard.SecondData);
                if (monsterData == null)
                {
                    return;
                }

                summons.Add(new ToSummon
                {
                    VNum = monsterData.MonsterVNum,
                    SpawnCell = new Position(x, y),
                    IsMoving = monsterData.CanWalk,
                    SummonChance = (byte)ctx.BCard.FirstData,
                    IsHostile = monsterData.RawHostility != (int)HostilityType.NOT_HOSTILE,
                    RemoveTick = true
                });

                _eventPipeline.ProcessEventAsync(new MonsterSummonEvent(sender.MapInstance, summons, sender)).ConfigureAwait(false).GetAwaiter().GetResult();
                break;
            case (byte)AdditionalTypes.FalconSkill.FalconFollowing:
                if (sender is not IPlayerEntity spyOutCharacter)
                {
                    return;
                }

                _spyOutManager.AddSpyOutSkill(spyOutCharacter.Id, spyOutCharacter.LastEntity.Item2, spyOutCharacter.LastEntity.Item1);
                spyOutCharacter.SpyOutStart = DateTime.UtcNow;
                break;
        }
    }
}