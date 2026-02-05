using System;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Plugin.Act4.Extension;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act4.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Revival;

namespace Plugin.Act4.Event;

public class RevivalAskEventDungeonHandler : IAsyncEventProcessor<RevivalAskEvent>
{
    private readonly Act4DungeonsConfiguration _act4DungeonsConfiguration;
    private readonly IGameLanguageService _languageService;

    public RevivalAskEventDungeonHandler(IGameLanguageService languageService, Act4DungeonsConfiguration act4DungeonsConfiguration)
    {
        _languageService = languageService;
        _act4DungeonsConfiguration = act4DungeonsConfiguration;
    }

    public async Task HandleAsync(RevivalAskEvent e, CancellationToken cancellation)
    {
        if (e.Sender.PlayerEntity.IsAlive() || e.AskRevivalType != AskRevivalType.DungeonRevival)
        {
            return;
        }

        int reputationCost = (int)(e.Sender.GetDungeonReputationRequirement(_act4DungeonsConfiguration.DungeonEntryCostMultiplier) / 1.8);
        reputationCost = (int)(Math.Round(reputationCost / 10.0) * 10);

        if (e.Sender.PlayerEntity.Reput < reputationCost)
        {
            e.Sender.PlayerEntity.UpdateRevival(DateTime.MinValue, RevivalType.DontPayRevival, ForcedType.Forced);
            return;
        }

        e.Sender.SendDialog(CharacterPacketExtension.GenerateRevivalPacket(RevivalType.TryPayRevival), CharacterPacketExtension.GenerateRevivalPacket(RevivalType.DontPayRevival),
            _languageService.GetLanguageFormat(GameDialogKey.ACT4_DUNGEON_REVIVAL_DIALOG, e.Sender.UserLanguage, reputationCost.ToString()));
    }
}