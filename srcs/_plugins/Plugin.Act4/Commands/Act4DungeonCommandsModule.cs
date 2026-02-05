using System;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Qmmands;
using WingsAPI.Scripting.ScriptManager;
using WingsEmu.Commands.Entities;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4.Commands;

public partial class Act4CommandsModule
{
    [Group("dungeon", "dungeons", "raid", "raids")]
    [Description("Module related to Act4 Dungeons management commands.")]
    public class Act4DungeonCommandsModule : SaltyModuleBase
    {
        private readonly IAct4DungeonManager _act4DungeonManager;
        private readonly IAsyncEventPipeline _asyncEventPipeline;
        private readonly IDungeonScriptManager _dungeonScriptManager;

        public Act4DungeonCommandsModule(IAsyncEventPipeline asyncEventPipeline, IDungeonScriptManager dungeonScriptManager, IAct4DungeonManager act4DungeonManager)
        {
            _asyncEventPipeline = asyncEventPipeline;
            _dungeonScriptManager = dungeonScriptManager;
            _act4DungeonManager = act4DungeonManager;
        }

        [Command("reload")]
        public async Task ReloadDungeons()
        {
            try
            {
                _dungeonScriptManager.Load();
                Context.Player.SendSuccessChatMessage("Dungeons reloaded, check your console output!");
            }
            catch (Exception e)
            {
                Log.Error("[ACT4_DUNGEON_SYSTEM] Reload failed: ", e);
                Context.Player.SendErrorChatMessage("Dungeons reload failed!");
            }
        }

        [Command("start")]
        public async Task StartDungeon(DungeonType dungeonType, FactionType factionType = FactionType.Neutral)
        {
            await _asyncEventPipeline.ProcessEventAsync(
                new Act4DungeonSystemStartEvent(factionType == FactionType.Neutral ? Context.Player.PlayerEntity.Faction : factionType, dungeonType));
        }

        [Command("stop")]
        public async Task StopDungeon()
        {
            await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonSystemStopEvent());
        }

        [Command("open")]
        public async Task<SaltyCommandResult> OpenPortal()
        {
            IClientSession session = Context.Player;
            if (!session.PlayerEntity.IsInFamily())
            {
                return new SaltyCommandResult(false, "Not in family.");
            }

            DungeonInstance dungeon = _act4DungeonManager.GetDungeon(session.PlayerEntity.Family.Id);
            if (dungeon == null)
            {
                return new SaltyCommandResult(false, "The dungeon doesn't exist for this family.");
            }

            if (!dungeon.DungeonSubInstances.TryGetValue(session.CurrentMapInstance.Id, out DungeonSubInstance instance))
            {
                return new SaltyCommandResult(false, "You have to be inside the dungeon.");
            }

            PortalGenerator portalGenerator = instance.PortalGenerators.FirstOrDefault();
            if (portalGenerator == null || instance.LastPortalGeneration == null)
            {
                return new SaltyCommandResult(false, "Portal doesn't exist or it's already opened.");
            }

            instance.PortalGenerators.Remove(portalGenerator);

            await _asyncEventPipeline.ProcessEventAsync(new SpawnPortalEvent(instance.MapInstance, portalGenerator.Portal));

            await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonBroadcastPacketEvent
            {
                DungeonInstance = dungeon
            });

            await _asyncEventPipeline.ProcessEventAsync(new Act4DungeonBroadcastBossOpenEvent
            {
                DungeonInstance = dungeon
            });

            return new SaltyCommandResult(true);
        }
    }
}