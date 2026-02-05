using System.Threading.Tasks;

namespace WingsEmu.Game.Entities;

public interface IGenericEventEmitter<in TEventType>
{
    public Task EmitEventAsync<T>(T eventArgs) where T : TEventType;
    public void EmitEvent<T>(T eventArgs) where T : TEventType;
}