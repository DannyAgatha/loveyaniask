// NosEmu
// 


using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace PhoenixLib.Logging
{
    public static class LoggingExtensions
    {
        public static void AddPhoenixLogging(this IServiceCollection services, LogLevel logLevel = LogLevel.Warning)
        {
            services.AddLogging(builder =>
            {
                builder.AddPhoenixLogging();
                builder.AddFilter("Microsoft", LogLevel.Warning);
                builder.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
            });
        }

        public static void AddPhoenixLogging(this ILoggingBuilder logging, LogLevel logLevel = LogLevel.Debug)
        {
            logging.AddSerilog(SerilogLogger.CreateLogger(logLevel), true);
        }
    }
}