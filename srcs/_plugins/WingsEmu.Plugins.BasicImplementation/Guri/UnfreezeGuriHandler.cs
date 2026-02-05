using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Raids;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class UnfreezeGuriHandler : IGuriHandler
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public UnfreezeGuriHandler(IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _gameLanguage = gameLanguage;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public long GuriEffectId => 502;

    public async Task ExecuteAsync(IClientSession session, GuriEvent guriPacket)
    {
        long id = guriPacket.Data;

        if (id == 0)
        {
            return;
        }

        IBattleEntity battleEntity = session.PlayerEntity.MapInstance.GetBattleEntities(x => x is not null && x.Id == id).FirstOrDefault();
        if (battleEntity is null)
        {
            return;
        }

        if (!battleEntity.IsFrozenByGlacerus())
        {
            return;
        }

        await battleEntity.RemoveEternalIce();

        if (battleEntity is not IPlayerEntity playerEntity)
        {
            return;
        }

        foreach (IMateEntity mateEntity in playerEntity.MateComponent.TeamMembers())
        {
            if (!mateEntity.IsAlive())
            {
                continue;
            }

            mateEntity.TeleportNearCharacter();
            mateEntity.MapInstance.Broadcast(s => mateEntity.GenerateIn(_gameLanguage, s.UserLanguage, _spPartnerConfiguration));
            session.SendCondMate(mateEntity);
            session.RefreshParty(_spPartnerConfiguration);
        }
    }
}