using System.Collections.Generic;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Game;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game._ItemUsage.Event;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Monster.Event;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.ItemUsage.Main.Special;

public class ChristmasBellHandler : IItemUsageByVnumHandler
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly IAsyncEventPipeline _eventPipeline;
    private readonly IRandomGenerator _randomGenerator;
    private readonly IGameLanguageService _languageService;
    private readonly ISpPartnerConfiguration _spPartner;
    private readonly List<int> catchableMonsters = new() { 1426, 1427, 1425, 1434, 1414, 1415, 1416, 1417, 1418, 1419, 1420, 1421, 1422, 1423, 1424 };

    public ChristmasBellHandler(IGameLanguageService gameLanguageService, INpcMonsterManager npcMonsterManager, IMateEntityFactory mateEntityFactory,
        IAsyncEventPipeline eventPipeline, IRandomGenerator randomGenerator, IGameLanguageService languageService, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _gameLanguage = gameLanguageService;
        _npcMonsterManager = npcMonsterManager;
        _mateEntityFactory = mateEntityFactory;
        _eventPipeline = eventPipeline;
        _randomGenerator = randomGenerator;
        _languageService = languageService;
        _spPartner = spPartnerConfiguration;
    }

    public long[] Vnums => new long[] { 1125, 1126, 1144 };

    public async Task HandleAsync(IClientSession session, InventoryUseItemEvent e)
    {
        if (!int.TryParse(e.Packet[3], out int x1))
        {
            return;
        }

        IBattleEntity battleEntity = session.CurrentMapInstance.GetBattleEntity(VisualType.Monster, x1);

        if (battleEntity == null)
        {
            return;
        }

        if (battleEntity is not IMonsterEntity monsterEntity)
        {
            return;
        }

        if (!monsterEntity.VesselChristmasMonster)
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.ITEM_WORKING_ONLY_ON_CHRISTMAS_VESSEL_MONSTERS, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        if (!catchableMonsters.Contains(monsterEntity.MonsterVNum))
        {
            session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_IMPOSSIBLE, session.UserLanguage), MsgMessageType.Middle);
            return;
        }

        int randomNumber = _randomGenerator.RandomNumber();
        bool captureSucceed = false;

        switch (e.Item.ItemInstance.ItemVNum)
        {
            case (short)ItemVnums.GOLDEN_BELL:
                session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.SKILL_SHOUTMESSAGE_CAUGHT_PET, session.UserLanguage,
                    _gameLanguage.GetLanguage(GameDataType.NpcMonster, monsterEntity.Name, session.UserLanguage)), MsgMessageType.Middle);
                captureSucceed = true;
                break;
            case (short)ItemVnums.SILVER_BELL:
                if (randomNumber > 30)
                {
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_FAILED, session.UserLanguage), MsgMessageType.Middle);
                    captureSucceed = false;
                }
                else
                {
                    captureSucceed = true;
                }
                break;
            case (short)ItemVnums.BRONZE_BELL:
                if (randomNumber > 10)
                {
                    session.SendMsg(_gameLanguage.GetLanguage(GameDialogKey.SKILL_SHOUTMESSAGE_CAPTURE_FAILED, session.UserLanguage), MsgMessageType.Middle);
                    captureSucceed = false;
                }
                else
                {
                    session.SendMsg(_gameLanguage.GetLanguageFormat(GameDialogKey.SKILL_SHOUTMESSAGE_CAUGHT_PET, session.UserLanguage,
                        _gameLanguage.GetLanguage(GameDataType.NpcMonster, monsterEntity.Name, session.UserLanguage)), MsgMessageType.Middle);
                    captureSucceed = true;
                }
                break;
        }

        if (captureSucceed)
        {
            IMonsterData data = _npcMonsterManager.GetNpc(monsterEntity.MonsterVNum);

            var npcMate = new MonsterData(data);

            IMateEntity mateEntity = _mateEntityFactory.CreateMateEntity(session.PlayerEntity, npcMate, MateType.Pet, 1, 0, new List<int>());

            await session.EmitEventAsync(new MateInitializeEvent
            {
                MateEntity = mateEntity
            });

            session.CurrentMapInstance.AddMate(mateEntity);
            session.CurrentMapInstance.Broadcast(s => mateEntity.GenerateIn(_languageService, s.UserLanguage, _spPartner));
            session.SendCondMate(mateEntity);
            string mateName = _languageService.GetLanguage(GameDataType.NpcMonster, mateEntity.Name, session.UserLanguage);
            GameDialogKey key = mateEntity.MateType == MateType.Pet ? GameDialogKey.PET_CHATMESSAGE_BEAD_EXTRACT : GameDialogKey.PARTNER_CHATMESSAGE_BEAD_EXTRACT;
            session.SendChatMessage(_languageService.GetLanguageFormat(key, session.UserLanguage, mateName), ChatMessageColorType.Green);

            await _eventPipeline.ProcessEventAsync(new MonsterDeathEvent(monsterEntity));
            monsterEntity.BroadcastDie();
            monsterEntity.MapInstance.RemoveMonster(monsterEntity);
        }

        await session.RemoveItemFromInventory(item: e.Item);
    }
}