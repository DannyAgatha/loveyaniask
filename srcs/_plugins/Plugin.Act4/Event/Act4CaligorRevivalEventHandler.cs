using PhoenixLib.Events;
using System.Threading;
using System.Threading.Tasks;
using WingsAPI.Game.Extensions.Groups;
using WingsEmu.DTOs.Maps;
using WingsEmu.Game;
using WingsEmu.Game._i18n;
using WingsEmu.Game._ItemUsage.Configuration;
using WingsEmu.Game.Act4;
using WingsEmu.Game.Characters;
using WingsEmu.Game.Extensions;
using WingsEmu.Game.Maps;
using WingsEmu.Game.Networking;
using WingsEmu.Game.Revival;
using WingsEmu.Packets.Enums;

namespace Plugin.Act4.Event
{
    public class Act4CaligorRevivalEventHandler : IAsyncEventProcessor<RevivalReviveEvent>
    {
        private readonly IGameLanguageService _languageService;
        private readonly ISpPartnerConfiguration _spPartner;
        private readonly IRandomGenerator _randomGenerator;
        private readonly IAct4CaligorManager _act4CaligorManager;

        public Act4CaligorRevivalEventHandler(IGameLanguageService languageService, ISpPartnerConfiguration spPartner,
            IRandomGenerator randomGenerator, IAct4CaligorManager act4CaligorManager)
        {
            _languageService = languageService;
            _spPartner = spPartner;
            _randomGenerator = randomGenerator;
            _act4CaligorManager = act4CaligorManager;
        }

        public async Task HandleAsync(RevivalReviveEvent e, CancellationToken cancellation)
        {
            IClientSession sender = e.Sender;
            IPlayerEntity character = e.Sender.PlayerEntity;

            if (!sender.CurrentMapInstance.HasMapFlag(MapFlags.ACT_4))
            {
                return;
            }

            if (sender.CurrentMapInstance.MapInstanceType != MapInstanceType.Caligor)
            {
                return;
            }

            character.DisableRevival();

            if (e.RevivalType == RevivalType.DontPayRevival && e.Forced != ForcedType.HolyRevival && !_act4CaligorManager.CaligorActive)
            {
                e.Sender.UpdateVisibility();
                e.Sender.PlayerEntity.Hp = 1;
                e.Sender.PlayerEntity.Mp = 1;
                await e.Sender.PlayerEntity.Restore(restoreHealth: false, restoreMana: false, restoreMates: false);
                short x = (short)(39 + _randomGenerator.RandomNumber(-2, 3));
                short y = (short)(42 + _randomGenerator.RandomNumber(-2, 3));
                if (e.Sender.PlayerEntity.Faction == FactionType.Angel)
                {
                    e.Sender.ChangeMap(130, x, y);
                }
                else if (e.Sender.PlayerEntity.Faction == FactionType.Demon)
                {
                    e.Sender.ChangeMap(131, x, y);
                }
                e.Sender.SendBuffsPacket();
                return;
            }

            e.Sender.UpdateVisibility();
            await e.Sender.PlayerEntity.Restore(restoreMates: false);
            _act4CaligorManager.TeleportPlayerToCaligorCamp(e.Sender.PlayerEntity);
            e.Sender.BroadcastRevive();
            e.Sender.BroadcastInTeamMembers(_languageService, _spPartner);
            e.Sender.RefreshParty(_spPartner);
            await e.Sender.CheckPartnerBuff();
            e.Sender.SendBuffsPacket();
        }
    }
}