using System.Threading;
using WingsEmu.Game.Mates;

namespace NosEmu.Plugins.BasicImplementations.Factories;

public class MateTransportFactory : IMateTransportFactory
{
    private readonly ReaderWriterLockSlim _lock = new();

    private int _mateId = 1_000_000;

    public int GenerateTransportId()
    {
        _lock.EnterWriteLock();
        try
        {
            _mateId += 1;

            return _mateId;
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }
}