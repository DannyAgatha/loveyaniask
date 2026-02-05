using PhoenixLib.Events;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Data.Character;
using WingsEmu.Game._enum;
using WingsEmu.Game.Networking;
using WingsEmu.Game.TrainerSpecialist;

namespace NosEmu.Plugins.BasicImplementations.TrainerSpecialist
{
    internal class UpdateTrainerQuestEventHandler : IAsyncEventProcessor<UpdateTrainerQuestEvent>
    {
        public Task HandleAsync(UpdateTrainerQuestEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            PetTrainerMissionType missionType = e.MissionType;

            TrainerQuestDto quest = session.PlayerEntity.TrainerQuestDto.FirstOrDefault(s => s.MissionType == (byte)missionType && s.Achievement != -1);

            if (quest == null)
            {
                session.PlayerEntity.TrainerQuestDto.Add(new()
                {
                    MissionType = (byte)missionType,
                    Achievement = 1
                });
                return Task.CompletedTask;
            }
            quest.Achievement++;
            return Task.CompletedTask;
        }
    }
}
