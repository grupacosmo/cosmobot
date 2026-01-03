using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;
using Object = System.Object;

namespace Cosmobot
{
    /// <summary>
    /// Allows all Robots and functions called by robot to use logger and automatically assigns programmable instance to
    /// every log.
    ///
    /// </summary>
    public static class RobotLogger
    {
        private static System.Object lck = new Object(); // for multioperation on logs;
        
        private const int GlobalLogsKey = 0; // Object Instance ID is never 0
        
        private static readonly AsyncLocal<int?> current = new();
        private static readonly ConcurrentDictionary<int, RobotLogs> logs;
        
        private static readonly ConcurrentDictionary<int, Action<ProgrammableData, LogEntry>> logEventHandlers = new();
        
        static RobotLogger()
        {
            logs = new ConcurrentDictionary<int, RobotLogs>();
            logs[GlobalLogsKey] = new RobotLogs(null);
        }

        public static void AddLogEventHandler(Programmable robot, Action<ProgrammableData, LogEntry> handler)
        {
            int robotId = robot.GetInstanceID();
            lock (lck)
            {
                if (logEventHandlers.TryGetValue(robotId, out Action<ProgrammableData, LogEntry> existing))
                {
                    logEventHandlers[robotId] = existing + handler;
                }
                else
                {
                    logEventHandlers[robotId] = handler;
                }
            }
        }

        public static void RemoveLogEventHandler(Programmable robot, Action<ProgrammableData, LogEntry> handler)
        {
            int robotId = robot.GetInstanceID();
            lock (lck)
            {
                if (logEventHandlers.TryGetValue(robotId, out Action<ProgrammableData, LogEntry> existing))
                {
                    existing -= handler;
                    if (existing == null)
                    {
                        logEventHandlers.Remove(robotId, out _);
                    }
                    else
                    {
                        logEventHandlers[robotId] = existing;
                    }
                }
            }
        }
        
        public static void InitCurrent(ProgrammableData robot)
        {
            if (robot == null)
            {
                throw new ArgumentException("robot cant be null and must exist", nameof(robot));
            }
            int robotId = robot.InstanceID;
            
            if (current.Value is not null)
            {
                int currentRobotId = current.Value.Value;
                throw new InvalidOperationException(
                    $"Cannot initialize Logger for '{robot.Name}' ([id: {robotId}]) because the same" +
                    $"async control flow is already initialized for [id: {currentRobotId}]");
            }

            if (logs.ContainsKey(robotId))
            {
                throw new InvalidOperationException($"Logger for {robot.Name}  is already initialized");
            }
            
            current.Value = robotId;
            logs[robotId] = new RobotLogs(robot);
        }

        public static void ClearCurrent()
        {
            int? currentRobotId = current.Value;
            if (currentRobotId == null)
            {
                Debug.Log("No context to clear");
                return;
            }
            logs.TryRemove(currentRobotId.Value, out _);
            lock (lck) // sync to (un)subscribe
            {
                logEventHandlers.TryRemove(currentRobotId.Value, out _);
            }
            current.Value = null;
        }

        public static void ClearGlobalLogs()
        {
            logs[GlobalLogsKey].logs.Clear();
        }
        
        public static void LogInfo(string message)
        {
            Log(LogLevel.Info, message);
        }

        public static void LogWarning(string message)
        {
            Log(LogLevel.Warn, message);
        }

        public static void LogError(string message)
        {
            Log(LogLevel.Error, message);
        }

        public static void Log(LogLevel level, string message)
        {
            int? currentRobotId = current.Value;
            LogEntry logEntry = new LogEntry(level, message);
            if (currentRobotId == null || !logs.ContainsKey(currentRobotId.Value))
            {
                Debug.LogWarning("Missing Robot thread context. Logging to global");
                logs[GlobalLogsKey].Add(logEntry);
                return;
            }

            RobotLogs currentRobotLogs = logs[currentRobotId.Value];
            currentRobotLogs.Add(logEntry);
            
            Action<ProgrammableData, LogEntry> localEvent;
            lock (lck) // sync to (un)subscribe
            {
                logEventHandlers.TryGetValue(currentRobotId.Value, out localEvent);
            }
            
            localEvent?.Invoke(currentRobotLogs.robot, logEntry);
            
#if UNITY_EDITOR
            LogType type;
            switch (logEntry.level)
            {
                case LogLevel.Info: type = LogType.Log; break;
                case LogLevel.Warn: type = LogType.Warning; break;
                case LogLevel.Error: type = LogType.Error; break;
                default: type = LogType.Log; break;
            }
            
            Debug.LogFormat(
                type,
                LogOption.None,
                currentRobotLogs.robot.Unity.ProgrammableComponent, /* should be safe coz its inside Unit's function? */
                "Robot: '{0}' ([id: {1:X8}]) at {2} [{3}]: {4}\n\nRobot Context Debug StackTrace (not real):\n{5}",
                currentRobotLogs.robot.Name,
                currentRobotId,
                logEntry.GetIsoTime(),
                logEntry.level.GetConstSizeName(),
                logEntry.message,
                RobotDebugHelper.GetCurrentRobotContextStackTraceAsString());
#endif
        }

        /// <summary>
        /// Return logs for current control flow. If current control flow is not initialized or was cleared, will return
        /// null.
        ///
        /// Returned collection is bound to concurrent log collection and may grow over time. 
        /// </summary>
        /// <returns>current control flow robot logs or null if not initialized/cleared</returns>
        [CanBeNull]
        public static IReadOnlyCollection<LogEntry> GetCurrentLogs()
        {
            int? currentRobotId = current.Value;
            if (currentRobotId == null) return null;
            return logs[currentRobotId.Value].logs;
        }

        public static bool TryGetCurrentLogs(out IReadOnlyCollection<LogEntry> outLogs)
        {
            int? currentRobotId = current.Value;
            if (currentRobotId == null)
            {
                outLogs = null;
                return false;
            }
            
            bool result = logs.TryGetValue(currentRobotId.Value, out RobotLogs foundRobotLogs);
            outLogs = foundRobotLogs.logs; // cast
            return result;
        }
        
        /// <summary>
        /// Returned collection is bound to concurrent log collection and may grow over time. 
        /// </summary>
        public static IReadOnlyCollection<LogEntry> GetGlobalLogs()
        {
            return logs[GlobalLogsKey].logs;
        }

        public static IReadOnlyCollection<LogEntry> GetRobotLogs(Programmable robot)
        {
            int robotId = robot.GetInstanceID();
            if (logs.TryGetValue(robotId, out RobotLogs foundRobotLogs))
                return foundRobotLogs.logs;
            return Array.Empty<LogEntry>();
        }

        public static bool TryGetRobotLogs(Programmable robot, out IReadOnlyCollection<LogEntry> outLogs)
        {
            int robotId = robot.GetInstanceID();
            bool result = logs.TryGetValue(robotId, out RobotLogs foundRobotLogs);
            outLogs = foundRobotLogs.logs;
            return result;
        }

        /// <summary>
        /// Returns a read-only view of the logs mapped by robot. A `0` key represents global logs. The returned
        /// dictionary reflects the underlying concurrent log collection: keys may be added and removed over time, and
        /// each value collection may grow with new log entries.
        ///
        /// Clone returned dictionary to ensure stability.
        /// </summary>
        public static IReadOnlyDictionary<ProgrammableData, IReadOnlyCollection<LogEntry>> GetAllLogs()
        {
            return logs.ToDictionary(
                kv => kv.Value.robot,
                kv => (IReadOnlyCollection<LogEntry>) kv.Value.logs);
        }
        
        private struct RobotLogs
        {
            public ProgrammableData robot;
            public List<LogEntry> logs;

            public RobotLogs(ProgrammableData robot)
            {
                this.robot = robot;
                this.logs = new List<LogEntry>();
            }

            public void Add(LogEntry log)
            {
                logs.Add(log);
            }
        }
    }
}
