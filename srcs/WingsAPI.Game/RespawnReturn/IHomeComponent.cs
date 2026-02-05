using WingsAPI.Packets.Enums;
using WingsEmu.DTOs.Respawns;
using WingsEmu.Game._enum;

namespace WingsEmu.Game.RespawnReturn;

public interface IHomeComponent
{
    public RespawnType RespawnType { get; }
    public Act5RespawnType Act5RespawnType { get; }

    public CharacterReturnDto Return { get; }
    public void ChangeRespawn(RespawnType type);
    public void ChangeAct5Respawn(Act5RespawnType type);

    public void ChangeReturn(CharacterReturnDto returnDto);
}

public class HomeComponent : IHomeComponent
{
    public HomeComponent(CharacterReturnDto characterDtoReturn) => Return = characterDtoReturn ?? new CharacterReturnDto();

    public RespawnType RespawnType { get; private set; }
    public Act5RespawnType Act5RespawnType { get; private set; }

    public void ChangeRespawn(RespawnType type)
    {
        RespawnType = type;
    }

    public void ChangeAct5Respawn(Act5RespawnType type)
    {
        Act5RespawnType = type;
    }

    public CharacterReturnDto Return { get; private set; }

    public void ChangeReturn(CharacterReturnDto returnDto)
    {
        Return = returnDto;
    }
}