using PhoenixLib.Events;
using WingsAPI.Communication.Sessions.Model;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Alzanor;
using WingsEmu.Game.Alzanor.Configurations;
using WingsEmu.Game.Alzanor.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Alzanor.EventHandlers;

public class AlzanorProcessLifeEventHandler : IAsyncEventProcessor<AlzanorProcessLifeEvent>
{
    private readonly IAlzanorManager _alzanorManager;

    public AlzanorProcessLifeEventHandler(IAlzanorManager alzanorManager)
    {
        _alzanorManager = alzanorManager;
    }

    public async Task HandleAsync(AlzanorProcessLifeEvent e, CancellationToken cancellation)
    {
        if (!_alzanorManager.IsActive)
            return;
        
        List<AlzanorEventStats>? top5Players = _alzanorManager.GetTopPlayers();
        if (top5Players == null)
        {
            return;
        }
        
        var rankingStrings = top5Players
            .Select((player, index) => $"[{index + 1}] {player.Player.PlayerEntity.Name} | {player.Points} Points | {player.Kills} / {player.Deaths}")
            .ToList();
        
        await BroadcastMessageAsync("=== Alzanor Ranking ===");
        
        foreach (string ranking in rankingStrings)
        {
            await BroadcastMessageAsync(ranking);
        }
        
        foreach (IClientSession? player in _alzanorManager.AlzanorInstance.Sessions)
        {
            AlzanorEventStats? playerResult = _alzanorManager.GetAlzanorEventStats(player);
            if (playerResult == null)
            {
                continue;
            }
            
            int points = playerResult.Points < 0 ? 0 : playerResult.Points;
            player.SendChatMessageNoId($"*** {points} Points | {playerResult.Kills} / {playerResult.Deaths} ***", ChatMessageColorType.Blue);
        }
        
        await BroadcastMessageAsync("=================");
    }
    
    private Task BroadcastMessageAsync(string message)
    {
        return _alzanorManager.AlzanorInstance.BroadcastAsync(_ =>
            Task.FromResult($"say 1 -1 6 {message}"));
    }
}
