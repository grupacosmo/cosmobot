using System;
using UnityEngine;

namespace Cosmobot
{
    public class RobotDebugDummyLogReceiver : MonoBehaviour
    {
        private void OnEnable()
        {
            RobotLogger.AddAllLogEventHandler(OnLog);
        }

        private void OnDisable()
        {
            RobotLogger.RemoveAllLogEventHandler(OnLog);
        }

        private void OnLog(ProgrammableData data, LogEntry logEntry)
        {
            Debug.Log($"[Dummy Logger] [{data.Name}] {logEntry.GetIsoTime()} [{logEntry.level.GetConstSizeName()}]:  {logEntry.message}");
        }
    }
}
