using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game._enum;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Battle;
using WingsEmu.Game.Buffs;
using WingsEmu.Game.Buffs.Events;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Mates;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Skills;
using WingsEmu.Packets.Enums;

namespace NosEmu.Plugins.BasicImplementations.Event.Buffs;

public class BuffPartnerCheckEventHandler : IAsyncEventProcessor<BuffPartnerCheckEvent>
{
    private readonly IBuffFactory _buffFactory;
    private readonly IMateBuffConfigsContainer _mateBuffConfigsContainer;
    private readonly ISpPartnerConfiguration _spPartner;

    public BuffPartnerCheckEventHandler(ISpPartnerConfiguration spPartner, IBuffFactory buffFactory, IMateBuffConfigsContainer mateBuffConfigsContainer)
    {
        _spPartner = spPartner;
        _buffFactory = buffFactory;
        _mateBuffConfigsContainer = mateBuffConfigsContainer;
    }

    public async Task HandleAsync(BuffPartnerCheckEvent e, CancellationToken cancellation)
    {
        IClientSession session = e.Sender;
        IMateEntity matePet = session.PlayerEntity.MateComponent.GetTeamMember(x => x.MateType == MateType.Pet);
        if (matePet != null)
        {
            await session.AddPetBuff(matePet, _mateBuffConfigsContainer, _buffFactory);
        }

        IMateEntity mateEntity = session.PlayerEntity.MateComponent.GetTeamMember(x => x.MateType == MateType.Partner);
        if (mateEntity == null)
        {
            return;
        }

        if (!mateEntity.IsUsingSp)
        {
            return;
        }

        SpPartnerInfo spInfo = _spPartner.GetByMorph(mateEntity.Specialist.GameItem.Morph);

        if (spInfo == null)
        {
            return;
        }

        if (spInfo.BuffId == 0)
        {
            return;
        }

        if (session.PlayerEntity.BuffComponent.HasBuff(spInfo.BuffId))
        {
            return;
        }

        int sum = 0;
        foreach (IBattleEntitySkill skill in mateEntity.Skills)
        {
            if (!(skill is PartnerSkill partnerSkill))
            {
                continue;
            }

            sum += partnerSkill.Rank;
        }

        if (sum == 0)
        {
            sum = 1;
        }
        else
        {
            sum /= 3;
        }

        if (sum < 1)
        {
            sum = 1;
        }

        await session.PlayerEntity.AddBuffAsync(_buffFactory.CreateBuff(spInfo.BuffId + (sum - 1), session.PlayerEntity, BuffFlag.PARTNER));
    }
}