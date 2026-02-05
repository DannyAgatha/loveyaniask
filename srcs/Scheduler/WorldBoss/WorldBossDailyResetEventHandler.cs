// using System.Threading;
// using System.Threading.Tasks;
// using PhoenixLib.Events;
// using PhoenixLib.Logging;
// using Plugin.WorldBoss.Manager;
//
// namespace WingsEmu.ClusterScheduler.WorldBoss;
//
// public class WorldBossDailyResetEventHandler : IAsyncEventProcessor<WorldBossDailyResetEvent>
// {
//     private readonly IWorldBossManager _worldBossManager;
//
//     public WorldBossDailyResetEventHandler(IWorldBossManager worldBossManager)
//     {
//         _worldBossManager = worldBossManager;
//     }
//
//     public Task HandleAsync(WorldBossDailyResetEvent e, CancellationToken cancellationToken)
//     {
//         _worldBossManager.ClearDailyFlags();
//         Log.Info("[WORLD_BOSS] Daily reset of World Boss flags completed.");
//         return Task.CompletedTask;
//     }
// }