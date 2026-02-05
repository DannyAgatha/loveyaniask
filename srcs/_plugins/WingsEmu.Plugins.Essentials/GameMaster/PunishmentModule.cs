using System;
using System.Collections.Generic;
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

namespace WingsEmu.Plugins.Essentials.GameMaster;

[Name("Punishment")]
[Description("Module related to player punishment system")]
[RequireAuthority(AuthorityType.GM)]
public class PunishmentModule : SaltyModuleBase
{
    private readonly IAccountService _accountService;
    private readonly ICharacterService _characterService;
    private readonly IMessagePublisher<PlayerKickMessage> _kickMessage;
    private readonly ISessionManager _sessionManager;
    private readonly IScheduler _scheduler;

    public PunishmentModule(IMessagePublisher<PlayerKickMessage> kickMessage, ISessionManager sessionManager, ICharacterService characterService, IAccountService accountService,
        IScheduler scheduler)
    {
        _kickMessage = kickMessage;
        _sessionManager = sessionManager;
        _characterService = characterService;
        _accountService = accountService;
        _scheduler = scheduler;
    }

    [Command("penaltyinfo")]
    [Description("Get player penalty history")]
    public async Task<SaltyCommandResult> PenaltyInfo(IClientSession target)
    {
        if (!target.Account.Logs.Any())
        {
            return new SaltyCommandResult(true, "Player doesn't have any penalties!");
        }

        IClientSession session = Context.Player;
        foreach (AccountPenaltyDto penalty in target.Account.Logs)
        {
            session.SendErrorChatMessage($"------[Id: {penalty.Id}]-------");
            session.SendInformationChatMessage($"Penalty type: {penalty.PenaltyType.ToString()}");
            session.SendInformationChatMessage($"Judge name: {penalty.JudgeName}");
            session.SendInformationChatMessage($"Target name: {penalty.TargetName}");
            session.SendInformationChatMessage($"Start: {penalty.Start}");
            session.SendInformationChatMessage($"Mute remaining time: {penalty.RemainingTime}");
            session.SendInformationChatMessage($"Reason: {penalty.Reason}");
            if (!string.IsNullOrEmpty(penalty.UnlockReason))
            {
                session.SendInformationChatMessage($"Unlock reason: {penalty.UnlockReason}");
            }

            session.SendErrorChatMessage("----------------");
        }

        return new SaltyCommandResult(true);
    }

    [Command("penaltyinfo")]
    [Description("Get penalty history by account id")]
    public async Task<SaltyCommandResult> PenaltyInfo(long accountId)
    {
        AccountPenaltyGetAllResponse response = null;
        try
        {
            response = await _accountService.GetAccountPenalties(new AccountPenaltyGetRequest
            {
                AccountId = accountId
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'penaltyinfo'] Unexpected error: ", e);
        }

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, "Couldn't retrieve the account's penalties from AccountService");
        }

        IEnumerable<AccountPenaltyDto> penalties = response.AccountPenaltyDtos.ToList();

        if (!penalties.Any())
        {
            return new SaltyCommandResult(true, $"No history was found in the database by account id: {accountId}.");
        }

        IClientSession session = Context.Player;
        foreach (AccountPenaltyDto penalty in penalties)
        {
            session.SendErrorChatMessage($"------[Id: {penalty.Id}]-------");
            session.SendInformationChatMessage($"Penalty type: {penalty.PenaltyType.ToString()}");
            session.SendInformationChatMessage($"Judge name: {penalty.JudgeName}");
            session.SendInformationChatMessage($"Target name: {penalty.TargetName}");
            session.SendInformationChatMessage($"Start: {penalty.Start}");
            session.SendInformationChatMessage($"Mute remaining time: {penalty.RemainingTime}");
            session.SendInformationChatMessage($"Reason: {penalty.Reason}");
            if (!string.IsNullOrEmpty(penalty.UnlockReason))
            {
                session.SendInformationChatMessage($"Unlock reason: {penalty.UnlockReason}");
            }

            session.SendErrorChatMessage("----------------");
        }

        return new SaltyCommandResult(true);
    }

    [Command("kick-id")]
    [Description("Kick player by player id")]
    public async Task<SaltyCommandResult> KickAsync(long playerId)
    {
        IClientSession session = _sessionManager.GetSessionByCharacterId(playerId);
        if (session != null)
        {
            session.ForceDisconnect();
            return new SaltyCommandResult(true, $"Player [{session.PlayerEntity.Name}] has been kicked.");
        }

        if (!_sessionManager.IsOnline(playerId))
        {
            return new SaltyCommandResult(false, "Player is offline");
        }

        await _kickMessage.PublishAsync(new PlayerKickMessage
        {
            PlayerId = playerId
        });

        return new SaltyCommandResult(true, $"Kicking player with ID: {playerId} on different channel...");
    }

    [Command("kick")]
    [Description("Kick player by player name")]
    public async Task<SaltyCommandResult> KickAsync(string playerName)
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

    [Command("mute")]
    [Description("Mute player (duration in minutes)")]
    public async Task<SaltyCommandResult> Mute(IClientSession target, short minutes, [Remainder] string reason)
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

    [Command("unmute")]
    [Description("Unmute player")]
    public async Task<SaltyCommandResult> Unmute(IClientSession target, [Remainder] string reason)
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

    [Command("ban")]
    [Description("Ban player by player id (duration in hours - max 1 week)")]
    public async Task<SaltyCommandResult> Ban(long playerId, short hours, [Remainder] string reason)
    {
        IClientSession session = Context.Player;
        IClientSession target = _sessionManager.GetSessionByCharacterId(playerId);

        // Max 1 week
        var time = TimeSpan.FromHours(hours > 168 ? 168 : hours);
        DateTime utcNow = DateTime.UtcNow;

        AccountBanDto newBan = new()
        {
            JudgeName = session.PlayerEntity.Name,
            Start = utcNow,
            End = utcNow + time,
            Reason = reason
        };

        if (target != null)
        {
            newBan.AccountId = target.Account.Id;
            newBan.TargetName = target.PlayerEntity.Name;
            AccountBanSaveResponse response = null;
            try
            {
                response = await _accountService.SaveAccountBan(new AccountBanSaveRequest
                {
                    AccountBanDto = newBan
                });
            }
            catch (Exception e)
            {
                Log.Error("[PUNISHMENT_MODULE][Command: 'ban'] Unexpected error: ", e);
            }

            if (response?.ResponseType != RpcResponseType.SUCCESS)
            {
                return new SaltyCommandResult(false, "Couldn't save the AccountBan through AccountService");
            }

            target.BroadcastEffectInRange(Game._enum.EffectType.Ban);
            _scheduler.Schedule(TimeSpan.FromMilliseconds(2200), () =>
            {
                target.ForceDisconnect();
            });

            return new SaltyCommandResult(true, $"Player [{target.PlayerEntity.Name}] has been banned for [{reason}] with duration of [{hours}] hours.");
        }

        DbServerGetCharacterResponse targetResponse = null;
        try
        {
            targetResponse = await _characterService.GetCharacterById(new DbServerGetCharacterByIdRequest
            {
                CharacterId = playerId
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'ban'] Unexpected error: ", e);
        }

        if (targetResponse?.RpcResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, "Couldn't retrieve the CharacterDTO from CharacterService");
        }

        CharacterDTO targetCharacter = targetResponse.CharacterDto;

        await _kickMessage.PublishAsync(new PlayerKickMessage
        {
            PlayerId = playerId
        });

        newBan.AccountId = targetCharacter.AccountId;
        newBan.TargetName = targetCharacter.Name;
        AccountBanSaveResponse response2 = null;
        try
        {
            response2 = await _accountService.SaveAccountBan(new AccountBanSaveRequest
            {
                AccountBanDto = newBan
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'ban'] Unexpected error: ", e);
        }

        return response2?.ResponseType != RpcResponseType.SUCCESS
            ? new SaltyCommandResult(false, "Couldn't save the AccountBan through AccountService")
            : new SaltyCommandResult(true, $"Banning player with ID: [{playerId}] for [{reason}] with duration of [{hours}] hours.");
    }

    [Command("ban")]
    [Description("Ban player by player name (duration in hours)")]
    public async Task<SaltyCommandResult> Ban(string playerName, short hours, [Remainder] string reason)
    {
        IClientSession session = Context.Player;
        IClientSession target = _sessionManager.GetSessionByCharacterName(playerName);

        // Max 1 week
        var time = TimeSpan.FromHours(hours > 168 ? 168 : hours);
        DateTime utcNow = DateTime.UtcNow;

        AccountBanDto newBan = new()
        {
            JudgeName = session.PlayerEntity.Name,
            Start = utcNow,
            End = utcNow + time,
            Reason = reason
        };

        if (target != null)
        {
            newBan.AccountId = target.Account.Id;
            newBan.TargetName = target.PlayerEntity.Name;

            AccountBanSaveResponse response = null;
            try
            {
                response = await _accountService.SaveAccountBan(new AccountBanSaveRequest
                {
                    AccountBanDto = newBan
                });
            }
            catch (Exception e)
            {
                Log.Error("[PUNISHMENT_MODULE][Command: 'ban'] Unexpected error: ", e);
            }

            if (response?.ResponseType != RpcResponseType.SUCCESS)
            {
                return new SaltyCommandResult(false, "Couldn't save the AccountBan through AccountService");
            }

            target.BroadcastEffectInRange(Game._enum.EffectType.Ban);
            _scheduler.Schedule(TimeSpan.FromMilliseconds(2200), () =>
            {
                target.ForceDisconnect();
            });

            return new SaltyCommandResult(true, $"Player [{playerName}] has been banned for [{reason}] with duration of [{hours}] hours.");
        }

        DbServerGetCharacterResponse targetResponse = null;
        try
        {
            targetResponse = await _characterService.GetCharacterByName(new DbServerGetCharacterRequestByName
            {
                CharacterName = playerName
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'ban'] Unexpected error: ", e);
        }

        if (targetResponse?.RpcResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, "Couldn't retrieve the CharacterDTO from CharacterService");
        }

        CharacterDTO targetCharacter = targetResponse.CharacterDto;

        await _kickMessage.PublishAsync(new PlayerKickMessage
        {
            PlayerName = playerName
        });

        newBan.AccountId = targetCharacter.AccountId;
        newBan.TargetName = targetCharacter.Name;

        AccountBanSaveResponse response2 = null;
        try
        {
            response2 = await _accountService.SaveAccountBan(new AccountBanSaveRequest
            {
                AccountBanDto = newBan
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'ban'] Unexpected error: ", e);
        }

        return response2?.ResponseType != RpcResponseType.SUCCESS
            ? new SaltyCommandResult(false, "Couldn't save the AccountBan through AccountService")
            : new SaltyCommandResult(true, $"Banning [{playerName}] for [{reason}] with duration of [{hours}] hours.");
    }

    [Command("unban")]
    [Description("Unban player by player id")]
    public async Task<SaltyCommandResult> Unban(long playerId, [Remainder] string reason)
    {
        DbServerGetCharacterResponse targetResponse = null;
        try
        {
            targetResponse = await _characterService.GetCharacterById(new DbServerGetCharacterByIdRequest
            {
                CharacterId = playerId
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'unban'] Unexpected error: ", e);
        }

        if (targetResponse?.RpcResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, "Couldn't retrieve the CharacterDTO from CharacterService");
        }

        CharacterDTO target = targetResponse.CharacterDto;

        AccountBanGetResponse response = null;
        try
        {
            response = await _accountService.GetAccountBan(new AccountBanGetRequest
            {
                AccountId = target.AccountId
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'unban'] Unexpected error: ", e);
        }

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, "Couldn't get the AccountBan through AccountService");
        }

        AccountBanDto ban = response.AccountBanDto;

        ban.End = DateTime.UtcNow;
        ban.UnlockReason = reason;

        AccountBanSaveResponse saveResponse = null;
        try
        {
            saveResponse = await _accountService.SaveAccountBan(new AccountBanSaveRequest
            {
                AccountBanDto = ban
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'unban'] Unexpected error: ", e);
        }

        return saveResponse?.ResponseType != RpcResponseType.SUCCESS
            ? new SaltyCommandResult(false, "Couldn't save the AccountBan through AccountService")
            : new SaltyCommandResult(true, $"Player [{target.Name}] has been unbanned with the reason [{reason}]");
    }

    [Command("unban")]
    [Description("Unban player by player name")]
    public async Task<SaltyCommandResult> Unban(string playerName, [Remainder] string reason)
    {
        DbServerGetCharacterResponse targetResponse = null;
        try
        {
            targetResponse = await _characterService.GetCharacterByName(new DbServerGetCharacterRequestByName
            {
                CharacterName = playerName
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'unban'] Unexpected error: ", e);
        }

        if (targetResponse?.RpcResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, "Couldn't retrieve the CharacterDTO from CharacterService");
        }

        CharacterDTO target = targetResponse.CharacterDto;

        AccountBanGetResponse response = null;
        try
        {
            response = await _accountService.GetAccountBan(new AccountBanGetRequest
            {
                AccountId = target.AccountId
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'unban'] Unexpected error: ", e);
        }

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, "Couldn't get the AccountBan through AccountService");
        }

        AccountBanDto ban = response.AccountBanDto;
        ban.End = DateTime.UtcNow;
        ban.UnlockReason = reason;
        AccountBanSaveResponse saveResponse = null;
        try
        {
            saveResponse = await _accountService.SaveAccountBan(new AccountBanSaveRequest
            {
                AccountBanDto = ban
            });
        }
        catch (Exception e)
        {
            Log.Error("[PUNISHMENT_MODULE][Command: 'ban'] Unexpected error: ", e);
        }

        return saveResponse?.ResponseType != RpcResponseType.SUCCESS
            ? new SaltyCommandResult(false, "Couldn't save the AccountBan through AccountService")
            : new SaltyCommandResult(true, $"Player [{playerName}] has been unbanned with the reason [{reason}]");
    }
}