using System;
using WingsEmu.Core;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardHatusHandler : IBCardEffectAsyncHandler
{
    private readonly Range<int> _blue = new()
    {
        Minimum = 18,
        Maximum = 28
    };

    private readonly IDungeonManager _dungeonManager;

    private readonly Range<int> _green = new()
    {
        Minimum = 44,
        Maximum = 54
    };

    private readonly IRandomGenerator _randomGenerator;

    private readonly Range<int> _red = new()
    {
        Minimum = 31,
        Maximum = 41
    };

    public BCardHatusHandler(IRandomGenerator randomGenerator, IDungeonManager dungeonManager)
    {
        _randomGenerator = randomGenerator;
        _dungeonManager = dungeonManager;
    }

    public BCardType HandledType => BCardType.LordHatus;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        BCardDTO bCard = ctx.BCard;
        int damageDealt = ctx.DamageDealt;
        int firstDataValue = bCard.FirstDataValue(sender.Level);
        int secondDataValue = bCard.SecondDataValue(sender.Level);

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.LordHatus.InflictDamageAtLocation:

                int randomNumber = _randomGenerator.RandomNumber(1, 8);
                HatusState newHatusState = new()
                {
                    CastTime = TimeSpan.FromMilliseconds(2 * ctx.Skill.CastTime * 100),
                    DealtDamage = firstDataValue * 0.01
                };

                // for blue
                if ((randomNumber & 1) == 1)
                {
                    newHatusState.BlueAttack = true;
                    newHatusState.BlueX = (short)_randomGenerator.RandomNumber(_blue.Minimum, _blue.Maximum + 1);
                }

                // for red
                if ((randomNumber & 2) == 2)
                {
                    newHatusState.RedAttack = true;
                    newHatusState.RedX = (short)_randomGenerator.RandomNumber(_red.Minimum, _red.Maximum + 1);
                }

                // for green
                if ((randomNumber & 4) == 4)
                {
                    newHatusState.GreenAttack = true;
                    newHatusState.GreenX = (short)_randomGenerator.RandomNumber(_green.Minimum, _green.Maximum + 1);
                }

                _dungeonManager.AddNewHatusState(sender.MapInstance.Id, newHatusState);
                break;
            case (byte)AdditionalTypes.LordHatus.RestoreHpByDamageTaken:
                if (!sender.IsAlive())
                {
                    return;
                }
                
                if (sender.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }

                int healHp = (int)(damageDealt * (firstDataValue * 0.01));

                if (healHp > secondDataValue)
                {
                    healHp = secondDataValue;
                }

                sender.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = sender,
                    HpHeal = healHp
                });
                break;
            
            case (byte)AdditionalTypes.LordHatus.RestoreMpByDamageTaken:
                if (!sender.IsAlive())
                {
                    return;
                }
                
                if (sender.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }

                int healMp = (int)(damageDealt * (firstDataValue * 0.01));

                if (healMp > secondDataValue)
                {
                    healMp = secondDataValue;
                }

                sender.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = sender,
                    MpHeal = healMp
                });
                break;
        }
    }
}