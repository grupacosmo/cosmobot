using Cosmobot.Utils;
using UnityEngine;

namespace Cosmobot
{
    [DefaultExecutionOrder(ExecutionOrder.RobotLogger)]
    public class RobotLoggerSingletonSystem : SingletonSystem<RobotLoggerSingletonSystem>
    {
        public bool robotLoggingToUnityConsole = true;
        public bool robotDebugStackTrace = true;

        public static bool RobotLoggingToUnityConsole => Instance?.robotLoggingToUnityConsole ?? true;
        public static bool RobotDebugStackTrace => Instance?.robotDebugStackTrace ?? true;

        protected override void SystemAwake()
        {
            RobotLogger.FullCleanup();
        }

        private void Update()
        {
            RobotLogger.Pump();
        }

        protected override void SystemOnDestroy()
        {
            RobotLogger.FullCleanup();
        }
    }
}
