using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Cosmobot
{
    // ui only functionality 
    public class ProgrammingUiLogEntry : MonoBehaviour
    {
        [SerializeField]
        private Image rootBackground;
        [SerializeField]
        private TMP_Text dateText;
        [SerializeField]
        private TMP_Text logLevelText;
        [SerializeField]
        private TMP_Text logText;

        public LogEntry LogEntry { get; private set; }

        public void SetLog(LogEntry logEntry)
        {
            LogEntry = logEntry;
            dateText.text = logEntry.GetIsoTime();
            logLevelText.text = logEntry.level.GetName();
            logText.text = logEntry.message;
            if (rootBackground)
                rootBackground.color = LogToColor(logEntry.level);
        }

        public void SetLog(long timestamp, LogLevel logLevel, string logMessage)
        {
            SetLog(new LogEntry(timestamp, logLevel, logMessage));
        }

        private Color LogToColor(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Info: return new Color(1, 1, 1, 0.2f);
                case LogLevel.Warn: return new Color(1, 1, 0, 0.2f);
                case LogLevel.Error: return new Color(1, 0, 0, 0.2f);
                default: return new Color(1, 1, 1, 0.2f);
            }
        }
    }
}
