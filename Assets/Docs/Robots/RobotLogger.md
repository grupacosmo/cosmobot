# RobotLogger
- All [RobotLogger](../../Scripts/RobotProgramming/RobotLogger.cs) functions are static and available without instance.


For code example, see:
- [RobotDebugDummyLogReceiver](../../Scripts/RobotProgramming/RobotDebugDummyLogReceiver.cs) - simple component logging to Unity console
- [RobotLoggingTest](../../Tests/EditModeTests/RobotLoggingTest.cs) - tests

# Quick Start
1. Add [RobotLoggerSingletonSystem](../../Scripts/RobotProgramming/RobotLoggerSingletonSystem.cs) to an empty GameObject
   - _(optional)_ Adjust `RobotLoggerSingletonSystem` properties in the Inspector
2. Add Robots with [Programmable](../../Scripts/RobotProgramming/Programmable.cs) component to scene
   - Use `RobotLogger.LogInfo`, `RobotLogger.LogWarning` and `RobotLogger.LogError` in your robot code (see [Coroutine limitation](#coroutine-limitation) below)
3. Register log event handlers in the `OnEnable` Unity event and unregister them in the `OnDisable` 
   - `RobotLogger.AddLogEventHandler` and `RobotLogger.RemoveLogEventHandler` for logs of specific robot (`Programmable`)
   - `RobotLogger.AddAllLogEventHandler` and `RobotLogger.RemoveAllLogEventHandler` for logs of all robots
   - `RobotLogger.AddGlobalLogEventHandler` and `RobotLogger.RemoveGlobalLogEventHandler` for global logs (not paired with any robot)

- Access logs any time when needed
   - `IReadOnlyCollection<LogEntry> GetRobotLogs(Programmable robot)` - Logs of a specific robot
   - `bool TryGetRobotLogs(Programmable robot, out IReadOnlyCollection<LogEntry> outLogs)` - safe access to a specific robot logs
   - `IReadOnlyCollection<LogEntry> GetGlobalLogs()` - global logs (not paired with any robot)
   - `List<ReadOnlyRobotLogs> GetAllLogs()` - logs for all robots (paired with robots)
   - `IReadOnlyCollection<LogEntry> GetCurrentLogs()` - logs of current robot (only works in robot code)
   - `bool TryGetCurrentLogs(out IReadOnlyCollection<LogEntry> outLogs)` - safe access to current robot logs (only works in robot code)


# Setup explained

## Scene/Environment
In this manual: an "Environment" refers to any execution context outside a Unity Scene (eg. single function in unit tests).

- Every scene/environment requires a SingletonSystem: [RobotLoggerSingletonSystem](../../Scripts/RobotProgramming/RobotLoggerSingletonSystem.cs)
  - _(Scene only)_ When enabled this component automatically calls `RobotLogger.Pump()` once per frame (Unity `Update`). (Pump is described below)
  - _(Environment only)_ It is required to manually assign `Instance` property to created instance of this class. 
- Before starting scene/environment, it is recommended to call `RobotLogger.FullCleanup()`:
  - Clears all unregistered event listeners 
  - Removes old logs
  - _(Scene only)_ This is automatically done by `RobotLoggerSingletonSystem` in `SystemAwake`
  - **Important**: If any robot is executing code during scene/environment change, calling `FullCleanup()` may break that robot logging. Ensure all robots have finished execution beforehand.

## Pump
All log event handlers are synchronized to the thread in which `RobotLogger.Pump()` is called.
- (Un)subscribing handlers and handling events should be done on the same thread as `RobotLogger.Pump()`. 
- If (un)subscribing or event handling is performed on different threads, handlers may be invoked after being unsubscribed.

In Unity Scenes, it is advised to use `RobotLoggerSingletonSystem` to handle `Pump` automatically.

## Robot
Each robot must register itself in [RobotLogger](../../Scripts/RobotProgramming/RobotLogger.cs) before executing any robot code and unregister itself after executing final robot instruction. This is automatically done by the [Programmable](../../Scripts/RobotProgramming/Programmable.cs) component.

Robot code can use:
- `RobotLogger.LogInfo`
- `RobotLogger.LogWarning`
- `RobotLogger.LogError` 
- `RobotLogger.Log` - allows specifying the log level manually

`RobotLogger` automatically manages context, so there is no need to pass the `Programmable` instance explicitly (see [Coroutine limitation](#coroutine-limitation) below).

### Log Options
- Use `LogOptions.SkipUnityDebugLog` as the last argument in `Log` and `LogXXX` function to skip writing to Unity Console
- writing to Unity Console can be globally disabled in `RobotLoggerSingletonSystem`
  - if disabled globally, no logs will be written to Unity Console and `LogOptions.SkipUnityDebugLog` has no effect

### "non Programmable Component" robots
If implementing robots without the `Programmable` Component:
- Before executing robot code call `RobotLogger.InitCurrent(ProgrammableData)` 
- After executing the last robot instruction, call `RobotLogger.ClearCurrent()` **on the same thread** as `InitCurrent` was called. This is required to avoid memory leaks.
- If thread exits without calling `RobotLogger.ClearCurrent()`, memory can be cleared using `RobotLogger.FullCleanup()`, but this also:
  - Removes all event handlers
  - Clears all other robot contexts 
  - **Use with caution** 

# Coroutine limitation
Currently, code started with `StartCoroutine` does **not** properly preserve the robot logging context. This results in all logs from unity coroutine ending up in global logs without association to specific robot.

Suggestion for future fix: Implement custom coroutine, that will preserve `ExecutionContext` between calls (there is possibility to wrap Unity's `StartCoroutine`, however it can impact performance overhead in some situations)