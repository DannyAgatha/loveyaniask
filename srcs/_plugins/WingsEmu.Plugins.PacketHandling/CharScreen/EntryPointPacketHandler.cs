using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;
using WingsAPI.Data.Account;
using WingsAPI.Data.Character;
using WingsEmu.Core.Extensions;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Inventory;
using WingsEmu.DTOs.Mates;
using WingsEmu.Game;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Health;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.PacketHandling.CharScreen;

public class EntryPointPacketHandler : GenericCharScreenPacketHandlerBase<EntryPointPacket>
{
    private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly IAccountService _accountService;
    private readonly ICharacterService _characterService;
    private readonly IGameItemInstanceFactory _itemInstanceFactory;
    private readonly IMaintenanceManager _maintenanceManager;
    private readonly IServerManager _serverManager;
    private readonly ISessionManager _sessionManager;
    private readonly ISessionService _sessionService;

    public EntryPointPacketHandler(ISessionService sessionService, ICharacterService characterService, IMaintenanceManager maintenanceManager, IAccountService accountService,
        ISessionManager sessionManager, IServerManager serverManager, IGameItemInstanceFactory itemInstanceFactory)
    {
        _sessionService = sessionService;
        _characterService = characterService;
        _maintenanceManager = maintenanceManager;
        _accountService = accountService;
        _sessionManager = sessionManager;
        _serverManager = serverManager;
        _itemInstanceFactory = itemInstanceFactory;
    }

    public async Task EntryPointAsync(IClientSession session, EntryPointPacket packet)
    {
        await _semaphoreSlim.WaitAsync();
        try
        {
            if (session.Account == null)
            {
                SessionResponse sessionResponse = await _sessionService.GetSessionByAccountName(new GetSessionByAccountNameRequest()
                {
                    AccountName = packet.SessionName
                });

                if (sessionResponse.ResponseType != RpcResponseType.SUCCESS)
                {
                    Log.Debug($"Can't get session with ID {packet.SessionName}");
                    session.ForceDisconnect();
                    return;
                }

                Session sharedSession = sessionResponse.Session;
                if (sharedSession.State != SessionState.ServerSelection)
                {
                    Log.Debug($"Incorrect state for {sharedSession.Id}");
                    session.ForceDisconnect();
                    return;
                }

                if (session.SessionId != sessionResponse.Session.EncryptionKey)
                {
                    session.ForceDisconnect();
                    return;
                }

                SessionResponse sessionResponseAccount = await _sessionService.GetSessionByAccountId(new GetSessionByAccountIdRequest
                {
                    AccountId = sharedSession.AccountId
                });

                if (sessionResponseAccount is null)
                {
                    session.ForceDisconnect();
                    return;
                }

                if (sessionResponseAccount.Session.EncryptionKey != session.SessionId)
                {
                    session.ForceDisconnect();
                    return;
                }

                if (sessionResponseAccount.Session.State != SessionState.ServerSelection)
                {
                    session.ForceDisconnect();
                    return;
                }

                if (_maintenanceManager.IsMaintenanceActive && sharedSession.Authority < AuthorityType.GM)
                {
                    Log.Debug("[ENTRY_POINT] Maintenance is active");
                    session.ForceDisconnect();
                    return;
                }

                if (_sessionManager.SessionsCount >= _serverManager.AccountLimit && sharedSession.Authority < AuthorityType.GS)
                {
                    Log.Debug("[ENTRY_POINT] Account limit reached");
                    session.ForceDisconnect();
                    return;
                }

                AccountBanGetResponse banResponse = null;
                try
                {
                    banResponse = await _accountService.GetAccountBan(new AccountBanGetRequest
                    {
                        AccountId = sharedSession.AccountId
                    });
                }
                catch (Exception e)
                {
                    Log.Error($"[ENTRY_POINT][SESSION_ID: '{session.SessionId.ToString()}'] Unexpected error: ", e);
                }

                if (banResponse?.ResponseType != RpcResponseType.SUCCESS)
                {
                    Log.Warn($"[ENTRY_POINT][SESSION_ID: '{session.SessionId.ToString()}'] Failed to get account ban for accountId: '{sharedSession.AccountId}'");
                    session.ForceDisconnect();
                    return;
                }

                AccountBanDto characterPenalty = banResponse.AccountBanDto;
                if (characterPenalty != null)
                {
                    Log.Info($"[ENTRY_POINT][SESSION_ID: '{session.SessionId.ToString()}'] connected from {session.IpAddress} while being banned");
                    session.ForceDisconnect();
                    return;
                }

                AccountLoadResponse accountResponse = await _accountService.LoadAccountById(new AccountLoadByIdRequest
                {
                    AccountId = sharedSession.AccountId
                });

                if (accountResponse.ResponseType != RpcResponseType.SUCCESS)
                {
                    Log.Debug("[ENTRY_POINT] Failed to load account");
                    session.ForceDisconnect();
                    return;
                }

                AccountDTO account = accountResponse.AccountDto;
                if (packet.Password != "thisisgfmode")
                {
                    if (!string.Equals(account.Password, packet.Password.ToSha512(), StringComparison.Ordinal))
                    {
                        Log.Debug("[ENTRY_POINT] Incorrect password");
                        session.ForceDisconnect();
                        return;
                    }
                }

                var accountObject = new Account
                {
                    Id = account.Id,
                    Name = account.Name,
                    MasterAccountId = account.MasterAccountId,
                    Authority = account.Authority,
                    BankMoney = account.BankMoney,
                    Password = account.Password,
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

                string currentIpAddress = sessionResponse.Session.IpAddress;
                
                int activeSessionsWithSameIp = _sessionManager.Sessions.Count(s => s.IpAddress == currentIpAddress && s.Account.Id != session.Account?.Id);
                
                if (activeSessionsWithSameIp > 2)
                {
                    session.ForceDisconnect();
                    Log.Info($"[ENTRY_POINT] More than two sessions detected from the same IP {currentIpAddress}. Disconnecting the current session.");
                }
                
                session.InitializeAccount(accountObject, response.Session);
            }

            if (session.Account != null)
            {
                DbServerGetCharactersResponse response2 = await _characterService.GetCharacters(new DbServerGetCharactersRequest
                {
                    AccountId = session.Account.Id
                });
                Log.Info($"[ACCOUNT_ARRIVED] {session.Account.Name}");
                
                session.SendPacket("clist_start 0");

                if (response2.Characters == null)
                {
                    session.SendPacket("clist_end");
                    return;
                }

                foreach (CharacterDTO character in response2.Characters)
                {
                    List<CharacterInventoryItemDto> inventory = character.EquippedStuffs;

                    var equipment = new Dictionary<EquipmentType, GameItemInstance>();
                    foreach (CharacterInventoryItemDto equipmentEntry in inventory)
                    {
                        if (equipmentEntry.InventoryType != InventoryType.EquippedItems)
                        {
                            continue;
                        }

                        GameItemInstance instance = _itemInstanceFactory.CreateItem(equipmentEntry.ItemInstance);
                        if (instance == null)
                        {
                            continue;
                        }

                        equipment[instance.GameItem.EquipmentSlot] = instance;
                    }

                    string petlist = string.Empty;
                    List<MateDTO> mates = character.NosMates;
                    for (int i = 0; i < 26; i++)
                    {
                        //0.2105.1102.319.0.632.0.333.0.318.0.317.0.9.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1.-1
                        petlist += $"{(i != 0 ? "." : "")}{(mates.Count > i ? $"{mates[i].Skin}.{mates[i].NpcMonsterVNum}" : "-1")}";
                    }

                    // 1 1 before long string of -1.-1 = act completion
                    session.SendPacket(
                        $"clist {character.Slot} {character.Name} 0 {(byte)character.Gender} {(byte)character.HairStyle} " +
                        $"{(byte)character.HairColor} 0 {(byte)character.Class} {character.Level} {character.HeroLevel} " +
                        $"{(character.HideHat ? 0 : equipment.GetOrDefault(EquipmentType.Hat)?.ItemVNum ?? -1)}." +
                        $"{equipment.GetOrDefault(EquipmentType.Armor)?.ItemVNum ?? -1}." +
                        $"{equipment.GetOrDefault(EquipmentType.WeaponSkin)?.ItemVNum ?? equipment.GetOrDefault(EquipmentType.MainWeapon)?.ItemVNum ?? -1}." +
                        $"{equipment.GetOrDefault(EquipmentType.SecondaryWeapon)?.ItemVNum ?? -1}." +
                        $"{equipment.GetOrDefault(EquipmentType.Mask)?.ItemVNum ?? -1}.{equipment.GetOrDefault(EquipmentType.Fairy)?.ItemVNum ?? -1}." +
                        $"{equipment.GetOrDefault(EquipmentType.CostumeSuit)?.ItemVNum ?? -1}." +
                        $"{(character.HideHat ? 0 : equipment.GetOrDefault(EquipmentType.CostumeHat)?.ItemVNum ?? -1)} " +
                        $"{character.JobLevel} 1 1 {petlist} " +
                        $"{(equipment.GetOrDefault(EquipmentType.Hat)?.GameItem.IsColorable == true ? equipment.GetOrDefault(EquipmentType.Hat).Design : 0)} " +
                        $"{(character.IsAvailableToChangeName ? 1 : 0)}");
                }
            }

            session.SendPacket("clist_end");
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    protected override async Task HandlePacketAsync(IClientSession session, EntryPointPacket packet)
    {
        await EntryPointAsync(session, packet);
    }
}