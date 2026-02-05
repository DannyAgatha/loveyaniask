using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace PhoenixLib.Logging
{
    public static class Log
    {
        private static readonly ILogger _logger = new SerilogLogger();

        public static void Debug(string msg)
        {
            _logger.Debug(msg);
        }

        public static void Debug(string msg, Exception ex)
        {
            _logger.Debug(msg, ex);
        }

        public static void Debug(string msg, params object[] objs)
        {
            _logger.DebugFormat(msg, objs);
        }

        public static void Info(string msg)
        {
            _logger.Info(msg);
        }

        public static void Info(string msg, Exception ex)
        {
            _logger.Info(msg, ex);
        }

        public static void Info(string msg, params object[] objs)
        {
            _logger.InfoFormat(msg, objs);
        }

        public static void Warn(string msg)
        {
            _logger.Warn(msg);
        }


        public static void Warn(string msg, Exception ex)
        {
            _logger.Warn(msg, ex);
        }

        public static void WarnFormat(string msg, params object[] objs)
        {
            _logger.WarnFormat(msg, objs);
        }

        public static void Error(string msg, Exception ex)
        {
            _logger.Error(msg, ex);
            DiscordLogger.SendErrorToDiscordAsync(msg, ex).Wait();
        }

        public static void ErrorFormat(string msg, Exception ex, params object[] objs)
        {
            _logger.ErrorFormat(msg, ex, objs);
        }

        public static void Fatal(string msg, Exception ex)
        {
            _logger.Fatal(msg, ex);
        }
    }
}