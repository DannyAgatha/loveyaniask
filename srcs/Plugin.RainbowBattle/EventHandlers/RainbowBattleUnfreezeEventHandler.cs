using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.Groups;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleUnfreezeEventHandler : IAsyncEventProcessor<RainbowBattleUnfreezeEvent>
    {
        private readonly IBuffFactory _buffFactory;
        private readonly IGameLanguageService _gameLanguageService;
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IRankingManager _rankingManager;
        private readonly IReputationConfiguration _reputationConfiguration;
        private readonly ISpPartnerConfiguration _spPartner;

        public RainbowBattleUnfreezeEventHandler(IGameLanguageService gameLanguageService, ISpPartnerConfiguration spPartner, RainbowBattleConfiguration rainbowBattleConfiguration,
            IRandomGenerator randomGenerator, IBuffFactory buffFactory, IReputationConfiguration reputationConfiguration, IRankingManager rankingManager)
        {
            _gameLanguageService = gameLanguageService;
            _spPartner = spPartner;
            _rainbowBattleConfiguration = rainbowBattleConfiguration;
            _randomGenerator = randomGenerator;
            _buffFactory = buffFactory;
            _reputationConfiguration = reputationConfiguration;
            _rankingManager = rankingManager;
        }

        public async Task HandleAsync(RainbowBattleUnfreezeEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IPlayerEntity target = e.Unfreezer;

            RainbowBattleParty rainbowBattleParty = session.PlayerEntity.RainbowBattleComponent.RainbowBattleParty;
            IMapInstance mapInstance = rainbowBattleParty?.MapInstance;
            if (rainbowBattleParty == null || mapInstance == null)
            {
                return;
            }

            if (!session.PlayerEntity.RainbowBattleComponent.IsFrozen)
            {
                return;
            }

            if (target != null)
            {
                if (!target.RainbowBattleComponent.IsInRainbowBattle)
                {
                    return;
                }

                if (target.RainbowBattleComponent.Team != session.PlayerEntity.RainbowBattleComponent.Team)
                {
                    return;
                }

                if (target.RainbowBattleComponent.IsFrozen)
                {
                    return;
                }

                if (target.Position.GetDistance(session.PlayerEntity.Position) > 5)
                {
                    return;
                }
            }

            session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
            session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
            session.PlayerEntity.RainbowBattleComponent.IsFrozen = false;
            session.PlayerEntity.RainbowBattleComponent.FrozenTime = null;
            await session.PlayerEntity.RemoveNegativeBuffs(100);
            session.SendCondPacket();
            session.RefreshStat();

            foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
            {
                session.PlayerEntity.MapInstance.AddMate(mate);
            }

            if ((session.PlayerEntity.RainbowBattleComponent.Deaths % 3) == 0)
            {
                Buff angry = _buffFactory.CreateBuff((short)BuffVnums.ANGRY, session.PlayerEntity);
                await session.PlayerEntity.AddBuffAsync(angry);
            }

            session.BroadcastIn(_reputationConfiguration, _rankingManager.TopReputation, new ExceptSessionBroadcast(session));
            session.BroadcastGidx(session.PlayerEntity.Family, _gameLanguageService);
            session.BroadcastRainbowTeamType();
            session.BroadcastInTeamMembers(_gameLanguageService, _spPartner);
            session.RefreshParty(_spPartner);

            if (target != null)
            {
                IReadOnlyList<IClientSession> members = session.PlayerEntity.RainbowBattleComponent.Team == RainbowBattleTeamType.Red ? rainbowBattleParty.RedTeam : rainbowBattleParty.BlueTeam;

                foreach (IClientSession member in members)
                {
                    member.SendMsg(member.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_SHOUTMESSAGE_UNFROZEN, session.PlayerEntity.Name), MsgMessageType.Middle);
                }

                target.RainbowBattleComponent.ActivityPoints += _rainbowBattleConfiguration.UnfreezeActivityPoints;
                session.PlayerEntity.RainbowBattleComponent.UnfrozenPlayerCount += 1;
                return;
            }

            // It's by time so move frozen player to start position / main flag

            Buff buff = _buffFactory.CreateBuff((short)BuffVnums.RAINBOW_PROTECTION, session.PlayerEntity, BuffFlag.NORMAL);
            await session.PlayerEntity.AddBuffAsync(buff);

            switch (session.PlayerEntity.RainbowBattleComponent.Team)
            {
                case RainbowBattleTeamType.Red:

                    short randomX = (short)_randomGenerator.RandomNumber(_rainbowBattleConfiguration.RedStartX, _rainbowBattleConfiguration.RedEndX + 1);
                    short randomY = (short)_randomGenerator.RandomNumber(_rainbowBattleConfiguration.RedStartY, _rainbowBattleConfiguration.RedEndY + 1);

                    if (mapInstance.IsBlockedZone(randomX, randomY))
                    {
                        randomX = 0;
                        randomY = 34;
                    }

                    session.PlayerEntity.TeleportOnMap(randomX, randomY, true);

                    break;
                case RainbowBattleTeamType.Blue:

                    randomX = (short)_randomGenerator.RandomNumber(_rainbowBattleConfiguration.BlueStartX, _rainbowBattleConfiguration.BlueEndX + 1);
                    randomY = (short)_randomGenerator.RandomNumber(_rainbowBattleConfiguration.BlueStartY, _rainbowBattleConfiguration.BlueEndY + 1);

                    if (mapInstance.IsBlockedZone(randomX, randomY))
                    {
                        randomX = 117;
                        randomY = 42;
                    }

                    session.PlayerEntity.TeleportOnMap(randomX, randomY, true);

                    break;
            }
        }
    }
}