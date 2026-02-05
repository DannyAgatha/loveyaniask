using PhoenixLib.Events;
using System;
using System.Linq;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardFuelHeatHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEvent;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;
    private readonly ISkillsManager _skillManager;

    public BCardFuelHeatHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline asyncEvent, IBuffFactory buffFactory, ISkillsManager skillManager)
    {
        _randomGenerator = randomGenerator;
        _asyncEvent = asyncEvent;
        _buffFactory = buffFactory;
        _skillManager = skillManager;
    }

    public BCardType HandledType => BCardType.FuelHeatPoint;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;
        SkillInfo skillInfo = ctx.Skill;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.FuelHeatPoint.ConsumeFuelPointsReceiveEffect:
            {
                if (sender is not IPlayerEntity player)
                {
                    return;
                }

                if (player.EnergyBar <= firstData)
                {
                    return;
                }

                player.LastEnergyRefill = DateTime.UtcNow;
                player.UpdateEnergyBar(-firstData).ConfigureAwait(false).GetAwaiter().GetResult();
                sender.AddBuffAsync(_buffFactory.CreateBuff(secondData, sender)).ConfigureAwait(false).GetAwaiter().GetResult();
            }
                break;
            
            case (byte)AdditionalTypes.FuelHeatPoint.ConsumeFuelPointsInflictTarget:
            {
                if (sender is not IPlayerEntity player)
                {
                    return;
                }

                if (player.EnergyBar <= firstData)
                {
                    return;
                }

                player.LastEnergyRefill = DateTime.UtcNow;
                player.UpdateEnergyBar(-firstData).ConfigureAwait(false).GetAwaiter().GetResult();
                target.AddBuffAsync(_buffFactory.CreateBuff(secondData, target)).ConfigureAwait(false).GetAwaiter().GetResult();
            }
                break;
            
            case (byte)AdditionalTypes.FuelHeatPoint.ReceiveHeatPoints:
                {
                    if (sender is not IPlayerEntity player)
                    {
                        return;
                    }

                    player.LastEnergyRefill = DateTime.UtcNow;
                    player.UpdateEnergyBar(firstData).ConfigureAwait(false).GetAwaiter().GetResult();
                }

                break;
            case (byte)AdditionalTypes.FuelHeatPoint.LoseHeatPoints:
                {
                    if (sender is not IPlayerEntity player)
                    {
                        return;
                    }

                    player.LastEnergyRefill = DateTime.UtcNow;
                    player.UpdateEnergyBar(-firstData).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                break;
            
            case (byte)AdditionalTypes.FuelHeatPoint.ConsumeFuelPointsChanceToResetCooldown:
            {
                if (sender is not IPlayerEntity playerEntity)
                {
                    return;
                }

                if (playerEntity.EnergyBar < firstData)
                {
                    return;
                }

                playerEntity.LastEnergyRefill = DateTime.UtcNow;
                playerEntity.UpdateEnergyBar(-firstData).ConfigureAwait(false).GetAwaiter().GetResult();

                if (!sender.IsSucceededChance(secondData))
                {
                    return;
                }

                skillInfo.Cooldown = 0;
            }
                break;
        }
    }
}