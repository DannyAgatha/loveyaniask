using System.Linq;
using System.Threading.Tasks;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game.Configurations;
using WingsEmu.Game.Core.SkillHandling;
using WingsEmu.Game.Core.SkillHandling.Event;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Packets.Enums;
using WingsEmu.Packets.Enums.Chat;

namespace NosEmu.Plugins.BasicImplementations.Skills;

public class ResetLevelSkillHandler : ISkillHandler
{
    private readonly PetMaxLevelConfiguration _configuration;

    public ResetLevelSkillHandler(PetMaxLevelConfiguration configuration)
    {
        _configuration = configuration;
    }

    public long[] SkillId => new long[] { 1795 };

    public async Task ExecuteAsync(IClientSession session, SkillEvent e)
    {
        if (e.Target is not IMateEntity mateEntity)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }
        
        if (mateEntity.MateType == MateType.Partner)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        if (!session.PlayerEntity.MapInstance.HasMapFlag(MapFlags.IS_MINILAND_MAP))
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        MaxPetLevelConfiguration infos = _configuration.Configurations.FirstOrDefault(s => s.Stars == mateEntity.Stars);

        if (infos == null)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        if (mateEntity.HeroLevel < infos.MaxLevel)
        {
            e.SkillInfo.IsCanceled = true;
            return;
        }

        session.SendQnaiPacket($"guri 452 {mateEntity.PetSlot} {mateEntity.MonsterVNum} {mateEntity.Stars}", Game18NConstString.ConfirmTrainingReset);
    }
}