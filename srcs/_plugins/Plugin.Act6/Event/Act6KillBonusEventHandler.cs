using PhoenixLib.Events;
using WingsAPI.Data.Drops;
using WingsAPI.Game.Extensions.ItemExtension;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.DTOs.Bonus;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Act6;
using WingsEmu.Game.Act6.Configuration;
using WingsEmu.Game.Act6.Event;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Characters.Events;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Groups;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Inventory.Event;
using WingsEmu.Game.Items;
using WingsEmu.Game.Managers;
using WingsEmu.Game.Managers.ServerData;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace Plugin.Act6.Event
{
    public class Act6KillBonusEventHandler : IAsyncEventProcessor<KillBonusEvent>
    {
        private readonly Act6Configuration _conf;
        private readonly IAct6Manager _act6Manager;
        private readonly IAct6InstanceManager _act6InstanceManager;

        private readonly IDropManager _dropManager;
        private readonly IDropRarityConfigurationProvider _dropRarityConfigurationProvider;
        private readonly IAsyncEventPipeline _eventPipeline;
        private readonly IGameItemInstanceFactory _gameItemInstance;
        private readonly IGameLanguageService _gameLanguage;
        private readonly IItemsManager _itemsManager;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IRankingManager _rankingManager;
        private readonly IReputationConfiguration _reputationConfiguration;
        private readonly IServerManager _serverManager;
        private readonly ISessionManager _sessionManager;

        public Act6KillBonusEventHandler(Act6Configuration conf, IAct6Manager act6Manager, IAct6InstanceManager act6InstanceManager, IDropManager dropManager, IDropRarityConfigurationProvider dropRarityConfigurationProvider, IAsyncEventPipeline eventPipeline, IGameItemInstanceFactory gameItemInstance, IGameLanguageService gameLanguage, IItemsManager itemsManager, IRandomGenerator randomGenerator, IRankingManager rankingManager, IReputationConfiguration reputationConfiguration, IServerManager serverManager, ISessionManager sessionManager)
        {
            _conf = conf;
            _act6Manager = act6Manager;
            _act6InstanceManager = act6InstanceManager;
            _dropManager = dropManager;
            _dropRarityConfigurationProvider = dropRarityConfigurationProvider;
            _eventPipeline = eventPipeline;
            _gameItemInstance = gameItemInstance;
            _gameLanguage = gameLanguage;
            _itemsManager = itemsManager;
            _randomGenerator = randomGenerator;
            _rankingManager = rankingManager;
            _reputationConfiguration = reputationConfiguration;
            _serverManager = serverManager;
            _sessionManager = sessionManager;
        }

        public async Task HandleAsync(KillBonusEvent e, CancellationToken cancellation)
        {
            IMonsterEntity monsterEntityToAttack = e.MonsterEntity;
            IPlayerEntity character = e.Sender.PlayerEntity;
            IClientSession session = e.Sender;

            if (monsterEntityToAttack == null || monsterEntityToAttack.IsStillAlive || monsterEntityToAttack.SummonerType is VisualType.Player)
            {
                return;
            }

            if (!monsterEntityToAttack.MapInstance.HasMapFlag(MapFlags.ACT_6_1))
            {
                return;
            }

            await session.EmitEventAsync(new Act6FactionPointsIncreaseEvent());

            // Handle Custom drops

            if (!ShouldMonsterDrop(monsterEntityToAttack))
            {
                return;
            }

            // owner set
            IPlayerEntity? dropOwner = null;

            if (monsterEntityToAttack.Damagers.Count > 0)
            {
                IBattleEntity? entityDropOwner = monsterEntityToAttack.Damagers.FirstOrDefault();
                if (entityDropOwner != null)
                {
                    dropOwner = entityDropOwner switch
                    {
                        IMonsterEntity monsterEntity
                            => monsterEntity.SummonerType != null && monsterEntity.SummonerId != null && monsterEntity.SummonerType == VisualType.Player
                                ? monsterEntity.MapInstance.GetCharacterById(monsterEntity.SummonerId.Value)
                                : null,
                        IPlayerEntity playerEntity => playerEntity,
                        IMateEntity mateEntity => mateEntity.Owner,
                        _ => null
                    };
                }
            }

            // Check if owner is online and it's at the same map
            IClientSession? firstAttacker = dropOwner != null ? _sessionManager.GetSessionByCharacterId(dropOwner.Id) : null;
            if (firstAttacker == null)
            {
                dropOwner = character;
            }
            else
            {
                dropOwner = firstAttacker.CurrentMapInstance?.Id == character.MapInstance.Id ? firstAttacker.PlayerEntity : character;
            }

            PlayerGroup? playerGroup = null;
            if (dropOwner != null)
            {
                playerGroup = dropOwner.GetGroup();
            }

            // end owner set
            if (!session.HasCurrentMapInstance)
            {
                return;
            }

            if (dropOwner != null)
            {
                await HandleDrops(monsterEntityToAttack, playerGroup, dropOwner);
            }
        }

        private bool ShouldMonsterDrop(IMonsterEntity monsterEntityToAttack)
        {
            return (MonsterVnum)monsterEntityToAttack.MonsterVNum switch
            {
                MonsterVnum.TRAINING_STAKE or MonsterVnum.DEMON_CAMP or MonsterVnum.ANGEL_CAMP => false,
                _ => true,
            };
        }

        private async Task HandleDrops(IMonsterEntity monsterEntityToAttack, PlayerGroup? playerGroup, IPlayerEntity firstAttacker)
        {
            // act6 drops

            if (!_act6InstanceManager.PvpInstance.InstanceActive || !monsterEntityToAttack.MapInstance.IsAct6PvpInstance)
            {
                return;
            }

            IClientSession session = firstAttacker.Session;

            var additionalDrop = new List<DropDTO>();

            switch (_act6InstanceManager.Audience.InstanceFaction)
            {
                case FactionType.Angel:
                    additionalDrop.Add(new DropDTO
                    {
                        Amount = 1,
                        DropChance = 500,
                        ItemVNum = 5883 
                    });
                    break;

                case FactionType.Demon:
                    additionalDrop.Add(new DropDTO
                    {
                        Amount = 1,
                        DropChance = 500,
                        ItemVNum = 5882 
                    });
                    break;
            }

            int secondChanceDropBCard = session.PlayerEntity.BCardComponent
            .GetAllBCardsInformation(BCardType.DropItemTwice, (byte)AdditionalTypes.DropItemTwice.DoubleDropChance, session.PlayerEntity.Level).firstData;
            bool secondChanceDrop = secondChanceDropBCard != 0 && _randomGenerator.RandomNumber() <= secondChanceDropBCard;

            if (secondChanceDrop)
            {
                session.PlayerEntity.BroadcastEffectInRange(EffectType.DoubleChanceDrop);
            }

            int genericRate = _serverManager.GenericDropRate;

            for (int i = 0; i < genericRate; i++)
            {
                foreach (DropDTO drop in additionalDrop)
                {
                    float rndChance = _randomGenerator.RandomNumber(0, 10000);
                    float chance = drop.DropChance + drop.DropChance * _serverManager.GenericDropChance * 1.0f;
                    if (rndChance > chance)
                    {
                        continue;
                    }

                    await DropItem(session, monsterEntityToAttack, drop.ItemVNum, drop.Amount, playerGroup);

                    if (!secondChanceDrop)
                    {
                        continue;
                    }

                    await DropItem(session, monsterEntityToAttack, drop.ItemVNum, drop.Amount, playerGroup);
                }
            }
        }

        private async Task DropItem(IClientSession session, IMonsterEntity monsterEntityToAttack, int itemVnum, int amount, PlayerGroup? playerGroup)
        {
            if (session.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4) || session.CurrentMapInstance.HasMapFlag(MapFlags.HAS_DROP_DIRECTLY_IN_INVENTORY_ENABLED)
                || monsterEntityToAttack.DropToInventory || session.PlayerEntity.HaveStaticBonus(StaticBonusType.AutoLoot))
            {
                var alreadyGifted = new HashSet<long>();
                foreach (IBattleEntity entity in monsterEntityToAttack.Damagers)
                {
                    long charId = entity.Id;
                    if (alreadyGifted.Contains(charId))
                    {
                        continue;
                    }

                    IClientSession giftSession = _sessionManager.GetSessionByCharacterId(charId);

                    if (giftSession == null)
                    {
                        continue;
                    }

                    if (giftSession.PlayerEntity.MapInstance?.Id != monsterEntityToAttack.MapInstance?.Id)
                    {
                        continue;
                    }

                    bool shouldReceiveDrop = ShouldReceiveDrop(giftSession, monsterEntityToAttack);
                    if (!shouldReceiveDrop)
                    {
                        continue;
                    }

                    IGameItem item = _itemsManager.GetItem(itemVnum);
                    sbyte randomRarity = _dropRarityConfigurationProvider.GetRandomRarity(item.ItemType);

                    GameItemInstance itemInstance = _gameItemInstance.CreateItem(itemVnum, amount, 0, randomRarity);

                    if (item.ItemType == ItemType.Map)
                    {
                        continue;
                    }

                    await giftSession.AddNewItemToInventory(itemInstance, true, ChatMessageColorType.Yellow, true);
                    alreadyGifted.Add(charId);
                }

                return;
            }

            short newX = (short)(monsterEntityToAttack.PositionX + _randomGenerator.RandomNumber(-1, 2));
            short newY = (short)(monsterEntityToAttack.PositionY + _randomGenerator.RandomNumber(-1, 2));

            if (monsterEntityToAttack.MapInstance.IsBlockedZone(newX, newY))
            {
                newX = monsterEntityToAttack.PositionX;
                newY = monsterEntityToAttack.PositionY;
            }

            var newItemPosition = new Position(newX, newY);

            if (playerGroup == null)
            {
                var dropItem = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, newItemPosition, (short)itemVnum, amount, ownerId: session.PlayerEntity.Id);
                await _eventPipeline.ProcessEventAsync(dropItem);
                return;
            }

            if (playerGroup.SharingMode == (byte)GroupSharingType.ByOrder)
            {
                long? dropOwner = playerGroup.GetNextOrderedCharacterId(session.PlayerEntity);

                if (!dropOwner.HasValue)
                {
                    return;
                }

                foreach (IPlayerEntity s in playerGroup.Members)
                {
                    string itemName = _gameLanguage.GetItemName(_itemsManager.GetItem(itemVnum), s.Session);
                    s.Session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.GROUP_CHATMESSAGE_DROP_ORDERED, s.Session.UserLanguage, amount,
                        itemName, playerGroup.Members.FirstOrDefault(c => c.Id == (long)dropOwner)?.Name), ChatMessageColorType.Yellow);
                }

                var dropItem = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, newItemPosition, (short)itemVnum, amount, ownerId: dropOwner.Value);
                await _eventPipeline.ProcessEventAsync(dropItem);
            }
            else
            {
                foreach (IPlayerEntity s in playerGroup.Members)
                {
                    string itemName = _gameLanguage.GetItemName(_itemsManager.GetItem(itemVnum), s.Session);
                    s.Session.SendChatMessage(_gameLanguage.GetLanguageFormat(GameDialogKey.GROUP_CHATMESSAGE_DROP_SHARED, s.Session.UserLanguage, amount, itemName), ChatMessageColorType.Yellow);
                }

                var dropItem = new DropMapItemEvent(session, session.PlayerEntity.MapInstance, newItemPosition, (short)itemVnum, amount, ownerId: session.PlayerEntity.Id);
                await _eventPipeline.ProcessEventAsync(dropItem);
            }
        }

        private bool ShouldReceiveDrop(IClientSession giftSession, IMonsterEntity monsterEntityToAttack)
        {
            long? tankPlayer = null;
            int highestHits = 0;
            foreach (IBattleEntity damager in monsterEntityToAttack.Damagers)
            {
                if (damager is not IPlayerEntity playerEntity)
                {
                    continue;
                }

                if (!playerEntity.HitsByMonsters.TryGetValue(monsterEntityToAttack.Id, out int hits))
                {
                    continue;
                }

                if (highestHits > hits)
                {
                    continue;
                }

                tankPlayer = playerEntity.Id;
                highestHits = hits;
            }

            if (tankPlayer != null && giftSession.PlayerEntity.Id == tankPlayer.Value)
            {
                return true;
            }

            IPlayerEntity player = giftSession.PlayerEntity;
            if (!monsterEntityToAttack.PlayersDamage.TryGetValue(player.Id, out int damage))
            {
                return false;
            }

            int damageToDealt = (int)(monsterEntityToAttack.MaxHp * 0.05);
            return damageToDealt <= damage;
        }
    }
}