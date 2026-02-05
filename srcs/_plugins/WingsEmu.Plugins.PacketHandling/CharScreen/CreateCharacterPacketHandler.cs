// NosEmu
// 


using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CloneExtensions;
using Mapster;
using PhoenixLib.DAL.Redis.Locks;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.AccountService;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Data.Account;
using WingsAPI.Data.CarvedRune;
using WingsAPI.Data.Character;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Customization.NewCharCustomisation;
using WingsEmu.DTOs.Account;
using WingsEmu.DTOs.Inventory;
using WingsEmu.DTOs.Items;
using WingsEmu.DTOs.Skills;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Configurations.InitialConfiguration;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Battle;
using WingsEmu.Plugins.PacketHandling.Customization;
using ItemInstanceDTO = WingsEmu.DTOs.Items.ItemInstanceDTO;

namespace WingsEmu.Plugins.PacketHandling.CharScreen;

public class CreateCharacterPacketHandler : GenericCharScreenPacketHandlerBase<CharacterCreatePacket>
{
    private readonly BaseCharacter _baseCharacter;
    private readonly BaseInventory _baseInventory;
    private readonly BaseQuicklist _baseQuicklist;
    private readonly BaseSkill _baseSkill;
    private readonly ICharacterService _characterService;
    private readonly EntryPointPacketHandler _entrypoint;
    private readonly IForbiddenNamesManager _forbiddenNamesManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IGameItemInstanceFactory _itemInstanceFactory;
    private readonly IMapManager _mapManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IRespawnDefaultConfiguration _respawnDefaultConfiguration;
    private readonly IExpirableLockService _expirableLock;
    private readonly IAccountService _accountService;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly IPlayerEntityFactory _playerEntityFactory;
    private readonly InitialMateConfiguration _initialMateConfiguration;
    private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
    private readonly ConcurrentDictionary<long, int> _lockRequestsCount = new();

    public CreateCharacterPacketHandler(EntryPointPacketHandler entrypoint, IGameLanguageService gameLanguage, BaseCharacter baseCharacter, BaseSkill baseSkill, BaseQuicklist baseQuicklist,
        BaseInventory baseInventory, IGameItemInstanceFactory gameItemInstanceFactory, ICharacterService characterService, IMapManager mapManager,
        IRespawnDefaultConfiguration respawnDefaultConfiguration, IRandomGenerator randomGenerator, IGameItemInstanceFactory itemInstanceFactory, IForbiddenNamesManager forbiddenNamesManager,
        IExpirableLockService expirableLock,
        IAccountService accountService, INpcMonsterManager npcMonsterManager, IMateEntityFactory mateEntityFactory, IPlayerEntityFactory playerEntityFactory,
        InitialMateConfiguration initialMateConfiguration)
    {
        _entrypoint = entrypoint;
        _gameLanguage = gameLanguage;
        _baseCharacter = baseCharacter;
        _baseSkill = baseSkill;
        _baseQuicklist = baseQuicklist;
        _baseInventory = baseInventory;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _characterService = characterService;
        _mapManager = mapManager;
        _respawnDefaultConfiguration = respawnDefaultConfiguration;
        _randomGenerator = randomGenerator;
        _itemInstanceFactory = itemInstanceFactory;
        _forbiddenNamesManager = forbiddenNamesManager;
        _expirableLock = expirableLock;
        _accountService = accountService;
        _npcMonsterManager = npcMonsterManager;
        _mateEntityFactory = mateEntityFactory;
        _playerEntityFactory = playerEntityFactory;
        _initialMateConfiguration = initialMateConfiguration;
    }

    protected override async Task HandlePacketAsync(IClientSession session, CharacterCreatePacket packet)
    {
        if (session.HasCurrentMapInstance)
        {
            Log.Warn("HAS_CURRENTMAP_INSTANCE");
            return;
        }

        // TODO: Hold Account Information in Authorized object
        long accountId = session.Account.Id;
        byte slot = packet.Slot;
        string characterName = packet.Name;

        try
        {
            await _semaphoreSlim.WaitAsync();

            DbServerGetCharacterResponse response = await _characterService.GetCharacterBySlot(new DbServerGetCharacterFromSlotRequest
            {
                AccountId = accountId,
                Slot = slot
            });

            if (response.RpcResponseType == RpcResponseType.SUCCESS)
            {
                Log.Warn($"[CREATE_CHARACTER_PACKET_HANDLER] Character slot is already busy. Slot: '{slot.ToString()}'");
                return;
            }

            if (slot > 3)
            {
                Log.Info("SLOTS > 3");
                return;
            }

            string lockKey = $"game:locks:character-creation:{session.Account.Id}";

            _lockRequestsCount.AddOrUpdate(session.Account.Id, 1, (key, oldValue) => oldValue + 1);

            if (_lockRequestsCount[session.Account.Id] > 5)
            {
                var banRequest = new BanAccountRequest
                {
                    AccountId = session.Account.Id,
                    Reason = "Exceeded lock request limit during character creation"
                };

                AccountBanSaveResponse banResponse = await _accountService.BanAccount(banRequest);

                if (banResponse.ResponseType != RpcResponseType.SUCCESS)
                {
                    return;
                }

                _lockRequestsCount.TryRemove(session.Account.Id, out _);
                Log.Info($"Account ID [{session.Account.Id}] has been banned for exceeding lock request limit during character creation.");
                session.ForceDisconnect();
                return;
            }

            if (!await _expirableLock.TryAddTemporaryLockAsync(lockKey, DateTime.UtcNow.AddSeconds(5)))
            {
                Log.Warn($"[CREATE_CHARACTER_PACKET_HANDLER] Tried to spam character creation, AccountId: '{session.Account.Id}'");
                return;
            }

            if (characterName.Length is < 3 or >= 15 && session.Account.Authority < AuthorityType.GM)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_CREATION_INFO_INVALID_CHARNAME, session.UserLanguage));
                return;
            }

            if ((byte)packet.HairColor > 9)
            {
                Log.Info("COLOR NOT VALID FOR A NEW CHARACTER");
                return;
            }

            if (packet.HairStyle != HairStyleType.A && packet.HairStyle != HairStyleType.B)
            {
                Log.Info("HAIRSTYLE NOT VALID FOR A NEW CHARACTER");
                return;
            }

            var rg = new Regex(@"^[a-zA-Z0-9_\-\*]*$");
            if (rg.Matches(characterName).Count != 1)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_CREATION_INFO_INVALID_CHARNAME, session.UserLanguage));
                return;
            }

            if (session.Account.Authority <= AuthorityType.GM)
            {
                string lowerCharName = characterName.ToLower();
                if (_forbiddenNamesManager.IsBanned(lowerCharName, out string bannedName))
                {
                    session.SendInfo(_gameLanguage.GetLanguageFormat(GameDialogKey.CHARACTER_CREATION_INFO_BANNED_CHARNAME, session.UserLanguage, bannedName));
                    return;
                }
            }

            DbServerGetCharacterResponse response2 = await _characterService.GetCharacterByName(new DbServerGetCharacterRequestByName
            {
                CharacterName = characterName
            });

            if (response2.RpcResponseType == RpcResponseType.SUCCESS)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_CREATION_INFO_ALREADY_TAKEN, session.UserLanguage));
                return;
            }

            CharacterDTO newCharacter = _baseCharacter.GetCharacter();

            newCharacter.AccountId = accountId;
            newCharacter.Gender = packet.Gender;
            newCharacter.HairColor = packet.HairColor;
            newCharacter.HairStyle = packet.HairStyle;
            newCharacter.Name = characterName;
            newCharacter.Slot = slot;
            newCharacter.QuickGetUp = true;
            newCharacter.UiBlocked = true;
            newCharacter.IsPartnerAutoRelive = true;
            newCharacter.IsPetAutoRelive = true;
            newCharacter.Dignity = 100;
            newCharacter.MinilandPoint = 2000;

            RespawnDefault getRespawn = _respawnDefaultConfiguration.GetReturn(RespawnType.NOSVILLE_SPAWN);
            if (getRespawn != null)
            {
                IMapInstance mapInstance = _mapManager.GetBaseMapInstanceByMapId(getRespawn.MapId);
                if (mapInstance != null)
                {
                    int randomX = getRespawn.MapX + _randomGenerator.RandomNumber(getRespawn.Radius, -getRespawn.Radius);
                    int randomY = getRespawn.MapY + _randomGenerator.RandomNumber(getRespawn.Radius, -getRespawn.Radius);

                    if (mapInstance.IsBlockedZone(randomX, randomY))
                    {
                        randomX = getRespawn.MapX;
                        randomY = getRespawn.MapY;
                    }

                    newCharacter.MapX = (short)randomX;
                    newCharacter.MapY = (short)randomY;
                }
            }

            BaseSkill skills = _baseSkill.GetClone();
            if (skills != null)
            {
                newCharacter.LearnedSkills.AddRange(skills.Skills);
            }

            BaseQuicklist quicklist = _baseQuicklist.GetClone();
            if (quicklist != null)
            {
                newCharacter.Quicklist.AddRange(quicklist.Quicklist);
            }

            BaseInventory startupInventory = _baseInventory.GetClone();
            var listOfItems = new List<InventoryItem>();
            if (startupInventory != null)
            {
                foreach (BaseInventory.StartupInventoryItem item in startupInventory.Items)
                {
                    GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(item.Vnum, item.Quantity);
                    var inventoryItem = new InventoryItem
                    {
                        InventoryType = item.InventoryType,
                        IsEquipped = false,
                        ItemInstance = newItem,
                        CharacterId = newCharacter.Id,
                        Slot = item.Slot
                    };

                    listOfItems.Add(inventoryItem);
                }
            }

            newCharacter.EquippedStuffs = listOfItems.Where(s => s is { IsEquipped: true }).Select(s =>
            {
                CharacterInventoryItemDto tmp = s.Adapt<CharacterInventoryItemDto>();
                tmp.ItemInstance = _itemInstanceFactory.CreateDto(s.ItemInstance);
                return tmp;
            }).ToList();
            newCharacter.Inventory = listOfItems.Where(s => s is { IsEquipped: false }).Select(s =>
            {
                CharacterInventoryItemDto tmp = s.Adapt<CharacterInventoryItemDto>();
                tmp.ItemInstance = _itemInstanceFactory.CreateDto(s.ItemInstance);
                return tmp;
            }).ToList();
            newCharacter.LifetimeStats = new CharacterLifetimeStatsDto();

            IPlayerEntity playerEntity = _playerEntityFactory.CreatePlayerEntity(newCharacter);

            foreach (MateConfiguration mateConfig in _initialMateConfiguration.Mates)
            {
                if (mateConfig == null)
                {
                    Log.Warn("Null mate configuration; skipping this configuration.");
                    continue;
                }

                var npcMate = new MonsterData(_npcMonsterManager.GetNpc(mateConfig.NpcVnum));
                IMateEntity mateEntity = _mateEntityFactory.CreateMateEntity(playerEntity, npcMate, mateConfig.MateType, mateConfig.Level, mateConfig.HeroLevel, []);
                mateEntity.IsTeamMember = mateConfig.IsTeamMember;
                mateEntity.PetSlot = mateConfig.PetSlot;
                mateEntity.Stars = mateConfig.Stars;
                mateEntity.Attack = mateConfig.Attack;
                mateEntity.Defence = mateConfig.Defence;

                foreach (ItemConfiguration itemConfig in mateConfig.Items)
                {
                    if (itemConfig == null)
                    {
                        Log.Warn("Null item configuration; skipping this configuration.");
                        continue;
                    }

                    var itemInstance = new ItemInstanceDTO
                    {
                        Type = itemConfig.ItemInstanceType,
                        Rarity = itemConfig.Rarity,
                        Upgrade = itemConfig.Upgrade,
                        ItemVNum = itemConfig.ItemVnum,
                        CarvedRunes = new CarvedRunesDto(),
                        PityCounter = new Dictionary<int, int>(),
                        OriginalItemVnum = itemConfig.OriginalItemVnum,
                        WeaponHitRateAdditionalValue = itemConfig.WeaponHitRateAdditionalValue,
                        WeaponMaxDamageAdditionalValue = itemConfig.WeaponMaxDamageAdditionalValue,
                        WeaponMinDamageAdditionalValue = itemConfig.WeaponMinDamageAdditionalValue,
                        ArmorDodgeAdditionalValue = itemConfig.ArmorDodgeAdditionalValue,
                        ArmorRangeAdditionalValue = itemConfig.ArmorRangeAdditionalValue,
                        ArmorMagicAdditionalValue = itemConfig.ArmorMagicAdditionalValue,
                        ArmorMeleeAdditionalValue = itemConfig.ArmorMeleeAdditionalValue,
                        FireResistance = itemConfig.FireResistance,
                        LightResistance = itemConfig.LightResistance,
                        WaterResistance = itemConfig.WaterResistance,
                        DarkResistance = itemConfig.DarkResistance,
                        Agility = itemConfig.Agility,
                        PartnerSkill1 = itemConfig.PartnerSkill1,
                        PartnerSkill2 = itemConfig.PartnerSkill2,
                        PartnerSkill3 = itemConfig.PartnerSkill3,
                        PartnerSkills = itemConfig.PartnerSkills.Select(ps => new PartnerSkillDTO
                        {
                            SkillId = ps.SkillId,
                            Rank = ps.Rank,
                            Slot = ps.Slot
                        }).ToList(),
                        SkillRank1 = itemConfig.SkillRank1,
                        SkillRank2 = itemConfig.SkillRank2,
                        SkillRank3 = itemConfig.SkillRank3,
                        SpDamage = itemConfig.SpDamage,
                        SpDefence = itemConfig.SpDefence,
                        SpCriticalDefense = itemConfig.SpCriticalDefense,
                        SpHP = itemConfig.SpHp,
                        SpFire = itemConfig.SpFire,
                        SpLight = itemConfig.SpLight,
                        SpWater = itemConfig.SpWater,
                        SpDark = itemConfig.SpDark,
                        SpStoneUpgrade = itemConfig.SpStoneUpgrade
                    };

                    newCharacter.PartnerInventory.Add(new CharacterPartnerInventoryItemDto
                    {
                        PartnerSlot = mateEntity.PetSlot,
                        IsEquipped = itemConfig.Equipped,
                        ItemInstance = itemInstance
                    });
                }

                newCharacter.NosMates.Add(_mateEntityFactory.CreateMateDto(mateEntity));
            }

            DbServerSaveCharacterResponse response3 = await _characterService.CreateCharacter(new DbServerSaveCharacterRequest
            {
                Character = newCharacter
            });

            if (response3.RpcResponseType != RpcResponseType.SUCCESS)
            {
                session.SendInfo(_gameLanguage.GetLanguage(GameDialogKey.CHARACTER_CREATION_INFO_ALREADY_TAKEN, session.UserLanguage));
                return;
            }

            _lockRequestsCount.TryUpdate(session.Account.Id, 0, _lockRequestsCount[session.Account.Id]);
            await _entrypoint.EntryPointAsync(session, null);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }
}