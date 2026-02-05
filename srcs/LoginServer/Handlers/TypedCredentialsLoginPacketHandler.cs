// NosEmu
// 


using LoginServer.Network;
using PhoenixLib.Logging;
using PhoenixLib.MultiLanguage;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
using WingsEmu.DTOs.Account;
using WingsEmu.Health;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace LoginServer.Handlers
{
    public class TypedCredentialsLoginPacketHandler : GenericLoginPacketHandlerBase<Nos0575Packet>
    {
        private readonly IAccountService _accountService;
        private readonly IMaintenanceManager _maintenanceManager;
        private readonly IServerApiService _serverApiService;
        private readonly ISessionService _sessionService;
        private readonly ICharacterService _characterService;
        private string Checksum = string.Empty;

        public TypedCredentialsLoginPacketHandler(ISessionService sessionService, IServerApiService serverApiService, IMaintenanceManager maintenanceManager, IAccountService accountService,
            ICharacterService characterService)
        {
            _sessionService = sessionService;
            _serverApiService = serverApiService;
            _maintenanceManager = maintenanceManager;
            _accountService = accountService;
            _characterService = characterService;
        }

        protected override async Task HandlePacketAsync(LoginClientSession session, Nos0575Packet nos0575Packet)
        {
            if (nos0575Packet == null)
            {
                return;
            }
            
            AccountLoadResponse accountLoadResponse = null;
            try
            {
                accountLoadResponse = await _accountService.LoadAccountByName(new AccountLoadByNameRequest
                {
                    Name = nos0575Packet.Name
                });
            }
            catch (Exception e)
            {
                Log.Error("[NEW_TYPED_AUTH] Unexpected error: ", e);
            }

            if (accountLoadResponse?.ResponseType != RpcResponseType.SUCCESS)
            {
                Log.Warn($"[NEW_TYPED_AUTH] Failed to load account for accountName: '{nos0575Packet.Name}'");
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.AccountOrPasswordWrong));
                session.Disconnect();
                return;
            }

            AccountDTO loadedAccount = accountLoadResponse.AccountDto;
            if (!string.Equals(loadedAccount.Password, nos0575Packet.Password, StringComparison.CurrentCultureIgnoreCase))
            {
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.AccountOrPasswordWrong));
                Log.Debug($"[NEW_TYPED_AUTH] WRONG_CREDENTIALS : {loadedAccount.Name}");
                session.Disconnect();
                return;
            }

            SessionResponse modelResponse = await _sessionService.CreateSession(new CreateSessionRequest
            {
                AccountId = loadedAccount.Id,
                AccountName = loadedAccount.Name,
                AuthorityType = loadedAccount.Authority,
                IpAddress = session.IpAddress
            });

            if (modelResponse.ResponseType != RpcResponseType.SUCCESS)
            {
                Log.Debug($"[NEW_TYPED_AUTH] FAILED TO CREATE SESSION {loadedAccount.Id}");
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.AlreadyConnected));
                session.Disconnect();
                return;
            }

            AuthorityType type = loadedAccount.Authority;

            AccountBanGetResponse banResponse = null;
            try
            {
                banResponse = await _accountService.GetAccountBan(new AccountBanGetRequest
                {
                    AccountId = loadedAccount.Id
                });
            }
            catch (Exception e)
            {
                Log.Error("[NEW_TYPED_AUTH] Unexpected error: ", e);
            }

            if (banResponse?.ResponseType != RpcResponseType.SUCCESS)
            {
                Log.Warn($"[NEW_TYPED_AUTH] Failed to get account ban for accountId: '{loadedAccount.Id.ToString()}'");
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.UnhandledError));
                session.Disconnect();
                return;
            }

            AccountBanDto characterPenalty = banResponse.AccountBanDto;
            if (characterPenalty != null)
            {
                session.SendPacket(session.GenerateFailcPacket(LoginFailType.Banned));
                Log.Debug($"[NEW_TYPED_AUTH] ACCOUNT_BANNED : {loadedAccount.Name}");
                session.Disconnect();
                return;
            }

            switch (type)
            {
                case AuthorityType.Banned:
                    session.SendPacket(session.GenerateFailcPacket(LoginFailType.Banned));
                    Log.Debug("[NEW_TYPED_AUTH] ACCOUNT_BANNED");
                    break;

                case AuthorityType.Unconfirmed:
                case AuthorityType.Closed:
                    session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
                    Log.Debug("[NEW_TYPED_AUTH] ACCOUNT_NOT_VERIFIED");
                    break;

                default:
                    
                    if (_maintenanceManager.IsMaintenanceActive && loadedAccount.Authority < AuthorityType.GM)
                    {
                        session.SendPacket(session.GenerateFailcPacket(LoginFailType.Maintenance));
                        break;
                    }

                    SessionResponse connectResponse = await _sessionService.ConnectToLoginServer(new ConnectToLoginServerRequest
                    {
                        AccountId = loadedAccount.Id,
                        ClientVersion = "BYPASS",
                        HardwareId = "e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855"
                    });

                    if (connectResponse.ResponseType != RpcResponseType.SUCCESS)
                    {
                        Log.Warn("[NEW_AUTH] General Error SessionId: " + session.Id);
                        session.SendPacket(session.GenerateFailcPacket(LoginFailType.CantConnect));
                        break;
                    }

                    Session connectedSession = connectResponse.Session;

                    Log.Debug($"[NEW_TYPED_AUTH] Connected : {nos0575Packet.Name}:{connectedSession.EncryptionKey}:{connectedSession.HardwareId}");

                    RetrieveRegisteredWorldServersResponse worldServersResponse = await _serverApiService.RetrieveRegisteredWorldServers(new RetrieveRegisteredWorldServersRequest
                    {
                        RequesterAuthority = loadedAccount.Authority
                    });

                    if (worldServersResponse?.WorldServers is null || !worldServersResponse.WorldServers.Any())
                    {
                        session.SendPacket(session.GenerateFailcPacket(LoginFailType.Maintenance));
                        break;
                    }

                    string[] splitedArg = nos0575Packet.RegionCode.Split((char)0xB);
                    if (!byte.TryParse(splitedArg[0], out byte lang))
                    {
                        lang = 0;
                    }

                    byte characterInServer = 0;
                    DbServerGetCharactersResponse response = await _characterService.GetCharacters(new DbServerGetCharactersRequest
                    {
                        AccountId = loadedAccount.Id
                    });

                    if (response.Characters != null)
                    {
                        characterInServer = (byte)response.Characters.Count();
                    }

                    session.SendChannelPacketList(
                        connectedSession.EncryptionKey,
                        loadedAccount.Name,
                        (RegionLanguageType)lang,
                        worldServersResponse.WorldServers,
                        true,
                        characterInServer
                    );
                    break;
            }
            
            session.Disconnect();
        }

        public static string MD5Hash(string input)
        {
            StringBuilder hash = new();
            byte[] bytes = MD5.HashData(new UTF8Encoding().GetBytes(input));

            for (int i = 0; i < bytes.Length; i++)
            {
                hash.Append(bytes[i].ToString("x2"));
            }

            return hash.ToString();
        }

        private static string GetMD5Checksum(string filename)
        {
            using var md5 = MD5.Create();
            using FileStream stream = File.OpenRead(filename);
            byte[] hash = md5.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}