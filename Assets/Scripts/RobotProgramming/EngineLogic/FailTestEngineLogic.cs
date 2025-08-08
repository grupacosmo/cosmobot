using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Collections.Concurrent;

namespace Cosmobot
{
    [DisallowMultipleComponent]
    public class FailTestEngineLogic : MonoBehaviour, IEngineLogic
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
            return new Dictionary<string, Delegate>() {
                { "VectorTest", wrapper.Wrap(TestFun)},
                { "TurnLeft", wrapper.Wrap(TurnLeft)},
                { "TurnRight", wrapper.Wrap(TurnRight)}
            };
        }

        //ROBOT FUNCTIONS
        private void TurnLeft()
        {
            transform.Rotate(Vector3.up, -90);
            taskCompletedEvent.Set();
        }

        private void TurnRight()
        {
            transform.Rotate(Vector3.up, -90);
            taskCompletedEvent.Set();
        }

        private Vector3 TestFun()
        {
            taskCompletedEvent.Set();
            return Vector3.one;
        }
    }
}
