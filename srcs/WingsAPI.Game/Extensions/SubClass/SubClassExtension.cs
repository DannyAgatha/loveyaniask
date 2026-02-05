using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Game.Extensions.SubClass
{
    public static class SubClassExtension
    {
        private static readonly ConcurrentDictionary <long, Timer> ExpTimers = new();
        private static readonly ConcurrentDictionary <long, int> ExpAccumulators = new();
        
        public static bool IsPveSubClass(this SubClassType subClassType)
        {
            return subClassType switch
            {
                SubClassType.OathKeeper => true,
                SubClassType.ArrowLord => true,
                SubClassType.ArcaneSage => true,
                SubClassType.EmperorsBlade => true,
                _ => false
            };
        }

        public static bool IsPvpSubClass(this SubClassType subClassType)
        {
            return subClassType switch
            {
                SubClassType.CrimsonFury => true,
                SubClassType.SilentStalker => true,
                SubClassType.DarkNecromancer => true,
                SubClassType.StealthShadow => true,
                _ => false
            };
        }

        public static bool IsPvpAndPveSubClass(this SubClassType subClassType)
        {
            return subClassType switch
            {
                SubClassType.CelestialPaladin => true,
                SubClassType.ShadowHunter => true,
                SubClassType.Pyromancer => true,
                SubClassType.ZenWarrior => true,
                _ => false
            };
        }
        
        private static int GetExperienceRequirementForNextLevel(int tierLevel)
        {
            return tierLevel switch
            {
                1 => 10000,
                2 => 30000,
                3 => 90000,
                4 => 270000,
                5 => 810000,
                _ => 810000
            };
        }

        public static void AddTierExperience(this IClientSession session, int experienceToAdd, IGameLanguageService languageService, bool handleExpTimer = true)
        {
            if (session?.PlayerEntity == null)
            {
                return;
            }

            ExpAccumulators.TryAdd(session.PlayerEntity.Id, 0);

            session.PlayerEntity.TierExperience += experienceToAdd;
            ExpAccumulators[session.PlayerEntity.Id] += experienceToAdd;

            while (true)
            {
                int experienceRequirementForNextLevel = GetExperienceRequirementForNextLevel(session.PlayerEntity.TierLevel);

                if (session.PlayerEntity.TierExperience >= experienceRequirementForNextLevel)
                {
                    session.PlayerEntity.TierExperience -= experienceRequirementForNextLevel;
                    session.PlayerEntity.TierLevel += 1;
                    
                    session.EmitEventAsync(new ChangeSubClassEvent
                    {
                        NewSubClass = session.PlayerEntity.SubClass,
                        TierLevel = session.PlayerEntity.TierLevel,
                        TierExperience = session.PlayerEntity.TierExperience
                    });
                    
                    session.BroadcastEffectInRange(EffectType.JobLevelUp);
                    session.SendChatMessage(languageService.GetLanguageFormat(GameDialogKey.TIER_LEVEL_UP, session.UserLanguage, session.PlayerEntity.TierLevel), ChatMessageColorType.Green);
                }
                else
                {
                    break;
                }
            }

            if (handleExpTimer)
            {
                HandleExpTimer(session, languageService);
            }
            else
            {
                if (ExpAccumulators[session.PlayerEntity.Id] <= 0)
                {
                    return;
                }

                int experienceRequirementForNextTier = GetExperienceRequirementForNextLevel(session.PlayerEntity.TierLevel);

                session.SendChatMessage(
                    languageService.GetLanguageFormat(GameDialogKey.CURRENT_EXPERIENCE_TIER_LEVEL, session.UserLanguage, session.PlayerEntity.TierLevel, session.PlayerEntity.TierExperience,
                        experienceRequirementForNextTier), ChatMessageColorType.Yellow);

                ExpAccumulators[session.PlayerEntity.Id] = 0;
            }
        }
        
        private static void HandleExpTimer(IClientSession session, IGameLanguageService languageService)
        {
            Timer timer = ExpTimers.GetOrAdd(session.PlayerEntity.Id, _ => new Timer(_ =>
            {
                if (ExpAccumulators.TryGetValue(session.PlayerEntity.Id, out int expAccumulator) && expAccumulator > 0)
                {
                    int experienceRequirementForNextLevel = GetExperienceRequirementForNextLevel(session.PlayerEntity.TierLevel);

                    while (session.PlayerEntity.TierExperience >= experienceRequirementForNextLevel)
                    {
                        session.PlayerEntity.TierExperience -= experienceRequirementForNextLevel;
                        session.PlayerEntity.TierLevel += 1;
                        
                        session.EmitEventAsync(new ChangeSubClassEvent
                        {
                            NewSubClass = session.PlayerEntity.SubClass,
                            TierLevel = session.PlayerEntity.TierLevel,
                            TierExperience = session.PlayerEntity.TierExperience
                        });
                        
                        session.BroadcastEffectInRange(EffectType.JobLevelUp);
                        session.SendChatMessage(languageService.GetLanguageFormat(GameDialogKey.TIER_LEVEL_UP, session.UserLanguage, session.PlayerEntity.TierLevel), ChatMessageColorType.Green);

                        experienceRequirementForNextLevel = GetExperienceRequirementForNextLevel(session.PlayerEntity.TierLevel);
                    }

                    session.SendChatMessage(
                        languageService.GetLanguageFormat(GameDialogKey.CURRENT_EXPERIENCE_TIER_LEVEL, session.UserLanguage, session.PlayerEntity.TierLevel, session.PlayerEntity.TierExperience,
                            experienceRequirementForNextLevel), ChatMessageColorType.Yellow);

                    ExpAccumulators[session.PlayerEntity.Id] = 0;
                }

                if (ExpTimers.TryRemove(session.PlayerEntity.Id, out Timer currentTimer))
                {
                    currentTimer.Dispose();
                }
            }, null, 5000, Timeout.Infinite));

            timer.Change(5000, Timeout.Infinite);
        }
    }
}
