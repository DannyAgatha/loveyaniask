using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsAPI.Game.Extensions.Quicklist;
using WingsAPI.Packets.Enums;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act7.Tattoos;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.Act7.Tattoos;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Act7.Tattoos;

public class UpgradeTattooEventHandler : IAsyncEventProcessor<UpgradeTattooEvent>
{
    private readonly TattooUpgradeConfiguration _tattooUpgradeConfiguration;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IEvtbConfiguration _evtbConfiguration;
    public UpgradeTattooEventHandler(TattooUpgradeConfiguration tattooUpgradeConfiguration, IRandomGenerator randomGenerator, IEvtbConfiguration evtbConfiguration)
    {
        _tattooUpgradeConfiguration = tattooUpgradeConfiguration;
        _randomGenerator = randomGenerator;
        _evtbConfiguration = evtbConfiguration;
    }
    
    public async Task HandleAsync(UpgradeTattooEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        TattooUpgradeProtection protectionType = e.UpgradeProtection;
        bool isProtected = false;

        if (!session.PlayerEntity.CharacterSkills.TryGetValue(e.TattooSkill.Skill.Id, out CharacterSkill skill))
        {
            return;
        }

        TattooUpgrade upgradeInfo = _tattooUpgradeConfiguration.TattooUpgrade.FirstOrDefault(s => s.Upgrade == (skill.UpgradeSkill + 1));

        if (upgradeInfo == null)
        {
            // Packet logger
            return;
        }
        
        if (!session.PlayerEntity.SkillCanBeUsed(skill))
        {
            session.SendShopEndPacket(ShopEndType.Item);
            session.SendShopEndPacket(ShopEndType.Npc);
            session.SendShopEndPacket(ShopEndType.Player);
            return;
        }
        
        if (upgradeInfo.Items.Any(item => !session.PlayerEntity.HasItem(item.Vnum, item.Quantity)))
        {
            return;
        }
        
        if (!session.PlayerEntity.RemoveGold(upgradeInfo.Gold))
        {
            session.SendChatMessage(session.GetLanguage(GameDialogKey.INTERACTION_MESSAGE_NOT_ENOUGH_GOLD), ChatMessageColorType.PlayerSay);
            return;
        }
        
        if (protectionType != TattooUpgradeProtection.NONE)
        {
            isProtected = true;
        }

        if (isProtected)
        {
            int requiredItemVnum = session.PlayerEntity.HasItem((short)ItemVnums.TATTOO_SAFEGUARD_SCROLL_LIMITED) ?
                (short)ItemVnums.TATTOO_SAFEGUARD_SCROLL_LIMITED : (short)ItemVnums.TATTOO_SAFEGUARD_SCROLL;
            
            if (!session.PlayerEntity.HasItem(requiredItemVnum))
            {
                session.SendSayi(ChatMessageColorType.Yellow, Game18NConstString.NoTattooSafeScroll);
                return;
            }

            await session.RemoveItemFromInventory(requiredItemVnum);
        }
        
        await TattooUpgrade(session, skill, protectionType, isProtected);

        IEnumerable<IBattleEntitySkill> filteredSkills = session.PlayerEntity.Skills.Where(s => s.LastUse >= DateTime.UtcNow);
        foreach (IBattleEntitySkill battleEntitySkill in filteredSkills)
        {
            int seconds = (battleEntitySkill.LastUse - DateTime.UtcNow).Milliseconds;
            session.SendSkillCooldownResetAfter(battleEntitySkill.Skill.CastId, battleEntitySkill.Skill.Cooldown, seconds);
        }
    }
    
    private async Task TattooUpgrade(IClientSession session, CharacterSkill skill, TattooUpgradeProtection protectionType,
        bool isProtected)
    {
        TattooUpgrade upgradeInfo = _tattooUpgradeConfiguration.TattooUpgrade.FirstOrDefault(s => s.Upgrade == (skill.UpgradeSkill + 1));

        if (upgradeInfo == null)
        {
            return;
        }

        if (protectionType != TattooUpgradeProtection.NONE)
        {
            isProtected = true;
        }
        
        var randomBag = new RandomBag<TattooUpgradeResult>(_randomGenerator);
        
        randomBag.AddEntry(TattooUpgradeResult.Succeed, upgradeInfo.Success * (1 + _evtbConfiguration.GetValueForEventType(EvtbType.INCREASE_CHANCE_UPGRADE_TATTOOS) * 0.01));

        randomBag.AddEntry(TattooUpgradeResult.Fail, 100 - upgradeInfo.Success - upgradeInfo.MajorFail);

        if (upgradeInfo.MajorFail > 0)
        {
            randomBag.AddEntry(TattooUpgradeResult.MajorFailure, upgradeInfo.MajorFail);
        }

        TattooUpgradeResult upgradeResult = randomBag.GetRandom();

        switch (upgradeResult)
        {
            case TattooUpgradeResult.MajorFailure when isProtected:
                await HandleDamagedButProtectedResult(session, upgradeInfo);
                break;
            case TattooUpgradeResult.MajorFailure:
                await HandleDamagedResult(session, skill, upgradeInfo);
                break;
            case TattooUpgradeResult.Succeed:
                await HandleSucceedResult(session, skill, upgradeInfo);
                break;
            case TattooUpgradeResult.Fail when isProtected:
                await HandleFailButProtectedResult(session, upgradeInfo);
                break;
            case TattooUpgradeResult.Fail:
                await HandleFailResult(session, upgradeInfo);
                break;
        }

        session.SendShopEndPacket(ShopEndType.Item);
    }
    
    private async Task HandleDamagedButProtectedResult(IClientSession session, TattooUpgrade tattooUpgrade)
    {
        foreach (TattooUpgradeItem requiredItem in tattooUpgrade.Items)
        {
            await session.RemoveItemFromInventory(requiredItem.Vnum, requiredItem.Quantity);
        }
        session.SendMsgi(MessageType.Default, Game18NConstString.TheTattooProtectionScrollProtectedLevel);
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.TheTattooProtectionScrollProtectedLevel);
        session.SendEffect(EffectType.UpgradeFail);
    }

    private async Task HandleDamagedResult(IClientSession session, CharacterSkill skill, TattooUpgrade tattooUpgrade)
    {
        if (skill.UpgradeSkill > 0)
        {
            skill.UpgradeSkill--;
        }
        
        session.SendModali(Game18NConstString.LevelHasBeenReducedByOne, 5);
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.LevelHasBeenReducedByOne, 5);
        session.SendShopEndPacket(ShopEndType.Item);
        session.RefreshSkillList();
        session.RefreshQuicklist();
        
        foreach (TattooUpgradeItem requiredItem in tattooUpgrade.Items)
        {
            await session.RemoveItemFromInventory(requiredItem.Vnum, requiredItem.Quantity);
        }
    }

    private async Task HandleSucceedResult(IClientSession session, CharacterSkill skill, TattooUpgrade tattooUpgrade)
    {
        skill.UpgradeSkill++;
        session.SendModali(Game18NConstString.TattooUpgraded, 5, skill.Skill.Id, skill.UpgradeSkill);
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.TattooUpgraded, 5, skill.Skill.Id, skill.UpgradeSkill);
        session.SendEffect(EffectType.UpgradeSuccess);
        session.SendSound(SoundType.TATTOO_UPGRADE);
        session.RefreshSkillList();
        session.RefreshQuicklist();
        foreach (TattooUpgradeItem requiredItem in tattooUpgrade.Items)
        {
            await session.RemoveItemFromInventory(requiredItem.Vnum, requiredItem.Quantity);
        }
    }

    private async Task HandleFailButProtectedResult(IClientSession session, TattooUpgrade tattooUpgrade)
    {
        session.SendMsgi(MessageType.Default, Game18NConstString.UpgradeFailed);
        session.SendSayi(ChatMessageColorType.Red, Game18NConstString.UsedTattooSafeScroll);
        session.SendEffect(EffectType.UpgradeFail);
        session.SendSound(SoundType.TATTOO_FAIL);
        foreach (TattooUpgradeItem requiredItem in tattooUpgrade.Items)
        {
            await session.RemoveItemFromInventory(requiredItem.Vnum, requiredItem.Quantity);
        }
    }

    private async Task HandleFailResult(IClientSession session, TattooUpgrade tattooUpgrade)
    {
        session.SendMsgi(MessageType.Default, Game18NConstString.UpgradeFailed);
        session.SendEffect(EffectType.UpgradeFail);
        session.SendSound(SoundType.TATTOO_FAIL);
        foreach (TattooUpgradeItem requiredItem in tattooUpgrade.Items)
        {
            await session.RemoveItemFromInventory(requiredItem.Vnum, requiredItem.Quantity);
        }
    }
}
