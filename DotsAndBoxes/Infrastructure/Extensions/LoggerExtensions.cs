using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace DotsAndBoxes;

public static class LoggerExtensions
{
    public static void LogWithCallerInfo(this ILogger logger,
                                         LogLevel logLevel,
                                         string message,
                                         Exception exception = null,
                                         [CallerFilePath] string filePath = "",
                                         [CallerMemberName] string memberName = "",
                                         [CallerLineNumber] int lineNumber = 0)
    {
        var className = System.IO.Path.GetFileNameWithoutExtension(filePath);
        var callerInfo = $"[{className}.{memberName}:{lineNumber}]";
        var formattedMessage = $"{callerInfo} {message}";
        
        if (exception == null)
        {
            logger.Log(logLevel, "{message}", formattedMessage);
        }
        else
        {
            logger.Log(logLevel, exception, "{message}", formattedMessage);
        }
    }
}