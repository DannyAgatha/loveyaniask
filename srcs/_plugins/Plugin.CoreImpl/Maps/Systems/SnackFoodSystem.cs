using System;
using WingsAPI.Data.Families;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.Shells;
using WingsEmu.Core.Extensions;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.SnackFood;
using WingsEmu.Game.SnackFood.Events;
using WingsEmu.Packets.Enums;

namespace Plugin.CoreImpl.Maps.Systems
{
    public sealed class SnackFoodSystem
    {
        private const double _bufferFraction = 0.2;

        private static readonly TimeSpan _tickDelay = TimeSpan.FromSeconds(2);
        private readonly GameMinMaxConfiguration _configuration;
        private readonly IEvtbConfiguration _evtbConfiguration;

        public SnackFoodSystem(GameMinMaxConfiguration configuration, IEvtbConfiguration evtbConfiguration)
        {
            _configuration = configuration;
            _evtbConfiguration = evtbConfiguration;
        }

        public void ProcessUpdate(IPlayerEntity character, DateTime time)
        {
            ProcessFood(character, time);
            ProcessSnack(character, time);
            ProcessAdditionalFood(character, time);
            ProcessAdditionalSnack(character, time);
        }

        private void ProcessFood(IPlayerEntity character, in DateTime date)
        {
            FoodProgress progress = character.GetFoodProgress;
            if (progress == null)
            {
                return;
            }

            if (!character.IsSitting)
            {
                character.ClearFoodBuffer();
                return;
            }

            if (progress.LastTick + _tickDelay * progress.IncreaseTick > date)
            {
                return;
            }

            int hp = 0;
            int mp = 0;
            int sp = 0;

            if (progress.FoodHpBuffer > 0)
            {
                hp = (int)(progress.FoodHpBufferSize * _bufferFraction);
                progress.FoodHpBuffer -= hp;
            }
            else
            {
                progress.FoodHpBufferSize = 0;
                progress.FoodHpBuffer = 0;
            }

            if (progress.FoodMpBuffer > 0)
            {
                mp = (int)(progress.FoodMpBufferSize * _bufferFraction);
                progress.FoodMpBuffer -= mp;
            }
            else
            {
                progress.FoodMpBufferSize = 0;
                progress.FoodMpBuffer = 0;
            }

            if (progress.FoodSpBuffer > 0)
            {
                sp = (int)(progress.FoodSpBufferSize * _bufferFraction);
                progress.FoodSpBuffer -= sp;
            }
            else
            {
                progress.FoodSpBufferSize = 0;
                progress.FoodSpBuffer = 0;
            }

            int toAdd = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.IncreaseRecoveryItems, character.Level).firstData;
            toAdd += character.GetMaxArmorShellValue(ShellEffectType.IncreasedRecoveryItemSpeed);
            toAdd += character.Family?.UpgradeValues.GetOrDefault(FamilyUpgradeType.INCREASE_FOOD_SNACK_REGEN).Item1 ?? 0;
            int toRemove = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DecreaseRecoveryItems, character.Level).firstData;
            
            double finalHeal = (100 + (toAdd - toRemove)) * 0.01;

            hp = (int)(hp * finalHeal);
            mp = (int)(mp * finalHeal);

            if (hp <= 0)
            {
                hp = 0;
            }

            if (mp <= 0)
            {
                mp = 0;
            }

            int mateHeal = 0;
            if (progress.FoodMateMaxHpBuffer > 0)
            {
                mateHeal = (int)(progress.FoodMateMaxHpBufferSize * _bufferFraction);
                progress.FoodMateMaxHpBuffer -= mateHeal;
            }
            else
            {
                progress.FoodMateMaxHpBuffer = 0;
                progress.FoodMateMaxHpBufferSize = 0;
            }

            if (mateHeal != 0)
            {
                foreach (IMateEntity mate in character.MateComponent.TeamMembers())
                {
                    if (!mate.IsSitting)
                    {
                        continue;
                    }

                    int toHeal = (int)(mate.MaxHp * (mateHeal * 0.01));
                    character.Session.EmitEvent(new MateHealEvent
                    {
                        MateEntity = mate,
                        HpHeal = toHeal
                    });
                }
            }

            if (hp != 0 || mp != 0)
            {
                character.EmitEvent(new BattleEntityHealEvent
                {
                    HpHeal = hp,
                    MpHeal = mp,
                    HealMates = mateHeal == 0,
                    Entity = character
                });
            }

            if (sp != 0)
            {
                bool addToBonus = character.SpPointsBasic * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED) * 0.01) + sp > _configuration.MaxSpBasePoints;
                if (addToBonus)
                {
                    int remove = _configuration.MaxSpBasePoints - character.SpPointsBasic;
                    character.SpPointsBasic = _configuration.MaxSpBasePoints;

                    sp -= remove;
                    character.SpPointsBonus = character.SpPointsBonus + sp > _configuration.MaxSpAdditionalPoints ? _configuration.MaxSpAdditionalPoints : character.SpPointsBonus + sp;
                }
                else
                {
                    character.SpPointsBasic += sp;
                }
                
                character.Session.RefreshSpPoint();
            }

            progress.LastTick = date;
        }

        private void ProcessAdditionalFood(IPlayerEntity character, in DateTime date)
        {
            AdditionalFoodProgress progress = character.GetAdditionalFoodProgress;
            if (progress == null)
            {
                return;
            }

            if (progress.LastTick + _tickDelay > date)
            {
                return;
            }

            int hp = 0;
            int mp = 0;

            if (progress.FoodAdditionalHpBuffer > 0)
            {
                hp = (int)(progress.FoodAdditionalHpBufferSize * _bufferFraction);
                progress.FoodAdditionalHpBuffer -= hp;
            }
            else
            {
                progress.FoodAdditionalHpBufferSize = 0;
                progress.FoodAdditionalHpBuffer = 0;
            }

            if (progress.FoodAdditionalMpBuffer > 0)
            {
                mp = (int)(progress.FoodAdditionalMpBufferSize * _bufferFraction);
                progress.FoodAdditionalMpBuffer -= mp;
            }
            else
            {
                progress.FoodAdditionalMpBufferSize = 0;
                progress.FoodAdditionalMpBuffer = 0;
            }

            if (hp != 0 || mp != 0)
            {
                character.Session.EmitEvent(new AddAdditionalHpMpEvent
                {
                    Hp = hp,
                    Mp = mp,
                    MaxHpPercentage = progress.HpCap,
                    MaxMpPercentage = progress.MpCap
                });
            }

            progress.LastTick = date;
        }

        private void ProcessSnack(IPlayerEntity character, in DateTime date)
        {
            SnackProgress progress = character.GetSnackProgress;
            if (progress == null)
            {
                return;
            }

            if (progress.LastTick + _tickDelay > date)
            {
                return;
            }

            int hp = 0;
            int mp = 0;
            int sp = 0;

            if (progress.SnackHpBuffer > 0)
            {
                hp = (int)(progress.SnackHpBufferSize * _bufferFraction);
                progress.SnackHpBuffer -= hp;
            }
            else
            {
                progress.SnackHpBufferSize = 0;
                progress.SnackHpBuffer = 0;
            }

            if (progress.SnackMpBuffer > 0)
            {
                mp = (int)(progress.SnackMpBufferSize * _bufferFraction);
                progress.SnackMpBuffer -= mp;
            }
            else
            {
                progress.SnackMpBufferSize = 0;
                progress.SnackMpBuffer = 0;
            }

            if (progress.SnackSpBuffer > 0)
            {
                sp = (int)(progress.SnackSpBufferSize * _bufferFraction);
                progress.SnackSpBuffer -= sp;
            }
            else
            {
                progress.SnackSpBufferSize = 0;
                progress.SnackSpBuffer = 0;
            }

            int toAdd = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.IncreaseRecoveryItems, character.Level).firstData;
            toAdd += character.GetMaxArmorShellValue(ShellEffectType.IncreasedRecoveryItemSpeed);
            toAdd += character.Family?.UpgradeValues.GetOrDefault(FamilyUpgradeType.INCREASE_FOOD_SNACK_REGEN).Item1 ?? 0;
            int toRemove = character.BCardComponent.GetAllBCardsInformation(BCardType.LeonaPassiveSkill, (byte)AdditionalTypes.LeonaPassiveSkill.DecreaseRecoveryItems, character.Level).firstData;

            double finalHeal = (100 + (toAdd - toRemove)) * 0.01;

            hp = (int)(hp * finalHeal);
            mp = (int)(mp * finalHeal);

            if (hp <= 0)
            {
                hp = 0;
            }

            if (mp <= 0)
            {
                mp = 0;
            }

            if (hp != 0 || mp != 0)
            {
                character.EmitEvent(new BattleEntityHealEvent
                {
                    HpHeal = hp,
                    MpHeal = mp,
                    HealMates = true,
                    Entity = character
                });
            }

            if (sp != 0)
            {
                bool addToBonus = character.SpPointsBasic * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_FULLNESS_POINTS_RECEIVED) * 0.01) + sp > _configuration.MaxSpBasePoints;
                if (addToBonus)
                {
                    int remove = _configuration.MaxSpBasePoints - character.SpPointsBasic;
                    character.SpPointsBasic = _configuration.MaxSpBasePoints;

                    sp -= remove;
                    character.SpPointsBonus = character.SpPointsBonus + sp > _configuration.MaxSpAdditionalPoints ? _configuration.MaxSpAdditionalPoints : character.SpPointsBonus + sp;
                }
                else
                {
                    character.SpPointsBasic += sp;
                }
                
                character.Session.RefreshSpPoint();
            }

            progress.LastTick = date;
        }

        private void ProcessAdditionalSnack(IPlayerEntity character, in DateTime date)
        {
            AdditionalSnackProgress progress = character.GetAdditionalSnackProgress;
            if (progress == null)
            {
                return;
            }

            if (progress.LastTick + _tickDelay > date)
            {
                return;
            }

            int hp = 0;
            int mp = 0;

            if (progress.SnackAdditionalHpBuffer > 0)
            {
                hp = (int)(progress.SnackAdditionalHpBufferSize * _bufferFraction);
                progress.SnackAdditionalHpBuffer -= hp;
            }
            else
            {
                progress.SnackAdditionalHpBufferSize = 0;
                progress.SnackAdditionalHpBuffer = 0;
            }

            if (progress.SnackAdditionalMpBuffer > 0)
            {
                mp = (int)(progress.SnackAdditionalMpBufferSize * _bufferFraction);
                progress.SnackAdditionalMpBuffer -= mp;
            }
            else
            {
                progress.SnackAdditionalMpBufferSize = 0;
                progress.SnackAdditionalMpBuffer = 0;
            }

            if (hp != 0 || mp != 0)
            {
                character.Session.EmitEvent(new AddAdditionalHpMpEvent
                {
                    Hp = hp,
                    Mp = mp,
                    MaxHpPercentage = progress.HpCap,
                    MaxMpPercentage = progress.MpCap
                });
            }

            progress.LastTick = date;
        }
    }
}