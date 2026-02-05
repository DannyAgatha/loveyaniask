using System;
using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Game.Extensions.Families;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Algorithm;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Families.Enum;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Packets.Enums.Chat;
using WingsEmu.Packets.Enums.Families;

namespace WingsEmu.Plugins.Essentials.Administrator.Items;

[Name("Refund")]
[Description("Module related to Administrator commands for refunds.")]
[RequireAuthority(AuthorityType.GA)]
public class RefundModule : SaltyModuleBase
{
    private readonly ICharacterAlgorithm _characterAlgorithm;

    public RefundModule(ICharacterAlgorithm characterAlgorithm) => _characterAlgorithm = characterAlgorithm;

    [Command("resistances")]
    public async Task<SaltyCommandResult> Resistances(byte slot, byte upgrade, short fire, short water, short light, short shadow)
    {
        IClientSession session = Context.Player;
        InventoryItem getItem = session.PlayerEntity.GetItemBySlotAndType(slot, InventoryType.Equipment);
        if (getItem?.ItemInstance == null)
        {
            return new SaltyCommandResult(false, $"Couldn't find item on slot: {slot} in Equipment type");
        }

        GameItemInstance item = getItem.ItemInstance;
        if (item.GameItem.EquipmentSlot != EquipmentType.Gloves && item.GameItem.EquipmentSlot != EquipmentType.Boots)
        {
            return new SaltyCommandResult(false, "The item is not gloves or boots");
        }

        item.Upgrade = upgrade;
        item.DarkResistance = shadow;
        item.LightResistance = light;
        item.WaterResistance = water;
        item.FireResistance = fire;

        session.SendInventoryAddPacket(getItem);

        return new SaltyCommandResult(true, "Item completed, check inventory.");
    }

    [Command("addxp")]
    public async Task<SaltyCommandResult> AddXp(IClientSession target, long xp)
    {
        await target.EmitEventAsync(new AddExpEvent(xp, LevelType.Level));
        target.RefreshLevel(_characterAlgorithm);

        return new SaltyCommandResult(true, $"Added {xp} xp to the {target.PlayerEntity.Name}.");
    }

    [Group("set")]
    public class RefundSetModule : SaltyModuleBase
    {
        private readonly ICharacterAlgorithm _characterAlgorithm;
        private readonly IRankingManager _rankingManager;
        private readonly IReputationConfiguration _reputationConfiguration;
        private readonly IServerManager _serverManager;
        private readonly ISpPartnerConfiguration _spPartnerConfiguration;
        private readonly IGameLanguageService _gameLanguage;
        

        public RefundSetModule(IServerManager serverManager, ICharacterAlgorithm characterAlgorithm, ISpPartnerConfiguration spPartnerConfiguration,
            IRankingManager rankingManager, IReputationConfiguration reputationConfiguration, IGameLanguageService gameLanguage)
        {
            _serverManager = serverManager;
            _characterAlgorithm = characterAlgorithm;
            _spPartnerConfiguration = spPartnerConfiguration;
            _rankingManager = rankingManager;
            _reputationConfiguration = reputationConfiguration;
            _gameLanguage = gameLanguage;
        }

        [Command("level")]
        public async Task<SaltyCommandResult> Level(IClientSession target, byte level)
        {
            IPlayerEntity character = target.PlayerEntity;

            character.LevelXp = 0;
            character.Level = level;

            if (character.Level >= _serverManager.MaxLevel)
            {
                character.Level = (byte)_serverManager.MaxLevel;
                character.LevelXp = 0;
            }

            character.Session.RefreshStatChar();

            character.Hp = character.MaxHp;
            character.Mp = character.MaxMp;

            character.Session.RefreshStat();

            switch (character.Level)
            {
                case > 20 when (character.Level % 10) == 0:
                    await target.FamilyAddLogAsync(FamilyLogType.LevelUp, character.Name, character.Level.ToString());
                    await target.FamilyAddExperience(character.Level * 20, FamXpObtainedFromType.LevelUp);
                    break;
                case > 80:
                    await target.FamilyAddLogAsync(FamilyLogType.LevelUp, character.Name, character.Level.ToString());
                    break;
            }

            target.SendLevelUp();
            target.RefreshLevel(_characterAlgorithm);
            target.RefreshGroupLevelUi(_spPartnerConfiguration);
            target.SendMsg(target.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_LEVELUP), MsgMessageType.Middle);
            target.BroadcastEffectInRange(EffectType.NormalLevelUp);
            target.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);
            return new SaltyCommandResult(true, "Level successfully set.");
        }
        
        [Command("herolevel")]
        public async Task<SaltyCommandResult> HeroLevel(IClientSession target, byte heroLevel)
        {
            IPlayerEntity character = target.PlayerEntity;

            character.HeroXp = 0;
            character.HeroLevel = heroLevel;

            if (character.HeroLevel >= _serverManager.MaxHeroLevel)
            {
                character.HeroLevel = (byte)_serverManager.MaxHeroLevel;
                character.HeroXp = 0;
            }

            character.Hp = character.MaxHp;
            character.Mp = character.MaxMp;

            character.Session.RefreshStat();

            switch (character.HeroLevel)
            {
                case > 1 when (character.HeroLevel % 10) == 0:
                    await target.FamilyAddLogAsync(FamilyLogType.HeroLevelUp, character.Name, character.HeroLevel.ToString());
                    await target.FamilyAddExperience(character.HeroLevel * 20, FamXpObtainedFromType.LevelUp);
                    break;
                case > 50:
                    await target.FamilyAddLogAsync(FamilyLogType.HeroLevelUp, character.Name, character.HeroLevel.ToString());
                    break;
            }

            target.SendLevelUp();
            target.RefreshGroupLevelUi(_spPartnerConfiguration);
            target.SendMsg(target.GetLanguage(GameDialogKey.INFORMATION_HERO_LEVELUP), MsgMessageType.Middle);
            target.BroadcastEffectInRange(EffectType.NormalLevelUp);
            target.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);

            return new SaltyCommandResult(true, "Hero level successfully set.");
        }
        
        [Command("fairylevel")]
        public async Task<SaltyCommandResult> FairyLevel(IClientSession target, byte fairyLevel)
        {
            GameItemInstance fairy = target.PlayerEntity.Fairy;
            if (fairy == null)
            {
                return new SaltyCommandResult(false, "No fairy equipped.");
            }

            fairy.Xp = 0;
            fairy.ElementRate = fairyLevel;

            if ((fairy.ElementRate + fairy.GameItem.ElementRate) >= fairy.GameItem.MaxElementRate)
            {
                fairy.ElementRate = (byte)(fairy.GameItem.MaxElementRate - fairy.GameItem.ElementRate);
                fairy.Xp = 0;
                target.SendMsg(
                    _gameLanguage.GetLanguageFormat(
                        GameDialogKey.INFORMATION_SHOUTMESSAGE_FAIRYMAX,
                        target.UserLanguage,
                        _gameLanguage.GetLanguage(GameDataType.Item, fairy.GameItem.Name, target.UserLanguage)
                    ),
                    MsgMessageType.Middle
                );
            }
            else
            {
                target.SendMsg(
                    _gameLanguage.GetLanguageFormat(
                        GameDialogKey.INFORMATION_SHOUTMESSAGE_FAIRY_LEVELUP,
                        target.UserLanguage,
                        _gameLanguage.GetLanguage(GameDataType.Item, fairy.GameItem.Name, target.UserLanguage)
                    ),
                    MsgMessageType.Middle
                );
            }

            target.RefreshFairy();
            return new SaltyCommandResult(true, "Fairy level successfully set.");
        }

        [Command("job")]
        public async Task<SaltyCommandResult> Job(IClientSession target, byte level)
        {
            IPlayerEntity character = target.PlayerEntity;
            character.JobLevelXp = 0;
            character.JobLevel = level;

            if (character.JobLevel >= 20 && character.Class == ClassType.Adventurer)
            {
                character.JobLevel = 20;
                character.JobLevelXp = 0;
            }
            else if (character.JobLevel >= _serverManager.MaxJobLevel)
            {
                character.JobLevel = (byte)_serverManager.MaxJobLevel;
                character.JobLevelXp = 0;
            }

            target.SendLevelUp();
            target.RefreshLevel(_characterAlgorithm);
            target.SendMsg(target.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_JOB_LEVELUP), MsgMessageType.Middle);
            target.BroadcastEffectInRange(EffectType.JobLevelUp);
            character.SkillComponent.ResetSkillCooldowns = DateTime.UtcNow;

            return new SaltyCommandResult(true, "Job Level successfully set.");
        }

        [Command("spjob")]
        public async Task<SaltyCommandResult> SpJob(IClientSession target, byte level)
        {
            IPlayerEntity character = target.PlayerEntity;

            if (character.Specialist == null || !character.UseSp)
            {
                return new SaltyCommandResult(false, "Player doesn't have Specialist Card in slot.");
            }

            character.Specialist.Xp = 0;
            character.Specialist.SpLevel = level;

            if (character.Specialist.SpLevel >= _serverManager.MaxSpLevel)
            {
                character.Specialist.SpLevel = (byte)_serverManager.MaxSpLevel;
                character.Specialist.Xp = 0;
            }

            target.RefreshLevel(_characterAlgorithm);
            target.SendLevelUp();
            target.SendMsg(target.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_LEVELUP), MsgMessageType.Middle);
            target.BroadcastEffectInRange(EffectType.JobLevelUp);
            character.SkillComponent.ResetSpSkillCooldowns = DateTime.UtcNow;

            return new SaltyCommandResult(true, "Specialist Level successfully set.");
        }

        [Command("reput")]
        public async Task<SaltyCommandResult> Reputation(IClientSession target, long reput)
        {
            target.PlayerEntity.Reput = reput;
            target.RefreshReputation(_reputationConfiguration, _rankingManager.TopReputation);

            return new SaltyCommandResult(true, "Reputation successfully set.");
        }

        [Command("gold")]
        public async Task<SaltyCommandResult> Gold(IClientSession target, long gold)
        {
            if (gold < 0)
            {
                return new SaltyCommandResult(false, "Wrong value!");
            }

            if (gold > _serverManager.MaxGold)
            {
                target.PlayerEntity.Gold = _serverManager.MaxGold;
                target.RefreshGold();
                return new SaltyCommandResult(true, $"Your gold: {target.PlayerEntity.Gold}");
            }

            target.PlayerEntity.Gold = gold;
            target.RefreshGold();

            return new SaltyCommandResult(true, "Gold successfully set.");
        }
    }

    [Group("add")]
    public class RefundAddModule : SaltyModuleBase
    {
        private readonly ICharacterAlgorithm _characterAlgorithm;
        private readonly IRankingManager _rankingManager;
        private readonly IReputationConfiguration _reputationConfiguration;
        private readonly IServerManager _serverManager;
        private readonly ISpPartnerConfiguration _spPartnerConfiguration;

        public RefundAddModule(IServerManager serverManager, ICharacterAlgorithm characterAlgorithm, ISpPartnerConfiguration spPartnerConfiguration,
            IRankingManager rankingManager, IReputationConfiguration reputationConfiguration)
        {
            _serverManager = serverManager;
            _characterAlgorithm = characterAlgorithm;
            _spPartnerConfiguration = spPartnerConfiguration;
            _rankingManager = rankingManager;
            _reputationConfiguration = reputationConfiguration;
        }

        [Command("level")]
        public async Task<SaltyCommandResult> Level(IClientSession target, byte level)
        {
            IPlayerEntity character = target.PlayerEntity;

            character.LevelXp = 0;
            character.Level += level;

            if (character.Level >= _serverManager.MaxLevel)
            {
                character.Level = (byte)_serverManager.MaxLevel;
                character.LevelXp = 0;
            }

            character.Session.RefreshStatChar();

            character.Hp = character.MaxHp;
            character.Mp = character.MaxMp;

            character.Session.RefreshStat();

            if (character.Level > 20 && (character.Level % 10) == 0)
            {
                await target.FamilyAddLogAsync(FamilyLogType.LevelUp, character.Name, character.Level.ToString());
                await target.FamilyAddExperience(character.Level * 20, FamXpObtainedFromType.LevelUp);
            }
            else if (character.Level > 80)
            {
                await target.FamilyAddLogAsync(FamilyLogType.LevelUp, character.Name, character.Level.ToString());
            }

            target.SendLevelUp();
            target.RefreshLevel(_characterAlgorithm);
            target.RefreshGroupLevelUi(_spPartnerConfiguration);
            target.SendMsg(target.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_LEVELUP), MsgMessageType.Middle);
            target.BroadcastEffectInRange(EffectType.NormalLevelUp);
            target.BroadcastEffectInRange(EffectType.NormalLevelUpSubEffect);
            return new SaltyCommandResult(true, "Level successfully add.");
        }

        [Command("job")]
        public async Task<SaltyCommandResult> Job(IClientSession target, byte level)
        {
            IPlayerEntity character = target.PlayerEntity;
            character.JobLevelXp = 0;
            character.JobLevel += level;

            if (character.JobLevel >= 20 && character.Class == ClassType.Adventurer)
            {
                character.JobLevel = 20;
                character.JobLevelXp = 0;
            }
            else if (character.JobLevel >= _serverManager.MaxJobLevel)
            {
                character.JobLevel = (byte)_serverManager.MaxJobLevel;
                character.JobLevelXp = 0;
            }

            target.SendLevelUp();
            target.RefreshLevel(_characterAlgorithm);
            target.SendMsg(target.GetLanguage(GameDialogKey.INFORMATION_SHOUTMESSAGE_JOB_LEVELUP), MsgMessageType.Middle);
            target.BroadcastEffectInRange(EffectType.JobLevelUp);
            character.SkillComponent.ResetSkillCooldowns = DateTime.UtcNow;

            return new SaltyCommandResult(true, "Job Level successfully add.");
        }

        [Command("spjob")]
        public async Task<SaltyCommandResult> SpJob(IClientSession target, byte level)
        {
            IPlayerEntity character = target.PlayerEntity;

            if (character.Specialist == null || !character.UseSp)
            {
                return new SaltyCommandResult(false, "Player doesn't have Specialist Card in slot.");
            }

            character.Specialist.Xp = 0;
            character.Specialist.SpLevel += level;

            if (character.Specialist.SpLevel >= _serverManager.MaxSpLevel)
            {
                character.Specialist.SpLevel = (byte)_serverManager.MaxSpLevel;
                character.Specialist.Xp = 0;
            }

            target.RefreshLevel(_characterAlgorithm);
            target.SendLevelUp();
            target.SendMsg(target.GetLanguage(GameDialogKey.SPECIALIST_SHOUTMESSAGE_LEVELUP), MsgMessageType.Middle);
            target.BroadcastEffectInRange(EffectType.JobLevelUp);
            character.SkillComponent.ResetSpSkillCooldowns = DateTime.UtcNow;

            return new SaltyCommandResult(true, "Specialist Level successfully add.");
        }

        [Command("reput")]
        public async Task<SaltyCommandResult> Reputation(IClientSession target, long reput)
        {
            target.PlayerEntity.Reput += reput;
            target.RefreshReputation(_reputationConfiguration, _rankingManager.TopReputation);

            return new SaltyCommandResult(true, "Reputation successfully add.");
        }

        [Command("gold")]
        public async Task<SaltyCommandResult> Gold(IClientSession target, long gold)
        {
            if (gold < 0)
            {
                return new SaltyCommandResult(false, "Wrong value!");
            }

            if (gold > _serverManager.MaxGold)
            {
                target.PlayerEntity.Gold = _serverManager.MaxGold;
                target.RefreshGold();
                return new SaltyCommandResult(true, $"Your gold: {target.PlayerEntity.Gold}");
            }

            target.PlayerEntity.Gold += gold;

            if (target.PlayerEntity.Gold > _serverManager.MaxGold)
            {
                target.PlayerEntity.Gold = _serverManager.MaxGold;
            }

            target.RefreshGold();

            return new SaltyCommandResult(true, "Gold successfully add.");
        }
    }
}