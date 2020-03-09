using System;
using System.Runtime.CompilerServices;
using Serilog;

namespace SmartKG.Common.Logger
{
    public static class LoggerExtensions
    {
        public static ILogger Here(this ILogger logger,  
            [ContextStatic] string sourceContext = "",
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            return logger.ForContext("MemberName", memberName)
                .ForContext("FilePath", sourceFilePath)
                .ForContext("LineNumber", sourceLineNumber);
        }
    }
}
