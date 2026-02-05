// NosEmu
// 
// Developed by NosWings Team

using System;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardFourthGlacernonFamilyRaidHandler : IBCardEffectAsyncHandler
{
    public BCardType HandledType => BCardType.FourthGlacernonFamilyRaid;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);
        BCardDTO bCard = ctx.BCard;
        int damage = ctx.DamageDealt;

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.FourthGlacernonFamilyRaid.RemoveRandomDebuff:
            {
                if (target.IsSameEntity(sender))
                {
                    return;
                }

                if (!sender.IsSucceededChance(secondData))
                {
                    return;
                }

                sender.RemoveRandomNegativeBuff(firstData).ConfigureAwait(false).GetAwaiter().GetResult();
            }
                break;
            case (byte)AdditionalTypes.FourthGlacernonFamilyRaid.HPIncreasedDamageGiven:
            {
                if (!sender.IsAlive())
                {
                    return;
                }

                int missingHp = 100 - sender.GetHpPercentage();
                double hpToIncrease = damage * ((double)missingHp / firstData * 0.01);
                sender.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = sender,
                    HpHeal = (int)hpToIncrease
                });
            }
                break;
            
            case (byte)AdditionalTypes.FourthGlacernonFamilyRaid.IncreaseMovementSpeedTick:
                {
                    if (sender is IMonsterEntity monsterEntity)
                    {
                        monsterEntity.RefreshStats();
                    }

                    IClientSession session = (sender as IPlayerEntity)?.Session;
                    if (session?.PlayerEntity == null)
                    {
                        return;
                    }

                    if (sender.BCardDataComponent.IncreaseSpeedTick.HasValue)
                    {
                        sender.BCardDataComponent.IncreaseSpeedTick++;
                    }
                    session.PlayerEntity.LastSpeedChange = DateTime.UtcNow;
                    session.RefreshStatChar();
                    session.RefreshStat();
                    session.SendCondPacket();
                }
                break;
            case (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DecreaseMovementSpeedTick:
                {
                    if (sender is IMonsterEntity monsterEntity)
                    {
                        monsterEntity.RefreshStats();
                    }

                    IClientSession session = (sender as IPlayerEntity)?.Session;
                    if (session?.PlayerEntity == null)
                    {
                        return;
                    }
                    if (sender.BCardDataComponent.DecreaseSpeedTick.HasValue)
                    {
                        sender.BCardDataComponent.DecreaseSpeedTick++;
                    }
                    session.PlayerEntity.LastSpeedChange = DateTime.UtcNow;
                    session.RefreshStatChar();
                    session.RefreshStat();
                    session.SendCondPacket();
                }
                break;
            
            
        }
    }
}