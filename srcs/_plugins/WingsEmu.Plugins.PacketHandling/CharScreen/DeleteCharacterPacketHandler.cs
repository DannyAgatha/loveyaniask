// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FamilyServer.Managers;
using PhoenixLib.Extensions;
using PhoenixLib.Logging;
using Plugin.Database.Families;
using WingsAPI.Communication;
using WingsAPI.Communication.Bazaar;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Communication.Families;
using WingsAPI.Communication.Sessions;
using WingsAPI.Communication.Sessions.Model;
using WingsAPI.Communication.Sessions.Request;
using WingsAPI.Communication.Sessions.Response;
using WingsAPI.Data.Account;
using WingsAPI.Data.Character;
using WingsAPI.Data.Families;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Plugins.PacketHandling.CharScreen;

public class DeleteCharacterPacketHandler : GenericCharScreenPacketHandlerBase<CharacterDeletePacket>
{
    private readonly IAccountService _accountService;
    private readonly IBazaarService _bazaarService;
    private readonly ICharacterService _characterService;
    private readonly EntryPointPacketHandler _entryPoint;
    private readonly IFamilyService _familyService;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISessionService _sessionService;


    public DeleteCharacterPacketHandler(EntryPointPacketHandler entryPoint, IGameLanguageService gameLanguage, IFamilyService familyService, ICharacterService characterService,
        IBazaarService bazaarService, IAccountService accountService, ISessionService sessionService)
    {
        _entryPoint = entryPoint;
        _gameLanguage = gameLanguage;
        _familyService = familyService;
        _characterService = characterService;
        _bazaarService = bazaarService;
        _accountService = accountService;
        _sessionService = sessionService;
    }

    protected override async Task HandlePacketAsync(IClientSession session, CharacterDeletePacket packet)
    {
        if (session.Account == null)
        {
            return;
        }

        if (session.HasCurrentMapInstance)
        {
            return;
        }

        SessionResponse sessionResponse = await _sessionService.GetSessionByAccountName(new GetSessionByAccountNameRequest
        {
            AccountName = session.Account.Name
        });

        if (sessionResponse.ResponseType != RpcResponseType.SUCCESS)
        {
            return;
        }

        if (sessionResponse.Session.State != SessionState.CharacterSelection)
        {
            return;
        }

        AccountLoadResponse accountLoadResponse = null;
        try
        {
            accountLoadResponse = await _accountService.LoadAccountById(new AccountLoadByIdRequest
            {
                AccountId = session.Account.Id
            });
        }
        catch (Exception e)
        {
            Log.Error("[DELETE_CHARACTER] Unexpected error: ", e);
        }

        if (accountLoadResponse?.ResponseType != RpcResponseType.SUCCESS)
        {
            Log.Warn($"[DELETE_CHARACTER] Failed to load the account with id: '{session.Account.Id.ToString()}'");
            return;
        }

        AccountDTO account = accountLoadResponse.AccountDto;
        if (session.Account.Id != account.Id)
        {
            return;
        }
        
        if (!string.Equals(account.Password, packet.AccountPassword.ToSha512(), StringComparison.Ordinal))
        {
            session.SendInfo(GameDialogKey.ACCOUNT_INFO_BAD_ID);
            return;
        }

        DbServerGetCharacterResponse response = await _characterService.GetCharacterBySlot(new DbServerGetCharacterFromSlotRequest
        {
            AccountId = account.Id,
            Slot = packet.Slot
        });

        if (response.RpcResponseType != RpcResponseType.SUCCESS || response.CharacterDto == null)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_DELETION_INFO_UNABLE_TO_RETRIEVE_CHARACTER, session.UserLanguage));
            Log.Warn($"[DELETE_CHARACTER] Failed to retrieve the targeted character. AccountId: '{account.Id.ToString()}' TargetedSlot: '{packet.Slot.ToString()}'");
            return;
        }

        CharacterDTO character = response.CharacterDto;

        BazaarRemoveItemsByCharIdResponse bazaarResponse = await _bazaarService.RemoveItemsByCharacterIdFromBazaar(new BazaarRemoveItemsByCharIdRequest
        {
            CharacterId = character.Id
        });

        if (bazaarResponse.ResponseType == RpcResponseType.MAINTENANCE_MODE)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.BAZAAR_INFO_MAINTENANCE_MODE, session.UserLanguage));
            Log.Warn("[DELETE_CHARACTER] Failed deleting Character's Bazaar Items because the BazaarServer is in Maintenance mode.");
        }

        if (bazaarResponse.ResponseType != RpcResponseType.SUCCESS)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_DELETION_INFO_FAILED, session.UserLanguage));
            Log.Warn($"[DELETE_CHARACTER] Failed deleting Character's Bazaar Items. CharacterId: '{character.Id.ToString()}'");
            return;
        }

        BasicRpcResponse removeMemberFromFamilyResponse = null;
        try
        {
            removeMemberFromFamilyResponse = await _familyService.RemoveMemberByCharIdAsync(new FamilyRemoveMemberByCharIdRequest
            {
                CharacterId = character.Id
            });
        }
        catch (Exception e)
        {
            Log.Error("[DELETE_CHARACTER] ", e);
        }

        if (removeMemberFromFamilyResponse == null)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_DELETION_INFO_FAILED, session.UserLanguage));
            Log.Warn($"[DELETE_CHARACTER] Failed to contact with FamilyService to request RemoveMemberByCharId. CharacterId: '{character.Id.ToString()}'");
            return;
        }
        
        DbServerDeleteCharacterResponse response2 = await _characterService.DeleteCharacter(new DbServerDeleteCharacterRequest
        {
            CharacterDto = character
        });

        if (response2.RpcResponseType != RpcResponseType.SUCCESS)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_DELETION_INFO_FAILED, session.UserLanguage));
            Log.Warn($"[DELETE_CHARACTER] For unknown reasons the character couldn't be deleted. AccountId: '{account.Id.ToString()}' TargetedSlot: '{character.Slot.ToString()}'");
            return;
        }

        await _entryPoint.EntryPointAsync(session, null);
    }
}