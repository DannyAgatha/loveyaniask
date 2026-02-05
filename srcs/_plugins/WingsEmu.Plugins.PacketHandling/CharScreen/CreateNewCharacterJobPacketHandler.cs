using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CloneExtensions;
using Mapster;
using PhoenixLib.Logging;
using WingsAPI.Communication;
using WingsAPI.Communication.DbServer.CharacterService;
using WingsAPI.Data.CarvedRune;
using WingsAPI.Data.Character;
using WingsAPI.Packets.Enums.Character;
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
using WingsEmu.Game.Inventory;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs;
using WingsEmu.Packets.ClientPackets;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Character;
using WingsEmu.Plugins.PacketHandling.Customization;

namespace WingsEmu.Plugins.PacketHandling.CharScreen;

public class CreateNewCharacterJobPacketHandler : GenericCharScreenPacketHandlerBase<NewCharacterJobPacket>
{
    private readonly BaseCharacter _baseCharacter;
    private readonly BaseInventorySwordman _baseInventorySwordman;
    private readonly BaseSkillSwordman _baseSkillSwordman;
    private readonly BaseQuickListSwordman _baseQuickListSwordman;
    private readonly BaseInventoryArcher _baseInventoryArcher;
    private readonly BaseSkillArcher _baseSkillArcher;
    private readonly BaseQuickListArcher _baseQuickListArcher;
    private readonly BaseInventoryMagician _baseInventoryMagician;
    private readonly BaseSkillMagician _baseSkillMagician;
    private readonly BaseQuickListMagician _baseQuickListMagician;
    private readonly BaseInventoryMartialArtist _baseInventoryMartialArtist;
    private readonly BaseSkillMartialArtist _baseSkillMartialArtist;
    private readonly BaseQuickListMartialArtist _baseQuickListMartialArtist;
    private readonly ICharacterService _characterService;
    private readonly EntryPointPacketHandler _entrypoint;
    private readonly IForbiddenNamesManager _forbiddenNamesManager;
    private readonly IGameItemInstanceFactory _gameItemInstanceFactory;
    private readonly IGameLanguageService _gameLanguage;
    private readonly IGameItemInstanceFactory _itemInstanceFactory;
    private readonly IMapManager _mapManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IRespawnDefaultConfiguration _respawnDefaultConfiguration;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly IPlayerEntityFactory _playerEntityFactory;
    private readonly InitialMateConfiguration _initialMateConfiguration;
    private readonly InitialCharacterJobConfiguration _initialCharacterJobConfiguration;

    public CreateNewCharacterJobPacketHandler(
        EntryPointPacketHandler entrypoint,
        IGameLanguageService gameLanguage,
        BaseCharacter baseCharacter,
        BaseInventorySwordman baseInventorySwordman,
        BaseSkillSwordman baseSkillSwordman,
        BaseQuickListSwordman baseQuickListSwordman,
        BaseInventoryArcher baseInventoryArcher,
        BaseSkillArcher baseSkillArcher,
        BaseQuickListArcher baseQuickListArcher,
        BaseInventoryMagician baseInventoryMagician,
        BaseSkillMagician baseSkillMagician,
        BaseQuickListMagician baseQuickListMagician,
        BaseInventoryMartialArtist baseInventoryMartialArtist,
        BaseSkillMartialArtist baseSkillMartialArtist,
        BaseQuickListMartialArtist baseQuickListMartialArtist,
        IGameItemInstanceFactory gameItemInstanceFactory,
        ICharacterService characterService,
        IMapManager mapManager,
        IRespawnDefaultConfiguration respawnDefaultConfiguration,
        IRandomGenerator randomGenerator,
        IGameItemInstanceFactory itemInstanceFactory,
        IForbiddenNamesManager forbiddenNamesManager,
        INpcMonsterManager npcMonsterManager,
        IMateEntityFactory mateEntityFactory,
        IPlayerEntityFactory playerEntityFactory,
        InitialMateConfiguration initialMateConfiguration,
        InitialCharacterJobConfiguration initialCharacterJobConfiguration)
    {
        _entrypoint = entrypoint;
        _gameLanguage = gameLanguage;
        _baseCharacter = baseCharacter;
        _baseInventorySwordman = baseInventorySwordman;
        _baseSkillSwordman = baseSkillSwordman;
        _baseQuickListSwordman = baseQuickListSwordman;
        _baseInventoryArcher = baseInventoryArcher;
        _baseSkillArcher = baseSkillArcher;
        _baseQuickListArcher = baseQuickListArcher;
        _baseInventoryMagician = baseInventoryMagician;
        _baseSkillMagician = baseSkillMagician;
        _baseQuickListMagician = baseQuickListMagician;
        _baseInventoryMartialArtist = baseInventoryMartialArtist;
        _baseSkillMartialArtist = baseSkillMartialArtist;
        _baseQuickListMartialArtist = baseQuickListMartialArtist;
        _gameItemInstanceFactory = gameItemInstanceFactory;
        _characterService = characterService;
        _mapManager = mapManager;
        _respawnDefaultConfiguration = respawnDefaultConfiguration;
        _randomGenerator = randomGenerator;
        _itemInstanceFactory = itemInstanceFactory;
        _forbiddenNamesManager = forbiddenNamesManager;
        _npcMonsterManager = npcMonsterManager;
        _mateEntityFactory = mateEntityFactory;
        _playerEntityFactory = playerEntityFactory;
        _initialMateConfiguration = initialMateConfiguration;
        _initialCharacterJobConfiguration = initialCharacterJobConfiguration;
    }


    protected override async Task HandlePacketAsync(IClientSession session, NewCharacterJobPacket packet)
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

        DbServerGetCharactersResponse response4 = await _characterService.GetCharacters(new DbServerGetCharactersRequest
        {
            AccountId = session.Account.Id
        });

        if (response4?.Characters?.Any(s => s.Class == ClassType.MartialArtist) == true)
        {
            Log.Warn($"[CHARACTER_CREATION] Account already has a Martial Artist. Slot: {slot}");
            return;
        }

        if (packet.CharacterCreationClass == CharacterCreationClassOption.FirstOption)
        {
            if (response4?.Characters == null || !response4.Characters.Any())
            {
                Log.Warn($"[CHARACTER_CREATION] Account {session.Account.Id} cannot create Martial Artist - no existing characters");
                return;
            }

            if (!response4.Characters.Any(c => c.Level >= 80))
            {
                Log.Warn($"[CHARACTER_CREATION] Account {session.Account.Id} lacks required level 80 character");
                return;
            }
        }

        if (slot > 3)
        {
            Log.Info("SLOTS > 3");
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

        newCharacter.Class = packet.CharacterCreationClass switch
        {
            CharacterCreationClassOption.FirstOption => ClassType.MartialArtist, // 0 → MartialArtist (4)
            CharacterCreationClassOption.SecondOption => ClassType.Swordman, // 2 → Swordman (1)
            CharacterCreationClassOption.ThirdOption => ClassType.Archer, // 3 → Archer (2)
            CharacterCreationClassOption.FourOption => ClassType.Magician, // 4 → Magician (3)
            _ => ClassType.Adventurer
        };

        CharacterJobConfiguration characterJob = _initialCharacterJobConfiguration.CharacterJobs[packet.CharacterCreationClass];

        (newCharacter.Level, newCharacter.JobLevel, newCharacter.Hp, newCharacter.Mp, newCharacter.Reput, newCharacter.Dignity, newCharacter.MinilandPoint) =
        (
            characterJob.Level,
            characterJob.JobLevel,
            characterJob.Hp,
            characterJob.Mp,
            characterJob.Reput,
            characterJob.Dignity,
            characterJob.MinilandPoints
        );

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

        switch (packet.CharacterCreationClass)
        {
            case CharacterCreationClassOption.FirstOption when _baseSkillMartialArtist.GetClone() is { } maSkills:
                newCharacter.LearnedSkills.AddRange(maSkills.DefaultSkills);
                newCharacter.LearnedSkills.AddRange(maSkills.PassiveSkills);
                if (_baseQuickListMartialArtist.GetClone() is { } maQuicklist)
                {
                    newCharacter.Quicklist.AddRange(maQuicklist.Quicklist);
                }

                break;

            case CharacterCreationClassOption.SecondOption when _baseSkillSwordman.GetClone() is { } swSkills:
                newCharacter.LearnedSkills.AddRange(swSkills.DefaultSkills);
                newCharacter.LearnedSkills.AddRange(swSkills.PassiveSkills);
                if (_baseQuickListSwordman.GetClone() is { } swQuicklist)
                {
                    newCharacter.Quicklist.AddRange(swQuicklist.Quicklist);
                }

                break;

            case CharacterCreationClassOption.ThirdOption when _baseSkillArcher.GetClone() is { } arSkills:
                newCharacter.LearnedSkills.AddRange(arSkills.DefaultSkills);
                newCharacter.LearnedSkills.AddRange(arSkills.PassiveSkills);
                if (_baseQuickListArcher.GetClone() is { } arQuicklist)
                {
                    newCharacter.Quicklist.AddRange(arQuicklist.Quicklist);
                }

                break;

            case CharacterCreationClassOption.FourOption when _baseSkillMagician.GetClone() is { } mgSkills:
                newCharacter.LearnedSkills.AddRange(mgSkills.DefaultSkills);
                newCharacter.LearnedSkills.AddRange(mgSkills.PassiveSkills);
                if (_baseQuickListMagician.GetClone() is { } mgQuicklist)
                {
                    newCharacter.Quicklist.AddRange(mgQuicklist.Quicklist);
                }

                break;
            default:
                break;
        }

        List<BaseInventory.StartupInventoryItem> startupItems = packet.CharacterCreationClass switch
        {
            CharacterCreationClassOption.FirstOption => _baseInventoryMartialArtist.GetClone().Items,
            CharacterCreationClassOption.SecondOption => _baseInventorySwordman.GetClone().Items,
            CharacterCreationClassOption.ThirdOption => _baseInventoryArcher.GetClone().Items,
            CharacterCreationClassOption.FourOption => _baseInventoryMagician.GetClone().Items,
            _ => []
        };

        var listOfItems = startupItems.Select(item =>
        {
            GameItemInstance newItem = _gameItemInstanceFactory.CreateItem(item.Vnum, item.Quantity);

            if (item.Upgrade > 0)
            {
                newItem.Upgrade = item.Upgrade;
            }

            if (item.Rare > 0)
            {
                newItem.Rarity = item.Rare;
            }

            if (item.Options?.Count > 0)
            {
                newItem.EquipmentOptions = item.Options;
            }

            if (!(item.SpecialistOptions?.Count > 0))
            {
                return new InventoryItem
                {
                    InventoryType = item.InventoryType,
                    ItemInstance = newItem,
                    CharacterId = newCharacter.Id,
                    Slot = item.Slot
                };
            }

            ItemInstanceDTO sp = item.SpecialistOptions[0];
            newItem.SpLevel = sp.SpLevel;
            newItem.SpStoneUpgrade = sp.SpStoneUpgrade;
            newItem.SpDamage = sp.SpDamage;
            newItem.SpDefence = sp.SpDefence;
            newItem.SpElement = sp.SpElement;
            newItem.SpHP = sp.SpHP;
            newItem.SpFire = sp.SpFire;
            newItem.SpWater = sp.SpWater;
            newItem.SpLight = sp.SpLight;
            newItem.SpDark = sp.SpDark;

            return new InventoryItem
            {
                InventoryType = item.InventoryType,
                ItemInstance = newItem,
                CharacterId = newCharacter.Id,
                Slot = item.Slot
            };
        }).ToList();

        newCharacter.EquippedStuffs = listOfItems
            .Where(i => i.IsEquipped)
            .Select(i =>
            {
                CharacterInventoryItemDto dto = i.Adapt<CharacterInventoryItemDto>();
                dto.ItemInstance = _itemInstanceFactory.CreateDto(i.ItemInstance);
                return dto;
            }).ToList();

        newCharacter.Inventory = listOfItems
            .Where(i => !i.IsEquipped)
            .Select(i =>
            {
                CharacterInventoryItemDto dto = i.Adapt<CharacterInventoryItemDto>();
                dto.ItemInstance = _itemInstanceFactory.CreateDto(i.ItemInstance);
                return dto;
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

        await _entrypoint.EntryPointAsync(session, null);
    }
}