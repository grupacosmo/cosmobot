using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Jint;

namespace Cosmobot
{
    public class Programmable : MonoBehaviour
    {
        private EngineLogicInterface[] engineLogicInterfaces;
        [TextArea(10, 20)]
        [SerializeField] private string code;

        private ManualResetEvent _taskCompletedEvent = new ManualResetEvent(false); //for waiting for Unity thread
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource(); //for thread killing
        private static SynchronizationContext _mainThreadContext = SynchronizationContext.Current; //for handing Unity functions to main thread

        void Start()
        {
            engineLogicInterfaces = GetComponents<EngineLogicInterface>();
            Task.Run(() => jsThread(_cancellationTokenSource.Token));
        }

        private void jsThread(CancellationToken token)
        {
            Engine jsEngine = new Engine();
            
            foreach(EngineLogicInterface logicInterface in engineLogicInterfaces)
            {
                logicInterface.SetupThread(_taskCompletedEvent, token, _mainThreadContext);
                var functions = logicInterface.GetFunctions();
                foreach (var function in functions)
                {
                    //TODO: Check if this function is safe here
                    //ValidateFunction()
                    if (jsEngine.Global.HasProperty(function.Key))
                    {
                        Debug.LogError("Duplicate key: " + function.Key + " in Interface: " + logicInterface.GetType().Name);
                    }else
                    {
                        jsEngine.SetValue(function.Key, function.Value);
                    }
                }
            }

            try
            {
                jsEngine.Execute(code);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Operation was cancelled");
                jsEngine.Dispose();
                jsEngine = null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error: " + ex.Message);
                jsEngine.Dispose();
                jsEngine = null;
            }
            finally
            {
                Debug.Log("Done");
            }
        }
        /*
        #if UNITY_EDITOR
        if(!Debug.isDebugBuild)
        {
            return;
        }
        const string RobotApiTypesNamespace = "Cosmobot.Api.Types";
        private void ValidateFunction(string key)
        {
            if (type.IsPrimitive || type.Namespace == RobotApiTypesNamespace)
            {
                return;
            }

            UnityEditor.isPlaying = false;
            throw new Exception("fuck you");
        #endif
        }*/
    }
}
