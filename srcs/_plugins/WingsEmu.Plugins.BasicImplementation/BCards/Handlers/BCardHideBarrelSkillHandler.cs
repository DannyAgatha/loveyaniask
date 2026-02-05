using PhoenixLib.Events;
using WingsEmu.DTOs.BCards;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardHideBarrelSkillHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEvent;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IBuffFactory _buffFactory;

    public BCardHideBarrelSkillHandler(IRandomGenerator randomGenerator, IAsyncEventPipeline asyncEvent, IBuffFactory buffFactory)
    {
        _randomGenerator = randomGenerator;
        _asyncEvent = asyncEvent;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.HideBarrelSkill;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        SkillInfo skill = ctx.Skill;
        BCardDTO bCard = ctx.BCard;
        int firstData = ctx.BCard.FirstDataValue(sender.Level);
        int secondData = ctx.BCard.SecondDataValue(sender.Level);
        int damage = ctx.DamageDealt;

        switch (bCard.SubType)
        {
            case (byte)AdditionalTypes.HideBarrelSkill.RestoreHpPerBuff:
            {
                int buffCount = sender.BuffComponent.GetAllBuffs().Count;

                int toHeal = firstData * buffCount;

                if (toHeal > secondData)
                {
                    toHeal = secondData;
                }
                
                sender.EmitEvent(new BattleEntityHealEvent
                {
                    Entity = sender,
                    HpHeal = toHeal
                });
            }
                break;
            case (byte)AdditionalTypes.HideBarrelSkill.ReducesHpPerBuff:
            {
                int buffCount = sender.BuffComponent.GetAllBuffs().Count;

                int toDamage = firstData * buffCount;

                if (toDamage > secondData)
                {
                    toDamage = secondData;
                }
                
                _asyncEvent.ProcessEventAsync(new EntityDamageEvent
                {
                    Damaged = sender,
                    Damager = sender,
                    Damage = toDamage,
                    CanKill = false,
                    SkillInfo = skill
                });
            }
                break;
        }
    }
}