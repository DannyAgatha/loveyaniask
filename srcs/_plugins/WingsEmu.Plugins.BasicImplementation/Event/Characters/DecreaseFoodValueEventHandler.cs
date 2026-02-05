using PhoenixLib.Events;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game._enum;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Extensions;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters
{
    public class DecreaseFoodValueEventHandler(IBuffFactory buffFactory) : IAsyncEventProcessor<DecreaseFoodValueEvent>
    {
        public async Task HandleAsync(DecreaseFoodValueEvent e, CancellationToken cancellation)
        {
            IPlayerEntity player = e.Sender.PlayerEntity;

            player.FoodValue -= e.Value;

            if (player.FoodValue < 0)
            {
                player.FoodValue = 0;
            }

            player.Session.SendPacket(player.Session.GenerateFoodPacket());
            
            (int MinValue, int MaxValue, int BuffId)[] ranges =
            [
                (1, 25000, (short)BuffVnums.FULLY_BELLY_I),
                (25001, 75000, (short)BuffVnums.FULLY_BELLY_II),
                (75001, 425000, (short)BuffVnums.FULLY_BELLY_III)
            ];

            bool isBuffApplied = false;

            foreach ((int MinValue, int MaxValue, int BuffId) range in ranges)
            {
                if (player.FoodValue < range.MinValue || player.FoodValue > range.MaxValue)
                {
                    continue;
                }

                if (!player.BuffComponent.HasBuff(range.BuffId))
                {
                    await player.AddBuffAsync(buffFactory.CreateBuff(range.BuffId, player, BuffFlag.BIG | BuffFlag.NO_DURATION));
                    
                    foreach ((int MinValue, int MaxValue, int BuffId) otherRange in ranges.Where(r => r.BuffId != range.BuffId))
                    {
                        await player.RemoveBuffAsync(otherRange.BuffId, true);
                    }
                }

                isBuffApplied = true;
                break;
            }
            
            if (!isBuffApplied && player.FoodValue <= 0)
            {
                foreach ((int MinValue, int MaxValue, int BuffId) range in ranges)
                {
                    if (player.BuffComponent.HasBuff(range.BuffId))
                    {
                        await player.RemoveBuffAsync(range.BuffId, true);
                    }
                }
            }
        }

    }
}