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
    public class BCardFrozenDebuffHandler : IBCardEffectAsyncHandler
    {
        private readonly IRandomGenerator _randomGenerator;
        private readonly IScheduler _scheduler;
        private readonly IMonsterEntityFactory _monsterEntityFactory;
        private readonly IAsyncEventPipeline _eventPipeline;
        
        public BCardFrozenDebuffHandler(IRandomGenerator randomGenerator, IScheduler scheduler, IMonsterEntityFactory monsterEntityFactory, IAsyncEventPipeline eventPipeline)
        {
            _randomGenerator = randomGenerator;
            _scheduler = scheduler;
            _monsterEntityFactory = monsterEntityFactory;
            _eventPipeline = eventPipeline;
        }

        public BCardType HandledType => BCardType.FrozenDebuff;

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
                case (byte)AdditionalTypes.FrozenDebuff.RecoverHpWithFuelPoints:
                {
                    if (sender is not IPlayerEntity player)
                    {
                        return;
                    }
                    
                    if (player.EnergyBar < firstData)
                    {
                        return;
                    }
                    
                    double totalFuelPoints = player.EnergyBar;
                    
                    double hpToRecoverPercentage = totalFuelPoints / firstData * secondData;
                    int hpToRecover = (int)(player.MaxHp * (hpToRecoverPercentage * 0.01));
                    
                    player.UpdateEnergyBar(-(int)totalFuelPoints).ConfigureAwait(false).GetAwaiter().GetResult();
                    
                    player.Hp += hpToRecover;
                    player.BroadcastHeal(hpToRecover);
                }
                    break;
            }
        }
    }
}