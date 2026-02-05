using System.Threading;
using System.Threading.Tasks;
using PhoenixLib.Events;
using WingsEmu.Game.Families.Event;
using WingsEmu.Game.Features;

namespace Plugin.FamilyImpl
{
    public class FamilyWarehouseCloseEventHandler : IAsyncEventProcessor<FamilyWarehouseCloseEvent>
    {
        private readonly IGameFeatureToggleManager _gameFeatureToggleManager;

        public FamilyWarehouseCloseEventHandler(IGameFeatureToggleManager gameFeatureToggleManager) => _gameFeatureToggleManager = gameFeatureToggleManager;

        public async Task HandleAsync(FamilyWarehouseCloseEvent e, CancellationToken cancellation)
        {
            e.Sender.PlayerEntity.IsFamilyWarehouseOpen = false;
            e.Sender.PlayerEntity.IsFamilyWarehouseLogsOpen = false;
        }
    }
}