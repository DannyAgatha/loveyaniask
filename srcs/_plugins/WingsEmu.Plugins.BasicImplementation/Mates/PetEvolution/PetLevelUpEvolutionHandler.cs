using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Configurations.PetEvolution;
using WingsEmu.Game.Entities;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Helpers.Damages;
using WingsEmu.Game.Managers.StaticData;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Mates.Events;
using WingsEmu.Game.Mates.PetEvolution;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Npcs;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Mates.PetEvolution;

public class PetLevelUpEvolutionHandler : IAsyncEventProcessor<PetLevelUpEvolutionEvent>
{
    private readonly PetEvolutionConfiguration _config;
    private readonly IMateEntityFactory _mateFactory;
    private readonly IGameLanguageService _language;
    private readonly ISpPartnerConfiguration _spPartner;
    private readonly INpcMonsterManager _npcMonsterManager;

    public PetLevelUpEvolutionHandler(PetEvolutionConfiguration config,
        IMateEntityFactory mateFactory,
        IGameLanguageService language,
        ISpPartnerConfiguration spPartner,
        INpcMonsterManager npcMonsterManager)
    {
        _config = config;
        _mateFactory = mateFactory;
        _language = language;
        _spPartner = spPartner;
        _npcMonsterManager = npcMonsterManager;
    }

    public async Task HandleAsync(PetLevelUpEvolutionEvent e, CancellationToken cancellation)
    {
        PetEvolutionDefinition match = _config.PetEvolutions.FirstOrDefault(x =>
            x.OriginalMonsterVnum == e.NosMateMonsterVnum &&
            x.LevelType == e.LevelType &&
            x.EvolveAtLevel == e.Level);

        if (match is null)
        {
            return;
        }

        IClientSession session = e.Sender;
        IPlayerEntity player = session.PlayerEntity;

        IMateEntity oldPet = player.MateComponent.GetMate(x =>
            x.MateType == MateType.Pet && x.NpcMonsterVNum == e.NosMateMonsterVnum);

        if (oldPet is null)
        {
            return;
        }
        
        if (oldPet.IsTeamMember)
        {
            await session.EmitEventAsync(new MateLeaveTeamEvent { MateEntity = oldPet });
        }

        await session.EmitEventAsync(new MateRemoveEvent { MateEntity = oldPet });

        IMonsterData monsterData = _npcMonsterManager.GetNpc((short)match.EvolvedMonsterVnum);
        if (monsterData is null)
        {
            return;
        }

        MonsterData wrappedData = new(monsterData);

        IMateEntity newPet = _mateFactory.CreateMateEntity(
            player,
            wrappedData,
            MateType.Pet,
            oldPet.Level,
            oldPet.HeroLevel,
            [],
            oldPet.IsLimited
        );

        newPet.Stars = oldPet.Stars;

        await session.EmitEventAsync(new MateInitializeEvent
        {
            MateEntity = newPet
        });

        await session.EmitEventAsync(new MateJoinTeamEvent
        {
            MateEntity = newPet,
            IsNewCreated = true
        });
        
        string evolvedName = _language.GetNpcMonsterName(wrappedData, session);
        string originalName = _language.GetNpcMonsterName(_npcMonsterManager.GetNpc((short)match.OriginalMonsterVnum), session);

        string message = e.ItemVnum.HasValue
            ? _language.GetLanguageFormat(GameDialogKey.PET_EVOLVED_WITH_ITEM, session.UserLanguage,
                originalName, evolvedName, e.ItemVnum.Value)
            : _language.GetLanguageFormat(GameDialogKey.PET_EVOLVED, session.UserLanguage,
                originalName, evolvedName);

        session.SendInfo(message);
    }
}
