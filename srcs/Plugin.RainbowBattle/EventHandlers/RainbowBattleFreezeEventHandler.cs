using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Extensions;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleFreezeEventHandler : IAsyncEventProcessor<RainbowBattleFreezeEvent>
    {
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;
        private readonly IBuffFactory _buffFactory;

        public RainbowBattleFreezeEventHandler(RainbowBattleConfiguration rainbowBattleConfiguration,  IBuffFactory buffFactory)
        {
            _rainbowBattleConfiguration = rainbowBattleConfiguration;
            _buffFactory = buffFactory;
        }

        public async Task HandleAsync(RainbowBattleFreezeEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            IBattleEntity killer = e.Killer;

            RainbowBattleParty rainbowParty = session.PlayerEntity.RainbowBattleComponent.RainbowBattleParty;
            if (rainbowParty == null)
            {
                return;
            }

            session.PlayerEntity.RainbowBattleComponent.IsFrozen = true;
            if ((short?)(rainbowParty.EndTime - DateTime.UtcNow).TotalSeconds >= 480.6)
            {
                session.PlayerEntity.RainbowBattleComponent.FrozenTime = DateTime.UtcNow.AddSeconds(15);
            }
            else if ((short?)(rainbowParty.EndTime - DateTime.UtcNow).TotalSeconds >= 240.6)
            {
                session.PlayerEntity.RainbowBattleComponent.FrozenTime = DateTime.UtcNow.AddSeconds(18);
            }
            else
            {
                session.PlayerEntity.RainbowBattleComponent.FrozenTime = DateTime.UtcNow.AddSeconds(20);
            }

            session.SendCondPacket();
            session.BroadcastEffect(EffectType.Frozen);
            await session.PlayerEntity.RemoveNegativeBuffs(100);

            session.PlayerEntity.Hp = session.PlayerEntity.MaxHp;
            session.PlayerEntity.Mp = session.PlayerEntity.MaxMp;
            session.RefreshStat();

            foreach (IMateEntity mate in session.PlayerEntity.MateComponent.TeamMembers())
            {
                session.PlayerEntity.MapInstance.RemoveMate(mate);
                session.PlayerEntity.MapInstance.Broadcast(mate.GenerateOut());
            }

            session.PlayerEntity.RainbowBattleComponent.Deaths++;
            session.PlayerEntity.RainbowBattleComponent.ActivityPoints += _rainbowBattleConfiguration.DeathActivityPoints;

            IPlayerEntity playerKiller = killer switch
            {
                IPlayerEntity playerEntity => playerEntity,
                IMateEntity mateEntity => mateEntity.Owner,
                IMonsterEntity monsterEntity => monsterEntity.SummonerType != null && monsterEntity.SummonerId != null && monsterEntity.SummonerType == VisualType.Player
                    ? monsterEntity.MapInstance.GetCharacterById(monsterEntity.SummonerId.Value)
                    : null,
                _ => null
            };

            if (playerKiller?.RainbowBattleComponent.RainbowBattleParty == null)
            {
                return;
            }

            IReadOnlyList<IClientSession> members = session.PlayerEntity.RainbowBattleComponent.RainbowBattleParty.MapInstance.Sessions;
            
            foreach (IClientSession member in members)
            {
                member.SendMsg(member.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_SHOUTMESSAGE_FROZEN, session.PlayerEntity.Name), MsgMessageType.Middle);
                member.SendChatMessage(member.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_SHOUTMESSAGE_FROZEN, session.PlayerEntity.Name), ChatMessageColorType.LightPurple);
            }
            
            RainbowBattleTeamType playerTeam = playerKiller.RainbowBattleComponent.Team;
            
            int teamPoints = 1; 
            
            if (playerKiller.BCardComponent.HasBCard(BCardType.RainbowBattleEffects, (byte)AdditionalTypes.RainbowBattleEffects.PointsEarnedMultiplierInRainbowBattle))
            {
                int multiplier = playerKiller.BCardComponent.GetAllBCardsInformation(BCardType.RainbowBattleEffects, 
                    (byte)AdditionalTypes.RainbowBattleEffects.PointsEarnedMultiplierInRainbowBattle, playerKiller.Level).firstData;
    
                teamPoints *= multiplier;
            }

            switch (playerTeam)
            {
                case RainbowBattleTeamType.Red:
                    playerKiller.RainbowBattleComponent.RainbowBattleParty.IncreaseRedPoints(teamPoints);
                    break;
                case RainbowBattleTeamType.Blue:
                    playerKiller.RainbowBattleComponent.RainbowBattleParty.IncreaseBluePoints(teamPoints);
                    break;
            }

            await session.EmitEventAsync(new RainbowBattleRefreshScoreEvent
            {
                RainbowBattleParty = rainbowParty
            });

            playerKiller.RainbowBattleComponent.Kills++;
            playerKiller.RainbowBattleComponent.KillStreak++;
            playerKiller.RainbowBattleComponent.ActivityPoints += _rainbowBattleConfiguration.KillActivityPoints;
            playerKiller.RainbowBattleComponent.IsInKillStreak = playerKiller.RainbowBattleComponent.KillStreak >= _rainbowBattleConfiguration.RequiredKillsForStreak;
            
            IReadOnlyList<IClientSession> playerMembers = playerKiller.RainbowBattleComponent.Team == RainbowBattleTeamType.Red
                ? playerKiller.RainbowBattleComponent.RainbowBattleParty.RedTeam
                : playerKiller.RainbowBattleComponent.RainbowBattleParty.BlueTeam;

            string memberList = playerKiller.RainbowBattleComponent.RainbowBattleParty.GenerateRainbowBattleWidget(playerKiller.RainbowBattleComponent.Team);
            foreach (IClientSession member in playerMembers)
            {
                member.SendPacket(memberList);
            }

            await session.EmitEventAsync(new RainbowBattleFrozenEvent
            {
                Id = playerKiller.RainbowBattleComponent.RainbowBattleParty.Id,
                Killer = new RainbowBattlePlayerDump
                {
                    CharacterId = playerKiller.Id,
                    Level = playerKiller.Level,
                    Class = playerKiller.Class,
                    Specialist = playerKiller.Specialist,
                    TotalFireResistance = playerKiller.FireResistance,
                    TotalWaterResistance = playerKiller.WaterResistance,
                    TotalLightResistance = playerKiller.LightResistance,
                    TotalDarkResistance = playerKiller.DarkResistance,
                    FairyLevel = playerKiller.Fairy?.ElementRate + playerKiller.Fairy?.GameItem.ElementRate,
                    Score = playerKiller.RainbowBattleComponent.ActivityPoints,
                    Team = playerKiller.RainbowBattleComponent.Team.ToString()
                },
                Killed = new RainbowBattlePlayerDump
                {
                    CharacterId = session.PlayerEntity.Id,
                    Level = session.PlayerEntity.Level,
                    Class = session.PlayerEntity.Class,
                    Specialist = session.PlayerEntity.Specialist,
                    TotalFireResistance = session.PlayerEntity.FireResistance,
                    TotalWaterResistance = session.PlayerEntity.WaterResistance,
                    TotalLightResistance = session.PlayerEntity.LightResistance,
                    TotalDarkResistance = session.PlayerEntity.DarkResistance,
                    FairyLevel = session.PlayerEntity.Fairy?.ElementRate + session.PlayerEntity.Fairy?.GameItem.ElementRate,
                    Score = session.PlayerEntity.RainbowBattleComponent.ActivityPoints,
                    Team = session.PlayerEntity.RainbowBattleComponent.Team.ToString()
                }
            });
        }
    }
}