using System;
using System.Linq;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using PhoenixLib.Scheduler;
using PhoenixLib.ServiceBus;
using Qmmands;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Punishment;
using WingsAPI.Data.Account;
using WingsAPI.Data.Character;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.Essentials.GS;

[Name("GsPunishment")]
[Description("Module related to player punishment system")]
[RequireAuthority(AuthorityType.GS)]
public class GsCommandModule: SaltyModuleBase
{
    private readonly IAccountService _accountService;
    private readonly ICharacterService _characterService;
    private readonly IMessagePublisher<PlayerKickMessage> _kickMessage;
    private readonly ISessionManager _sessionManager;
    private readonly IScheduler _scheduler;
    public GsCommandModule(IMessagePublisher<PlayerKickMessage> kickMessage, ISessionManager sessionManager, ICharacterService characterService, IAccountService accountService, IScheduler scheduler)
    {
        _kickMessage = kickMessage;
        _sessionManager = sessionManager;
        _characterService = characterService;
        _accountService = accountService;
        _scheduler = scheduler;
    }

    [Command("KickPlayer")]
    [Description("Kick player by player name")]
    public async Task<SaltyCommandResult> KickPlayerAsync(string playerName)
    {
        IClientSession session = _sessionManager.GetSessionByCharacterName(playerName);
        if (session != null)
        {
            session.ForceDisconnect();
            return new SaltyCommandResult(true, $"Player [{playerName}] has been kicked.");
        }

        if (!_sessionManager.IsOnline(playerName))
        {
            return new SaltyCommandResult(false, "Player is offline");
        }

        await _kickMessage.PublishAsync(new PlayerKickMessage
        {
            PlayerName = playerName
        });

        return new SaltyCommandResult(true, $"Kicking player [{playerName}] on different channel...");
    }

    [Command("MutePlayer")]
    [Description("Mute player (duration in minutes)")]
    public async Task<SaltyCommandResult> MutePlayer(IClientSession target, short minutes, [Remainder] string reason)
    {
        if (target.PlayerEntity.MuteRemainingTime.HasValue)
        {
            string timeLeft = target.PlayerEntity.MuteRemainingTime.Value.ToString(@"hh\:mm\:ss");
            return new SaltyCommandResult(false, $"Player is already muted - time left: {timeLeft}");
        }

        DateTime now = DateTime.UtcNow;
        var time = TimeSpan.FromMinutes(minutes);
        int? seconds = (int?)time.TotalSeconds;

        IClientSession session = Context.Player;
        AccountPenaltyDto newPenalty = new()
        {
            JudgeName = session.PlayerEntity.Name,
            TargetName = target.PlayerEntity.Name,
            AccountId = target.Account.Id,
            Start = now,
            RemainingTime = seconds,
            PenaltyType = PenaltyType.Muted,
            Reason = reason
        };

        target.Account.Logs.Add(newPenalty);
        target.PlayerEntity.MuteRemainingTime = TimeSpan.FromSeconds(time.TotalSeconds);
        target.PlayerEntity.LastChatMuteMessage = DateTime.MinValue;
        target.PlayerEntity.LastMuteTick = now;
        return new SaltyCommandResult(true, $"Player [{target.PlayerEntity.Name}] has been muted for [{reason}].");
    }

    [Command("UnmutePlayer")]
    [Description("Unmute player")]
    public async Task<SaltyCommandResult> UnmutePlayer(IClientSession target, [Remainder] string reason)
    {
        TimeSpan? muteTime = target.PlayerEntity.MuteRemainingTime;
        if (muteTime == null)
        {
            return new SaltyCommandResult(false, $"Player [{target.PlayerEntity.Name}] isn't muted.");
        }

        target.PlayerEntity.LastChatMuteMessage = null;
        target.PlayerEntity.MuteRemainingTime = null;

        AccountPenaltyDto penalty = target.Account.Logs.FirstOrDefault(x => x.PenaltyType == PenaltyType.Muted && x.RemainingTime.HasValue);
        if (penalty != null)
        {
            penalty.UnlockReason = reason;
            penalty.RemainingTime = null;
        }

        return new SaltyCommandResult(true, $"Player [{target.PlayerEntity.Name}] has been unmuted.");
    }
}