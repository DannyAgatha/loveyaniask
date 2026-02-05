using System;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardQuestHandler : IBCardEffectAsyncHandler
{
    private readonly IMonsterEntityFactory _monsterEntityFactory;
    private readonly INpcMonsterManager _monsterManager;
    private readonly IRandomGenerator _randomGenerator;

    public BCardQuestHandler(INpcMonsterManager monsterManager, IRandomGenerator randomGenerator, IMonsterEntityFactory monsterEntityFactory)
    {
        _monsterManager = monsterManager;
        _randomGenerator = randomGenerator;
        _monsterEntityFactory = monsterEntityFactory;
    }

    public BCardType HandledType => BCardType.Quest;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int damageDealt = ctx.DamageDealt;
        SkillInfo skillInfo = ctx.Skill;
        BCardDTO bCard = ctx.BCard;
        int firstData = bCard.FirstDataValue(sender.Level);
        int secondData = bCard.SecondDataValue(sender.Level);

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.Quest.SummonMonsterBased:
                {
                    IMonsterData monster;
                    if (ctx.Sender is IMonsterEntity monsterEntity)
                    {
                        monster = monsterEntity;
                    }
                    else
                    {
                        return;
                    }

                    for (int i = 0; i < monster.VNumRequired; i++)
                    {
                        int posX = ctx.Sender.PositionX + _randomGenerator.RandomNumber(-1, 1);
                        int posY = ctx.Sender.PositionY + _randomGenerator.RandomNumber(-1, 1);

                        IMonsterEntity mapMonster = _monsterEntityFactory.CreateMonster(monster.SpawnMobOrColor, ctx.Sender.MapInstance, new MonsterEntityBuilder
                        {
                            IsHostile = true,
                            IsWalkingAround = true
                        });
                        mapMonster.EmitEventAsync(new MapJoinMonsterEntityEvent(mapMonster, (short)posX, (short)posY, true));
                    }
                }
                break;
            case (byte)AdditionalTypes.Quest.HealHpByInflictedDamages:
            {
                if (!sender.IsAlive())
                {
                    return;
                }
                
                if (sender.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }
                
                int healHp = (int)(damageDealt * (firstData * 0.01));
                
                healHp = Math.Min(healHp, secondData);
                
                sender.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = sender,
                    HpHeal = healHp
                });
            }
                break;
            
            case (byte)AdditionalTypes.Quest.HealMpByInflictedDamages:
            {
                if (!sender.IsAlive())
                {
                    return;
                }
                
                if (sender.BCardComponent.HasBCard(BCardType.FourthGlacernonFamilyRaid, (byte)AdditionalTypes.FourthGlacernonFamilyRaid.DisableHPMPRecovery))
                {
                    return;
                }
                
                int healMp = (int)(damageDealt * (firstData * 0.01));
                
                healMp = Math.Min(healMp, secondData);
                
                sender.EmitEventAsync(new BattleEntityHealEvent
                {
                    Entity = sender,
                    MpHeal = healMp
                });
            }
                break;
        }
        
    }
}