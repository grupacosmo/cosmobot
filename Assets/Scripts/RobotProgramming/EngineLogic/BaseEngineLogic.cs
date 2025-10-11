using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cosmobot.Api.Types;
using UnityEngine;

namespace Cosmobot.Api
{
    [DisallowMultipleComponent]
    public class BaseEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ManualResetEvent taskCompletedEvent;
        private CancellationToken cancellationToken;
        private ProgrammableFunctionWrapper wrapper;

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
        {
            taskCompletedEvent = taskEvent;
            cancellationToken = token;
            wrapper = new ProgrammableFunctionWrapper(taskCompletedEvent, cancellationToken, commandQueue);
        }

        public IReadOnlyDictionary<string, Delegate> GetFunctions()
        {
            //Expose robot's ingame functions here
            return new Dictionary<string, Delegate>()
            {
                { "Wait", wrapper.Wrap<float>(Wait)},
                { "Log", wrapper.Wrap<string>(Log)},
                { "LogWarning", wrapper.Wrap<string>(LogWarning)},
                { "LogError", wrapper.Wrap<string>(LogError)},
                { "Dance", wrapper.Wrap(Dance)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to call "taskCompletedEvent.Set();" when yours code is finished or robot will wait infinitely
        public void Wait(float seconds)
        {
            StartCoroutine(WaitCoroutine(seconds));
        }

        public IEnumerator WaitCoroutine(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            taskCompletedEvent.Set();
        }

        public void Log(string message)
        {
            Debug.Log(message);
            taskCompletedEvent.Set();
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning(message);
            taskCompletedEvent.Set();
        }

        public void LogError(string message)
        {
            Debug.LogError(message);
            taskCompletedEvent.Set();
        }

        public void Dance()
        {
            StartCoroutine(DanceCoroutine());
        }

        public IEnumerator DanceCoroutine()
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
