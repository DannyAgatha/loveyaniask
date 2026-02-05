using System;
using PhoenixLib.Events;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Data.Character;
using WingsEmu.Game._enum;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TrainerSpecialist;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.StaticData;

namespace NosEmu.Plugins.BasicImplementations.TrainerSpecialist;

public class UpdatePetBookEventHandler : IAsyncEventProcessor<UpdatePetBookEvent>
{
    private readonly TrainerSpecialistPetBookConfiguration _trainerSpecialistPetBookConfiguration;
    private readonly INpcMonsterManager _npcMonsterManager;
    
    public UpdatePetBookEventHandler(TrainerSpecialistPetBookConfiguration trainerSpecialistPetBookConfiguration, INpcMonsterManager npcMonsterManager)
    {
        _trainerSpecialistPetBookConfiguration = trainerSpecialistPetBookConfiguration;
        _npcMonsterManager = npcMonsterManager;
    }

    public async Task HandleAsync(UpdatePetBookEvent e, CancellationToken cancellation)
    {
        IMateEntity mateEntity = e.MateEntity;
        IClientSession session = e.Sender;
        
        bool exists = _trainerSpecialistPetBookConfiguration.Pets.Any(s => s == mateEntity.MonsterVNum);
        
        if (!exists)
        {
            return;
        }
        
        MaxStarTrainerDto monster = session.PlayerEntity.MaxStarTrainerDto.FirstOrDefault(s => s.MonsterVnum == mateEntity.MonsterVNum);

        if (monster == null)
        {
            await HandleQuests(session, mateEntity);
            session.PlayerEntity.MaxStarTrainerDto.Add(new MaxStarTrainerDto
            {
                MonsterVnum = mateEntity.MonsterVNum,
                Stars = mateEntity.Stars,
                Level = mateEntity.HeroLevel
            });
            return;
        }

        if ((monster.Level < mateEntity.HeroLevel && mateEntity.Stars == monster.Stars) || monster.Stars < mateEntity.Stars)
        {
            monster.Level = mateEntity.HeroLevel;
            monster.Stars = mateEntity.Stars;
        }
        session.SendStpPacket(false, _trainerSpecialistPetBookConfiguration);
        session.SendStpPacket(true, _trainerSpecialistPetBookConfiguration);
    }

    private async Task HandleQuests(IClientSession session, IMateEntity mateEntity)
    {
        IMonsterData monsterData = _npcMonsterManager.GetNpc(mateEntity.MonsterVNum);

        if (monsterData == null)
        {
            return;
        }
        
        switch (monsterData.Stars)
        {
            case 1:
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Own50Unique1StarPet
                });
                break;

            case 2:
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Own50Unique2StarPet
                });
                break;

            case 3:
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Own25Unique3StarPet
                });
                break;

            case 4:
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Own10Unique4StarPet
                });
                break;

            case 5:
                session.EmitEvent(new UpdateTrainerQuestEvent
                {
                    MissionType = PetTrainerMissionType.Own5Unique5StarPet
                });
                break;
        }
        session.SendStpM();
    }
}
