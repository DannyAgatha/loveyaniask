using System.Collections.Generic;
using PhoenixLib.Events;
using WingsEmu.DTOs.Mates;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations.CharacterSizeModifiers;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Npcs;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Game.Mates;

public class MateEntityFactory : IMateEntityFactory
{
    private readonly IBattleEntityAlgorithmService _algorithm;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IMateTransportFactory _mateTransportFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly CharacterSizeModifiersConfiguration _characterSizeModifiersConfiguration;
    public MateEntityFactory(INpcMonsterManager npcMonsterManager, IAsyncEventPipeline eventPipeline, IBattleEntityAlgorithmService algorithm, IRandomGenerator randomGenerator,
        IMateTransportFactory mateTransportFactory, CharacterSizeModifiersConfiguration characterSizeModifiersConfiguration)
    {
        _npcMonsterManager = npcMonsterManager;
        _eventPipeline = eventPipeline;
        _algorithm = algorithm;
        _randomGenerator = randomGenerator;
        _mateTransportFactory = mateTransportFactory;
        _characterSizeModifiersConfiguration = characterSizeModifiersConfiguration;
    }

    public IMateEntity CreateMateEntity(IPlayerEntity owner, MonsterData monsterData, MateType mateType, List<int> trainerSkills) => CreateMateEntity(owner, monsterData, mateType, 1, 0, trainerSkills);

    public IMateEntity CreateMateEntity(IPlayerEntity playerEntity, MateDTO mateDto)
    {
        var monsterData = new MonsterData(_npcMonsterManager.GetNpc(mateDto.NpcMonsterVNum));
        var mate = new MateEntity(playerEntity, monsterData, mateDto.Level,mateDto.HeroLevel, mateDto.TrainerSkills, mateDto.MateType, _mateTransportFactory, _eventPipeline, _algorithm, _randomGenerator, _characterSizeModifiersConfiguration)
        {
            Attack = mateDto.Attack,
            CanPickUp = mateDto.CanPickUp,
            CharacterId = mateDto.CharacterId,
            Defence = mateDto.Defence,
            Direction = mateDto.Direction,
            Experience = mateDto.Experience,
            Hp = mateDto.Hp,
            Level = mateDto.Level,
            HeroLevel = mateDto.HeroLevel,
            Loyalty = mateDto.Loyalty,
            Mp = mateDto.Mp,
            MateName = mateDto.MateName,
            Skin = mateDto.Skin,
            IsSummonable = mateDto.IsSummonable,
            MapX = mateDto.MapX,
            MapY = mateDto.MapY,
            MateType = mateDto.MateType,
            PetSlot = mateDto.PetSlot,
            MinilandX = mateDto.MinilandX,
            MinilandY = mateDto.MinilandY,
            IsTeamMember = mateDto.IsTeamMember,
            IsLimited = mateDto.IsLimited,
            Stars = mateDto.Stars,
            TrainerExperience = mateDto.TrainerExperience,
            TrainerSkills = mateDto.TrainerSkills,
            HasDhaPremium = mateDto.HasDhaPremium,
            IsDhaLootEnabled = mateDto.IsDhaLootEnabled
        };

        return mate;
    }

    public IMateEntity CreateMateEntity(IPlayerEntity owner, int monsterVnum, MateType mateType, List<int> trainerSkills)
    {
        IMonsterData monsterData = _npcMonsterManager.GetNpc(monsterVnum);
        return monsterData == null ? null : CreateMateEntity(owner, new MonsterData(monsterData), mateType, 1, 0, trainerSkills);
    }

    public IMateEntity CreateMateEntity(IPlayerEntity owner, MonsterData monsterData, MateType mateType, byte level, byte heroLevel, List<int> trainerSkills) =>
        CreateMateEntity(owner, monsterData, mateType, level, heroLevel, trainerSkills, false);

    public IMateEntity CreateMateEntity(IPlayerEntity owner, MonsterData monsterData, MateType mateType, byte level, byte heroLevel, List<int> trainerSkills, bool isLimited) =>
        new MateEntity(owner, monsterData, level, heroLevel, trainerSkills, mateType, _mateTransportFactory, _eventPipeline, _algorithm, _randomGenerator, _characterSizeModifiersConfiguration)
        {
            IsLimited = isLimited
        };

    public MateDTO CreateMateDto(IMateEntity mateEntity) => new()
    {
        Id = mateEntity.Id,
        Attack = mateEntity.Attack,
        CanPickUp = mateEntity.CanPickUp,
        CharacterId = mateEntity.CharacterId,
        Defence = mateEntity.Defence,
        Direction = mateEntity.Direction,
        Experience = mateEntity.Experience,
        Hp = mateEntity.Hp,
        Level = mateEntity.Level,
        HeroLevel = mateEntity.HeroLevel,
        Loyalty = mateEntity.Loyalty,
        Mp = mateEntity.Mp,
        MateName = mateEntity.MateName,
        Skin = mateEntity.Skin,
        IsSummonable = mateEntity.IsSummonable,
        MapX = mateEntity.MapX,
        MapY = mateEntity.MapY,
        MateType = mateEntity.MateType,
        PetSlot = mateEntity.PetSlot,
        MinilandX = mateEntity.MinilandX,
        MinilandY = mateEntity.MinilandY,
        IsTeamMember = mateEntity.IsTeamMember,
        NpcMonsterVNum = mateEntity.NpcMonsterVNum,
        IsLimited = mateEntity.IsLimited,
        Stars = mateEntity.Stars,
        TrainerExperience = mateEntity.TrainerExperience,
        TrainerSkills = mateEntity.TrainerSkills,
        HasDhaPremium = mateEntity.HasDhaPremium,
        IsDhaLootEnabled = mateEntity.IsDhaLootEnabled
    };
}