// NosEmu
// 


using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.DAL;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Polly;
using Polly.Retry;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Data.Account;
using WingsAPI.Data.Character;
using WingsEmu.Game;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class SessionSaveEventHandler : IAsyncEventProcessor<SessionSaveEvent>
{
    private readonly IAccountService _accountService;
    private readonly ICharacterService _characterService;
    private readonly IMapper<AccountDTO, Account> _mapper;
    private readonly IPlayerEntityFactory _playerEntityFactory;

    public SessionSaveEventHandler(IPlayerEntityFactory playerEntityFactory, ICharacterService characterService, IAccountService accountService, IMapper<AccountDTO, Account> mapper)
    {
        _playerEntityFactory = playerEntityFactory;
        _characterService = characterService;
        _accountService = accountService;
        _mapper = mapper;
    }

    public async Task HandleAsync(SessionSaveEvent e, CancellationToken cancellation)
    {
        IPlayerEntity playerEntity = e.Sender.PlayerEntity;
        AccountDTO account = _mapper.Map(e.Sender.Account);
        long accountId = account.Id;
        AsyncRetryPolicy policy = Policy.Handle<Exception>().RetryAsync(3,
            (exception, i1) => Log.Error($"[CHARACTER_SAVE][ACCOUNT_ID: '{accountId.ToString()}'] Failed to save characters, try {i1.ToString()}. ", exception));
        AccountSaveResponse response = null;
        try
        {
            response = await policy.ExecuteAsync(() => _accountService.SaveAccount(new AccountSaveRequest
            {
                AccountDto = account
            }));
        }
        catch (Exception ex)
        {
            Log.Error($"[CHARACTER_SAVE][ACCOUNT_ID: '{account.Id.ToString()}'] Unexpected error: ", ex);
        }

        if (response?.ResponseType != RpcResponseType.SUCCESS)
        {
            Log.Warn($"[CHARACTER_SAVE][ACCOUNT_ID: '{account.Id.ToString()}'] Failed to save account");
        }

        CharacterDTO characterDto = _playerEntityFactory.CreateCharacterDto(playerEntity);
        DbServerSaveCharacterResponse response2 = null;
        try
        {
            response2 = await _characterService.SaveCharacter(new DbServerSaveCharacterRequest
            {
                Character = characterDto
            });
        }
        catch (Exception ex)
        {
            Log.Error($"[CHARACTER_SAVE][CHARACTER_ID: '{characterDto.Id.ToString()}'] Unexpected error: ", ex);
        }

        if (response2?.RpcResponseType != RpcResponseType.SUCCESS)
        {
            Log.Warn($"[CHARACTER_SAVE][CHARACTER_ID: '{characterDto.Id.ToString()}'] Failed to save character");
        }

        Log.Warn($"{characterDto.Name} was saved");

        CheckPlayerMute(playerEntity);
        AccountPenaltyMultiSaveResponse response3 = null;
        try
        {
            response3 = await _accountService.SaveAccountPenalties(new AccountPenaltyMultiSaveRequest
            {
                AccountPenaltyDtos = e.Sender.Account.Logs
            });
        }
        catch (Exception ex)
        {
            Log.Error($"[CHARACTER_SAVE][ACCOUNT_ID: '{account.Id.ToString()}'] Unexpected error: ", ex);
        }

        if (response3?.ResponseType != RpcResponseType.SUCCESS)
        {
            Log.Warn($"[CHARACTER_SAVE][ACCOUNT_ID: '{account.Id.ToString()}'] Failed to save account's penalties");
        }
    }

    private void CheckPlayerMute(IPlayerEntity playerEntity)
    {
        if (!playerEntity.MuteRemainingTime.HasValue)
        {
            return;
        }

        AccountPenaltyDto penalty = playerEntity.Session.Account.Logs.FirstOrDefault(x => x.PenaltyType == PenaltyType.Muted && x.RemainingTime.HasValue);
        if (penalty == null)
        {
            return;
        }

        if (playerEntity.MuteRemainingTime.Value.TotalMilliseconds > 0)
        {
            penalty.RemainingTime = (int?)playerEntity.MuteRemainingTime.Value.TotalSeconds;
        }
        else
        {
            penalty.RemainingTime = null;
        }
    }
}