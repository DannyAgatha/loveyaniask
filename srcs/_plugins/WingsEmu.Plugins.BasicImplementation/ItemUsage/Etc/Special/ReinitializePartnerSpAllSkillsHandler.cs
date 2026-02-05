// NosEmu
// 


using System;
using System.Collections.Generic;
using System.Linq;
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

public class ReinitializePartnerSpAllSkillsHandler : IItemUsageByVnumHandler
{
    private readonly IDelayManager _delayManager;
    private readonly IGameLanguageService _gameLanguage;
    private readonly ISessionManager _sessionManager;

    public ReinitializePartnerSpAllSkillsHandler(IGameLanguageService languageService, ISessionManager sessionManager, IDelayManager delayManager)
    {
        _gameLanguage = languageService;
        _sessionManager = sessionManager;
        _delayManager = delayManager;
    }

    public long[] Vnums => [(long)ItemVnums.PARTNER_SKILL_TICKET_ALL, (long)ItemVnums.PARTNER_SKILL_TICKET_ALL_LIMITED];

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        byte option = e.Option;
        InventoryItem inv = e.Item;
        IMateEntity partner = session.PlayerEntity.MateComponent.GetMate(s => s.IsTeamMember && s.MateType == MateType.Partner);
        string[] packetsplit = e.Packet;

        if (packetsplit == null)
        {
            _sessionManager.BroadcastToGameMaster(session, "ReinitializePartnerSpAllSkills - packetsplit == null");
            // Packet Hacking
            return;
        }

        if (packetsplit.Length < 9)
        {
            _sessionManager.BroadcastToGameMaster(session, "ReinitializePartnerSpAllSkills - packetsplit.Length < 9");
            // Packet hacking
            return;
        }

        if (!byte.TryParse(packetsplit[9], out byte skillSlot))
        {
            // out of range
            return;
        }

        if (!Enum.TryParse(packetsplit[8], out EquipmentType eqpType))
        {
            // Out of range
            return;
        }

        if (!byte.TryParse(packetsplit[6], out byte request))
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

        long itemVNum = inv.ItemInstance.ItemVNum;
        switch (itemVNum)
        {
            case var _ when itemVNum == Vnums[0]:
            case var _ when itemVNum == Vnums[1]:
                if (request == 3)
                {
                    bool canReset = await _delayManager.CanPerformAction(partner, DelayedActionType.PartnerResetAllSkills);
                    if (!canReset)
                    {
                        return;
                    }

                    partner.Specialist.PartnerSkill1 = false;
                    partner.Specialist.PartnerSkill2 = false;
                    partner.Specialist.PartnerSkill3 = false;
                    partner.Specialist.SkillRank1 = 0;
                    partner.Specialist.SkillRank2 = 0;
                    partner.Specialist.SkillRank3 = 0;
                    partner.Specialist.Agility = 100;
                    partner.Specialist.PartnerSkills.Clear();
                    session.SendModal(_gameLanguage.GetLanguage(GameDialogKey.PSP_MESSAGE_ALL_SKILLS_RESET, session.UserLanguage), ModalType.Confirm);
                    session.SendChatMessage(_gameLanguage.GetLanguage(GameDialogKey.PSP_MESSAGE_ALL_SKILLS_RESET, session.UserLanguage), ChatMessageColorType.Yellow);
                    session.SendPetInfo(partner, _gameLanguage);
                    await session.RemoveItemFromInventory(inv.ItemInstance.ItemVNum);
                    return;
                }

                switch (option)
                {
                    case 0:
                        session.SendQnaPacket($"u_i 1 {session.PlayerEntity.Id} {(short)inv.ItemInstance.GameItem.Type} {inv.Slot} 1 1 {(short)eqpType} {skillSlot}",
                            _gameLanguage.GetLanguage(GameDialogKey.PSP_DIALOG_ASK_RESET_ALL_SKILLS, session.UserLanguage));
                        return;
                    case 255:
                    {
                        DateTime waitUntil = await _delayManager.RegisterAction(partner, DelayedActionType.PartnerResetAllSkills);
                        session.SendMateDelay(partner, (int)(waitUntil - DateTime.UtcNow).TotalMilliseconds, GuriType.UsingItem,
                            $"#u_i^1^{session.PlayerEntity.Id}^{(short)inv.ItemInstance.GameItem.Type}^{inv.Slot}^3^1^{(short)eqpType}^{skillSlot}");
                        session.CurrentMapInstance?.Broadcast(partner.GenerateMateDance(), new RangeBroadcast(partner.PositionX, partner.PositionY));
                        break;
                    }
                }

                break;
        }
    }
}