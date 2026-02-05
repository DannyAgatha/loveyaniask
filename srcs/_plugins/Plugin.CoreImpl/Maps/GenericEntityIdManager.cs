using System.Threading;
using WingsEmu.Game.Maps;

namespace Plugin.CoreImpl.Maps
{
    public class GenericEntityIdManager : IEntityIdManager
    {
        private readonly ReaderWriterLockSlim _lock = new();

        private int _baseOffset = 25000;

        public int GenerateEntityId()
        {
            _lock.EnterWriteLock();
            try
            {
                _baseOffset++;
                return _baseOffset;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}