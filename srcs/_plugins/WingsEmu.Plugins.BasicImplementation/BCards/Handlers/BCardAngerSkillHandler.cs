using System;
using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using WingsEmu.Game;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.ServerPackets.Battle;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers
{
    public class BCardAngerSkillHandler : IBCardEffectAsyncHandler
    {
        private readonly IRandomGenerator _randomGenerator;
        private readonly IScheduler _scheduler;
        private readonly IMonsterEntityFactory _monsterEntityFactory;
        private readonly IAsyncEventPipeline _eventPipeline;
        
        public BCardAngerSkillHandler(IRandomGenerator randomGenerator, IScheduler scheduler, IMonsterEntityFactory monsterEntityFactory, IAsyncEventPipeline eventPipeline)
        {
            _randomGenerator = randomGenerator;
            _scheduler = scheduler;
            _monsterEntityFactory = monsterEntityFactory;
            _eventPipeline = eventPipeline;
        }

        public BCardType HandledType => BCardType.AngerSkill;

        public void Execute(IBCardEffectContext ctx)
        {
            IBattleEntity sender = ctx.Sender;
            IBattleEntity target = ctx.Target;
            SkillInfo skill = ctx.Skill;
            SuPacketHitMode hitMode = ctx.HitMode;
            int damageDealt = ctx.DamageDealt;
            int firstData = ctx.BCard.FirstDataValue(sender.Level);
            int secondData =  ctx.BCard.SecondDataValue(sender.Level);
            byte subType = ctx.BCard.SubType;

            switch (subType)
            {
                case (byte)AdditionalTypes.AngerSkill.AttackDelayShort:

                    if (target is not IPlayerEntity player)
                    {
                        return;
                    }
                    
                    player.BlockAllAttack = true;
                        
                    _scheduler.Schedule(TimeSpan.FromSeconds(firstData), () =>
                    {
                        player.BlockAllAttack = false;
                        player.Session.RefreshStat();
                    });

                    break;
            }
        }
    }
}