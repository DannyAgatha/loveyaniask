using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters
{
    public class CookingMealEventHandler(IScheduler scheduler, IRandomGenerator randomGenerator) : IAsyncEventProcessor<CookingMealEvent>
    {
        private static Dictionary<int, int[]> GuriType { get; } = new()
        {
            [(int)SkillsVnums.CHOP_INGREDIENTS] = [14, 12],
            [(int)SkillsVnums.ROAST] = [25, 31, 42, 15],
            [(int)SkillsVnums.SIMMER] = [23, 26, 40, 44],
            [(int)SkillsVnums.STIR_FRY] = [24, 30, 41, 45]
        };

        private static bool BasicCheck(CookingMealEvent e)
        {
            if (e.Sender.PlayerEntity.LastRecipeFromChefSp == null)
            {
                return false;
            }

            if (e.Sender.PlayerEntity.LastRecipeFromChefSp.ProducedItemVnum != e.RecipeGeneratedItemVnum)
            {
                return false;
            }

            if (e.Sender.PlayerEntity.LastSkillId != e.SkillVnum)
            {
                return false;
            }

            if (e.SkillVnum != (int)SkillsVnums.CHOP_INGREDIENTS && e.RecipeGeneratedItemAmount != 1)
            {
                return false;
            }

            if (e.SkillVnum == (int)SkillsVnums.CHOP_INGREDIENTS && e.RecipeGeneratedItemAmount != 1 && e.RecipeGeneratedItemAmount != 5)
            {
                return false;
            }

            if (e.Sender.PlayerEntity.LastRecipeFromChefSp.BearingChef == 2 && !e.Sender.PlayerEntity.HasUnlockedDesiredBearing(e.Recipes, 1))
            {
                e.Sender.SendSayi(ChatMessageColorType.PlayerSay, Game18NConstString.CannotCookRequirementNotMet);
                return false;
            }

            if (e.Sender.PlayerEntity.LastRecipeFromChefSp.BearingChef == 3 && !e.Sender.PlayerEntity.HasUnlockedDesiredBearing(e.Recipes, 2))
            {
                e.Sender.SendSayi(ChatMessageColorType.PlayerSay, Game18NConstString.CannotCookRequirementNotMet);
                return false;
            }
            
            return true;
        }

        private async Task HandlePreparation(CookingMealEvent e)
        {
            await e.Sender.EmitEventAsync(new StopCookingMealEvent(true));
            
            e.Sender.PlayerEntity.Session.CurrentMapInstance.Broadcast(e.Sender.PlayerEntity.Session.GenerateGuriPacket(6, 1, e.Sender.PlayerEntity.Id, GuriType[e.SkillVnum][0]));
            
            e.Sender.PlayerEntity.FirstChefAction = scheduler.Schedule(TimeSpan.FromMilliseconds(2500), async () =>
            {
                await e.Sender.PlayerEntity.Session.EmitEventAsync(new FinishCookingMealEvent(e.SkillVnum, e.RecipeGeneratedItemVnum, e.RecipeGeneratedItemAmount, e.Recipes));
            });
        }

        private async Task HandleCooking(CookingMealEvent e)
        {
            e.Sender.PlayerEntity.Session.CurrentMapInstance.Broadcast(e.Sender.PlayerEntity.Session.GenerateGuriPacket(6, 1, e.Sender.PlayerEntity.Id, GuriType[e.SkillVnum][0]));
            
            scheduler.Schedule(TimeSpan.FromMilliseconds(1000), () =>
            {
                e.Sender.PlayerEntity.Session.CurrentMapInstance.Broadcast(e.Sender.PlayerEntity.Session.GenerateGuriPacket(6, 1, e.Sender.PlayerEntity.Id, GuriType[e.SkillVnum][1]));
            });
            
            await e.Sender.PlayerEntity.Session.EmitEventAsync(new StopCookingMealEvent(true));
            // 15 sec total 
            // 3 <-> 10 sec
            int value = randomGenerator.RandomNumber(3, 11);

            e.Sender.PlayerEntity.Session.CurrentMapInstance.Broadcast(e.Sender.PlayerEntity.GenerateEffectS(EffectType.CookingNormalMeal, 1));
            e.Sender.PlayerEntity.Session.SendMSlotPacket(1013);

            e.Sender.PlayerEntity.FirstChefAction = scheduler.Schedule(TimeSpan.FromSeconds(value), () =>
            {
                e.Sender.PlayerEntity.Session.CurrentMapInstance.Broadcast(e.Sender.PlayerEntity.GenerateEffectS(EffectType.CookingCollectableMeal, 0));
                e.Sender.PlayerEntity.CanCollectMeal = true;

                e.Sender.PlayerEntity.SecondChefAction = scheduler.Schedule(TimeSpan.FromSeconds(5), () =>
                {
                    e.Sender.PlayerEntity.CanCollectMeal = false;
                });
            });

            e.Sender.PlayerEntity.ThirdChefAction = scheduler.Schedule(TimeSpan.FromSeconds(15), async () =>
            {
                e.Sender.PlayerEntity.Session.CurrentMapInstance.Broadcast(e.Sender.PlayerEntity.Session.GenerateGuriPacket(6, 1, e.Sender.PlayerEntity.Id, GuriType[e.SkillVnum][3]));
                e.Sender.PlayerEntity.Session.SendSayi(ChatMessageColorType.PlayerSay, Game18NConstString.HaveMadeMess);
                await e.Sender.PlayerEntity.Session.EmitEventAsync(new StopCookingMealEvent(true));
            });
        }

        public async Task HandleAsync(CookingMealEvent e, CancellationToken cancellation)
        {
            e.Sender.PlayerEntity.CancelCastingSkill();

            if (!BasicCheck(e))
            {
                return;
            }

            switch (e.SkillVnum)
            {
                case (int)SkillsVnums.CHOP_INGREDIENTS:
                    await HandlePreparation(e);
                    e.Sender.PlayerEntity.IsCooking = true;
                    return;

                case (int)SkillsVnums.ROAST:
                case (int)SkillsVnums.SIMMER:
                case (int)SkillsVnums.STIR_FRY:
                    await HandleCooking(e);
                    e.Sender.PlayerEntity.IsCooking = true;
                    return;
            }
        }
    }
}
