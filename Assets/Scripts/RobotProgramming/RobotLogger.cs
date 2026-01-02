using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

namespace Cosmobot
{
    public static class RobotLogger
    {
        private struct RobotLogs
        {
            public Programmable robot; // TODO: nie MB Programmable, tylko niezaleze
                                       //   posiadac moze wszystkie rzeczy robota jak nazwa itp
            public List<LogEntry> logs;

            public RobotLogs(Programmable robot)
            {
                this.robot = robot;
                this.logs = new List<LogEntry>();
            }

            public void Add(LogEntry log)
            {
                logs.Add(log);
            }
        }
        
        private const int GlobalLogsKey = 0; // Object Instance ID is never 0
        
        private static readonly AsyncLocal<int?> current = new();
        private static readonly ConcurrentDictionary<int, RobotLogs> logs;
        
        private static readonly ConcurrentDictionary<int, Action<int, LogEntry>> logEventHandlers = new();
        
        static RobotLogger()
        {
            logs = new ConcurrentDictionary<int, RobotLogs>();
            logs[GlobalLogsKey] = new RobotLogs(null);
        }

        public static void AddLogEventHandler(Programmable robot, Action<int, LogEntry> handler)
        {
            int robotId = robot.GetInstanceID();
            if (logEventHandlers.TryGetValue(robotId, out Action<int, LogEntry> existing))
            {
                logEventHandlers[robotId] = existing + handler;
            }
            else
            {
                logEventHandlers[robotId] = handler;
            }
        }

        public static void RemoveLogEventHandler(Programmable robot, Action<int, LogEntry> handler)
        {
            int robotId = robot.GetInstanceID();
            if (logEventHandlers.TryGetValue(robotId, out Action<int, LogEntry> existing))
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
        
        public static void InitCurrent(Programmable robot)
        {
            if (robot == null)
            {
                throw new ArgumentException("robot cant be null and must exist", nameof(robot));
            }

            int robotId = robot.GetInstanceID();
            if (logs.ContainsKey(robotId))
            {
                throw new InvalidOperationException($"Logger for {robot.name}  is already initialized");
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
            if (currentRobotId == null)
            {
                Debug.LogWarning("Missing Robot thread context. Logging to global");
                logs[GlobalLogsKey].Add(logEntry);
                return;
            }
            
            logs[currentRobotId.Value].Add(logEntry);
            logEventHandlers[currentRobotId.Value]?.Invoke(currentRobotId.Value, logEntry);
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
        public static IReadOnlyDictionary<Programmable, IReadOnlyCollection<LogEntry>> GetAllLogs()
        {
            return logs.ToDictionary(
                kv => kv.Value.robot,
                kv => (IReadOnlyCollection<LogEntry>) kv.Value.logs);
        }
    }
}
