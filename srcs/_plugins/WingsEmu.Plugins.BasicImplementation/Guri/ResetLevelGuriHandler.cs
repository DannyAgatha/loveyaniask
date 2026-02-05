using System.Linq;
using System.Threading.Tasks;
using WingsAPI.Packets.Enums.Chat;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game._Guri;
using WingsEmu.Game._Guri.Event;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Extensions.Mates;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Guri;

public class ResetLevelGuriHandler : IGuriHandler
{
    private readonly PetMaxLevelConfiguration _config;
    private readonly IGameLanguageService _gameLanguageService;
    private readonly ISpPartnerConfiguration _spPartnerConfiguration;

    public ResetLevelGuriHandler(PetMaxLevelConfiguration config, IGameLanguageService gameLanguageService, ISpPartnerConfiguration spPartnerConfiguration)
    {
        _config = config;
        _gameLanguageService = gameLanguageService;
        _spPartnerConfiguration = spPartnerConfiguration;
    }

    public long GuriEffectId => 452;

    public async Task ExecuteAsync(IClientSession session, GuriEvent e)
    {
        if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_MINILAND_MAP))
        {
            return;
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetMates().FirstOrDefault(s => s.PetSlot == e.Data);

        if (mateEntity == null)
        {
            return;
        }

        if (mateEntity.MateType == MateType.Partner)
        {
            return;
        }

        MaxPetLevelConfiguration infos = _config.Configurations.FirstOrDefault(s => s.Stars == mateEntity.Stars);

        if (infos == null)
        {
            return;
        }

        if (mateEntity.HeroLevel < infos.MaxLevel)
        {
            return;
        }

        mateEntity.HeroLevel = 0;
        foreach (int skillId in mateEntity.TrainerSkills)
        {
            mateEntity.Skills.RemoveAll(ski => ski.Skill.Id == skillId);
        }
        mateEntity.TrainerSkills.Clear();
        session.SendPetInfo(mateEntity, _gameLanguageService);
        if (mateEntity.IsTeamMember)
        {
            session.SendMateSkillPacket(mateEntity);
        }
        session.Broadcast(mateEntity.GenerateIn(_gameLanguageService, session.UserLanguage, _spPartnerConfiguration));
        session.SendStpS(mateEntity);
        session.SendMsgi(MessageType.Default, Game18NConstString.TrainingLevelResetTo, 4, mateEntity.HeroLevel);
    }
}