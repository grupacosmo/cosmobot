using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cosmobot.Api
{
    [DisallowMultipleComponent]
    public class BaseEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ProgrammableFunctionWrapper wrapper;

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
        {
            wrapper = new ProgrammableFunctionWrapper(taskEvent, token, commandQueue);
        }

        public void LogInternal(string message)
        {
            Debug.Log(message);
        }

        public void LogWarningInternal(string message)
        {
            Debug.LogWarning(message);
        }
        public void LogErrorInternal(string message)
        {
            Debug.LogError(message);
        }

        public IReadOnlyDictionary<string, Delegate> GetFunctions()
        {
            // Here you can expose the robot's functions in the game, use:
            // WrapOneFrame() for immediate functions and
            // WrapDeffered() for time-stretched functions (like coroutines)
            return new Dictionary<string, Delegate>()
            {
                { "wait", wrapper.WrapDeffered<float>(Wait)},
                { "log", wrapper.WrapOneFrame<string>(Log)},
                { "logWarning", wrapper.WrapOneFrame<string>(LogWarning)},
                { "logError", wrapper.WrapOneFrame<string>(LogError)},
                { "dance", wrapper.WrapDeffered(Dance)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to include "ManualResetEvent taskCompletedEvent" in arguments if using WrapDeffered and .Set() it at the end of action
        private void Wait(ManualResetEvent taskCompletedEvent, float seconds)
        {
            StartCoroutine(WaitCoroutine(taskCompletedEvent, seconds));
        }

        private IEnumerator WaitCoroutine(ManualResetEvent taskCompletedEvent, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            taskCompletedEvent.Set();
        }

        private void Log(string message)
        {
            LogInternal(message);
        }

        private void LogWarning(string message)
        {
            LogWarningInternal(message);
        }

        private void LogError(string message)
        {
            LogErrorInternal(message);
        }

        private void Dance(ManualResetEvent taskCompletedEvent)
        {
            StartCoroutine(DanceCoroutine(taskCompletedEvent));
        }

        private IEnumerator DanceCoroutine(ManualResetEvent taskCompletedEvent)
        {
            // This probably will be an animation in the future
            for (int i = 0; i < 3; ++i)
            {
                Quaternion q = transform.rotation;
                float spin = 0;
                while (spin < 720)
                {
                    transform.Rotate(Vector3.up, Time.deltaTime * 400);
                    spin += Time.deltaTime * 400;
                    yield return null;
                }
                transform.rotation = q;
                Vector3 pos = transform.position;
                while (transform.position.x < pos.x + 0.5f)
                {
                    transform.position += Vector3.right * Time.deltaTime * 4;
                    yield return null;
                }
                while (transform.position.x > pos.x - 0.5f)
                {
                    transform.position -= Vector3.right * Time.deltaTime * 4;
                    yield return null;
                }
                while (transform.position.x < pos.x)
                {
                    transform.position += Vector3.right * Time.deltaTime * 4;
                    yield return null;
                }
                transform.position = pos;
            }
            taskCompletedEvent.Set();
        }
    }
}
