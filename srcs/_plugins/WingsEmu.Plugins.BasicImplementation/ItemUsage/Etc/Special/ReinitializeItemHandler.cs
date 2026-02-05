using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.PacketGeneration;
using WingsAPI.Packets.Enums;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Networking.Broadcasting;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Etc.Special;

public class ReinitializeItemHandler : IItemUsageByVnumHandler
{
    private readonly IDelayManager _delayManager;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISessionManager _sessionManager;

    public ReinitializeItemHandler(IGameLanguageService gameLanguage, ISessionManager sessionManager, IDelayManager delayManager)
    {
        _gameLanguage = gameLanguage;
        _sessionManager = sessionManager;
        _delayManager = delayManager;
    }

    public long[] Effects => [11111];

    public long[] Vnums => [(long)ItemVnums.PARTNER_SKILL_TICKET_SINGLE, (long)ItemVnums.PARTNER_SKILL_UPGRADE_TICKET, (long)ItemVnums.PARTNER_SKILL_TICKET_SINGLE_LIMITED];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        byte option = e.Option;
        IMateEntity partner = session.PlayerEntity.MateComponent.GetMate(s => s.IsTeamMember && s.MateType == MateType.Partner);
        string[] packetsplit = e.Packet;
        InventoryItem inv = e.Item;

        if (packetsplit == null)
        {
            _sessionManager.BroadcastToGameMaster(session, "ReinitializeItemHandler - packetsplit == null");
            // Packet Hacking
            return;
        }

        if (packetsplit.Length < 9)
        {
            _sessionManager.BroadcastToGameMaster(session, "ReinitializeItemHandler - packetsplit.Length < 9");
            // Packet hacking
            return;
        }

        if (!byte.TryParse(packetsplit[9], out byte skillSlot))
        {
            // out of range
            return;
        }

        if (!Enum.TryParse(packetsplit[8], out EquipmentType sEqpType))
        {
            // Out of range
            return;
        }

        if (!byte.TryParse(packetsplit[6], out byte sRequest))
        {
            return;
        }

        if (partner == null)
        {
            session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_INFO_NO_PARTNER_IN_TEAM, session.UserLanguage), ModalType.Confirm);
            return;
        }

        if (partner.Specialist == null)
        {
            session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_MESSAGE_NO_SP_EQUIPPED, session.UserLanguage), ModalType.Confirm);
            return;
        }

        if (partner.Specialist.PartnerSkills == null)
        {
            return;
        }

        if (partner.IsUsingSp)
        {
            session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PARTNER_INFO_IS_WEARING_SP, session.UserLanguage), ModalType.Confirm);
            return;
        }

        if (!partner.HavePartnerSkill(skillSlot))
        {
            return;
        }
        
        List<PartnerSkill> partnerSkills = partner.Specialist.PartnerSkills;
        PartnerSkill skill = partnerSkills.Find(s => s.Slot == skillSlot);
        
        if (skill == null)
        {
            return;
        }

        long itemVNum = inv.ItemInstance.ItemVNum;
        
        switch (itemVNum)
        {
            case var _ when itemVNum == Vnums[0] || itemVNum == Vnums[2]:

                if (skill.Rank == 7)
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.PSP_MESSAGE_ENHANCEMENT_NOT_POSSIBLE, session.UserLanguage));
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PSP_MESSAGE_ENHANCEMENT_NOT_POSSIBLE, session.UserLanguage), ChatMessageColorType.Red);
                    return;
                }

                if (sRequest == 3)
                {
                    bool canReset = await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.PartnerResetSkill);
                    if (!canReset)
                    {
                        return;
                    }

                    await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.PartnerResetSkill);

                    switch (skillSlot)
                    {
                        case 0:
                            partner.Specialist.SkillRank1 += 1;
                            break;
                        case 1:
                            partner.Specialist.SkillRank2 += 1;
                            break;
                        case 2:
                            partner.Specialist.SkillRank3 += 1;
                            break;
                        default:
                            _sessionManager.BroadcastToGameMaster(session, "ReinitializeItemHandler - switch (sPos) default");
                            // Packet Hacking
                            return;
                    }

                    skill.Rank += 1;
                    session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PSP_MESSAGE_ONE_SKILL_ENHANCEMENT, session.UserLanguage), ModalType.Confirm);
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PSP_MESSAGE_ONE_SKILL_ENHANCEMENT, session.UserLanguage), ChatMessageColorType.Yellow);
                    session.SendPetInfo(partner, _gameLanguage);
                    await session.RemoveItemFromInventory(inv.ItemInstance.ItemVNum);
                    return;
                }

                switch (option)
                {
                    case 0:
                        session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(short)inv.ItemInstance.GameItem.Type} {inv.Slot} 1 1 {(short)sEqpType} {skillSlot}",
                            _gameLanguage.GetLanguage(GameDialogKey.PSP_DIALOG_ASK_ENHANCEMENT_SINGLE_SKILL, session.UserLanguage));
                        break;
                    case 255:
                    {
                        DateTime waitUntil = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.PartnerResetSkill);
                        session.SendMateDelay(partner, (int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.UsingItem,
                            $"#u_i^1^{session.PlayerEntity.Id}^{(short)inv.ItemInstance.GameItem.Type}^{inv.Slot}^3^1^{(short)sEqpType}^{skillSlot}");
                        session.CurrentMapInstance?.Broadcast(partner.GenerateMateDance(), new RangeBroadcast(partner.PositionX, partner.PositionY));
                        break;
                    }
                }

                break;
            
            case var _ when itemVNum == Vnums[1]:

                if (skill.Rank == 7)
                {
                    session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.PSP_MESSAGE_ENHANCEMENT_NOT_POSSIBLE, session.UserLanguage));
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PSP_MESSAGE_ENHANCEMENT_NOT_POSSIBLE, session.UserLanguage), ChatMessageColorType.Red);
                    return;
                }

                if (sRequest == 3)
                {
                    bool canReset = await _delayManager.CanPerformAction(session.PlayerEntity, DelayedActionType.PartnerResetSkill);
                    if (!canReset)
                    {
                        return;
                    }

                    await _delayManager.CompleteAction(session.PlayerEntity, DelayedActionType.PartnerResetSkill);

                    switch (skillSlot)
                    {
                        case 0:
                            partner.Specialist.SkillRank1 = 7;
                            break;
                        case 1:
                            partner.Specialist.SkillRank2 = 7;
                            break;
                        case 2:
                            partner.Specialist.SkillRank3 = 7;
                            break;
                        default:
                            _sessionManager.BroadcastToGameMaster(session, "ReinitializeItemHandler - switch (sPos) default");
                            // Packet Hacking
                            return;
                    }

                    skill.Rank = 7;
                    session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PSP_MESSAGE_ONE_SKILL_ENHANCEMENT, session.UserLanguage), ModalType.Confirm);
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PSP_MESSAGE_ONE_SKILL_ENHANCEMENT, session.UserLanguage), ChatMessageColorType.Yellow);
                    session.SendPetInfo(partner, _gameLanguage);
                    await session.RemoveItemFromInventory(inv.ItemInstance.ItemVNum);
                    return;
                }

                switch (option)
                {
                    case 0:
                        session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(short)inv.ItemInstance.GameItem.Type} {inv.Slot} 1 1 {(short)sEqpType} {skillSlot}",
                            _gameLanguage.GetLanguage(GameDialogKey.PSP_DIALOG_ASK_ENHANCEMENT_MAX_SKILL, session.UserLanguage));
                        break;
                    case 255:
                    {
                        DateTime waitUntil = await _delayManager.RegisterAction(session.PlayerEntity, DelayedActionType.PartnerResetSkill);
                        session.SendMateDelay(partner, (int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.UsingItem,
                            $"#u_i^1^{session.PlayerEntity.Id}^{(short)inv.ItemInstance.GameItem.Type}^{inv.Slot}^3^1^{(short)sEqpType}^{skillSlot}");
                        session.CurrentMapInstance?.Broadcast(partner.GenerateMateDance(), new RangeBroadcast(partner.PositionX, partner.PositionY));
                        break;
                    }
                }

                break;
        }
    }
}