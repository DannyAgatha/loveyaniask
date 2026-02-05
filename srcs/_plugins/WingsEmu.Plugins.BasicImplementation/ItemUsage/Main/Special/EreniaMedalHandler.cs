using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Data.Prestige;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Configurations.Prestige;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Prestige;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class EreniaMedalHandler : IItemHandler
{
    private readonly PrestigeConfiguration _gameLanguage;

    public EreniaMedalHandler(PrestigeConfiguration gameLanguage)
    {
        _gameLanguage = gameLanguage;
    }

    public ItemType ItemType => ItemType.Special;
    public long[] Effects => [201];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        CharacterPrestigeDto prestige = session.PlayerEntity.CharacterPrestigeDto;

        if (prestige == null)
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.PRESTIGE_NO_DATA));
            return;
        }

        if (prestige.Tasks == null || !prestige.Tasks.All(t => t.Completed))
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.PRESTIGE_TASKS_MUST_BE_COMPLETED));
            return;
        }

        PrestigeLevelConfig levelConfig = _gameLanguage.PrestigeLevels
            .FirstOrDefault(p => p.Level == (prestige.CurrentPrestigeLevel + 1));

        if (levelConfig?.FinalChallenge == null)
        {
            session.SendInfo(session.GetLanguage(GameDialogKey.PRESTIGE_FINAL_CHALLENGE_NOT_FOUND));
            return;
        }

        PrestigeRequirements requirements = levelConfig.FinalChallenge.Requirements;

        if (requirements != null)
        {
            if (session.PlayerEntity.Level < requirements.MinPlayerLevel)
            {
                session.SendInfo(session.GetLanguageFormat(GameDialogKey.PRESTIGE_MIN_LEVEL_REQUIRED, requirements.MinPlayerLevel));
                return;
            }

            if (requirements.MinPlayerHeroLevel.HasValue && session.PlayerEntity.HeroLevel < requirements.MinPlayerHeroLevel.Value)
            {
                session.SendInfo(session.GetLanguageFormat(GameDialogKey.PRESTIGE_MIN_HERO_LEVEL_REQUIRED, requirements.MinPlayerHeroLevel.Value));
                return;
            }
        }

        await session.RemoveItemFromInventory(item: e.Item, amount: 1);

        await session.EmitEventAsync(new PrestigeCreateFinalChallengeInstanceEvent
        {
            FinalChallenge = levelConfig.FinalChallenge
        });
    }
}
