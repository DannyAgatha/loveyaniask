using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;

namespace NosEmu.Plugins.BasicImplementations.Event.Characters;

public class CharacterPreLoadEventHandler : IAsyncEventProcessor<CharacterPreLoadEvent>
{
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly ICharacterService _characterService;
    private readonly IServerManager _serverManager;
    private readonly ISessionManager _sessionManager;
    private readonly ISessionService _sessionService;


    public CharacterPreLoadEventHandler(ISessionService sessionService, ICharacterService characterService, IServerManager serverManager, ISessionManager sessionManager)
    {
        _sessionService = sessionService;
        _characterService = characterService;
        _serverManager = serverManager;
        _sessionManager = sessionManager;
    }

    public async Task HandleAsync(CharacterPreLoadEvent e, CancellationToken cancellation)
    {
        await _semaphoreSlim.WaitAsync(cancellation);
        try
        {
            IClientSession session = e.Sender;
            if (session?.Account == null || session.HasSelectedCharacter)
            {
                return;
            }

            SessionResponse sessionResponse = await _sessionService.GetSessionByAccountId(new GetSessionByAccountIdRequest { AccountId = session.Account.Id });
            if (sessionResponse is not { ResponseType: RpcResponseType.SUCCESS })
            {
                session.ForceDisconnect();
                return;
            }

            Session sharedSession = sessionResponse.Session;

            if (sharedSession.State != SessionState.CharacterSelection)
            {
                session.ForceDisconnect();
                return;
            }

            if (sharedSession.ChannelId != _serverManager.ChannelId)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][PRE_LOAD] Wrong Channel: {sharedSession.AccountId} - {session.Account.MasterAccountId} - {sharedSession.HardwareId}");
                session.ForceDisconnect();
                return;
            }

            if (session.SessionId != sharedSession.EncryptionKey)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][PRE_LOAD] Wrong Encryption Key: {sharedSession.AccountId} - {session.Account.MasterAccountId} - {sharedSession.HardwareId}");
                session.ForceDisconnect();
                return;
            }

            DbServerGetCharacterResponse response = await _characterService.GetCharacterBySlot(new DbServerGetCharacterFromSlotRequest
            {
                AccountId = session.Account.Id,
                Slot = e.Slot
            });

            if (response.RpcResponseType != RpcResponseType.SUCCESS)
            {
                Log.Warn("[CHARACTER_SCREEN] Selected a character that does not exist");
                session.ForceDisconnect();
                return;
            }

            SessionResponse tmp = await _sessionService.ConnectCharacter(new ConnectCharacterRequest
            {
                AccountId = session.Account.Id,
                ChannelId = _serverManager.ChannelId,
                CharacterId = response.CharacterDto.Id
            });

            if (tmp.ResponseType != RpcResponseType.SUCCESS)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][PRE_LOAD] Looks like {response.CharacterDto.Name} - {session.Account.MasterAccountId} - {sharedSession.HardwareId} was already connected and tried to connect to {_serverManager.ChannelId}");
                session.ForceDisconnect();
                return;
            }

            if (sharedSession.State == SessionState.CrossChannelAuthentication && _serverManager.ChannelId != sharedSession.AllowedCrossChannelId)
            {
                session.ForceDisconnect();
                return;
            }

            if (sharedSession.State != SessionState.CharacterSelection)
            {
                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][PRE_LOAD] Looks like accountId: {sharedSession.AccountId} - {response.CharacterDto.Name} - {sharedSession.HardwareId} was already connected on the channel while being on {_serverManager.ChannelId}");
                session.ForceDisconnect();
                return;
            }

            if (_sessionManager.IsOnline(response.CharacterDto.Id))
            {
                session.ForceDisconnect();
                return;
            }

            IClientSession alreadyConnectedSession = _sessionManager.GetSessionByCharacterId(response.CharacterDto.Id);
            if (alreadyConnectedSession != null)
            {
                await alreadyConnectedSession.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][PRE_LOAD] Looks like {alreadyConnectedSession.PlayerEntity.Name} was already connected on the channel while being on {_serverManager.ChannelId}");
                alreadyConnectedSession.ForceDisconnect();

                await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.DANGER,
                    $"[POTENTIAL_DUPE][PRE_LOAD] Looks like {response.CharacterDto.Name} - {sharedSession.HardwareId} was already connected on the channel while being on {_serverManager.ChannelId}");
                session.ForceDisconnect();
                return;
            }

            session.SelectedCharacterSlot = e.Slot;

            session.SendPacket("OK");
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}