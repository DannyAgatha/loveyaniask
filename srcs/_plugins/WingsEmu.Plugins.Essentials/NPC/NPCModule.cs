using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Qmmands;
using WingsAPI.Game.Extensions.ItemExtension.Inventory;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game._enum;
using WingsEmu.Game._i18n;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Entities.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums.Chat;

namespace WingsEmu.Plugins.Essentials.NPC;

[Name("NPC")]
[Description("Module related to NPC commands.")]
[RequireAuthority(AuthorityType.GA)]
public class NPCModule : SaltyModuleBase
{
    private readonly IBuffFactory _buffFactory;
    private readonly IEnumerable<BuffPack> _buffPackConfiguration;
    private readonly ICardsManager _cardManager;
    private readonly IGameLanguageService _gameLanguage;
    private readonly INpcEntityFactory _npcEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;

    public NPCModule(INpcMonsterManager npcMonsterManager, IEnumerable<BuffPack> buffPackConfiguration, IGameLanguageService gameLanguage, ICardsManager cardManager, IBuffFactory buffFactory,
        INpcEntityFactory npcEntityFactory)
    {
        _npcMonsterManager = npcMonsterManager;
        _buffPackConfiguration = buffPackConfiguration;
        _gameLanguage = gameLanguage;
        _cardManager = cardManager;
        _buffFactory = buffFactory;
        _npcEntityFactory = npcEntityFactory;
    }

    [Command("buffpack", "buybuffpack")]
    [Description("List buff packs")]
    public async Task<SaltyCommandResult> BuffPackList()
    {
        IClientSession session = Context.Player;

        IEnumerable<BuffPack> kits = _buffPackConfiguration;

        session.SendChatMessage("[========= BUFF PACKS =========]", ChatMessageColorType.Yellow);
        foreach (BuffPack kit in kits)
        {
            string kitDetails = $"[{kit.Name}]: {kit.Price.ToString()} golds";
            session.SendChatMessage(kitDetails, ChatMessageColorType.Yellow);
            foreach (BuffPackElement buffPackElement in kit.Buffs)
            {
                string buffName = _gameLanguage.GetLanguage(GameDataType.Card, _cardManager.GetCardByCardId(buffPackElement.CardId).Name, session.UserLanguage);
                session.SendChatMessage($"{new string('-', kit.Name.Length + 2)}: {buffPackElement.Duration} {buffName} ", ChatMessageColorType.Yellow);
            }
        }

        session.SendChatMessage("[========================]", ChatMessageColorType.Yellow);
        return new SaltyCommandResult(true);
    }

    [Command("buffpack", "buybuffpack")]
    [Description("Buy a buff pack")]
    public async Task<SaltyCommandResult> BuffPackBuy([NotNull] string packName)
    {
        IClientSession session = Context.Player;

        BuffPack kit = _buffPackConfiguration.FirstOrDefault(s => s.Name == packName);

        if (kit == null)
        {
            session.SendErrorChatMessage($"Kit: {packName}");
            return new SaltyCommandResult(false);
        }

        if (!session.HasEnoughGold(kit.Price))
        {
            session.SendErrorChatMessage($"Not enough gold: {packName}:{kit.Price.ToString()}");
            return new SaltyCommandResult(false);
        }

        session.PlayerEntity.Gold -= kit.Price;
        session.RefreshGold();
        var buffs = new List<Buff>();
        foreach (BuffPackElement buff in kit.Buffs)
        {
            buffs.Add(_buffFactory.CreateBuff(buff.CardId, session.PlayerEntity, buff.Level, buff.Duration, buff.KeepOnDeath ? BuffFlag.BIG : BuffFlag.NORMAL));
        }

        session.BroadcastEffectInRange(54);
        session.BroadcastEffectInRange(55);
        session.BroadcastEffectInRange(56);
        session.BroadcastSoundInRange(1517);

        await session.PlayerEntity.AddBuffAsync(buffs.ToArray());

        return new SaltyCommandResult(true);
    }

    [Command("npcadd")]
    [Description("Add NPC")]
    public async Task<SaltyCommandResult> NpcAddAsync(
        [Description("NPC VNum")] short vnum)
    {
        IClientSession session = Context.Player;
        INpcEntity monster = _npcEntityFactory.CreateNpc(vnum, session.CurrentMapInstance);
        if (monster == null)
        {
            return new SaltyCommandResult(false, "Monster doesn't exist.");
        }

        await monster.EmitEventAsync(new MapJoinNpcEntityEvent(monster, session.PlayerEntity.PositionX, session.PlayerEntity.PositionY));
        return new SaltyCommandResult(true, "NPC has been created.");
    }
}