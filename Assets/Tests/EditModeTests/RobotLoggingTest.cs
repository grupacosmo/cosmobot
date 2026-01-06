using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Cosmobot.Utils;
using NUnit.Framework;
using UnityEngine;

namespace Cosmobot
{
    [TestFixture]
    public class RobotLoggingTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            RobotLoggerSingletonSystem s = new GameObject().AddComponent<RobotLoggerSingletonSystem>();
            s.robotLoggingToUnityConsole = false;
            s.robotDebugStackTrace = false;
            typeof(SingletonSystem<RobotLoggerSingletonSystem>).GetProperty("Instance", 
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, s);
        }

        [TearDown]
        public void TearDown()
        {
            RobotLogger.ClearCurrent();
            RobotLogger.FullCleanup();
        }

        [Test]
        public void MultithreadedRobotLoggingAndHandlerReceivesLogs()
        {
            const int RobotCount = 10;
            const int ExcludedFromSubCount = 1;
            const int LogsPerRobot = 5;
            string[] expectedLogs =  new string[(RobotCount + 1 - ExcludedFromSubCount) * LogsPerRobot];

            int expectedLogsIndex = 0;
            for (int l = 0; l < LogsPerRobot; l++)
            {
                // double the logs for additional listener for first robot
                expectedLogs[expectedLogsIndex++] = $"0: {l}";
                expectedLogs[expectedLogsIndex++] = $"0: {l}";
            }
                
            // start from 1 coz loop above
            for (int r = 1; r  < RobotCount - ExcludedFromSubCount; r++)
                for (int l = 0; l < LogsPerRobot; l++)
                    expectedLogs[expectedLogsIndex++] = $"{r}: {l}";
             
            
            
            List<string> logs = new List<string>();
            
            List<ProgrammableData> robots = new List<ProgrammableData>();
            for (int i = 0; i < RobotCount; i++)
            {
                robots.Add(GetMockedProgrammableData());
            }
            
            // Event listener for every robot expect last few
            for (int i = 0; i < RobotCount - ExcludedFromSubCount; i++)
            {
                int robotIndex = i;
                RobotLogger.AddLogEventHandler(robots[robotIndex].Unity.ProgrammableComponent, (d, e) =>
                {
                    Assert.AreEqual(robots[robotIndex].InstanceID, d.InstanceID);
                    logs.Add(e.message);
                });
            }
            // additional event listener for first robot (so it has two)
            RobotLogger.AddLogEventHandler(robots[0].Unity.ProgrammableComponent, (d, e) =>
            {
                Assert.AreEqual(robots[0].InstanceID, d.InstanceID);
                logs.Add(e.message);
            });
            
            
            Thread[] robotsThreads = new Thread[RobotCount];
            CountdownEvent loggedCounter = new CountdownEvent(RobotCount);
            ManualResetEventSlim[] robotResetGate = new ManualResetEventSlim[RobotCount];
            for (int i = 0 ; i < RobotCount; i++)
            {
                int robotIndex = i;
                robotResetGate[robotIndex] =  new ManualResetEventSlim(false);
                Thread robotThread = new Thread(() =>
                {
                    RobotLogger.InitCurrent(robots[robotIndex]);
                    Thread.Yield();
                    for (int j = 0; j < LogsPerRobot; j++)
                    {
                        RobotLogger.LogInfo($"{robotIndex}: {j}");
                        Thread.Yield();
                    }
                    loggedCounter.Signal();
                    robotResetGate[robotIndex].Wait();
                    RobotLogger.ClearCurrent();
                });
                robotsThreads[i] = robotThread;
                robotThread.Start();
            }

            loggedCounter.Wait();
            
            RobotLogger.Pump();
            
            foreach (ManualResetEventSlim gate in robotResetGate)
            {
                gate.Set();
            }
            foreach (Thread robotThread in robotsThreads)
            {
                robotThread.Join();
            }
            
            logs.Sort();
            
            Assert.AreEqual(expectedLogs.Length, logs.Count, "log count does not mach expected count");
            for (int i = 0; i < logs.Count; i++)
            {
                Assert.AreEqual(expectedLogs[i], logs[i]);
            }
        }

        [Test]
        public void OnlyNewLogsAreReceived()
        {
            int logCount = 0;
            ProgrammableData robot = GetMockedProgrammableData();
            RobotLogger.InitCurrent(robot);
            
            RobotLogger.LogInfo("old");   
            RobotLogger.Pump();
            RobotLogger.LogInfo("old");
            RobotLogger.Pump();
            
            RobotLogger.AddLogEventHandler(robot.Unity.ProgrammableComponent, (d, e) =>
            {
                Assert.AreEqual(robot.InstanceID, d.InstanceID);
                Assert.AreEqual("new", e.message);
                logCount++;
                
            });
            RobotLogger.LogInfo("new");   
            RobotLogger.Pump();
            RobotLogger.LogInfo("new");
            RobotLogger.Pump();
            
            Assert.AreEqual(2, logCount, "log count does not mach expected count");
            
            RobotLogger.ClearCurrent();
        }


        [Test]
        public void WillFallbackToGlobal()
        {
            int logCount = 0;
            ProgrammableData robot = GetMockedProgrammableData();
            Action<LogEntry> handler = m =>
            {
                logCount++;
                Assert.AreEqual("global", m.message);
            };
            RobotLogger.AddGlobalLogEventHandler(handler);
            
            RobotLogger.LogInfo("global");
            RobotLogger.Pump();
            
            RobotLogger.InitCurrent(robot);
            
            RobotLogger.LogInfo("inside context");
            
            RobotLogger.Pump();
            RobotLogger.ClearCurrent();
            
            RobotLogger.LogInfo("global");
            RobotLogger.Pump();
            
            RobotLogger.RemoveGlobalLogEventHandler(handler);
            Assert.AreEqual(2, logCount);
        }

        [Test]
        public void WillNotReceiveNewMessagesAfterRemovingHandler()
        { 
            int logCount = 0;
            ProgrammableData robot = GetMockedProgrammableData();
            Action<ProgrammableData, LogEntry> handler = (r, m) =>
            {
                Assert.AreEqual(robot.InstanceID, r.InstanceID);
                logCount++;
            };
            
            RobotLogger.AddLogEventHandler(robot.Unity.ProgrammableComponent, handler);
            
            RobotLogger.InitCurrent(robot);
            
            RobotLogger.LogInfo("1");
            RobotLogger.Pump();
            RobotLogger.LogInfo("2");
            RobotLogger.Pump();
            
            RobotLogger.RemoveLogEventHandler(robot.Unity.ProgrammableComponent, handler);
            
            RobotLogger.LogInfo("1");
            RobotLogger.Pump();
            RobotLogger.LogInfo("2");
            RobotLogger.Pump();
            
            RobotLogger.ClearCurrent();
            
            Assert.AreEqual(2, logCount);
        }

        [Test]
        public void CanIterateOverAllLoggedLogs()
        {
            const int RobotCount = 3;
            
            ProgrammableData[] robots = new ProgrammableData[RobotCount];
            Thread[] robotsThreads = new Thread[RobotCount];
            ManualResetEventSlim[] robotResetGate = new ManualResetEventSlim[RobotCount];
            CountdownEvent loggedCounter = new CountdownEvent(RobotCount);
            
            for(int i = 0 ; i < RobotCount ; i++)
            {
                int robotIndex = i;
                robots[i] = GetMockedProgrammableData();
                robotResetGate[i] = new ManualResetEventSlim(false);
                Thread t = new Thread(() =>
                {
                    int instanceID = robots[robotIndex].InstanceID;
                    RobotLogger.InitCurrent(robots[robotIndex]);
                    RobotLogger.LogInfo($"{instanceID}: 1");
                    RobotLogger.LogInfo($"{instanceID}: 2");
                    loggedCounter.Signal();
                    robotResetGate[robotIndex].Wait();
                    RobotLogger.ClearCurrent();
                });
                robotsThreads[i] = t;
                t.Start();
            }
            
            
            loggedCounter.Wait();
            RobotLogger.Pump();

            foreach (ProgrammableData programmableData in robots)
            {
                List<string> logEntries =
                    RobotLogger
                        .GetRobotLogs(programmableData.Unity.ProgrammableComponent)
                        .Select(e => e.message)
                        .ToList();
                int id = programmableData.InstanceID;
                Assert.AreEqual(2, logEntries.Count, "Get by robot. Should have 2 log entries");
                Assert.Contains($"{id}: 1", logEntries, $"Get by robot. robot '{id}' should have log '1'");
                Assert.Contains($"{id}: 2", logEntries, $"Get by robot. robot '{id}' should have log '2'");
            }
            
            foreach (RobotLogger.ReadOnlyRobotLogs logs in RobotLogger.GetAllLogs())
            {
                if (logs.IsGlobalLogs)
                {
                    Assert.AreEqual(0, logs.Logs.Count, "Global logs should be empty");
                    Assert.Null(logs.Robot, "Global logs 'robot' should be null");
                    continue;
                }
                Assert.NotNull(logs.Robot, "Robot logs 'robot' should not be null");
                
                List<string> logEntries = logs.Logs.Select(e => e.message).ToList();
                int id = logs.Robot.InstanceID;
                Assert.AreEqual(2, logEntries.Count);
                Assert.Contains($"{id}: 1", logEntries, $"Get all. robot '{id}' should have log '1'");
                Assert.Contains($"{id}: 2", logEntries, $"Get all. robot '{id}' should have log '2'");
            }


            foreach (ManualResetEventSlim gate in robotResetGate) gate.Set();
            foreach (Thread robotsThread in robotsThreads) robotsThread.Join();
        }

        [Test]
        public void TestEverything()
        {
            Assert.True(true);
        }

        private ProgrammableData GetMockedProgrammableData()
        {
            GameObject go = new GameObject();
            return new ProgrammableData(go.AddComponent<Programmable>());
        }
    }
}
