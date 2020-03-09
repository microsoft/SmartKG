using System;

namespace SmartKG.Common.Logger
{
    public interface KGBotLogHandler
    {
        void LogInformation(Serilog.ILogger log, string title, string content);
        
        void LogError(Serilog.ILogger log, Exception e);
    }
}
