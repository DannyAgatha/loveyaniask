using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Quests;
using WingsEmu.DTOs.Quests;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Quests;
using WingsEmu.Game.Quests.Configurations;
using WingsEmu.Game.Quests.Event;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.QuestImpl.Handlers
{
    public class AddSoundFlowerQuestEventHandler : IAsyncEventProcessor<AddSoundFlowerQuestEvent>
    {
        private readonly IGameLanguageService _gameLanguageService;
        private readonly IQuestManager _questManager;
        private readonly IRandomGenerator _randomGenerator;
        private readonly SoundFlowerConfiguration _soundFlowerConfiguration;

        public AddSoundFlowerQuestEventHandler(IQuestManager questManager, SoundFlowerConfiguration soundFlowerConfiguration, IRandomGenerator randomGenerator,
            IGameLanguageService gameLanguageService)
        {
            _questManager = questManager;
            _soundFlowerConfiguration = soundFlowerConfiguration;
            _randomGenerator = randomGenerator;
            _gameLanguageService = gameLanguageService;
        }

        public async Task HandleAsync(AddSoundFlowerQuestEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IPlayerEntity player = session.PlayerEntity;
            SoundFlowerType soundFlowerType = e.SoundFlowerType;

            IReadOnlyCollection<QuestDto> flowerQuests = soundFlowerType switch
            {
                SoundFlowerType.SOUND_FLOWER => _soundFlowerConfiguration.SoundFlowerQuestVnums.Select(s => _questManager.GetQuestById(s)).ToList(),
                SoundFlowerType.WILD_SOUND_FLOWER => _soundFlowerConfiguration.WildSoundFlowerQuestVnums.Select(s => _questManager.GetQuestById(s)).ToList(),
                _ => new List<QuestDto>()
            };

            if (!flowerQuests.Any())
            {
                return;
            }

            IReadOnlyCollection<QuestDto> possibleQuests =
                flowerQuests.Where(s => s.MinLevel <= player.Level && player.Level <= s.MaxLevel && player.GetCurrentQuests().All(t => t.QuestId != s.Id)).ToList();
            if (!possibleQuests.Any())
            {
                session.SendMsg(session.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_ALREADY_HAVE_QUEST), MsgMessageType.Middle);
                return;
            }

            if (session.GetEmptyQuestSlot(QuestSlotType.GENERAL, true) == -1)
            {
                session.SendMsg(_gameLanguageService.GetLanguage(GameDialogKey.QUEST_SHOUTMESSAGE_SLOT_FULL, session.UserLanguage), MsgMessageType.Middle);
                return;
            }

            QuestDto rndQuest = possibleQuests.ElementAt(_randomGenerator.RandomNumber(possibleQuests.Count));
            if (rndQuest == null)
            {
                return;
            }

            if (e.SoundFlowerType == SoundFlowerType.SOUND_FLOWER)
            {
                session.PlayerEntity.DecreasePendingSoundFlowerQuests();
            }

            await e.Sender.EmitEventAsync(new AddQuestEvent(rndQuest.Id, QuestSlotType.GENERAL));
        }
    }
}