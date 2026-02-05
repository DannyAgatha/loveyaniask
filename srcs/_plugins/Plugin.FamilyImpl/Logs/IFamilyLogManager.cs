using System.Collections.Generic;
using WingsAPI.Data.Families;

namespace Plugin.FamilyImpl.Logs
{
    public interface IFamilyLogManager
    {
        void SaveLogToBuffer(FamilyLogDto log);
        IReadOnlyList<FamilyLogDto> GetFamilyLogsInBuffer();
    }
}