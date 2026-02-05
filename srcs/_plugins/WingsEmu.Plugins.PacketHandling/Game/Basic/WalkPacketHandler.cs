using System;
using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Game.Extensions.AccountExtensions;
using WingsAPI.Game.Extensions.CharacterExtensions;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.PacketHandling.Game.Basic;

public class WalkPacketHandler : GenericGamePacketHandlerBase<WalkPacket>
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMeditationManager _meditationManager;
    private readonly ISacrificeManager _sacrificeManager;
    private readonly ISessionManager _sessionManager;
    private readonly IAccountService _accountService;

    public WalkPacketHandler(ISacrificeManager sacrificeManager, IGameLanguageService gameLanguage, IMeditationManager meditationManager, ISessionManager sessionManager, IAccountService accountService)
    {
        _sacrificeManager = sacrificeManager;
        _gameLanguage = gameLanguage;
        _meditationManager = meditationManager;
        _sessionManager = sessionManager;
        _accountService = accountService;
    }

    protected override async Task HandlePacketAsync(IClientSession session, WalkPacket walkPacket)
    {
        DateTime actualTime = DateTime.UtcNow;

        if (!session.HasCurrentMapInstance)
        {
            return;
        }

        if (walkPacket.Speed != session.PlayerEntity.Speed)
        {
            session.SendCondPacket();
        }

        if (session.CurrentMapInstance.IsBlockedZone(walkPacket.XCoordinate, walkPacket.YCoordinate) || session.PlayerEntity.HasShopOpened || session.PlayerEntity.IsInExchange())
        {
            BeforeReturnLogic(session);
            return;
        }

        if (!session.PlayerEntity.CanPerformMove())
        {
            session.RefreshStat();
        }

        (bool IsValid, Position Position) calculatedPosition = GetExpectedClientSidePosition(session, actualTime);
        Position expectedClientsidePosition = calculatedPosition.IsValid ? calculatedPosition.Position : session.PlayerEntity.Position;

        double distance = expectedClientsidePosition.GetDoubleDistance(walkPacket.XCoordinate, walkPacket.YCoordinate);
        int speed = session.PlayerEntity.Speed < 1 ? 1 : session.PlayerEntity.Speed;

        if (session.PlayerEntity.Morph == (int)MorphType.FlameDruidLeopardStance)
        {
            speed += 2;
        }

        if (speed > 40 && session.PlayerEntity.Authority < AuthorityType.GS)
        {
            await LogSpeedHackDetection(session, speed);
            BeforeReturnLogic(session);
            return;
        }

        double expectedMaximumDistance = speed * 0.4 + 3 + 2;
        double waitingtime = distance / speed * 2.5;
        if (distance > expectedMaximumDistance + (session.PlayerEntity.IsOnVehicle ? 3 : 0))
        {
            BeforeReturnLogic(session);
            return;
        }

        if (session.PlayerEntity.BuffComponent.HasBuff((int)BuffVnums.FISH_LINE))
        {
            await session.EmitEventAsync(new IncrementFishInteractionEvent());
            await session.PlayerEntity.RemoveBuffAsync((int)BuffVnums.FISH_LINE);
            session.SendSayi(ChatMessageColorType.White, Game18NConstString.FishingTimeIsUp);
        }

        await session.EmitEventAsync(new StopCookingMealEvent(true));
        session.PlayerEntity.RemoveMeditation(_meditationManager);

        session.PlayerEntity.LastWalk = new LastWalk
        {
            MapId = session.PlayerEntity.MapId,
            StartPosition = expectedClientsidePosition,
            EndPosition = new Position(walkPacket.XCoordinate, walkPacket.YCoordinate),
            WalkTimeStart = actualTime,
            WalkTimeEnd = actualTime.AddSeconds(waitingtime)
        };

        if (session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_BASE_MAP) && session.PlayerEntity.MapInstance.MapInstanceType != MapInstanceType.TimeSpaceInstance)
        {
            session.PlayerEntity.MapX = walkPacket.XCoordinate;
            session.PlayerEntity.MapY = walkPacket.YCoordinate;
        }

        session.PlayerEntity.ChangePosition(new Position(walkPacket.XCoordinate, walkPacket.YCoordinate));

        await CheckNobleGesture(session);
        await CheckSpiritOfSacrifice(session);

        if (!session.PlayerEntity.CheatComponent.IsInvisible)
        {
            session.BroadcastMovement(session.PlayerEntity, new ExceptSessionBroadcast(session));
        }

        switch (session.CurrentMapInstance.MapId)
        {
            case 1:
                {
                    foreach (IClientSession ses in _sessionManager.Sessions
                                 .Where(s => s?.CurrentMapInstance is { MapId: 228 }))
                    {
                        ses.SendPacket(session.PlayerEntity.GenerateMovement());
                    }

                    break;
                }
            case 228:
                {
                    foreach (IClientSession ses in _sessionManager.Sessions
                                 .Where(s => s?.CurrentMapInstance is { MapId: 1 }))
                    {
                        ses.SendPacket(session.PlayerEntity.GenerateMovement());
                    }

                    break;
                }
        }

        if (session.PlayerEntity.SkillComponent.PyjamaFakeDeadActivated)
        {
            session.PlayerEntity.SkillComponent.PyjamaFakeDeadActivated = false;
        }

        session.SendCondPacket();
        session.PlayerEntity.LastMove = actualTime;
    }
    private async Task LogSpeedHackDetection(IClientSession session, int detectedSpeed)
    {
        string logMessage =
            $"Player: {session.PlayerEntity.Name} (Id: {session.PlayerEntity.Id}), " +
            $"Speed: {detectedSpeed}, Map: {session.PlayerEntity.MapId}, " +
            $"Pos: ({session.PlayerEntity.PositionX}, {session.PlayerEntity.PositionY})";
        
        await session.HandleInvalidBehavior(_accountService, $"Speed Hack Detected: {logMessage}");
    }

    private (bool IsValid, Position Position) GetExpectedClientSidePosition(IClientSession session, DateTime actualTime)
    {
        if (session.PlayerEntity.LastSitting.AddSeconds(1) < actualTime || session.PlayerEntity.LastWalk.WalkTimeEnd < actualTime || session.PlayerEntity.LastWalk.MapId != session.PlayerEntity.MapId
            || session.PlayerEntity.PositionX != session.PlayerEntity.LastWalk.EndPosition.X || session.PlayerEntity.PositionY != session.PlayerEntity.LastWalk.EndPosition.Y)
        {
            return (false, default);
        }

        double expectedPositionRatio = (actualTime - session.PlayerEntity.LastWalk.WalkTimeStart) / (session.PlayerEntity.LastWalk.WalkTimeEnd - session.PlayerEntity.LastWalk.WalkTimeStart);

        short x = GetExpectedClientSideCoordinate(session.PlayerEntity.LastWalk.StartPosition.X, session.PlayerEntity.LastWalk.EndPosition.X, expectedPositionRatio);
        short y = GetExpectedClientSideCoordinate(session.PlayerEntity.LastWalk.StartPosition.Y, session.PlayerEntity.LastWalk.EndPosition.Y, expectedPositionRatio);

        return (true, new Position(x, y));
    }

    private short GetExpectedClientSideCoordinate(short xStart, short xEnd, double ratio) => Convert.ToInt16((xEnd - xStart) * ratio + xStart);


    private void BeforeReturnLogic(IClientSession session)
    {
        session.SendCondPacket();
        session.BroadcastTeleportPacket();
    }

    private async Task CheckSpiritOfSacrifice(IClientSession session)
    {
        if (!session.PlayerEntity.HasBuff(BuffVnums.SPIRIT_OF_SACRIFICE))
        {
            return;
        }

        IBattleEntity target = _sacrificeManager.GetTarget(session.PlayerEntity);
        if (target == null || target.GetDistance(session.PlayerEntity) <= 14)
        {
            return;
        }

        // WARNING
        if (session.PlayerEntity.GetDistance(target) == 15)
        {
            string message;
            if (target is IPlayerEntity character)
            {
                message = _gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_SACRIFICE_WARNING, character.Session.UserLanguage);
                character.Session.SendMsg(message, MsgMessageType.SmallMiddle);
            }

            message = _gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_SACRIFICE_WARNING, session.UserLanguage);
            session.SendMsg(message, MsgMessageType.SmallMiddle);
        }
        else
        {
            await session.PlayerEntity.RemoveSacrifice(target, _sacrificeManager, _gameLanguage);
        }
    }

    private async Task CheckNobleGesture(IClientSession session)
    {
        if (!session.PlayerEntity.HasBuff(BuffVnums.NOBLE_GESTURE))
        {
            return;
        }

        IBattleEntity caster = _sacrificeManager.GetCaster(session.PlayerEntity);
        if (caster == null || caster.GetDistance(session.PlayerEntity) <= 14)
        {
            return;
        }

        // WARNING
        if (caster.GetDistance(session.PlayerEntity) == 15)
        {
            string message;
            if (caster is IPlayerEntity character)
            {
                message = _gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_SACRIFICE_WARNING, character.Session.UserLanguage);
                character.Session.SendMsg(message, MsgMessageType.SmallMiddle);
            }

            message = _gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_SACRIFICE_WARNING, session.UserLanguage);
            session.SendMsg(message, MsgMessageType.SmallMiddle);
        }
        else
        {
            await caster.RemoveSacrifice(session.PlayerEntity, _sacrificeManager, _gameLanguage);
        }
    }
}