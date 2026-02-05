using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Data.Character;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Packets.ClientPackets;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game._enum;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.TrainerSpecialist;

public class GetTrainerSpecialistQuestRewardPacketHandler : GenericGamePacketHandlerBase<SpMselPacket>
{
    private readonly TrainerQuestConfiguration _trainerQuestConfiguration;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly ISessionManager _sessionManager;
    
    public GetTrainerSpecialistQuestRewardPacketHandler(TrainerQuestConfiguration trainerQuestConfiguration, IGameItemInstanceFactory gameItemInstanceFactory, ISessionManager sessionManager)
    {
        _trainerQuestConfiguration = trainerQuestConfiguration;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _sessionManager = sessionManager;
    }
    
    protected override async Task HandlePacketAsync(IClientSession session, SpMselPacket packet)
    {
        TrainerQuestDto characterQuest = session.PlayerEntity.TrainerQuestDto.FirstOrDefault(s => s.MissionType == packet.MissionType);
        QuestConfiguration getTrainerQuest = _trainerQuestConfiguration.Quests.FirstOrDefault(s => s.MissionType == (PetTrainerMissionType)packet.MissionType);

        if (characterQuest == null || getTrainerQuest == null)
        {
            return;
        }

        if (characterQuest.Achievement != getTrainerQuest.AchievementNeeded)
        {
            return;
        }

        characterQuest.Achievement = -1;
        await session.AddNewItemToInventory(_gameItemInstanceFactory.CreateItem(getTrainerQuest.RewardVnum), showMessage: true);
        session.SendSuccessMissionMessage(_sessionManager, Game18NConstString.HasCompletedPetBookMissionAndReceived, I18NArgumentType.ItemVnum,
            session.PlayerEntity.Name, getTrainerQuest.RewardVnum);
        session.SendStpM();
    }
}