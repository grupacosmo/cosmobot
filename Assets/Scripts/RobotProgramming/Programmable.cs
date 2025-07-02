using System;
using System.Threading;
using UnityEngine;
using Jint;
using System.Collections.Concurrent;

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
        private IEngineLogic[] engineLogicInterfaces;
        [TextArea(10, 20)]
        [SerializeField] private string code;

        private ManualResetEvent taskCompletedEvent; //for waiting for Unity thread
        private CancellationTokenSource cancellationTokenSource; //for thread killing
        private ConcurrentQueue<Action> commandQueue = new ConcurrentQueue<Action>();

        Thread task;

        static int staticDebugI;
        int debugI = 0;
        void Start()
        {
            taskCompletedEvent = new ManualResetEvent(false);
            cancellationTokenSource = new CancellationTokenSource();

            task = new Thread(() => jsThread(cancellationTokenSource.Token));
            task.IsBackground = true;
            task.Start();

            debugI = staticDebugI++;
            engineLogicInterfaces = GetComponents<IEngineLogic>();
           
        }

        private void Update()
        {
            if(commandQueue.TryDequeue(out Action currentCommand))
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

        private void jsThread(CancellationToken token)
        {
            Thread.CurrentThread.Name = $"jsEngine-{debugI}";

            using Engine jsEngine = new Engine();
            
            foreach(IEngineLogic logicInterface in engineLogicInterfaces)
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

            try
            {
                token.ThrowIfCancellationRequested();
                jsEngine.Execute(code);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Operation was cancelled");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error: " + ex.Message);
            }
            finally
            {
                Debug.Log("Done");
            }
        }

        #if DEBUG
        const string RobotApiTypesNamespace = "Cosmobot.Api.Types";
        private void ValidateFunction(string key, Delegate value, string name)
        {
            Type type = value.Method.ReturnType;
            if (type == typeof(void) || type.IsPrimitive || type.Namespace == RobotApiTypesNamespace)
            {
                return;
            }
            
            Debug.LogError($"Method returns disallowed type! Method: {type} {key} in {name}");
            throw new Exception($"Method returns disallowed type! Method: {type} {key} in {name}");
        }
        #endif
    }
}
