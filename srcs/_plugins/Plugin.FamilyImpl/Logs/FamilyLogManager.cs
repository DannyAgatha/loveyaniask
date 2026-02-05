using System.Collections.Concurrent;
using System.Collections.Generic;
using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.Logs
{
    public class FamilyLogManager : IFamilyLogManager
    {
        private readonly ConcurrentQueue<FamilyLogDto> _familyBufferForLogs = new();

        public void SaveLogToBuffer(FamilyLogDto log)
        {
            _familyBufferForLogs.Enqueue(log);
        }

        public IReadOnlyList<FamilyLogDto> GetFamilyLogsInBuffer()
        {
            if (_familyBufferForLogs.IsEmpty)
            {
                return null;
            }

            var list = new List<FamilyLogDto>();
            while (_familyBufferForLogs.TryDequeue(out FamilyLogDto log))
            {
                list.Add(log);
            }

            return list;
        }
    }
}