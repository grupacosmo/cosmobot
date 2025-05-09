using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Jint;
using System.Reflection;
using System.Collections.Concurrent;

namespace Cosmobot
{
    public class Programmable : MonoBehaviour
    {
        private EngineLogicInterface[] engineLogicInterfaces;
        [TextArea(10, 20)]
        [SerializeField] private string code;

        private ManualResetEvent _taskCompletedEvent; //for waiting for Unity thread
        private CancellationTokenSource _cancellationTokenSource; //for thread killing
        private ConcurrentQueue<Action> _commandQueue = new ConcurrentQueue<Action>();
        private Action currentCommand;

        Thread task;

        static int staticDebugI;
        int debugI = 0;
        void Start()
        {
            _taskCompletedEvent = new ManualResetEvent(false);
            _cancellationTokenSource = new CancellationTokenSource();

            task = new Thread(() => jsThread(_cancellationTokenSource.Token));
            task.IsBackground = true;
            task.Start();

            debugI = staticDebugI++;
            engineLogicInterfaces = GetComponents<EngineLogicInterface>();
           
        }

        private void Update()
        {
            if(_commandQueue.TryDequeue(out currentCommand))
            {
                currentCommand();
            }
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            _commandQueue.Clear();
            _cancellationTokenSource?.Cancel();
            staticDebugI = 0;
        }
        /*
        void OnGUI()
        {
            int h = 60; 
            int w = 500; 
            int y = (h * debugI) % 1000;
            int x = (h * debugI) / 1000 * w + 10;
            Rect pos = new Rect(x, y, w, h);
            GUI.Label(pos, $"[{gameObject.name}] Task: {task.Status} {task.Exception}");
        }
        */

        private void jsThread(CancellationToken token)
        {
            Thread.CurrentThread.Name = $"jsEngine-{debugI}";

            using Engine jsEngine = new Engine();
            
            foreach(EngineLogicInterface logicInterface in engineLogicInterfaces)
            {
                logicInterface.SetupThread(_taskCompletedEvent, token, _commandQueue);
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
