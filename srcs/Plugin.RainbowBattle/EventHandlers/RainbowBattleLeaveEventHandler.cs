using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Data.Character;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleLeaveEventHandler : IAsyncEventProcessor<RainbowBattleLeaveEvent>
    {
        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;

        public RainbowBattleLeaveEventHandler(IAsyncEventPipeline eventPipeline, RainbowBattleConfiguration rainbowBattleConfiguration)
        {
            _eventPipeline = eventPipeline;
            _rainbowBattleConfiguration = rainbowBattleConfiguration;
        }

        public async Task HandleAsync(RainbowBattleLeaveEvent e, CancellationToken cancellation)
        {
            IClientSession session = e.Sender;
            RainbowBattleParty rainbowBattleParty = session.PlayerEntity.RainbowBattleComponent.RainbowBattleParty;
            if (rainbowBattleParty == null)
            {
                return;
            }

            if (e.CheckIfFinished && rainbowBattleParty.FinishTime == null)
            {
                return;
            }

            RainbowBattleTeamType team = session.PlayerEntity.RainbowBattleComponent.Team;
            IReadOnlyList<IClientSession> members = team == RainbowBattleTeamType.Red ? rainbowBattleParty.RedTeam : rainbowBattleParty.BlueTeam;

            switch (team)
            {
                case RainbowBattleTeamType.Red:
                    session.PlayerEntity.RainbowBattleComponent.RainbowBattleParty.RemoveRedPlayer(session);
                    break;
                case RainbowBattleTeamType.Blue:
                    session.PlayerEntity.RainbowBattleComponent.RainbowBattleParty.RemoveBluePlayer(session);
                    break;
            }

            string membersPacket = rainbowBattleParty.GenerateRainbowMembers(team);
            string memberList = rainbowBattleParty.GenerateRainbowBattleWidget(team);
            string rainbowScore = rainbowBattleParty.GenerateRainbowScore(team);

            foreach (IClientSession member in members)
            {
                if (e.SendMessage)
                {
                    member.SendMsg(member.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_MESSAGE_PLAYER_LEFT, session.PlayerEntity.Name), MsgMessageType.Middle);
                    member.SendChatMessage(member.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_MESSAGE_PLAYER_LEFT, session.PlayerEntity.Name), ChatMessageColorType.Yellow);
                }

                member.SendPacket(membersPacket);
                member.SendPacket(memberList);
                member.SendPacket(rainbowScore);
            }

            session.SendPacket(RainbowBattleExtensions.GenerateRainbowTime(RainbowTimeType.End));
            session.SendPacket(RainbowBattleExtensions.GenerateRainBowEnter(false));
            session.SendPacket(RainbowBattleExtensions.GenerateRainBowExit());

            if (e.AddLeaverBuster && rainbowBattleParty.FinishTime == null)
            {
                ProcessLeaverBuster(session);
            }

            session.PlayerEntity.RainbowBattleComponent.RemoveRainbowBattle();
            session.ChangeToLastBaseMap();

            if (rainbowBattleParty.BlueTeam.Count == 0 || rainbowBattleParty.RedTeam.Count == 0)
            {
                await _eventPipeline.ProcessEventAsync(new RainbowBattleEndEvent
                {
                    RainbowBattleParty = rainbowBattleParty
                });
            }
        }

        private void ProcessLeaverBuster(IClientSession session)
        {
            session.PlayerEntity.RainbowBattleLeaverBusterDto ??= new RainbowBattleLeaverBusterDto();
            RainbowBattleLeaverBusterDto leaver = session.PlayerEntity.RainbowBattleLeaverBusterDto;
            if (leaver.Exits < 3) // Only 3 times per month
            {
                leaver.Exits++;
                return;
            }

            leaver.RewardPenalty++;
        }
    }
}