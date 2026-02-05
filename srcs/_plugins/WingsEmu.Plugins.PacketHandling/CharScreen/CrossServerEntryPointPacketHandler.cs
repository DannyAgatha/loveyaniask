// NosEmu
// 


using System;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;
using WingsAPI.Data.Account;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsEmu.DTOs.Account;
using WingsEmu.Game;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Health;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.CharScreen;

public class CrossServerEntryPointPacketHandler : GenericCharScreenPacketHandlerBase<CrossServerEntrypointPacket>
{
    private readonly IAccountService _accountService;
    private readonly IMaintenanceManager _maintenanceManager;
    private readonly IServerManager _serverManager;
    private readonly ISessionService _sessionService;

    public CrossServerEntryPointPacketHandler(IServerManager serverManager, ISessionService sessionService, IMaintenanceManager maintenanceManager, IAccountService accountService)
    {
        _sessionService = sessionService;
        _maintenanceManager = maintenanceManager;
        _accountService = accountService;
        _serverManager = serverManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, CrossServerEntrypointPacket packet)
    {
        if (session.Account != null)
        {
            Log.Warn("CrossServerEntryPointPacketHandler session.Account != null");
            return;
        }

        SessionResponse sessionResponse = await _sessionService.GetSessionByAccountName(new GetSessionByAccountNameRequest { AccountName = packet.AccountName });
        if (sessionResponse.ResponseType != RpcResponseType.SUCCESS)
        {
            Log.Info("Session response incorrect");
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
            session.ForceDisconnect();
            return;
        }

        Session sharedSession = sessionResponse.Session;
        if (sharedSession.State != SessionState.CrossChannelAuthentication)
        {
            Log.Debug($"Incorrect session state: {sharedSession.State}");
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
            session.ForceDisconnect();
            return;
        }

        AccountLoadResponse accountLoadResponse = null;
        try
        {
            accountLoadResponse = await _accountService.LoadAccountById(new AccountLoadByIdRequest
            {
                AccountId = sharedSession.AccountId
            });
        }
        catch (Exception e)
        {
            Log.Error("[CROSS_SERVER_AUTH] Unexpected error: ", e);
        }

        if (accountLoadResponse?.ResponseType != RpcResponseType.SUCCESS)
        {
            Log.Warn($"[CROSS_SERVER_AUTH][SESSION_ID: '{session.SessionId.ToString()}'] Failed to load account for accountId: {sharedSession.AccountId}'");
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
            session.ForceDisconnect();
            return;
        }

        AccountDTO account = accountLoadResponse.AccountDto;
        if (_maintenanceManager.IsMaintenanceActive && account.Authority < AuthorityType.GM)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.Maintenance));
            session.ForceDisconnect();
            return;
        }

        AccountBanGetResponse banResponse = null;
        try
        {
            banResponse = await _accountService.GetAccountBan(new AccountBanGetRequest
            {
                AccountId = account.Id
            });
        }
        catch (Exception e)
        {
            Log.Error($"[CROSS_SERVER_AUTH][SESSION_ID: '{session.SessionId.ToString()}'] Unexpected error: ", e);
        }

        if (banResponse?.ResponseType != RpcResponseType.SUCCESS)
        {
            Log.Warn($"[CROSS_SERVER_AUTH][SESSION_ID: '{session.SessionId.ToString()}'] Failed to get account ban for accountId: '{account.Id.ToString()}'");
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
            session.ForceDisconnect();
            return;
        }

        AccountBanDto characterPenalty = banResponse.AccountBanDto;
        if (characterPenalty != null)
        {
            session.SendPacket(session.GenerateFailcPacket(LoginFailType.Banned));
            Log.Info($"[CROSS_SERVER_AUTH][SESSION_ID: '{session.SessionId.ToString()}'] {account.Name} connected from {session.IpAddress} while being banned");
            session.ForceDisconnect();
            return;
        }

        var accountObj = new Account
        {
            Id = account.Id,
            Name = account.Name,
            MasterAccountId = account.MasterAccountId,
            Password = account.Password.ToLower(),
            Authority = account.Authority,
            BankMoney = account.BankMoney,
            Language = account.Language,
            RegistrationIP = account.RegistrationIP,
        };

        Log.Warn($"INITIALIZE_ACCOUNT : {account.Name}");

        SessionResponse response = await _sessionService.ConnectToWorldServer(new ConnectToWorldServerRequest
        {
            AccountId = account.Id,
            ChannelId = _serverManager.ChannelId,
            ServerGroup = _serverManager.ServerGroup
        });

        if (response is not { ResponseType: RpcResponseType.SUCCESS })
        {
            session.ForceDisconnect();
            return;
        }

        if (response.Session == null)
        {
            session.ForceDisconnect();
            return;
        }

        session.InitializeAccount(accountObj, sessionResponse.Session);

        await session.EmitEventAsync(new CharacterPreLoadEvent(packet.CharacterSlot));
    }
}