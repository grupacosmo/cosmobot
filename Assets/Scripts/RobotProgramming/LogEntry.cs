using System;
using UnityEngine;

namespace Cosmobot
{
    public struct LogEntry
    {
        public long timestamp;
        public LogLevel level;
        public string message;

        public LogEntry(long timestamp, LogLevel level, string message)
        {
            this.timestamp = timestamp;
            this.level = level;
            this.message = message;
        }

        public string GetIsoTime()
        {
            return DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime().ToString("HH:mm:ss");
        }
    }
}
