using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using Qmmands;
using WingsAPI.Communication;
using WingsAPI.Communication.Player;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Communication.Services;
using WingsAPI.Communication.Services.Requests;
using WingsAPI.Communication.Services.Responses;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Features;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.Essentials.Administrator;

[Name("Maintenance")]
[Description("Module related to maintenance admin commands.")]
[Group("status", "maintenance")]
[RequireAuthority(AuthorityType.Owner)]
public class AdministratorMaintenanceModule : SaltyModuleBase
{
    private readonly IClusterCharacterService _characterService;
    private readonly IClusterStatusService _service;

    public AdministratorMaintenanceModule(IClusterStatusService service, IClusterCharacterService characterService)
    {
        _service = service;
        _characterService = characterService;
    }

    [Command("show", "count", "players", "channels")]
    [Description("Shows amount of players per channel")]
    public async Task<SaltyCommandResult> ShowStats()
    {
        ClusterCharacterGetSortedResponse response = null;
        try
        {
            response = await _characterService.GetCharactersSortedByChannel(new EmptyRpcRequest());
        }
        catch (Exception e)
        {
            Log.Error("[MAINTENANCE_MODULE][SHOW_STATS] Unexpected error: ", e);
        }

        if (response?.ResponseType != RpcResponseType.SUCCESS || response.CharactersByChannel == null)
        {
            return new SaltyCommandResult(false, "Couldn't obtain the information from ClusterCharacterService.");
        }

        Context.Player.SendInformationChatMessage("[======== CLUSTER =========]");
        foreach ((byte channelId, List<ClusterCharacterInfo> characterInfos) in response.CharactersByChannel)
        {
            Context.Player.SendInformationChatMessage($"Channel {channelId.ToString()}: {(characterInfos?.Count ?? 0).ToString()} players");
        }

        return new SaltyCommandResult(true);
    }

    [Command("schedule", "start")]
    [Description("Schedules a general maintenance")]
    public async Task<SaltyCommandResult> ScheduleMaintenance([Description("The maintenance delay")] string inputTimeSpan, [Description("Reason of the maintenance")] params string[] reason)
    {
        if (!TimeSpan.TryParse(inputTimeSpan, out TimeSpan timeSpan))
        {
            return new SaltyCommandResult(false, "Failed to parse the introduced delay");
        }

        StringBuilder stringBuilder = new();
        foreach (string substring in reason)
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(substring);
        }

        BasicRpcResponse response;
        try
        {
            response = await _service.ScheduleGeneralMaintenance(new ServiceScheduleGeneralMaintenanceRequest
            {
                ShutdownTimeSpan = timeSpan,
                Reason = stringBuilder.ToString().TrimStart()
            });
        }
        catch (Exception e)
        {
            Log.Error("[MAINTENANCE_MODULE][SCHEDULE_MAINTENANCE]", e);
            return new SaltyCommandResult(false, "Couldn't connect with the Cluster/Master");
        }

        return response.ResponseType != RpcResponseType.SUCCESS
            ? new SaltyCommandResult(false, "Cluster/Master denied the general maintenance")
            : new SaltyCommandResult(true, "A general maintenance has been scheduled!");
    }

    [Command("unschedule", "stop", "cancel")]
    [Description("Unschedules a general maintenance")]
    public async Task<SaltyCommandResult> UnscheduleMaintenance()
    {
        BasicRpcResponse response;
        try
        {
            response = await _service.UnscheduleGeneralMaintenance(new EmptyRpcRequest());
        }
        catch (Exception e)
        {
            Log.Error("[MAINTENANCE_MODULE][UNSCHEDULE_MAINTENANCE]", e);
            return new SaltyCommandResult(false, "Couldn't connect with the Cluster/Master");
        }

        return response.ResponseType != RpcResponseType.SUCCESS
            ? new SaltyCommandResult(false, "Cluster/Master denied the unschedule of the maintenance")
            : new SaltyCommandResult(true, "The maintenance has been unscheduled!");
    }

    [Command("emergency")]
    [Description("Executes an emergency general maintenance")]
    public async Task<SaltyCommandResult> ExecuteEmergencyMaintenance([Description("Reason of the maintenance")] params string[] reason)
    {
        StringBuilder stringBuilder = new();
        foreach (string substring in reason)
        {
            stringBuilder.Append(' ');
            stringBuilder.Append(substring);
        }

        BasicRpcResponse response;
        try
        {
            response = await _service.ExecuteGeneralEmergencyMaintenance(new ServiceExecuteGeneralEmergencyMaintenanceRequest
            {
                Reason = stringBuilder.ToString().TrimStart()
            });
        }
        catch (Exception e)
        {
            Log.Error("[MAINTENANCE_MODULE][EXECUTE_EMERGENCY_MAINTENANCE]", e);
            return new SaltyCommandResult(false, "Couldn't connect with the Cluster/Master");
        }

        return response.ResponseType != RpcResponseType.SUCCESS
            ? new SaltyCommandResult(false, "Cluster/Master denied the emergency maintenance")
            : new SaltyCommandResult(true, "The Emergency Maintenance has been executed!");
    }

    [Command("lift")]
    [Description("Lifts a general maintenance (even emergency ones)")]
    public async Task<SaltyCommandResult> LiftMaintenance()
    {
        BasicRpcResponse response;
        try
        {
            response = await _service.LiftGeneralMaintenance(new EmptyRpcRequest());
        }
        catch (Exception e)
        {
            Log.Error("[MAINTENANCE_MODULE][LIFT_MAINTENANCE]", e);
            return new SaltyCommandResult(false, "Couldn't connect with the Cluster/Master");
        }

        return response.ResponseType != RpcResponseType.SUCCESS
            ? new SaltyCommandResult(false, "Cluster/Master denied the maintenance lift")
            : new SaltyCommandResult(true, "The Maintenance has been lifted!");
    }

    [Group("feature")]
    [Description("Sub-module related to individual management of features")]
    public class AdministratorMaintenanceFeatureModule : SaltyModuleBase
    {
        private readonly IGameFeatureToggleManager _gameFeatureToggleManager;

        public AdministratorMaintenanceFeatureModule(IGameFeatureToggleManager gameFeatureToggleManager) => _gameFeatureToggleManager = gameFeatureToggleManager;

        [Command("disable")]
        [Description("Disable a enabled feature")]
        public async Task<SaltyCommandResult> DisableFeature([Description("Name of the feature")] GameFeature feature)
        {
            bool disabled = await _gameFeatureToggleManager.IsDisabled(feature);
            if (disabled)
            {
                return new SaltyCommandResult(false, "This feature is already disabled");
            }

            await _gameFeatureToggleManager.Disable(feature);

            return new SaltyCommandResult(true, $"Successfully disabled: {feature}");
        }

        [Command("enable")]
        [Description("Enable a game disabled feature")]
        public async Task<SaltyCommandResult> EnableFeature([Description("Name of the feature")] GameFeature feature)
        {
            bool disabled = await _gameFeatureToggleManager.IsDisabled(feature);
            if (!disabled)
            {
                return new SaltyCommandResult(false, "This feature is already enabled");
            }

            await _gameFeatureToggleManager.Enable(feature);

            return new SaltyCommandResult(true, $"Successfully enabled: {feature}");
        }

        [Command("list")]
        [Description("List all game feature status")]
        public async Task<SaltyCommandResult> List()
        {
            Context.Player.SendInformationChatMessage("[========= GAME FEATURES STATUS =========]");
            foreach (GameFeature feature in Enum.GetValues<GameFeature>())
            {
                bool disabled = await _gameFeatureToggleManager.IsDisabled(feature);
                Context.Player.SendInformationChatMessage($"{feature}: {(!disabled ? "ON" : "OFF")}");
            }

            Context.Player.SendInformationChatMessage("[========================================]");
            return new SaltyCommandResult(true);
        }
    }

    [Group("service")]
    [Description("Sub-Module related to individual management of services.")]
    public class AdministratorMaintenanceServiceModule : SaltyModuleBase
    {
        private readonly IClusterStatusService _service;

        public AdministratorMaintenanceServiceModule(IClusterStatusService service) => _service = service;

        [Command("list")]
        [Description("Lists the status of the cluster services")]
        public async Task<SaltyCommandResult> ListMaintenanceAsync()
        {
            IClientSession session = Context.Player;
            ServiceGetAllResponse resp = await _service.GetAllServicesStatus(new EmptyRpcRequest());

            session.SendInformationChatMessage("[========= CLUSTER STATUS =========]");
            foreach (Service service in resp.Services)
            {
                session.SendInformationChatMessage($"[{service.Id}] {service.Status.ToString()} - {service.LastUpdate:yyyy-MM-dd HH:mm:ss}");
            }

            session.SendInformationChatMessage("[=============================]");

            return new SaltyCommandResult(true, "Services listed");
        }

        [Command("set", "enable", "activate")]
        [Description("Sets a designated service in maintenance mode")]
        public async Task<SaltyCommandResult> EnableMaintenanceMode(string serviceName)
        {
            BasicRpcResponse resp = await _service.EnableMaintenanceMode(new ServiceBasicRequest
            {
                ServiceName = serviceName
            });
            if (resp.ResponseType != RpcResponseType.SUCCESS)
            {
                return new SaltyCommandResult(false, $"{serviceName} could not be put in maintenance mode");
            }

            return new SaltyCommandResult(true, $"{serviceName} is now in maintenance mode");
        }

        [Command("unset", "disable", "deactivate")]
        [Description("Sets a designated service in maintenance mode")]
        public async Task<SaltyCommandResult> DisableMaintenanceMode(string serviceName)
        {
            BasicRpcResponse resp = await _service.DisableMaintenanceMode(new ServiceBasicRequest
            {
                ServiceName = serviceName
            });
            if (resp.ResponseType != RpcResponseType.SUCCESS)
            {
                return new SaltyCommandResult(false, $"{serviceName} could not be removed from maintenance mode");
            }

            return new SaltyCommandResult(true, $"{serviceName} is not in maintenance mode anymore");
        }
    }

    [Group("channel-list", "cl")]
    [Description("Sub-Module related to management of the Login's Channel List.")]
    public class AdministratorMaintenanceChannelListModule : SaltyModuleBase
    {
        private readonly IServerApiService _serverApiService;

        public AdministratorMaintenanceChannelListModule(IServerApiService serverApiService) => _serverApiService = serverApiService;

        [Command("set")]
        [Description("Sets a designated channel's visibility")]
        public async Task<SaltyCommandResult> SetVisibility(int channelId, string worldGroup = "Ascension", AuthorityType authorityType = AuthorityType.User)
        {
            BasicRpcResponse resp = await _serverApiService.SetWorldServerVisibility(new SetWorldServerVisibilityRequest
            {
                ChannelId = channelId,
                WorldGroup = worldGroup,
                AuthorityRequired = authorityType
            });
            if (resp.ResponseType != RpcResponseType.SUCCESS)
            {
                return new SaltyCommandResult(false, $"[{channelId.ToString()}:{worldGroup}] Failed to set visibility");
            }

            return new SaltyCommandResult(true, $"[{channelId.ToString()}:{worldGroup}] Visibility set to '{authorityType.ToString()}'");
        }
    }
}