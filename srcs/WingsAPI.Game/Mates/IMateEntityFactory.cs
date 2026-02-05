using System.Collections.Generic;
using WingsEmu.DTOs.Mates;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Npcs;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Mates;

public interface IMateEntityFactory
{
    public IMateEntity CreateMateEntity(IPlayerEntity playerEntity, MateDTO mateDto);
    public IMateEntity CreateMateEntity(IPlayerEntity owner, int monsterVnum, MateType mateType, List<int> trainerSkills);
    public IMateEntity CreateMateEntity(IPlayerEntity owner, MonsterData monsterData, MateType mateType, List<int> trainerSkills);
    public IMateEntity CreateMateEntity(IPlayerEntity owner, MonsterData monsterData, MateType mateType, byte level, byte heroLevel, List<int> trainerSkills);
    public IMateEntity CreateMateEntity(IPlayerEntity owner, MonsterData monsterData, MateType mateType, byte level, byte heroLevel, List<int> trainerSkills, bool isLimited);

    public MateDTO CreateMateDto(IMateEntity mateEntity);
}