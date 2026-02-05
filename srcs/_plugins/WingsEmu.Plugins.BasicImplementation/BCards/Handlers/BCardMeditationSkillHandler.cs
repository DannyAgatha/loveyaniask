// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
using WingsAPI.Game.Extensions.Quicklist;
using WingsEmu.DTOs.Quicklist;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.BCards.Handlers;

public class BCardMeditationSkillHandler : IBCardEffectAsyncHandler
{
    private readonly IBuffFactory _buffFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMeditationManager _meditationManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISacrificeManager _sacrificeManager;

    public BCardMeditationSkillHandler(IRandomGenerator randomGenerator, IBuffFactory buffFactory, IGameLanguageService gameLanguage, ISacrificeManager sacrificeManager,
        IMeditationManager meditationManager)
    {
        _randomGenerator = randomGenerator;
        _buffFactory = buffFactory;
        _gameLanguage = gameLanguage;
        _sacrificeManager = sacrificeManager;
        _meditationManager = meditationManager;
    }

    public BCardType HandledType => BCardType.MeditationSkill;

    public async void Execute(IBCardEffectContext ctx)
    {
        IBattleEntity sender = ctx.Sender;
        byte subType = ctx.BCard.SubType;
        int firstData = ctx.BCard.FirstData;
        if (sender is not IPlayerEntity character)
        {
            return;
        }

        IClientSession session = character.Session;

        if (subType != (byte)AdditionalTypes.MeditationSkill.CausingChance)
        {
            switch (subType)
            {
                case (byte)AdditionalTypes.MeditationSkill.ShortMeditation:
                    _meditationManager.SaveMeditation(session.PlayerEntity, (short)ctx.BCard.SecondData, DateTime.UtcNow.AddSeconds(4));
                    break;
                case (byte)AdditionalTypes.MeditationSkill.RegularMeditation:
                    _meditationManager.SaveMeditation(session.PlayerEntity, (short)ctx.BCard.SecondData, DateTime.UtcNow.AddSeconds(8));
                    break;
                case (byte)AdditionalTypes.MeditationSkill.LongMeditation:
                    _meditationManager.SaveMeditation(session.PlayerEntity, (short)ctx.BCard.SecondData, DateTime.UtcNow.AddSeconds(12));
                    break;
                case (byte)AdditionalTypes.MeditationSkill.Sacrifice:
                    // check if target has sacrifice
                    IBattleEntity caster = _sacrificeManager.GetCaster(ctx.Target);
                    IBattleEntity target = null;
                    if (caster != null)
                    {
                        target = _sacrificeManager.GetTarget(caster);
                    }

                    if (caster != null && target != null)
                    {
                        await caster.RemoveSacrifice(target, _sacrificeManager, _gameLanguage);
                        Buff targetBuff = _buffFactory.CreateBuff((short)BuffVnums.NOBLE_GESTURE, sender);
                        await ctx.Target.AddBuffAsync(targetBuff);
                    }

                    Buff buff = _buffFactory.CreateBuff(ctx.BCard.SecondData, sender);
                    await sender.AddBuffAsync(buff);
                    if (target is IPlayerEntity characterTarget)
                    {
                        characterTarget.Session.SendMsg(
                            _gameLanguage.GetLanguageFormat(GameDialogKey.SKILL_SHOUTMESSAGE_SACRIFICE, characterTarget.Session.UserLanguage, session.PlayerEntity.Name, characterTarget.Name),
                            MsgMessageType.SmallMiddle);
                        session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.SKILL_SHOUTMESSAGE_SACRIFICE, session.UserLanguage, session.PlayerEntity.Name, characterTarget.Name),
                            MsgMessageType.SmallMiddle);
                    }

                    _sacrificeManager.SaveSacrifice(character, character.MapInstance.GetBattleEntity(character.LastEntity.Item1, character.LastEntity.Item2));
                    break;
            }

            return;
        }

        if (_randomGenerator.RandomNumber() > firstData)
        {
            return;
        }

        if (!ctx.BCard.SkillVNum.HasValue)
        {
            return;
        }

        if (!character.UseSp)
        {
            return;
        }

        if (character.Specialist == null)
        {
            return;
        }

        if (character.Specialist.SpLevel < 20)
        {
            return;
        }

        SkillInfo skill = ctx.Skill;

        ComboSkillState comboSkillState = session.PlayerEntity.GetComboState();
        if (comboSkillState == null)
        {
            return;
        }

        SkillDTO newSkill = character.GetSkills().FirstOrDefault(x => x.Skill.Id == ctx.BCard.SecondData)?.Skill;
        if (newSkill == null)
        {
            return;
        }

        int usedSkillVnum = skill.Vnum;
        ComboSkillState combo = character.GetComboState();

        // Check if current skill is combo and find the original one
        if (combo != null)
        {
            usedSkillVnum = character.GetSkills().FirstOrDefault(x =>
                x.Skill.SkillType == SkillType.NormalPlayerSkill && x.Skill.CastId == combo.OriginalSkillCastId && x.Skill.Id > 200)?.Skill.Id ?? skill.Vnum;
        }

        int characterMorph = character.Specialist != null && character.UseSp ? character.Specialist.GameItem.Morph : 0;

        List<CharacterQuicklistEntryDto> quick = character.QuicklistComponent.GetQuicklist();
        IEnumerable<CharacterQuicklistEntryDto> skillShot = quick.Where(s => s.SkillVnum.HasValue && s.SkillVnum.Value == usedSkillVnum && s.Morph == characterMorph && s.Type == QuicklistType.SKILLS);
        character.Session.SendMSlotPacket(newSkill.Id);
        foreach (CharacterQuicklistEntryDto quickListEntry in skillShot)
        {
            character.Session.SendQuicklistSlot(quickListEntry, newSkill.CastId);
        }

        session.PlayerEntity.IncreaseComboState((byte)newSkill.CastId);
        character.LastSkillCombo = DateTime.UtcNow;
    }
}