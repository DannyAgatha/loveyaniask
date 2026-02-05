using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Packets.Enums.Rainbow;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Game.RainbowBattle;
using WingsEmu.Game.RainbowBattle.Event;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.RainbowBattle.EventHandlers
{
    public class RainbowBattleCaptureFlagEventHandler : IAsyncEventProcessor<RainbowBattleCaptureFlagEvent>
    {
        private readonly IDelayManager _delayManager;
        private readonly IGameLanguageService _gameLanguageService;
        private readonly RainbowBattleConfiguration _rainbowBattleConfiguration;

        public RainbowBattleCaptureFlagEventHandler(IDelayManager delayManager, IGameLanguageService gameLanguageService, RainbowBattleConfiguration rainbowBattleConfiguration)
        {
            _delayManager = delayManager;
            _gameLanguageService = gameLanguageService;
            _rainbowBattleConfiguration = rainbowBattleConfiguration;
        }

        public async Task HandleAsync(RainbowBattleCaptureFlagEvent e, CancellationToken cancellation)
        {
            INpcEntity npc = e.NpcEntity;
            IClientSession session = e.Sender;

            if (npc.RainbowFlag == null)
            {
                return;
            }

            RainbowBattleParty rainbowBattleParty = session.PlayerEntity.RainbowBattleComponent.RainbowBattleParty;
            if (rainbowBattleParty == null)
            {
                return;
            }

            if (!rainbowBattleParty.Started)
            {
                return;
            }

            if (rainbowBattleParty.FinishTime != null)
            {
                return;
            }

            if (session.PlayerEntity.RainbowBattleComponent.IsFrozen)
            {
                return;
            }

            int distance = session.PlayerEntity.Position.GetDistance(npc.Position);
            if (distance > 5)
            {
                return;
            }

            RainbowBattleTeamType? flagTeam = npc.RainbowFlag.FlagTeamType switch
            {
                RainbowBattleFlagTeamType.Red => RainbowBattleTeamType.Red,
                RainbowBattleFlagTeamType.Blue => RainbowBattleTeamType.Blue,
                _ => null
            };

            RainbowBattleTeamType playerTeam = session.PlayerEntity.RainbowBattleComponent.Team;

            if (flagTeam == playerTeam)
            {
                session.SendChatMessage(session.GetLanguage(GameDialogKey.RAINBOW_BATTLE_CHATMESSAGE_ALREADY_CAPTURED), ChatMessageColorType.Yellow);
                return;
            }

            if (npc.RainbowFlag.RainbowBattleLastTakeOver.AddSeconds(_rainbowBattleConfiguration.DelayBetweenCapture) > DateTime.UtcNow)
            {
                int cooldown = (int)(npc.RainbowFlag.RainbowBattleLastTakeOver.AddSeconds(_rainbowBattleConfiguration.DelayBetweenCapture) - DateTime.UtcNow).TotalSeconds;
                session.SendChatMessage(session.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_CHATMESSAGE_CAPTURE_COOLDOWN, cooldown), ChatMessageColorType.Yellow);
                return;
            }

            if (!e.IsConfirm)
            {
                await session.PlayerEntity.RemoveInvisibility();
                DateTime waitUntil = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.RainbowBattleCaptureFlag);
                session.SendDelay((int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.IceBreaker, $"guri 504 {npc.Id}");
                return;
            }

            if (!await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.RainbowBattleCaptureFlag))
            {
                return;
            }

            await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.RainbowBattleCaptureFlag);

            session.PlayerEntity.RainbowBattleComponent.CapturedFlagCount += 1;
            // If flag was red and player is in blue team
            if (flagTeam != null && flagTeam != playerTeam)
            {
                ConcurrentDictionary<RainbowBattleFlagType, byte> flags = flagTeam == RainbowBattleTeamType.Red ? rainbowBattleParty.RedFlags : rainbowBattleParty.BlueFlags;

                byte flagsCounter = (byte)(flags.TryGetValue(npc.RainbowFlag.FlagType, out byte count) ? count : 0);
                if (flagsCounter > 0)
                {
                    flagsCounter--;
                    flags[npc.RainbowFlag.FlagType] = flagsCounter;
                }
            }

            int flagPoints = 2;
            
            if (session.PlayerEntity.BCardComponent.HasBCard(BCardType.RainbowBattleEffects, (byte)AdditionalTypes.RainbowBattleEffects.PointsEarnedMultiplierInRainbowBattle))
            {
                int multiplier = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.RainbowBattleEffects, (byte)AdditionalTypes.RainbowBattleEffects.PointsEarnedMultiplierInRainbowBattle, session.PlayerEntity.Level).firstData;
                flagPoints *= multiplier;
            }
            
            if (session.PlayerEntity.BCardComponent.HasBCard(BCardType.RainbowBattleEffects, (byte)AdditionalTypes.RainbowBattleEffects.PointsEarnedReductionInRainbowBattle))
            {
                int reductionFactor = session.PlayerEntity.BCardComponent.GetAllBCardsInformation(BCardType.RainbowBattleEffects, 
                    (byte)AdditionalTypes.RainbowBattleEffects.PointsEarnedReductionInRainbowBattle, session.PlayerEntity.Level).firstData;
    
                flagPoints = Math.Max(1, flagPoints / reductionFactor);
            }
            
            npc.RainbowFlag.RainbowBattleLastTakeOver = DateTime.UtcNow;

            switch (playerTeam)
            {
                case RainbowBattleTeamType.Red:

                    byte redFlags = (byte)(rainbowBattleParty.RedFlags.TryGetValue(npc.RainbowFlag.FlagType, out byte redCount) ? redCount : 0);
                    redFlags++;
                    rainbowBattleParty.RedFlags[npc.RainbowFlag.FlagType] = redFlags;

                    npc.RainbowFlag.FlagTeamType = RainbowBattleFlagTeamType.Red;

                    rainbowBattleParty.IncreaseRedPoints(flagPoints);
                    rainbowBattleParty.IncreaseBluePoints(-flagPoints);

                    break;
                case RainbowBattleTeamType.Blue:

                    byte blueFlags = (byte)(rainbowBattleParty.BlueFlags.TryGetValue(npc.RainbowFlag.FlagType, out byte blueCount) ? blueCount : 0);
                    blueFlags++;
                    rainbowBattleParty.BlueFlags[npc.RainbowFlag.FlagType] = blueFlags;

                    npc.RainbowFlag.FlagTeamType = RainbowBattleFlagTeamType.Blue;

                    rainbowBattleParty.IncreaseBluePoints(flagPoints);
                    rainbowBattleParty.IncreaseRedPoints(-flagPoints);

                    break;
            }

            npc.MapInstance.Broadcast(npc.GenerateFlagPacket());

            EffectType effectType = npc.RainbowFlag.FlagTeamType switch
            {
                RainbowBattleFlagTeamType.None => EffectType.NoneFlag,
                RainbowBattleFlagTeamType.Red => EffectType.RedFlag,
                RainbowBattleFlagTeamType.Blue => EffectType.BlueFlag,
                _ => EffectType.NoneFlag
            };

            npc.MapInstance.Broadcast(npc.GenerateEffectPacket(effectType));

            foreach (IClientSession red in rainbowBattleParty.RedTeam)
            {
                string npcName = _gameLanguageService.GetNpcMonsterName(npc, red);

                if (playerTeam == RainbowBattleTeamType.Red)
                {
                    red.SendMsg(red.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_MESSAGE_ALLY_CAPTURED, session.PlayerEntity.Name, npcName), MsgMessageType.Middle);
                    red.SendChatMessage(red.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_MESSAGE_ALLY_CAPTURED, session.PlayerEntity.Name, npcName), ChatMessageColorType.Green);
                }
                else
                {
                    red.SendMsg(red.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_MESSAGE_ENEMY_CAPTURED, npcName), MsgMessageType.Middle);
                    red.SendChatMessage(red.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_MESSAGE_ENEMY_CAPTURED, npcName), ChatMessageColorType.Red);
                }
            }

            foreach (IClientSession blue in rainbowBattleParty.BlueTeam)
            {
                string npcName = _gameLanguageService.GetNpcMonsterName(npc, blue);

                if (playerTeam == RainbowBattleTeamType.Blue)
                {
                    blue.SendMsg(blue.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_MESSAGE_ALLY_CAPTURED, session.PlayerEntity.Name, npcName), MsgMessageType.Middle);
                    blue.SendChatMessage(blue.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_MESSAGE_ALLY_CAPTURED, session.PlayerEntity.Name, npcName), ChatMessageColorType.Green);
                }
                else
                {
                    blue.SendMsg(blue.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_MESSAGE_ENEMY_CAPTURED, npcName), MsgMessageType.Middle);
                    blue.SendChatMessage(blue.GetLanguageFormat(GameDialogKey.RAINBOW_BATTLE_MESSAGE_ENEMY_CAPTURED, npcName), ChatMessageColorType.Red);
                }
            }

            session.PlayerEntity.RainbowBattleComponent.ActivityPoints += _rainbowBattleConfiguration.CaptureActivityPoints;

            await session.EmitEventAsync(new RainbowBattleRefreshScoreEvent
            {
                RainbowBattleParty = rainbowBattleParty
            });
        }
    }
}