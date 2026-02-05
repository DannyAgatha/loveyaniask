// NosEmu
// 


using System.Collections.Generic;
using Mapster;
using WingsAPI.Data.Character;
using WingsAPI.Data.Prestige;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;

namespace WingsEmu.Plugins.PacketHandling.Customization;

public class BaseCharacter
{
    public BaseCharacter() => Character = new CharacterDTO
    {
        Class = ClassType.Adventurer,
        Gender = GenderType.Male,
        HairColor = HairColorType.Black,
        HairStyle = HairStyleType.Gel,
        Hp = 2970,
        JobLevel = 20,
        Level = 55,
        MapId = 1,
        MapX = 78,
        MapY = 109,
        Mp = 1635,
        Reput = 5001,
        MaxPetCount = 10,
        MaxPartnerCount = 3,
        Gold = 0,
        SpPointsBasic = 10000,
        SpPointsBonus = 0,
        Name = "template",
        Slot = 0,
        AccountId = 0,
        MinilandMessage = string.Empty,
        CharacterPrestigeDto = new CharacterPrestigeDto
        {
            CurrentPrestigeLevel = 0,
            CurrentPrestigeExp = 0,
            Tasks = []
        }
    };

    public CharacterDTO Character { get; set; }

    public CharacterDTO GetCharacter() => Character.Adapt<CharacterDTO>();
}