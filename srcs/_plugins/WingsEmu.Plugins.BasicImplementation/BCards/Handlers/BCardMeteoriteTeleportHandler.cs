// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using PhoenixLib.Events;
using WingsAPI.Communication.Sessions.Model;
using WingsEmu.DTOs.BCards;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardMeteoriteTeleportHandler : IBCardEffectAsyncHandler
{
    private readonly IAsyncEventPipeline _asyncEventPipeline;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ITeleportManager _teleportManager;
    private readonly ISkillsManager _skillsManager;
    private readonly IBuffFactory _buffFactory;

    public BCardMeteoriteTeleportHandler(ITeleportManager teleportManager, IRandomGenerator randomGenerator, IAsyncEventPipeline asyncEventPipeline, ISkillsManager skillsManager, IBuffFactory buffFactory)
    {
        _teleportManager = teleportManager;
        _randomGenerator = randomGenerator;
        _asyncEventPipeline = asyncEventPipeline;
        _skillsManager = skillsManager;
        _buffFactory = buffFactory;
    }

    public BCardType HandledType => BCardType.MeteoriteTeleport;

    public void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        IBattleEntity target = ctx.Target;
        int firstData = ctx.BCard.FirstData;
        BCardDTO bCard = ctx.BCard;


        switch (ctx.BCard.SubType)
        {
            case (byte)AdditionalTypes.MeteoriteTeleport.CauseMeteoriteFall:
                if (sender is not IPlayerEntity senderCharacter)
                {
                    return;
                }

                SkillDTO skinfo = _skillsManager.GetSkill((short)SkillsVnums.METEOR_STORM);
                int firstDataValue = bCard.FirstDataValue(sender.Level);
                int meteoritesAmount = firstDataValue + 10;
                var meteorites = new List<ToSummon>();
                Position positionNonTarget = ctx.Position;

                for (int i = 0; i < meteoritesAmount; i++)
                {
                    int x = positionNonTarget.X + _randomGenerator.RandomNumber(-skinfo.AoERange, skinfo.AoERange);
                    int y = positionNonTarget.Y + _randomGenerator.RandomNumber(-skinfo.AoERange, skinfo.AoERange);

                    if (senderCharacter.MapInstance.IsBlockedZone(x, y))
                    {
                        x = positionNonTarget.X;
                        y = positionNonTarget.Y;
                    }

                    int vnum = _randomGenerator.RandomNumber((short)MonsterVnum.FIRST_METEORITE, (short)MonsterVnum.SECOND_METEORITE + 1);
                    var toSummon = new ToSummon
                    {
                        VNum = (short)vnum,
                        SpawnCell = new Position((short)x, (short)y),
                        IsMoving = true,
                        IsHostile = true,
                        IgnoreSkillRange = true
                    };

                    meteorites.Add(toSummon);
                }

                senderCharacter.SkillComponent.ArchMageMeteorites.AddRange(meteorites);

                break;
            case (byte)AdditionalTypes.MeteoriteTeleport.TeleportYouAndGroupToSavedLocation:
                
                if (sender is not IPlayerEntity character)
                {
                    return;
                }

                IFamily family = character.Family;

                if (!character.HasBuff(BuffVnums.MEMORIAL))
                {
                    _teleportManager.RemovePosition(character.Id);
                    return;
                }

                Position position = _teleportManager.GetPosition(character.Id);
                if (position.X == 0 && position.Y == 0)
                {
                    _teleportManager.SavePosition(character.Id, character.Position);
                    character.BroadcastEffectGround(EffectType.ArchmageTeleportSet, character.PositionX, character.PositionY, false);
                    character.SetSkillCooldown(ctx.Skill);
                    character.RemoveCastingSkill();
                    return;
                }

                short savedX = _teleportManager.GetPosition(character.Id).X;
                short savedY = _teleportManager.GetPosition(character.Id).Y;

                IEnumerable<IBattleEntity> allies = sender.GetAlliesInRange(target, ctx.Skill.AoERange);
                int counter = 0;
                foreach (IBattleEntity entity in allies)
                {
                    if (counter == firstData)
                    {
                        break;
                    }

                    if (entity.IsNpc() && !entity.IsMate())
                    {
                        continue;
                    }

                    if (entity.IsMate())
                    {
                        var mateEntity = (IMateEntity)entity;
                        IPlayerEntity mateOwner = mateEntity.Owner;
                        if (character.Id == mateOwner?.Id)
                        {
                            continue;
                        }

                        if (mateOwner == null)
                        {
                            continue;
                        }

                        PlayerGroup mateOwnerGroup = mateOwner.GetGroup();
                        PlayerGroup sessionGroup = character.GetGroup();

                        bool isInGroupMate = mateOwnerGroup?.GroupId == sessionGroup?.GroupId;
                        if (!isInGroupMate)
                        {
                            if (mateOwner.Family?.Id != family?.Id)
                            {
                                continue;
                            }
                        }

                        mateEntity.BroadcastEffectGround(EffectType.ArchmageTeleport, mateEntity.PositionX, mateEntity.PositionY, false);
                        mateEntity.MapInstance.Broadcast(mateEntity.GenerateEffectPacket(EffectType.ArchmageTeleportAfter));
                        mateEntity.TeleportOnMap(savedX, savedY);
                        counter++;
                        continue;
                    }

                    var anotherCharacter = (IPlayerEntity)entity;
                    if (character.Id == anotherCharacter.Id)
                    {
                        continue;
                    }

                    if (!anotherCharacter.IsInGroup() && !anotherCharacter.IsInFamily())
                    {
                        continue;
                    }

                    PlayerGroup anotherCharacterGroup = anotherCharacter.GetGroup();
                    PlayerGroup characterGroup = character.GetGroup();

                    bool isInGroup = anotherCharacterGroup?.GroupId == characterGroup?.GroupId;
                    if (!isInGroup)
                    {
                        if (anotherCharacter.Family?.Id != family?.Id)
                        {
                            continue;
                        }
                    }

                    anotherCharacter.BroadcastEffectGround(EffectType.ArchmageTeleport, anotherCharacter.PositionX, anotherCharacter.PositionY, false);
                    anotherCharacter.MapInstance.Broadcast(anotherCharacter.GenerateEffectPacket(EffectType.ArchmageTeleportAfter));
                    anotherCharacter.TeleportOnMap(savedX, savedY);
                    counter++;
                }

                character.BroadcastEffectGround(EffectType.ArchmageTeleportWhiteEffect, character.PositionX, character.PositionY, false);
                character.TeleportOnMap(savedX, savedY, true);
                _teleportManager.RemovePosition(character.Id);
                character.BroadcastEffectGround(EffectType.ArchmageTeleportSet, savedX, savedY, true);

                SkillInfo fakeTeleport = character.GetFakeTeleportSkill();

                Buff memorialBuff = character.BuffComponent.GetBuff((short)BuffVnums.MEMORIAL);
                character.RemoveBuffAsync(false, memorialBuff);
                character.SetSkillCooldown(fakeTeleport);
                character.RemoveCastingSkill();
                character.SkillComponent.SendTeleportPacket = DateTime.UtcNow;
                break;
            case (byte)AdditionalTypes.MeteoriteTeleport.SummonInVisualRange:
                break;
            case (byte)AdditionalTypes.MeteoriteTeleport.TransformTarget:
                break;
        }
    }
}