// NosEmu
// 


using System;

namespace PhoenixLib.Logging
{
    public interface ILogger
    {
        void Debug(string msg);
        void Debug(string msg, Exception ex);
        void DebugFormat(string msg, params object[] objs);

        void Info(string msg);
        void Info(string msg, Exception ex);
        void InfoFormat(string msg, params object[] objs);

        void Warn(string msg);
        void Warn(string msg, Exception ex);
        void WarnFormat(string msg, params object[] objs);

        void Error(string msg, Exception ex);
        void ErrorFormat(string msg, Exception ex, params object[] objs);

        void Fatal(string msg, Exception ex);
    }
}