using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using JetBrains.Annotations;
using UnityEngine;

namespace Cosmobot
{
    /// <summary>
    /// Allows all Robots and functions called by robot to use logger and automatically assigns programmable instance to
    /// every log.
    ///
    /// </summary>
    public static class RobotLogger
    {
        [Flags]
        public enum LogOptions
        {
            None = 0,
            SkipUnityDebugLog = 1 << 0,
        }

        private const int GlobalLogsKey = 0; // Object Instance ID is never 0

        private static readonly AsyncLocal<int?> current = new();
        private static readonly ConcurrentDictionary<int, RobotLogs> logs;

        // In case of performance issues with Invoke try ImmutableDictionary + ImmutableInterlocked (official, nuget)
        private static readonly ConcurrentDictionary<int, Action<ProgrammableData, LogEntry>> logEventHandlers = new();
        private static Action<LogEntry> globalLogsHandler;
        private static Action<ProgrammableData, LogEntry> allLogsEventHandlers;

        static RobotLogger()
        {
            logs = new ConcurrentDictionary<int, RobotLogs>();
            logs[GlobalLogsKey] = new RobotLogs(null);
        }

        /// <summary>
        /// Should be called only by single instance of specialized MonoBehaviour logger dispatcher
        /// </summary>
        public static void Pump()
        {
            // most (if not all) logEvent listeners will be MonoBehaviour components or bounded to Unity's Main Thread
            // so to ensure best (un)subscribe sync and thread safety we will emit all log events from Unity's Main
            // Thread.
            // Note: any other listener (not running on Main Thread), should keep in mind, that his events would not be
            // emitted on his thread and events can be emitted even after calling unsubscribe


            // collect values to remove before emiting events, so no handler will be left without last message 
            // when context cleaning occurs between emitting and clearing
            List<int> toRemove = logs.Where(vk => vk.Value.markedToDispose).Select(vk => vk.Key).ToList();

            Action<ProgrammableData, LogEntry> allLogsHandler = allLogsEventHandlers;
            Action<LogEntry> targetGlobalHandler = globalLogsHandler;

            foreach ((int key, RobotLogs robotLogs) in logs)
            {
                if (logEventHandlers.TryGetValue(key, out Action<ProgrammableData, LogEntry> targetHandler))
                {
                    if (targetHandler == null) continue;

                    while (robotLogs.MessageQueue.TryDequeue(out LogEntry logEntry))
                    {
                        robotLogs.Logs.Add(logEntry);
                        targetHandler.Invoke(robotLogs.Robot, logEntry);
                        allLogsHandler?.Invoke(robotLogs.Robot, logEntry);
                    }
                }
                else if (key == GlobalLogsKey && targetGlobalHandler != null)
                {
                    while (robotLogs.MessageQueue.TryDequeue(out LogEntry logEntry))
                    {
                        robotLogs.Logs.Add(logEntry);
                        targetGlobalHandler.Invoke(logEntry);
                        allLogsHandler?.Invoke(null, logEntry);
                    }
                }
                else
                {
                    while (robotLogs.MessageQueue.TryDequeue(out LogEntry logEntry))
                    {
                        robotLogs.Logs.Add(logEntry);
                        allLogsHandler?.Invoke(robotLogs.Robot, logEntry);
                    }
                }
            }

            foreach (int id in toRemove)
            {
                logs.Remove(id, out RobotLogs _);
            }
        }

        /// <summary>
        /// Will clear all logs and event listeners. Can be used before loading new scene. <b>Can break already working
        /// robot threads</b>
        /// </summary>
        public static void FullCleanup()
        {
            logEventHandlers.Clear();
            globalLogsHandler = null;
            allLogsEventHandlers = null;
            logs.Clear();
            logs[GlobalLogsKey] = new RobotLogs(null);
        }

        public static void AddLogEventHandler(Programmable robot, Action<ProgrammableData, LogEntry> handler)
        {
            int robotId = robot.GetInstanceID();
            logEventHandlers.AddOrUpdate(robotId, handler, (_, existing) => existing + handler);
        }

        public static void RemoveLogEventHandler(Programmable robot, Action<ProgrammableData, LogEntry> handler)
        {
            int robotId = robot.GetInstanceID();
            logEventHandlers.AddOrUpdate(
                robotId,
                _ => null,
                (_, existing) => existing - handler);
            // TryRemove(pair(robotId, null)) - keys are removed over time in ClearCurrent(), no need to clear right
            // away also in NetStandard 2.1 there is no TryRemove(KeyValuePair) so it would require additional lock
            // for safety removal of null value here 
        }

        public static void AddAllLogEventHandler(Action<ProgrammableData, LogEntry> handler)
        {
            Action<ProgrammableData, LogEntry> currentHandler;
            Action<ProgrammableData, LogEntry> updatedHandler;
            do
            {
                currentHandler = allLogsEventHandlers;
                updatedHandler = (Action<ProgrammableData, LogEntry>)Delegate.Combine(currentHandler, handler);
            } while (Interlocked.CompareExchange(ref allLogsEventHandlers, updatedHandler, currentHandler) != currentHandler);
        }

        public static void RemoveAllLogEventHandler(Action<ProgrammableData, LogEntry> handler)
        {
            Action<ProgrammableData, LogEntry> currentHandler;
            Action<ProgrammableData, LogEntry> updatedHandler;
            do
            {
                currentHandler = allLogsEventHandlers;
                updatedHandler = (Action<ProgrammableData, LogEntry>)Delegate.Remove(currentHandler, handler);
            } while (Interlocked.CompareExchange(ref allLogsEventHandlers, updatedHandler, currentHandler) != currentHandler);
        }


        public static void AddGlobalLogEventHandler(Action<LogEntry> handler)
        {
            Action<LogEntry> currentHandler;
            Action<LogEntry> updatedHandler;
            do
            {
                currentHandler = globalLogsHandler;
                updatedHandler = (Action<LogEntry>)Delegate.Combine(currentHandler, handler);
            } while (Interlocked.CompareExchange(ref globalLogsHandler, updatedHandler, currentHandler) != currentHandler);
        }

        public static void RemoveGlobalLogEventHandler(Action<LogEntry> handler)
        {
            Action<LogEntry> currentHandler;
            Action<LogEntry> updatedHandler;
            do
            {
                currentHandler = globalLogsHandler;
                updatedHandler = (Action<LogEntry>)Delegate.Remove(currentHandler, handler);
            } while (Interlocked.CompareExchange(ref globalLogsHandler, updatedHandler, currentHandler) != currentHandler);
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
                throw new InvalidOperationException($"Logger for {robot.Name} is already initialized");
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


            logs.AddOrUpdate(
                currentRobotId.Value,
                (_) => // required, should not be used, but just in case simple implementation
                {
                    RobotLogs robotLogs = new RobotLogs();
                    robotLogs.markedToDispose = true;
                    return robotLogs;
                },
                (_, val) =>
                {
                    val.markedToDispose = true;
                    return val;
                }
            );

            current.Value = null;
        }

        public static void ClearGlobalLogs()
        {
            logs[GlobalLogsKey].Logs.Clear();
        }

        public static void LogInfo(string message, LogOptions options = LogOptions.None)
        {
            Log(LogLevel.Info, message, options);
        }

        public static void LogWarning(string message, LogOptions options = LogOptions.None)
        {
            Log(LogLevel.Warn, message, options);
        }

        public static void LogError(string message, LogOptions options = LogOptions.None)
        {
            Log(LogLevel.Error, message, options);
        }

        public static void Log(LogLevel level, string message, LogOptions options = LogOptions.None)
        {
            int? currentRobotId = current.Value;
            LogEntry logEntry = new LogEntry(level, message);
            if (currentRobotId == null || !logs.TryGetValue(currentRobotId.Value, out RobotLogs currentRobotLogs))
            {
                Debug.LogWarning("[RobotLogger] Missing Robot thread context. Logging to global");
                logs[GlobalLogsKey].Enqueue(logEntry);
                return;
            }

            currentRobotLogs.Enqueue(logEntry);

#if UNITY_EDITOR
            if ((options & LogOptions.SkipUnityDebugLog) != 0) return;
            if (!RobotLoggerSingletonSystem.RobotLoggingToUnityConsole) return;
            
            LogType type;
            switch (logEntry.level)
            {
                case LogLevel.Info: type = LogType.Log; break;
                case LogLevel.Warn: type = LogType.Warning; break;
                case LogLevel.Error: type = LogType.Error; break;
                default: type = LogType.Log; break;
            }

            string robotStackTrace =
                RobotLoggerSingletonSystem.RobotDebugStackTrace
                    ? RobotDebugHelper.GetCurrentRobotContextStackTraceAsString(currentRobotLogs.Robot)
                    : "<Stack trace disabled>";
            
            Debug.LogFormat( // Debug.Log are thread safe
                type,
                LogOption.None,
                currentRobotLogs.Robot.Unity.ProgrammableComponent, // do not use after stopping playmode 
                "Robot: '{0}' ([id: {1:X8}]) at {2} [{3}]: {4}\n\nRobot Context Debug StackTrace:\n{5}",
                currentRobotLogs.Robot.Name,
                currentRobotId,
                logEntry.GetIsoTime(),
                logEntry.level.GetConstSizeName(),
                logEntry.message,
                robotStackTrace);
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
            return logs[currentRobotId.Value].Logs;
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
            outLogs = foundRobotLogs.Logs; // cast
            return result;
        }

        /// <summary>
        /// Returned collection is bound to concurrent log collection and may grow over time. 
        /// </summary>
        public static IReadOnlyCollection<LogEntry> GetGlobalLogs()
        {
            return logs[GlobalLogsKey].Logs;
        }

        public static IReadOnlyCollection<LogEntry> GetRobotLogs(Programmable robot)
        {
            int robotId = robot.GetInstanceID();
            if (logs.TryGetValue(robotId, out RobotLogs foundRobotLogs))
                return foundRobotLogs.Logs;
            return Array.Empty<LogEntry>();
        }

        public static bool TryGetRobotLogs(Programmable robot, out IReadOnlyCollection<LogEntry> outLogs)
        {
            int robotId = robot.GetInstanceID();
            bool result = logs.TryGetValue(robotId, out RobotLogs foundRobotLogs);
            outLogs = foundRobotLogs.Logs;
            return result;
        }

        /// <summary>
        /// Returns a list of the global and all the robots logs. The returned list represents currently active robots
        /// logs, but the <see cref="ReadOnlyRobotLogs.Logs"/> for specified robot are just readonly view of actual
        /// robot's logs, and it may grow with new log entries. Even with growing list, read operation is thread safe. 
        /// <p>
        /// If you need a snapshot of logs then you must clone <see cref="ReadOnlyRobotLogs.Logs"/> collection  
        /// </p><p>
        /// The value with <c><see cref="ReadOnlyRobotLogs.Robot"/> == null</c> represents global logs. 
        /// </p>
        /// </summary>
        public static List<ReadOnlyRobotLogs> GetAllLogs()
        {
            return logs.Select(kv => new ReadOnlyRobotLogs(kv.Value.Robot, kv.Value.Logs)).ToList();
        }

        public struct ReadOnlyRobotLogs
        {
            public bool IsGlobalLogs => Robot == null;
            public ProgrammableData Robot { get; private set; }
            public IReadOnlyList<LogEntry> Logs { get; private set; }

            public ReadOnlyRobotLogs(ProgrammableData robot, IList<LogEntry> logs)
            {
                this.Robot = robot;
                this.Logs = (IReadOnlyList<LogEntry>)logs;
            }
        }

        private struct RobotLogs : IEquatable<RobotLogs>
        {
            public readonly ProgrammableData Robot;
            public readonly List<LogEntry> Logs;
            public readonly ConcurrentQueue<LogEntry> MessageQueue;
            public bool markedToDispose;

            public RobotLogs(ProgrammableData robot)
            {
                this.Robot = robot;
                Logs = new List<LogEntry>();
                MessageQueue = new ConcurrentQueue<LogEntry>();
                markedToDispose = false;
            }

            public void Enqueue(LogEntry log)
            {
                MessageQueue.Enqueue(log);
            }

            public bool Equals(RobotLogs other)
            {
                return Equals(Robot, other.Robot) && markedToDispose == other.markedToDispose;
            }

            public override bool Equals(object obj)
            {
                return obj is RobotLogs other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(Robot, markedToDispose);
            }
        }
    }
}
