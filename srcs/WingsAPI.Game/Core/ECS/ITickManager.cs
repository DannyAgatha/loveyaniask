// NosEmu
// 


namespace WingsEmu.Game._ECS;

public interface ITickManager
{
    void AddProcessable(ITickProcessable processable);

    void RemoveProcessable(ITickProcessable processable);

    void Start();
    void Stop();
}