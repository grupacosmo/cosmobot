namespace Cosmobot
{
    public enum LogLevel
    {
        Info = 0,
        Warn = 1,
        Error = 2
    }

    public static class LogLevelExtension
    {
        public static string GetName(this LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info: return "INFO";
                case LogLevel.Warn: return "WARN";
                case LogLevel.Error: return "ERROR";
                default: return "UNKNOWN";
            }
        }

        public static string GetConstSizeName(this LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Info: return "INFO ";
                case LogLevel.Warn: return "WARN ";
                case LogLevel.Error: return "ERROR";
                default: return " N/A ";
            }
        }

        public static bool ShouldLog(this LogLevel level, LogLevel other)
        {
            return level >= other;
        }
    }
}
