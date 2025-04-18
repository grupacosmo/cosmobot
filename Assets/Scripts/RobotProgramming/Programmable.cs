using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Jint;
using System.Reflection;

namespace Cosmobot
{
    public class Programmable : MonoBehaviour
    {
        private EngineLogicInterface[] engineLogicInterfaces;
        [TextArea(10, 20)]
        [SerializeField] private string code;

        private ManualResetEvent _taskCompletedEvent; //for waiting for Unity thread
        private CancellationTokenSource _cancellationTokenSource; //for thread killing
        private static SynchronizationContext _mainThreadContext; //for handing Unity functions to main thread

        private int milliStartSyncCheck = 1000;
        Task task;

        static int staticDebugI;
        int debugI = 0;
        void Start()
        {
            _taskCompletedEvent = new ManualResetEvent(false);
            _cancellationTokenSource = new CancellationTokenSource();
            _mainThreadContext = SynchronizationContext.Current;

            debugI = staticDebugI++;
            engineLogicInterfaces = GetComponents<EngineLogicInterface>();
            task = Task.Run(() => jsThread(_cancellationTokenSource.Token));
            RobotTaskManager.TaskList.Add(task);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            _cancellationTokenSource?.Cancel();
            staticDebugI = 0;
            RobotTaskManager.TaskList.Clear();
        }

        void OnGUI()
        {
            int h = 60; 
            int w = 500; 
            int y = (h * debugI) % 1000;
            int x = (h * debugI) / 1000 * w + 10;
            Rect pos = new Rect(x, y, w, h);
            GUI.Label(pos, $"[{gameObject.name}] Task: {task.Status} {task.Exception}");
        }

        private void jsThread(CancellationToken token)
        {
            Thread.CurrentThread.Name = $"jsEngine-{debugI}";

            using Engine jsEngine = new Engine();
            
            foreach(EngineLogicInterface logicInterface in engineLogicInterfaces)
            {
                logicInterface.SetupThread(_taskCompletedEvent, token, _mainThreadContext);
                var functions = logicInterface.GetFunctions();
                foreach (var function in functions)
                {
                    #if DEBUG
                    ValidateFunction(function.Key, function.Value, logicInterface.GetType().Name);
                    #endif
                    if (jsEngine.Global.HasProperty(function.Key))
                    {
                        RobotTaskManager.TaskList.Remove(task);
                        Debug.LogError($"Duplicate key: {function.Key} in Interface: {logicInterface.GetType().Name}");
                        throw new Exception($"Duplicate key: {function.Key} in Interface: {logicInterface.GetType().Name}");
                    }
                    else
                    {
                        jsEngine.SetValue(function.Key, function.Value);
                    }
                }
            }

            Thread.Sleep(100);
            while(RobotTaskManager.CountTasksReady() < RobotTaskManager.TaskList.Count)
            {
                WaitHandle.WaitAny(new[] { RobotTaskManager.allReady, token.WaitHandle }, milliStartSyncCheck);
                token.ThrowIfCancellationRequested();
                Debug.Log($"{RobotTaskManager.CountTasksReady()}/{RobotTaskManager.TaskList.Count} Tasks ready");
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
                RobotTaskManager.TaskList.Remove(task);
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
            RobotTaskManager.TaskList.Remove(task);
            throw new Exception($"Method returns disallowed type! Method: {type} {key} in {name}");
        }
        #endif
    }
}
