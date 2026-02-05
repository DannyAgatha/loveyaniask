using PhoenixLib.Events;
using System.Collections.Generic;
using System.Linq;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardDTeamArenaBuffHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;

    public BCardDTeamArenaBuffHandler(IBuffFactory buffFactory) => _buffFactory = buffFactory;

    public BCardType HandledType => BCardType.TeamArenaBuff;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        BCardDTO bCard = ctx.BCard;

        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);

        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.TeamArenaBuff.HealHPByPercentageHPMax:
            {
                if (sender.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }
                
                int heal = (int)(sender.MaxHp * firstDataValue * 0.01);

                heal = heal < secondDataValue ? secondDataValue : heal;

                sender.EmitEvent(new BattleEntityHealEvent
                {
                    Entity = sender,
                    HpHeal = heal
                });
            }
                break;
            case (byte)AdditionalTypes.TeamArenaBuff.HealMPByPercentageMPMax:
            {
                if (sender.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }
                
                int heal = (int)(sender.MaxMp * firstDataValue * 0.01);

                heal = heal < secondDataValue ? secondDataValue : heal;

                if (sender.Mp + heal > sender.MaxMp)
                {
                    sender.Mp = sender.MaxMp;
                }
                else
                {
                    sender.Mp += heal;
                }
            }
                break;
        }
        
        if (sender is not IPlayerEntity playerEntity)
        {
            return;
        }

        playerEntity.Session.RefreshStat();
    }
}