using System;
using System.Threading;
using System.Threading.Tasks;
using NosEmu.Plugins.BasicImplementations.Revival;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.Revival;

namespace NosEmu.Plugins.BasicImplementations.Event.Mates;

public class MateReviveEventHandler : IAsyncEventProcessor<MateReviveEvent>
{
    private readonly RevivalEventBaseHandler _eventBaseHandler;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IGameLanguageService _language;
    private readonly IRankingManager _rankingManager;
    private readonly IReputationConfiguration _reputationConfiguration;
    private readonly ISpPartnerConfiguration _spPartner;

    public MateReviveEventHandler(ISpPartnerConfiguration spPartner, IGameLanguageService language,
        RevivalEventBaseHandler eventBaseHandler, IRankingManager rankingManager, IReputationConfiguration reputationConfiguration, IGameLanguageService gameLanguage)
    {
        _spPartner = spPartner;
        _language = language;
        _eventBaseHandler = eventBaseHandler;
        _rankingManager = rankingManager;
        _reputationConfiguration = reputationConfiguration;
        _gameLanguage = gameLanguage;
    }

    public async Task HandleAsync(MateReviveEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IMateEntity mateEntity = e.MateEntity;

        if (mateEntity.IsAlive())
        {
            return;
        }

        if (!BasicUnregisteringForMates(mateEntity, e.Delayed, e.ExpectedGuid))
        {
            return;
        }

        mateEntity.Hp = mateEntity.MaxHp / 2;
        mateEntity.Mp = mateEntity.MaxMp / 2;
        session.SendMateLife(mateEntity);

        if (session.PlayerEntity.IsOnVehicle)
        {
            return;
        }

        // yes, twice
        session.CurrentMapInstance.Broadcast(x => mateEntity.GenerateOut());
        session.CurrentMapInstance.Broadcast(x => mateEntity.GenerateOut());

        bool isMiniland = mateEntity.MapInstance.MapInstanceType == MapInstanceType.Miniland;

        if (!isMiniland)
        {
            mateEntity.TeleportNearCharacter();
        }

        switch (mateEntity.MapInstance.MapInstanceType)
        {
            case MapInstanceType.RainbowBattle:
                session.BroadcastIn(_reputationConfiguration, _rankingManager.TopReputation, new ExceptSessionBroadcast(session));
                session.BroadcastGidx(session.PlayerEntity.Family, _gameLanguage);
                session.BroadcastRainbowTeamType();
                break;
        }

        if (isMiniland)
        {
            mateEntity.MapInstance.Broadcast(s => mateEntity.GenerateIn(_language, s.UserLanguage, _spPartner));
        }
        else
        {
            session.CurrentMapInstance.Broadcast(s => mateEntity.GenerateIn(_language, s.UserLanguage, _spPartner));
        }

        session.SendCondMate(mateEntity);
        session.RefreshParty(_spPartner);
    }

    public bool BasicUnregisteringForMates(IMateEntity mateEntity, bool delayed, Guid expectedGuid)
        => mateEntity.Owner.Session.IsConnected && _eventBaseHandler.BasicUnregistering(mateEntity.Id, delayed ? ForcedType.Forced : ForcedType.NoForced, expectedGuid);
}