using System.Collections.Generic;
using System.Threading.Tasks;
using Qmmands;
using WingsEmu.Commands.Checks;
using WingsEmu.Commands.Entities;
using WingsEmu.DTOs.Account;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs;
using WingsEmu.Packets.Enums;

namespace WingsEmu.Plugins.Essentials.NPC;

[Name("Mate Creation")]
[Description("Module related to mate creation commands.")]
[RequireAuthority(AuthorityType.GM)]
public class MateCreationModule : SaltyModuleBase
{
    private readonly IGameLanguageService _gameLanguage;
    private readonly IMateEntityFactory _mateEntityFactory;
    private readonly INpcMonsterManager _npcMonsterManager;
    private readonly IRandomGenerator _randomGenerator;
    private readonly ISpPartnerConfiguration _spPartner;

    public MateCreationModule(INpcMonsterManager npcMonsterManager, IGameLanguageService gameLanguage, ISpPartnerConfiguration spPartner, IRandomGenerator randomGenerator,
        IMateEntityFactory mateEntityFactory)
    {
        _npcMonsterManager = npcMonsterManager;
        _gameLanguage = gameLanguage;
        _spPartner = spPartner;
        _randomGenerator = randomGenerator;
        _mateEntityFactory = mateEntityFactory;
    }

    [Command("addmate")]
    [Description("Add mate")]
    public async Task<SaltyCommandResult> AddMateAsync(
        [Description("Mob VNUM")] short vnum,
        [Description("Level")] byte level, byte? attack = null, byte? defence = null)
    {
        IClientSession session = Context.Player;
        IMonsterData data = _npcMonsterManager.GetNpc(vnum);

        if (data == null)
        {
            return new SaltyCommandResult(false, "Monster doesn't exist!");
        }

        var npcMate = new MonsterData(data);

        IMateEntity mateEntity = _mateEntityFactory.CreateMateEntity(session.PlayerEntity, npcMate, MateType.Pet, level, 0, new List<int>());
        mateEntity.Attack = attack ?? 0;
        mateEntity.Defence = defence ?? 0;

        await session.EmitEventAsync(new MateInitializeEvent
        {
            MateEntity = mateEntity
        });

        await session.EmitEventAsync(new MateJoinTeamEvent { MateEntity = mateEntity });
        return new SaltyCommandResult(true, "NosMate has been added.");
    }

    [Command("addpartner")]
    [Description("Add partner")]
    public async Task<SaltyCommandResult> AddPartnerAsync(
        [Description("Mob VNUM")] short vnum,
        [Description("Level")] byte level)
    {
        IClientSession session = Context.Player;

        IMonsterData data = _npcMonsterManager.GetNpc(vnum);

        if (data == null)
        {
            return new SaltyCommandResult(false, "Monster doesn't exist!");
        }

        var npcPartner = new MonsterData(data);

        IMateEntity partner = _mateEntityFactory.CreateMateEntity(session.PlayerEntity, npcPartner, MateType.Partner, level, 0, new List<int>());
        await session.EmitEventAsync(new MateInitializeEvent
        {
            MateEntity = partner
        });

        await session.EmitEventAsync(new MateJoinTeamEvent { MateEntity = partner });

        return new SaltyCommandResult(true, "Partner has been added.");
    }
}