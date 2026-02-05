using WingsAPI.Data.Character;

namespace WingsEmu.Game.Characters;

public interface IPlayerEntityFactory
{
    public IPlayerEntity CreatePlayerEntity(CharacterDTO characterDto);
    public CharacterDTO CreateCharacterDto(IPlayerEntity playerEntity);
}