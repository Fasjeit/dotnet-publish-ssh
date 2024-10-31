using System;

namespace DotnetPublishSsh
{
    internal static class Logger
    {
        public static LogLevel Enable { get; set; } = LogLevel.Info;

        public static void WriteInfoLine()
        {
            Logger.WriteInfoLine(string.Empty);
        }

        public static void WriteInfoLine(string str)
        {
            if (Logger.Enable.HasFlag(LogLevel.Info)) { Console.WriteLine(str); }
        }

        public static void WriteErrorLine(string str)
        {
            if (Logger.Enable.HasFlag(LogLevel.Error)) { Console.WriteLine(str); }
        }
    }

    [Flags]
    internal enum LogLevel
    {
        NoLogs = 0,
        Error = 1,
        Info = 1<<1 | LogLevel.Error,
    }
}
