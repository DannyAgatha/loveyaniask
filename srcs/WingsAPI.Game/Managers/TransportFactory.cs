// NosEmu
// 


using System.Threading;

namespace WingsEmu.Game.Managers;

public class TransportFactory
{
    private static TransportFactory _instance;
    private long _lastTransportId = 100000;

    private TransportFactory()
    {
        // do nothing
    }

    public static TransportFactory Instance => _instance ??= new TransportFactory();

    public long GenerateTransportId()
    {
        Interlocked.Increment(ref _lastTransportId);

        if (_lastTransportId >= long.MaxValue)
        {
            _lastTransportId = 0;
        }

        return _lastTransportId;
    }
}