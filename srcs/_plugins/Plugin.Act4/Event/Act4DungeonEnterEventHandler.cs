using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using PhoenixLib.Logging;
using Plugin.Act4.Extension;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Act4.Event;
using WingsEmu.Game.Extensions;

namespace Plugin.Act4.Event;

public class Act4DungeonSystemException : Exception
{
    public Act4DungeonSystemException(string message) : base(message)
    {
    }
}

public class Act4DungeonEnterEventHandler : IAsyncEventProcessor<Act4DungeonEnterEvent>
{
    private readonly Act4DungeonsConfiguration _act4DungeonsConfiguration;
    private readonly IDungeonFactory _dungeonFactory;
    private readonly IAct4DungeonManager _dungeonManager;
    private readonly IGameLanguageService _languageService;

    public Act4DungeonEnterEventHandler(IAct4DungeonManager dungeonManager, IGameLanguageService languageService, Act4DungeonsConfiguration act4DungeonsConfiguration, IDungeonFactory dungeonFactory)
    {
        _dungeonManager = dungeonManager;
        _languageService = languageService;
        _act4DungeonsConfiguration = act4DungeonsConfiguration;
        _dungeonFactory = dungeonFactory;
    }

    public async Task HandleAsync(Act4DungeonEnterEvent e, CancellationToken cancellation)
    {
        if (!_dungeonManager.DungeonsActive)
        {
            return;
        }

        if (e.Sender.PlayerEntity.Faction != _dungeonManager.AllowedFaction)
        {
            e.Sender.SendInformationChatMessage(_languageService.GetLanguage(GameDialogKey.PORTAL_CHATMESSAGE_BLOCKED, e.Sender.UserLanguage));
            return;
        }

        if (!e.Sender.PlayerEntity.IsInFamily())
        {
            e.Sender.SendInformationChatMessage(_languageService.GetLanguage(GameDialogKey.ACT4_DUNGEON_CHATMESSAGE_FAMILY_NEEDED, e.Sender.UserLanguage));
            return;
        }

        long familyId = e.Sender.PlayerEntity.Family.Id;

        int reputationCost = e.Sender.GetDungeonReputationRequirement(_act4DungeonsConfiguration.DungeonEntryCostMultiplier);
        if (!e.Confirmed)
        {
            e.Sender.SendQnaPacket("preq 1", _languageService.GetLanguageFormat(GameDialogKey.ACT4_DUNGEON_QNAMESSAGE_ENTRY_COST, e.Sender.UserLanguage, reputationCost));
            return;
        }

        if (!e.Sender.PlayerEntity.RemoveReputation(reputationCost))
        {
            e.Sender.SendInformationChatMessage(_languageService.GetLanguage(GameDialogKey.INFORMATION_CHATMESSAGE_NOT_ENOUGH_REPUT, e.Sender.UserLanguage));
            return;
        }

        DungeonInstance dungeon = _dungeonManager.GetDungeon(familyId);

        if (dungeon == null)
        {
            dungeon = _dungeonFactory.CreateDungeon(familyId, _dungeonManager.DungeonType);

            if (dungeon != null)
            {
                _dungeonManager.RegisterDungeon(dungeon);
            }
        }

        if (dungeon == null)
        {
            Log.Error($"[ACT4_DUNGEON_ENTER] Wasn't able to generate a new Dungeon for the Family with FamilyId: '{familyId.ToString()}'",
                new Act4DungeonSystemException($"FamilyId: {familyId} Dungeon Faction: {_dungeonManager.AllowedFaction} Dungeon Type: {_dungeonManager.DungeonType}"));
            return;
        }

        e.Sender.ChangeMap(dungeon.SpawnInstance.MapInstance, dungeon.SpawnPoint.X, dungeon.SpawnPoint.Y);
    }
}