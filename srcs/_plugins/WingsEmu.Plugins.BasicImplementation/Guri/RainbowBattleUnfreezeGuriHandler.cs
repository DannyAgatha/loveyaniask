using System.Threading.Tasks;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class RainbowBattleUnfreezeGuriHandler : IGuriHandler
{
    private readonly IDelayManager _delayManager;

    public RainbowBattleUnfreezeGuriHandler(IDelayManager delayManager) => _delayManager = delayManager;

    public long GuriEffectId => 505;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        if (!await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.RainbowBattleUnfreeze))
        {
            return;
        }

        await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.RainbowBattleUnfreeze);

        RainbowBattleParty rainbowBattleParty = session.PlayerEntity.RainbowBattleComponent.RainbowBattleParty;
        if (rainbowBattleParty?.MapInstance == null)
        {
            return;
        }

        long characterId = e.Data;
        if (characterId == session.PlayerEntity.Id)
        {
            return;
        }

        IPlayerEntity target = rainbowBattleParty.MapInstance.GetCharacterById(characterId);
        if (target?.RainbowBattleComponent.RainbowBattleParty == null)
        {
            return;
        }

        if (!target.RainbowBattleComponent.IsFrozen)
        {
            return;
        }

        if (target.RainbowBattleComponent.Team != session.PlayerEntity.RainbowBattleComponent.Team)
        {
            return;
        }

        await target.Session.EmitEventAsync(new RainbowBattleUnfreezeEvent
        {
            Unfreezer = session.PlayerEntity
        });
    }
}