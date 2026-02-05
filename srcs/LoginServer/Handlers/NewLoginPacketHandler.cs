using System;
using System.Linq;
using System.Threading.Tasks;
using LoginServer.Auth;
using LoginServer.Network;
using PhoenixLib.Logging;
using PhoenixLib.MultiLanguage;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.ServerApi;
using WingsAPI.Communication.ServerApi.Protocol;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;
using WingsAPI.Data.Account;
using WingsAPI.Packets.ClientPackets;
using WingsEmu.DTOs.Account;
using WingsEmu.Health;
using WingsEmu.Packets.Enums;

namespace LoginServer.Handlers;
public class NewLoginPacketHandler : GenericLoginPacketHandlerBase<Nos0577Packet>
{
    private readonly IAccountService _accountService;
    private readonly IMaintenanceManager _maintenanceManager;
    private readonly IServerApiService _serverApiService;
    private readonly ISessionService _sessionService;
    private readonly ICharacterService _characterService;
    
    public NewLoginPacketHandler(
        ISessionService sessionService,
        IServerApiService serverApiService,
        IMaintenanceManager maintenanceManager,
        IAccountService accountService,
        ICharacterService characterService)
    {
        _sessionService = sessionService;
        _serverApiService = serverApiService;
        _maintenanceManager = maintenanceManager;
        _accountService = accountService;
        _characterService = characterService;
    }
    protected override async Task HandlePacketAsync(LoginClientSession session, Nos0577Packet nos0577Packet)
    {
        if (nos0577Packet == null)
        {
            return;
        }
        
        string[] clientVersion = nos0577Packet.ClientVersion.Split('\v');
        
        SessionResponse sessionResponse = await GetSessionAsync(session, nos0577Packet);
        if (sessionResponse == null)
        {
            return;
        }

        AccountLoadResponse accountLoadResponse = await LoadAccountAsync(session, sessionResponse);
        if (accountLoadResponse == null)
        {
            return;
        }

        AccountDTO loadedAccount = accountLoadResponse.AccountDto;
        
        if (await CheckBanStatusAsync(session, loadedAccount))
        {
            return;
        }

        if (!await HandleAccountAuthorityAsync(session, loadedAccount))
        {
            return;
        }

        await ConnectAndSendChannelList(session, loadedAccount, clientVersion[1], nos0577Packet.Hwid, clientVersion[0]);
    }
    private async Task<SessionResponse> GetSessionAsync(LoginClientSession session, Nos0577Packet nos0577Packet)
    {
        try
        {
            SessionResponse sessionResponse = await _sessionService.GetSessionBySessionId(new GetSessionBySessionIdRequest
            {
                SessionId = nos0577Packet.SessionHex.DecodeHexString()
            });
            if (sessionResponse?.ResponseType != RpcResponseType.SUCCESS)
            {
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
                Log.Debug("[NEW_PIPE_AUTH] Session not found: " + nos0577Packet.SessionHex.DecodeHexString());
                return null;
            }
            if (sessionResponse.Session.State != SessionState.Disconnected)
            {
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.AlreadyConnected));
                Log.Debug("[NEW_PIPE_AUTH] Session is not disconnected: " + sessionResponse.Session.AccountName);
                return null;
            }
            return sessionResponse;
        }
        catch (Exception e)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
            Log.Error("[NEW_PIPE_AUTH] Unexpected error: ", e);
            return null;
        }
    }
    private async Task<AccountLoadResponse> LoadAccountAsync(LoginClientSession session, SessionResponse sessionResponse)
    {
        try
        {
            AccountLoadResponse accountLoadResponse = await _accountService.LoadAccountByName(new AccountLoadByNameRequest
            {
                Name = sessionResponse.Session.AccountName
            });
            
            if (accountLoadResponse?.ResponseType != RpcResponseType.SUCCESS)
            {
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
                Log.Debug("[NEW_PIPE_AUTH] Account not found: " + sessionResponse.Session.AccountName);
                return null;
            }
            
            return accountLoadResponse;
        }
        catch (Exception e)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
            Log.Error("[NEW_PIPE_AUTH] Unexpected error: ", e);
            return null;
        }
    }
    private async Task<bool> CheckBanStatusAsync(LoginClientSession session, AccountDTO loadedAccount)
    {
        try
        {
            AccountBanGetResponse banResponse = await _accountService.GetAccountBan(new AccountBanGetRequest
            {
                AccountId = loadedAccount.Id
            });
            
            if (banResponse?.ResponseType != RpcResponseType.SUCCESS)
            {
                Log.Warn($"[NEW_PIPE_AUTH] Failed to get account ban for accountId: '{loadedAccount.Id.ToString()}'");
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
                session.Disconnect();
                return true;
            }
            AccountBanDto characterPenalty = banResponse.AccountBanDto;
            
            if (characterPenalty != null)
            {
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.Banned));
                Log.Debug($"[NEW_PIPE_AUTH] ACCOUNT_BANNED : {loadedAccount.Name}");
                session.Disconnect();
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
            Log.Error("[NEW_PIPE_AUTH] Unexpected error: ", e);
            return true;
        }
    }
    private async Task<bool> HandleAccountAuthorityAsync(LoginClientSession session, AccountDTO loadedAccount)
    {
        switch (loadedAccount.Authority)
        {
            case AuthorityType.Banned:
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.Banned));
                Log.Debug("[NEW_PIPE_AUTH] ACCOUNT_BANNED");
                session.Disconnect();
                return false;
            case AuthorityType.Unconfirmed:
            case AuthorityType.Closed:
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
                Log.Debug("[NEW_PIPE_AUTH] ACCOUNT_NOT_VERIFIED");
                session.Disconnect();
                return false;
            default:
                if (_maintenanceManager.IsMaintenanceActive && loadedAccount.Authority < AuthorityType.GM)
                {
                    session.SendPacket(session.GenerateFailcPacket(LoginFailType.Maintenance));
                    return false;
                }
                return true;
        }
    }
    
    private async Task ConnectAndSendChannelList(LoginClientSession session, AccountDTO loadedAccount, string clientVersion, string hwid, string lang)
    {
        SessionResponse connectResponse = await _sessionService.ConnectToLoginServer(new ConnectToLoginServerRequest
        {
            AccountId = loadedAccount.Id,
            ClientVersion = clientVersion,
            HardwareId = hwid
        });

        if (connectResponse.ResponseType != RpcResponseType.SUCCESS)
        {
            Log.Warn("[NEW_PIPE_AUTH] General Error SessionId: " + session.Id);
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
            session.Disconnect();
            return;
        }

        Session connectedSession = connectResponse.Session;
        Log.Debug($"[NEW_PIPE_AUTH] Connected : {loadedAccount.Name}:{connectedSession.EncryptionKey}:{connectedSession.HardwareId}");

        RetrieveRegisteredWorldServersResponse worldServersResponse = await _serverApiService.RetrieveRegisteredWorldServers(new RetrieveRegisteredWorldServersRequest
        {
            RequesterAuthority = loadedAccount.Authority
        });

        if (worldServersResponse?.WorldServers is null || worldServersResponse.WorldServers.Count == 0)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.Maintenance));
            session.Disconnect();
            return;
        }
        
        short language = short.Parse(lang);
        
        byte characterInServer = 0;
        DbServerGetCharactersResponse response = await _characterService.GetCharacters(new DbServerGetCharactersRequest
        {
            AccountId = loadedAccount.Id
        });

        if (response.Characters != null)
        {
            characterInServer = (byte)response.Characters.Count();
        }
        
        session.SendChannelPacketList(connectedSession.EncryptionKey, loadedAccount.Name, (RegionLanguageType)language, worldServersResponse.WorldServers, false, characterInServer);
        session.Disconnect();
    }
}