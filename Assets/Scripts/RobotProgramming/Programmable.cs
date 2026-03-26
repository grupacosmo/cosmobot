using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cosmobot.Utils;
using Jint;
using Jint.Runtime.Interop;
using UnityEngine;

namespace Cosmobot
{
    /// <summary>
    /// Base class for programmable devices (eg. robots).
    ///
    /// Attaches to a GameObject and allows it to be programmed using JavaScript.
    /// Automatically searches for components implementing the EngineLogicInterface,
    /// which provide methods that can be called from the JavaScript code.
    /// </summary>
    public class Programmable : MonoBehaviour
    {

        [SerializeField]
        private ProgrammingUiLogManager logManager;
        
        public string EngineStackTrace => engineInstance.Advanced.StackTrace;

        private ProgrammableData instance;
        private Engine engineInstance;

        private IEngineLogic[] engineLogicInterfaces;
        [TextArea(10, 20)]

        public string code;
        public ProgrammingUiFileEntry activeFile;

        private ManualResetEvent taskCompletedEvent; //for waiting for Unity thread
        private CancellationTokenSource cancellationTokenSource; //for thread killing
        private ConcurrentQueue<Action> commandQueue = new ConcurrentQueue<Action>();

        private Thread task;

        private static int staticDebugI;
        private int debugI = 0;
        private string objectName;

        void Start()
        {
            instance = new ProgrammableData(this);
            taskCompletedEvent = new ManualResetEvent(false);

            debugI = staticDebugI++;
            engineLogicInterfaces = GetComponents<IEngineLogic>();
            objectName = gameObject.name;
        }

        public void RunTask()
        {
            if (task != null && task.IsAlive)
            {
                logManager.CreateLog(LogLevel.Warn, "Task already running");
                return;
            }

            cancellationTokenSource = new CancellationTokenSource();

            task = new Thread(() => JsThread(cancellationTokenSource.Token));
            task.IsBackground = true;
            task.Start();
        }

        public void StopTask()
        {
            StopAllCoroutines();
            commandQueue.Clear();

            cancellationTokenSource?.Cancel();
            cancellationTokenSource?.Dispose();
            cancellationTokenSource = null;
        }

        private void Update()
        {
            if (commandQueue.TryDequeue(out Action currentCommand))
            {
                currentCommand();
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            commandQueue.Clear();
            cancellationTokenSource?.Cancel();
            staticDebugI = 0;
        }

        private void JsThread(CancellationToken token)
        {
            Thread.CurrentThread.Name = $"jsEngine-{debugI}";

            using Engine jsEngine = new Engine();
            engineInstance = jsEngine;

            foreach (IEngineLogic logicInterface in engineLogicInterfaces)
            {
                logicInterface.SetupThread(taskCompletedEvent, token, commandQueue);
                var functions = logicInterface.GetFunctions();
                foreach (var function in functions)
                {
#if DEBUG
                    ValidateFunction(function.Key, function.Value, logicInterface.GetType().Name);
#endif
                    if (jsEngine.Global.HasProperty(function.Key))
                    {
                        Debug.LogError($"Duplicate key: {function.Key} in Interface: {logicInterface.GetType().Name}");
                        throw new Exception($"Duplicate key: {function.Key} in Interface: {logicInterface.GetType().Name}");
                    }
                    else
                    {
                        jsEngine.SetValue(function.Key, function.Value);
                    }
                }
            }

            Type apiVec2Type = typeof(Cosmobot.Api.Types.Vec2);
            string apiNamespace = apiVec2Type.Namespace;
            IEnumerable<Type> apiTypes = apiVec2Type.Assembly.GetTypes().Where(t => t.Namespace == apiNamespace);
            //jsEngine.SetValue("Types", new NamespaceReference(jsEngine, apiNamespace));
            foreach (Type type in apiTypes)
            {
                jsEngine.SetValue(type.Name, TypeReference.CreateTypeReference(jsEngine, type));
            }

            try
            {
                token.ThrowIfCancellationRequested();
                RobotLogger.InitCurrent(instance);
                jsEngine.Execute(code, "main.js");
            }
            catch (OperationCanceledException)
            {
                commandQueue.Enqueue(() => logManager.CreateLog(
                    LogLevel.Info,
                    "Operation was cancelled"));
                RobotLogger.LogError("Program was stopped/cancelled", RobotLogger.LogOptions.SkipUnityDebugLog);
            }
            catch (Jint.Runtime.JavaScriptException ex)
            {
                string jsStackTrace =
                    ex.JavaScriptStackTrace != null
                        ? ("[JS]  " + ex.JavaScriptStackTrace.Replace("\n", "\n[JS]  "))
                        : "[No JS stack trace available]";
                commandQueue.Enqueue(() => logManager.CreateLog(
                    LogLevel.Error,
                    $"[Programmable JsThread] JS Error ({objectName}): {ex.Error} | {ex.Location}\n{jsStackTrace}\n{ex.StackTrace}"));
                RobotLogger.LogError(
                    $"[JavaScript Exception]: {ex.Error}\n\n{ex.JavaScriptStackTrace ?? "Stack trace not available"}",
                    RobotLogger.LogOptions.SkipUnityDebugLog);
            }
            catch (Exception ex)
            {
                commandQueue.Enqueue(() => logManager.CreateLog(
                    LogLevel.Error,
                    $"[Programmable JsThread] Error: ({objectName}): {ex.Message}\n {ex.StackTrace}"));
                RobotLogger.LogError("Internal unknown error occurred! This error is outside of your code, and you " +
                                     "have probably discovered a \"real\"-world glitch! Unfortunately, this is a game" +
                                     $"bug - you can report it at {GameInfo.BugReportUrl}.\n\n" +
                                     "If you are interested in more detailed information about this error, here is " +
                                     "the raw message (you probably won’t understand it because it’s an internal " +
                                     "error and games normally don’t show these to players - but this is a game " +
                                     "about programming, so why not?):\n\n" +
                                     $"{ex.GetType().Name}: {ex.Message}",
                    RobotLogger.LogOptions.SkipUnityDebugLog);
            }
            finally
            {
                RobotLogger.ClearCurrent();
                commandQueue.Enqueue(() => logManager.CreateLog(
                    LogLevel.Info,
                    "[Programmable JsThread] Done"));
            }
        }

#if DEBUG
        const string RobotApiTypesNamespace = "Cosmobot.Api.Types";
        const string RobotApiTypesInternalNamespace = "Cosmobot.Api.TypesInternal";

        private void ValidateFunction(string key, Delegate value, string name)
        {
            Type type = value.Method.ReturnType;

            Type underNullableType = Nullable.GetUnderlyingType(type);
            if (underNullableType != null)
            {
                type = underNullableType;
            }

            if (type.IsGenericType)
            {
                Type[] argsTypes = type.GetGenericArguments();
                foreach (var arg in argsTypes)
                {
                    if (arg == typeof(void) || arg.IsPrimitive || arg.Namespace == RobotApiTypesNamespace || arg.Namespace == RobotApiTypesInternalNamespace)
                    {
                        continue;
                    }
                    Debug.LogError($"Method returns disallowed type argument in generic type! Method: {arg} in {type.GetGenericTypeDefinition()} {key} in {name}");
                    throw new Exception($"Method returns disallowed type argument in generic type! Method: {arg} in {type.GetGenericTypeDefinition()} {key} in {name}");
                }
                return;
            }

            if (type == typeof(void) || type.IsPrimitive || type.Namespace == RobotApiTypesNamespace || type.Namespace == RobotApiTypesInternalNamespace)
            {
                return;
            }

            Debug.LogError($"Method returns disallowed type! Method: {type} {key} in {name}");
            throw new Exception($"Method returns disallowed type! Method: {type} {key} in {name}");
        }
#endif
    }
}
