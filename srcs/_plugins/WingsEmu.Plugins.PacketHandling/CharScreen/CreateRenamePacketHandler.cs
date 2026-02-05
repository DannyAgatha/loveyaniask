// NosEmu
// 


using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Data.Character;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.CharScreen;

public class CreateRenamePacketHandler : GenericCharScreenPacketHandlerBase<CharacterRenamePacket>
{
    private readonly ICharacterService _characterService;
    private readonly EntryPointPacketHandler _entrypoint;
    private readonly IForbiddenNamesManager _forbiddenNamesManager;
    private readonly IGameLanguageService _gameLanguage;

    public CreateRenamePacketHandler(EntryPointPacketHandler entrypoint, IGameLanguageService gameLanguage, ICharacterService characterService, IForbiddenNamesManager forbiddenNamesManager)
    {
        _entrypoint = entrypoint;
        _gameLanguage = gameLanguage;
        _characterService = characterService;
        _forbiddenNamesManager = forbiddenNamesManager;
    }

    protected override async Task HandlePacketAsync(IClientSession session, CharacterRenamePacket packet)
    {
        if (session.HasCurrentMapInstance)
        {
            Log.Warn("HAS_CURRENTMAP_INSTANCE");
            return;
        }

        long accountId = session.Account.Id;
        byte slot = packet.Slot;
        string characterName = packet.Name;
        DbServerGetCharacterResponse response = await _characterService.GetCharacterBySlot(new DbServerGetCharacterFromSlotRequest
        {
            AccountId = accountId,
            Slot = slot
        });

        if (response.RpcResponseType != RpcResponseType.SUCCESS || response.CharacterDto == null)
        {
            Log.Warn($"[RENAME_CHARACTER] Failed to retrieve the targeted character. AccountId: '{accountId.ToString()}' TargetedSlot: '{packet.Slot.ToString()}'");
            return;
        }

        if (slot > 3)
        {
            Log.Info("SLOTS > 3");
            return;
        }
        
        if (!response.CharacterDto.IsAvailableToChangeName)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CANNOT_BE_USED, session.UserLanguage));
            return;
        }

        if (characterName.Length is < 3 or >= 15 && session.Account.Authority < AuthorityType.GM)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_CREATION_INFO_INVALID_CHARNAME, session.UserLanguage));
            return;
        }

        var rg = new Regex(@"^[a-zA-Z0-9_\-\*]*$");
        if (rg.Matches(characterName).Count != 1)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_CREATION_INFO_INVALID_CHARNAME, session.UserLanguage));
            return;
        }

        if (session.Account.Authority <= AuthorityType.GM)
        {
            string lowerCharName = characterName.ToLower();
            if (_forbiddenNamesManager.IsBanned(lowerCharName, out string bannedName))
            {
                session.SendInfo(_gameLanguage.GetLanguageFormat(GameDialogKey.CHARACTER_CREATION_INFO_BANNED_CHARNAME, session.UserLanguage, bannedName));
                return;
            }
        }

        DbServerGetCharacterResponse response2 = await _characterService.GetCharacterByName(new DbServerGetCharacterRequestByName
        {
            CharacterName = characterName
        });

        if (response2.RpcResponseType == RpcResponseType.SUCCESS)
        {
            session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_CREATION_INFO_ALREADY_TAKEN, session.UserLanguage));
            return;
        }
        CharacterDTO newCharacter = response.CharacterDto;

        newCharacter.Name = characterName;
        newCharacter.IsAvailableToChangeName = false;

        await _characterService.CreateCharacter(new DbServerSaveCharacterRequest
        {
            Character = newCharacter,
            IgnoreSlotCheck = true
        });

        await _entrypoint.EntryPointAsync(session, null);
    }
}