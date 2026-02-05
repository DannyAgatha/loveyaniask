using System;
using PhoenixLib.Events;
using PhoenixLib.Scheduler;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Packets.Enums.Chat;
using WingsAPI.Game.Extensions.ItemExtension.Item;
using WingsEmu.DTOs.Recipes;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Packets.Enums;
using WingsAPI.Data.Character;
using WingsAPI.Packets.Enums;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations;
using WingsAPI.Packets.Enums.BattlePass;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Packets.Enums.Battle;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters
{
    public class FinishCookingMealEventHandler(IScheduler scheduler, IRandomGenerator randomGenerator, IItemsManager itemsManager
        , IGameLanguageService gameLanguageService,IGameItemInstanceFactory gameItemInstanceFactory, IEvtbConfiguration evtbConfiguration, IRecipeManager _recipeManager) : IAsyncEventProcessor<FinishCookingMealEvent>
    {
        private readonly IEvtbConfiguration _evtbConfiguration = evtbConfiguration;

        private static Dictionary<int, int[]> GuriType { get; } = new()
        {
            [(int)SkillsVnums.CHOP_INGREDIENTS] = [14, 12],
            [(int)SkillsVnums.ROAST] = [25, 31, 42, 15],
            [(int)SkillsVnums.SIMMER] = [23, 26, 40, 44],
            [(int)SkillsVnums.STIR_FRY] = [24, 30, 41, 45]
        };

        private static Dictionary<byte, int> ExtraRewardsByPalier { get; } = new()
        {
            [1] = 9235,
            [2] = 9236,
            [3] = 9237,
        };

        private static Dictionary<int, int> BookRewardsByBearing { get; } = new()
        {
            [1012] = 2538, // roasted meal
            [1014] = 2599, // stew
            [1018] = 2618, // stir fries
        };

        private static List<short> SpiceItemsVnum = new()
        {
            2573, 2574, 2575, 2576, 2577 
        };

        private bool BasicCheck(FinishCookingMealEvent e)
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

            if (!e.Sender.PlayerEntity.LastRecipeFromChefSp.Items.Any() || 
                e.Sender.PlayerEntity.LastRecipeFromChefSp.Items.Any(ite => !e.Sender.PlayerEntity.HasItem(ite.ItemVNum, (short)(ite.Amount * e.RecipeGeneratedItemAmount))))
            {
                e.Sender.PlayerEntity.IsCraftingItem = false;
                return false;
            }

            IGameItem producedItem = itemsManager.GetItem(e.Sender.PlayerEntity.LastRecipeFromChefSp.ProducedItemVnum);
            if (producedItem == null)
            {
                return false;
            }

            return true;
        }

        private async Task HandlePreparation(FinishCookingMealEvent e, bool ingredients)
        {
            e.Sender.CurrentMapInstance.Broadcast(e.SkillVnum == (int)SkillsVnums.CHOP_INGREDIENTS
                ? e.Sender.GenerateGuriPacket(6, 1, e.Sender.PlayerEntity.Id, GuriType[e.SkillVnum][1])
                : e.Sender.GenerateGuriPacket(6, 1, e.Sender.PlayerEntity.Id, GuriType[e.SkillVnum][2]));
            
            e.Sender.CurrentMapInstance.Broadcast(e.Sender.GenerateEffectPacket(EffectType.CookingSuccess));

            Recipe recipe = e.Sender.PlayerEntity.LastRecipeFromChefSp;
            
            if (!e.Sender.PlayerEntity.HasSpaceFor(recipe.ProducedItemVnum, (short)(recipe.Amount * e.RecipeGeneratedItemAmount)))
            {
                e.Sender.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.FullInventory);
                e.Sender.SendCancelPacket(CancelType.NotInCombatMode);
                return;
            }

            e.Sender.CurrentMapInstance.Broadcast(e.Sender.GenerateEffectPacket(EffectType.CookingSuccessBlue));

            await RemoveItem(e, ingredients);

            short amountRewards = (short)(recipe.Amount * e.RecipeGeneratedItemAmount);

            int percent = e.Sender.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.FocusEnemyAttentionSkill,
                                       (byte)AdditionalTypes.FocusEnemyAttentionSkill.ExtraMealsChance, e.Sender.PlayerEntity.Level).firstData;
            if (percent != 0 && randomGenerator.RandomNumber() < percent && !ingredients)
            {
                amountRewards *= 2;
            }

            percent = e.Sender.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.FocusEnemyAttentionSkill,
                                       (byte)AdditionalTypes.FocusEnemyAttentionSkill.ExtraIngredientsChance, e.Sender.PlayerEntity.Level).firstData;
            if (percent != 0 && randomGenerator.RandomNumber() < percent && ingredients)
            {
                amountRewards *= 2;
            }

            scheduler.Schedule(TimeSpan.FromMilliseconds(2000), async () =>
            {
                GameItemInstance newItem = gameItemInstanceFactory.CreateItem(recipe.ProducedItemVnum, amountRewards, 0, 0);
                await e.Sender.AddNewItemToInventory(newItem);
                e.Sender.SendShopEndPacket(ShopEndType.Player);
                e.Sender.SendPdtiPacket(PdtiType.ItemHasBeenProduced, newItem.ItemVNum, (short)newItem.Amount, 0, newItem.Upgrade, newItem.Rarity);
                e.Sender.SendSound(SoundType.CRAFTING_SUCCESS);
                e.Sender.SendMsgi(MessageType.Default, Game18NConstString.ItemProduced, 1, newItem.ItemVNum);
            });

            double moreExp = 1 + e.Sender.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.FocusEnemyAttentionSkill, (byte)AdditionalTypes.FocusEnemyAttentionSkill.IncreaseCookingExp, e.Sender.PlayerEntity.Level).firstData * 0.01;
            moreExp -= e.Sender.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.FocusEnemyAttentionSkill, (byte)AdditionalTypes.FocusEnemyAttentionSkill.DecreaseCookingExp, e.Sender.PlayerEntity.Level).firstData * 0.01;
            moreExp += _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_COOKING_EXPERIENCE_GAIN) * 0.01;

            await e.Sender.EmitEventAsync(new AddExpEvent((long)(recipe.ProducedChefXp * moreExp), LevelType.SpJobLevel));
            e.Sender.PlayerEntity.LastSkillId = 0;

            CharacterCookingDto log = e.Sender.PlayerEntity.CharacterCookingDto.FirstOrDefault(s => s.RecipeVnum == e.RecipeGeneratedItemVnum);
            if (log == null)
            {
                log = new CharacterCookingDto
                {
                    Amount = 1,
                    RecipeVnum = e.RecipeGeneratedItemVnum
                };
                e.Sender.PlayerEntity.CharacterCookingDto.Add(log);
            }
            else
            {
                log.Amount++;
            }

            int percentOfExtra = 10;
            double moreExtra = 1 + e.Sender.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.FocusEnemyAttentionSkill, (byte)AdditionalTypes.FocusEnemyAttentionSkill.IncreaseCookingSuccess, e.Sender.PlayerEntity.Level).firstData * 0.01;
            moreExtra -= e.Sender.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.FocusEnemyAttentionSkill, (byte)AdditionalTypes.FocusEnemyAttentionSkill.DecreaseCookingSuccess, e.Sender.PlayerEntity.Level).firstData * 0.01;

            percentOfExtra = (int)(percentOfExtra * moreExtra);

            if (randomGenerator.RandomNumber() < percentOfExtra && e.Sender.PlayerEntity.LastRecipeFromChefSp.BearingChef.HasValue)
            {
                int rewards = ExtraRewardsByPalier[e.Sender.PlayerEntity.LastRecipeFromChefSp.BearingChef.Value];
                await e.Sender.AddNewItemToInventory(gameItemInstanceFactory.CreateItem(rewards, 1, 0, 0),true);
            }

            await e.Sender.EmitEventAsync(new IncreaseBattlePassObjectiveEvent(MissionType.CookXMeal));

            await CheckBookRewards(e);
        }

        private async Task CheckBookRewards(FinishCookingMealEvent e)
        {
            if (e.SkillVnum == (int)SkillsVnums.CHOP_INGREDIENTS)
            {
                return;
            }

            int itemRewards = BookRewardsByBearing[e.SkillVnum];
            if (e.Sender.PlayerEntity.FishRewardsEarnedDto.Any(s => s.Vnum == itemRewards))
            {
                return;
            }

            var bearing = new List<bool>();
            foreach (Recipe i in e.Recipes)
            {
                CharacterCookingDto log = e.Sender.PlayerEntity.CharacterCookingDto.FirstOrDefault(s => s.RecipeVnum == e.Sender.PlayerEntity.LastRecipeFromChefSp.ProducedItemVnum);
                if (log == null)
                {
                    bearing.Add(false);
                    continue;
                }
                if (log.Amount < i.LimitCrafting)
                {
                    bearing.Add(false);
                    continue;
                }
                bearing.Add(true);
            }

            if (bearing.Any(s => s == false))
            {
                return;
            }

            await e.Sender.AddNewItemToInventory(gameItemInstanceFactory.CreateItem(itemRewards, 1, 0, 0), true);
            e.Sender.PlayerEntity.FishRewardsEarnedDto.Add(new CharacterFishRewardsEarnedDto
            {
                Vnum = itemRewards
            });
        }

        private async Task RemoveItem(FinishCookingMealEvent e, bool ingredients)
        {
            Recipe recipe = e.Sender.PlayerEntity.LastRecipeFromChefSp;

            bool removeSpice = true;

            double extraSpice = e.Sender.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.Other, (byte)AdditionalTypes.Other.ProvidesChanceSpicesNotConsumed, e.Sender.PlayerEntity.Level).firstData;
            extraSpice -= e.Sender.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.Other, (byte)AdditionalTypes.Other.ProvidesChanceSpicesAreConsumed, e.Sender.PlayerEntity.Level).firstData;

            if (extraSpice > 0 && randomGenerator.RandomNumber() < (int)extraSpice)
            {
                removeSpice = false;
            }

            foreach (RecipeItemDTO recipeItem in recipe.Items)
            {
                if (!removeSpice && !ingredients && SpiceItemsVnum.Contains(recipeItem.ItemVNum))
                {
                    continue;
                }
                await e.Sender.RemoveItemFromInventory(recipeItem.ItemVNum, (short)(recipeItem.Amount * e.RecipeGeneratedItemAmount));
            }
        }

        private async Task HandleCooking(FinishCookingMealEvent e)
        {
            if (!e.Sender.PlayerEntity.CanCollectMeal)
            {
                await RemoveItem(e, false);

                e.Sender.SendSayi(ChatMessageColorType.PlayerSay, Game18NConstString.HaveNotCookedIt);
                e.Sender.CurrentMapInstance.Broadcast(e.Sender.GenerateGuriPacket(6, 1, e.Sender.PlayerEntity.Id, GuriType[e.SkillVnum][3]));
                await e.Sender.EmitEventAsync(new StopCookingMealEvent(true));
                return;
            }

            await HandlePreparation(e, false);

            await e.Sender.EmitEventAsync(new StopCookingMealEvent(true));
            e.Sender.SendSayi(ChatMessageColorType.PlayerSay, Game18NConstString.CulinarySkillsWorked);
        }

        public async Task HandleAsync(FinishCookingMealEvent e, CancellationToken cancellation)
        {
            if (!BasicCheck(e))
            {
                e.Sender.PlayerEntity.CancelCastingSkill();
                return;
            }

            e.Sender.PlayerEntity.IsCooking = false;
            
            switch (e.SkillVnum)
            {
                case (int)SkillsVnums.CHOP_INGREDIENTS:
                    await HandlePreparation(e, true);
                    return;

                case (int)SkillsVnums.ROAST:
                case (int)SkillsVnums.SIMMER:
                case (int)SkillsVnums.STIR_FRY:
                    await HandleCooking(e);
                    return;
            }
        }
    }
}