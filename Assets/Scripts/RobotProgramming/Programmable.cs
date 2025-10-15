using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Jint;
using UnityEngine;
using Cosmobot.Api.Types;
using Jint.Runtime.Interop;

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

        private static int staticDebugI;
        private int debugI = 0;
        private string objectName;

        void Start()
        {
            taskCompletedEvent = new ManualResetEvent(false);
            cancellationTokenSource = new CancellationTokenSource();

            task = new Thread(() => JsThread(cancellationTokenSource.Token));
            task.IsBackground = true;
            task.Start();

            debugI = staticDebugI++;
            engineLogicInterfaces = GetComponents<IEngineLogic>();
            objectName = gameObject.name;
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


            Type apiVec2Type = typeof(Cosmobot.Api.Types.vec2);
            string apiNamespace = apiVec2Type.Namespace;
            IEnumerable<Type> apiTypes = apiVec2Type.Assembly.GetTypes().Where(t => t.Namespace == apiNamespace);
            //jsEngine.SetValue("Types", new NamespaceReference(jsEngine, apiNamespace));
            foreach (Type type in apiTypes)
            {
                jsEngine.SetValue(type.Name, TypeReference.CreateTypeReference(jsEngine, type));
                Debug.Log("Exposed type: " + type.Name);
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
            catch (Jint.Runtime.JavaScriptException ex)
            {
                Debug.LogError($"JS Error ({objectName}): {ex.Error} | {ex.Location}\n{ex.StackTrace}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error: ({objectName}): {ex.Message}");
            }
            finally
            {
                Debug.Log("Done");
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
