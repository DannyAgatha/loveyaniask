using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using Plugin.RainbowBattle.Managers;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleStartEventHandler : IAsyncEventProcessor<RainbowBattleStartEvent>
    {
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;
        private readonly IRainbowBattleManager _rainbowBattleManager;
        private readonly IRainbowFactory _rainbowFactory;
        private readonly IRandomGenerator _randomGenerator;

        public RainbowBattleStartEventHandler(IRainbowFactory rainbowFactory, IRainbowBattleManager rainbowBattleManager,
            RainbowBattleConfiguration rainbowBattleConfiguration, IRandomGenerator randomGenerator)
        {
            _rainbowFactory = rainbowFactory;
            _rainbowBattleManager = rainbowBattleManager;
            _rainbowBattleConfiguration = rainbowBattleConfiguration;
            _randomGenerator = randomGenerator;
        }

        public async Task HandleAsync(RainbowBattleStartEvent e, CancellationToken cancellation)
        {
            RainbowBattleParty rainbowBattleParty = await _rainbowFactory.CreateRainbowBattle(e.RedTeam, e.BlueTeam);
            if (rainbowBattleParty == null)
            {
                return;
            }

            if (!_rainbowBattleManager.IsActive)
            {
                _rainbowBattleManager.IsActive = true;
            }

            _rainbowBattleManager.AddRainbowBattle(rainbowBattleParty);

            await HandleStart(rainbowBattleParty, RainbowBattleTeamType.Red);
            await HandleStart(rainbowBattleParty, RainbowBattleTeamType.Blue);
        }

        private async Task HandleStart(RainbowBattleParty rainbowBattleParty, RainbowBattleTeamType teamType)
        {
            string score = rainbowBattleParty.GenerateRainbowScore(teamType);
            string timeEnter = RainbowBattleExtensions.GenerateRainbowTime(RainbowTimeType.Enter);
            string timeStart = RainbowBattleExtensions.GenerateRainbowTime(RainbowTimeType.Start, (short?)(rainbowBattleParty.EndTime - DateTime.UtcNow).TotalSeconds);
            string rainbowEnter = RainbowBattleExtensions.GenerateRainBowEnter(true);

            string membersPacket = rainbowBattleParty.GenerateRainbowMembers(teamType);
            string memberList = rainbowBattleParty.GenerateRainbowBattleWidget(teamType);

            IReadOnlyList<IClientSession> members = teamType == RainbowBattleTeamType.Red ? rainbowBattleParty.RedTeam : rainbowBattleParty.BlueTeam;

            IMapInstance mapInstance = rainbowBattleParty.MapInstance;
            
            foreach (IClientSession member in members)
            {
                member.PlayerEntity.RainbowBattleComponent.SetRainbowBattle(rainbowBattleParty, teamType);

                member.PlayerEntity.Hp = member.PlayerEntity.MaxHp;
                member.PlayerEntity.Mp = member.PlayerEntity.MaxMp;

                await member.PlayerEntity.RemovePositiveBuffs(100);
                await member.PlayerEntity.RemoveNegativeBuffs(100);
                await member.EmitEventAsync(new RemoveVehicleEvent());

                member.PlayerEntity.ClearSkillCooldowns();
                foreach (IBattleEntitySkill skill in member.PlayerEntity.Skills)
                {
                    skill.LastUse = DateTime.MinValue;
                    member.SendSkillCooldownReset(skill.Skill.CastId);
                }

                short randomX;
                short randomY;

                switch (teamType)
                {
                    case RainbowBattleTeamType.Red:
                        randomX = (short)_randomGenerator.RandomNumber(_rainbowBattleConfiguration.RedStartX, _rainbowBattleConfiguration.RedEndX + 1);
                        randomY = (short)_randomGenerator.RandomNumber(_rainbowBattleConfiguration.RedStartY, _rainbowBattleConfiguration.RedEndY + 1);

                        if (mapInstance.IsBlockedZone(randomX, randomY))
                        {
                            randomX = 0;
                            randomY = 34;
                        }

                        break;
                    case RainbowBattleTeamType.Blue:

                        randomX = (short)_randomGenerator.RandomNumber(_rainbowBattleConfiguration.BlueStartX, _rainbowBattleConfiguration.BlueEndX + 1);
                        randomY = (short)_randomGenerator.RandomNumber(_rainbowBattleConfiguration.BlueStartY, _rainbowBattleConfiguration.BlueEndY + 1);

                        if (mapInstance.IsBlockedZone(randomX, randomY))
                        {
                            randomX = 117;
                            randomY = 42;
                        }

                        break;
                    default:
                        member.PlayerEntity.RainbowBattleComponent.RemoveRainbowBattle();
                        continue;
                }

                member.ChangeMap(mapInstance, randomX, randomY);
                
                member.SendPacket(score);
                member.SendPacket(timeEnter);
                member.SendPacket(timeStart);
                member.SendPacket(membersPacket);
                member.SendPacket(rainbowEnter);
                member.SendPacket(memberList);
                member.BroadcastRainbowTeamType();
                
                for (int countdown = 5; countdown >= 1; countdown--)
                {
                    foreach (IClientSession sessionTeam in members)
                    {
                        sessionTeam.SendMsg(member.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_SHOUTMESSAGE_START_IN, countdown), MsgMessageType.Middle);
                    }
                    await Task.Delay(1000);
                }
                
                await member.EmitEventAsync(new RainbowBattleJoinEvent());
            }
        }
    }
}