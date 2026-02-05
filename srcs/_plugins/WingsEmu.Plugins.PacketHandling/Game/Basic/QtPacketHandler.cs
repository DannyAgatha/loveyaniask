using System.Threading.Tasks;
using PhoenixLib.Logging;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.ClientPackets;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class QtPacketHandler : GenericGamePacketHandlerBase<QtPacket>
{
    private readonly IQuestManager _questManager;
    private readonly GeneralServerConfiguration _generalServerConfiguration;

    public QtPacketHandler(IQuestManager questManager, GeneralServerConfiguration generalServerConfiguration)
    {
        _questManager = questManager;
        _generalServerConfiguration = generalServerConfiguration;
    }

    protected override async Task HandlePacketAsync(IClientSession session, QtPacket packet)
    {
        int packetSlot = packet.Slot;
        int action = packet.Action;


        CharacterQuest quest = session.GetQuestByActionSlot(action, packetSlot);
        if (quest == null && action != (short)QtAction.START_QUEST)
        {
            Log.Debug($"[ERROR] PACKET QT: Not a valid quest found for the slot: {packetSlot.ToString()}");
            return;
        }

        switch (action)
        {
            case (short)QtAction.CHARACTER_IN_TARGET:
                await session.EmitEventAsync(new QuestCompletedEvent(quest, true));
                break;
            case (short)QtAction.START_QUEST:
                if (session.PlayerEntity.GetPendingSoundFlowerQuests() == 0)
                {
                    await session.NotifyStrangeBehavior(StrangeBehaviorSeverity.ABUSING, "Player tried to start a sound flower quest without having any.");
                    return;
                }

                await session.EmitEventAsync(new AddSoundFlowerQuestEvent
                {
                    SoundFlowerType = SoundFlowerType.SOUND_FLOWER
                });
                break;
            case (short)QtAction.LEAVE_QUEST:

                if (quest == null)
                {
                    break;
                }

                if (_questManager.IsNpcBlueAlertQuest(quest.QuestId) && !_generalServerConfiguration.CanDeleteDailyQuest)
                {
                    session.SendInfo(session.GetLanguage(GameDialogKey.QUEST_INFO_CANT_GIVE_UP));
                    break;
                }

                await session.EmitEventAsync(new QuestRemoveEvent(quest, false));
                break;
            case (short)QtAction.QUEST_REWARD_CLICKED:
                if (!session.PlayerEntity.IsQuestCompleted(quest))
                {
                    return;
                }

                await session.EmitEventAsync(new QuestCompletedEvent(quest, true, false));
                break;
            default:
                Log.Debug($"[ERROR] PACKET QT: Invalid action: {action.ToString()}");
                return;
        }
    }

    private enum QtAction
    {
        CHARACTER_IN_TARGET = 1,
        START_QUEST = 2,
        LEAVE_QUEST = 3,
        QUEST_REWARD_CLICKED = 4
    }
}