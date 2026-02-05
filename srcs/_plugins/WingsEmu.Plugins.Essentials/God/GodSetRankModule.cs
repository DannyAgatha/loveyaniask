// NosEmu
// 


using System;
using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Data.Character;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;

namespace WingsEmu.Plugins.Essentials.God;

[Name("Administrator")]
[Description("Module related to God commands.")]
[RequireAuthority(AuthorityType.DEV)]
public class GodSetRankModule : SaltyModuleBase
{
    private readonly ICharacterService _characterService;
    private readonly IPlayerEntityFactory _playerEntityFactory;

    public GodSetRankModule(ICharacterService characterService, IPlayerEntityFactory playerEntityFactory)
    {
        _characterService = characterService;
        _playerEntityFactory = playerEntityFactory;
    }


    [Command("setrank")]
    [Description("Sets the rank of the given player")]
    public async Task<SaltyCommandResult> SetRank(IClientSession character, int authority)
    {
        var targetAuthority = (AuthorityType)authority;
        if (targetAuthority > Context.Player.PlayerEntity.Authority)
        {
            return new SaltyCommandResult(false, "You can't set someone's rights to higher than your own rights");
        }

        character.Account.Authority = targetAuthority;

        Context.Player.SendInformationChatMessage($"{character.PlayerEntity.Name}'s new rank is {character.Account.Authority.ToString()}");
        character.ChangeMap(character.CurrentMapInstance.Id);
        return new SaltyCommandResult(true);
    }

    [Command("ranks")]
    [Description("Gives the list of available ranks")]
    public async Task<SaltyCommandResult> SetRank()
    {
        Context.Player.SendInformationChatMessage("---- RANKS ----");
        foreach (object? rank in Enum.GetValues(typeof(AuthorityType)))
        {
            Context.Player.SendInformationChatMessage($"{rank}: {(int)rank}");
        }

        return new SaltyCommandResult(true);
    }

    [Command("force-cache")]
    [Description("Gives the list of available ranks")]
    public async Task<SaltyCommandResult> ForceRemoveCharacterFromCache([Description("characterName")] string characterName)
    {
        DbServerGetCharacterResponse response = await _characterService.ForceRemoveCharacterFromCache(new DbServerGetCharacterRequestByName
        {
            CharacterName = characterName
        });
        string successMessage = $"[CACHE_CLEAN] {characterName} has been removed from cache";
        string errorMessage = $"[CACHE_CLEAN] {characterName} couldn't be found";
        bool isSuccess = response.RpcResponseType == RpcResponseType.SUCCESS;
        return new SaltyCommandResult(isSuccess, isSuccess ? successMessage : errorMessage);
    }

    [Command("change-name")]
    [Description("Changes the name of the target to new name")]
    public async Task<SaltyCommandResult> ForceRemoveCharacterFromCache([Description("targetSession")] IClientSession target, [Description("New expected name")] string newName)
    {
        CharacterDTO newCharacter = _playerEntityFactory.CreateCharacterDto(target.PlayerEntity);
        newCharacter.Name = newName;

        // force flush to db method
        DbServerSaveCharacterResponse response = await _characterService.CreateCharacter(new DbServerSaveCharacterRequest
        {
            Character = newCharacter,
            IgnoreSlotCheck = true
        });

        if (response.RpcResponseType != RpcResponseType.SUCCESS)
        {
            return new SaltyCommandResult(false, $"[CHANGE_NAME] {newName} already existed");
        }

        string oldName = target.PlayerEntity.Name;
        target.PlayerEntity.Name = newName;
        target.ChangeMap(target.CurrentMapInstance, target.PlayerEntity.PositionX, target.PlayerEntity.PositionY);
        return new SaltyCommandResult(true, $"[CHANGE_NAME] {oldName} has changed to {newName}");
    }
}