// NosEmu
// 


using System;
using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardSummonSkillHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly INpcMonsterManager _manager;
    private readonly IRandomGenerator _randomGenerator;

    public BCardSummonSkillHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline eventPipeline, INpcMonsterManager manager)
    {
        _randomGenerator = randomGenerator;
        _eventPipeline = eventPipeline;
        _manager = manager;
    }

    public BCardType HandledType => BCardType.SummonSkill;

    public void Execute(IBCardEffectContext ctx)
    {
        if (ctx.Sender == null)
        {
            return;
        }

        if (ctx.Target == null)
        {
            return;
        }

        IBattleEntity sender = ctx.Sender;
        byte subType = ctx.BCard.SubType;

        int firstData = ctx.BCard.FirstData;
        int secondData = ctx.BCard.SecondData;
        int procChance = ctx.BCard.ProcChance;

        var summons = new List<ToSummon>();

        Position entityPosition = sender.Position;

        switch (subType)
        {
            case (byte)AdditionalTypes.SummonSkill.BearTransformation:
            {
                if (sender is not IPlayerEntity player)
                {
                    return;
                }

                if (player.Morph == (byte)MorphType.FlameDruid && !ctx.BCard.CardId.HasValue)
                {
                    break;
                }

                player.Morph = (byte)MorphType.FlameDruidBearStance;
                player.IsFlameDruidTransformed = true;
                player.Session.BroadcastCMode();
                player.Session.BroadcastEffect(EffectType.Transform);
                player.Session.SendCancelPacket(CancelType.InCombatMode);
            }
                break;
            
            case (byte)AdditionalTypes.SummonSkill.RevertTransformation:
            {
                if (sender is not IPlayerEntity player)
                {
                    return;
                }

                if (player.Morph == (byte)MorphType.FlameDruid && !ctx.BCard.CardId.HasValue)
                {
                    break;
                }

                player.Morph = (byte)MorphType.FlameDruid;
                player.IsFlameDruidTransformed = false;
                player.Session.BroadcastCMode();
                player.Session.BroadcastEffect(EffectType.Transform);
                player.Session.SendCancelPacket(CancelType.InCombatMode);
            }
                break;
        }
    }
}