using System.Collections.Concurrent;
using System;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.LandOfDeath;
using WingsEmu.Game.Maps;
using System.Threading.Tasks;
using System.Linq;
using PhoenixLib.Logging;
using System.Collections.Generic;
using System.Threading;
using Plugin.LandOfDeath.Utils;
using WingsAPI.Packets.Enums.LandOfDeath;
using WingsEmu.Game.Entities; // <- necesario para IMonsterEntity

namespace Plugin.LandOfDeath
{
    public interface ILandOfDeathFactory
    {
        LandOfDeathInstance CreateSoloLandOfDeath(int playerId);
        LandOfDeathInstance CreateGroupLandOfDeath(int currentGroupId, List<int> playerIds, LandOfDeathMode mode);
        LandOfDeathInstance CreateFamilyLandOfDeath(long familyId, LandOfDeathMode mode);
        LandOfDeathInstance CreatePublicLandOfDeath(LandOfDeathMode mode);
    }

    public class LandOfDeathFactory : ILandOfDeathFactory
    {
        private readonly IMapManager _mapManager;
        private readonly ILandOfDeathManager _landOfDeathManager;
        private readonly LandOfDeathConfiguration _landOfDeathConfiguration;
        private ConcurrentDictionary<int, List<GroupInstanceInfo>> _playerGroupInstanceHistory = new();
        private CancellationTokenSource _cleanupTaskCancellationTokenSource = new();

        public LandOfDeathFactory(IMapManager mapManager, ILandOfDeathManager landOfDeathManager, LandOfDeathConfiguration landOfDeathConfiguration)
        {
            _mapManager = mapManager;
            _landOfDeathManager = landOfDeathManager;
            _landOfDeathConfiguration = landOfDeathConfiguration;
            StartEmptyInstanceCleanupTask();
        }

        private void StartEmptyInstanceCleanupTask()
        {
            CancellationToken token = _cleanupTaskCancellationTokenSource.Token;

            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    var emptyInstances = _landOfDeathManager.GetGroupLandOfDeathInstances()
                        .Where(instance => !instance.MapInstance.GetCharacters().Any() &&
                                           DateTime.UtcNow - instance.CreationTime > TimeSpan.FromMinutes(5))
                        .ToList();

                    int instancesCount = 0;
                    foreach (LandOfDeathInstance instance in emptyInstances)
                    {
                        instancesCount++;
                        _landOfDeathManager.RemoveLandOfDeathInstance(instance);
                    }

                    Log.Warn("[LAND_OF_DEATH] " + instancesCount + " Empty lod group instances cleaned.");
                    await Task.Delay(TimeSpan.FromMinutes(5), token);
                }
            }, token);
        }

        public LandOfDeathInstance CreateSoloLandOfDeath(int playerId)
        {
            IMapInstance mapInstance = _mapManager.GenerateMapInstanceByMapId(_landOfDeathConfiguration.MapVnum, MapInstanceType.LandOfDeath);
            LandOfDeathInstance newInstance = new(mapInstance)
            {
                Mode = LandOfDeathMode.Normal // 👈 default
            };
            _landOfDeathManager.AddLandOfDeathInstanceByPlayer(playerId, newInstance);
            return newInstance;
        }

        public LandOfDeathInstance CreateGroupLandOfDeath(int currentGroupId, List<int> playerIds, LandOfDeathMode mode)
        {
            DateTime now = DateTime.UtcNow;

            foreach (int playerId in playerIds)
            {
                List<GroupInstanceInfo> playerInstanceHistory = _playerGroupInstanceHistory.GetOrAdd(playerId, new List<GroupInstanceInfo>());
                playerInstanceHistory.RemoveAll(x => now - x.CreationTime > TimeSpan.FromMinutes(10));

                GroupInstanceInfo recentInstance = playerInstanceHistory.FirstOrDefault();
                if (recentInstance != null)
                {
                    TimeSpan timeRemaining = TimeSpan.FromMinutes(10) - (now - recentInstance.CreationTime);
                    throw new InvalidOperationException(
                        $"You must wait {timeRemaining.Minutes} minutes and {timeRemaining.Seconds} seconds before creating a new group instance.");
                }

                playerInstanceHistory.Add(new GroupInstanceInfo
                {
                    OriginalGroupId = currentGroupId,
                    CreationTime = now
                });
            }

            IMapInstance mapInstance = _mapManager.GenerateMapInstanceByMapId(_landOfDeathConfiguration.MapVnum, MapInstanceType.LandOfDeath);
            LandOfDeathInstance newInstance = new(mapInstance)
            {
                Mode = mode
            };

            // ⚔️ Ajustar stats de monstruos según modo
            foreach (IMonsterEntity mob in newInstance.MapInstance.GetAliveMonsters())
            {
                if (newInstance.Mode == LandOfDeathMode.Easy)
                {
                    mob.MaxHp = (int)(mob.MaxHp * 0.5);
                    mob.Hp = mob.MaxHp;

                    mob.DamagesMinimum = (int)(mob.DamagesMinimum * 0.5);
                    mob.DamagesMaximum = (int)(mob.DamagesMaximum * 0.5);
                }

                mob.RefreshStats();
            }

            _landOfDeathManager.AddLandOfDeathInstanceByGroup(currentGroupId, mode, newInstance);
            return newInstance;
        }

        public LandOfDeathInstance CreateFamilyLandOfDeath(long familyId, LandOfDeathMode mode)
        {
            IMapInstance mapInstance = _mapManager.GenerateMapInstanceByMapId(_landOfDeathConfiguration.MapVnum, MapInstanceType.LandOfDeath);
            LandOfDeathInstance newInstance = new(mapInstance)
            {
                Mode = mode
            };

            foreach (IMonsterEntity mob in newInstance.MapInstance.GetAliveMonsters())
            {
                if (newInstance.Mode == LandOfDeathMode.Easy)
                {
                    mob.MaxHp = (int)(mob.MaxHp * 0.5);
                    mob.Hp = mob.MaxHp;

                    mob.DamagesMinimum = (int)(mob.DamagesMinimum * 0.5);
                    mob.DamagesMaximum = (int)(mob.DamagesMaximum * 0.5);
                }

                mob.RefreshStats();
            }

            _landOfDeathManager.AddLandOfDeathInstanceByFamily(familyId, mode, newInstance);
            return newInstance;
        }
        
        public LandOfDeathInstance CreatePublicLandOfDeath(LandOfDeathMode mode)
        {
            IMapInstance mapInstance = _mapManager.GenerateMapInstanceByMapId(_landOfDeathConfiguration.MapVnum, MapInstanceType.LandOfDeath);
            LandOfDeathInstance newInstance = new(mapInstance)
            {
                Mode = mode
            };

            foreach (IMonsterEntity mob in newInstance.MapInstance.GetAliveMonsters())
            {
                if (newInstance.Mode == LandOfDeathMode.Easy)
                {
                    mob.MaxHp = (int)(mob.MaxHp * 0.5);
                    mob.Hp = mob.MaxHp;

                    mob.DamagesMinimum = (int)(mob.DamagesMinimum * 0.5);
                    mob.DamagesMaximum = (int)(mob.DamagesMaximum * 0.5);
                }

                mob.RefreshStats();
            }

            _landOfDeathManager.AddLandOfDeathInstanceByPublic(mode, newInstance);
            return newInstance;
        }
    }
}
