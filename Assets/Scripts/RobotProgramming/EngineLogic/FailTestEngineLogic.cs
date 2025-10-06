using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cosmobot
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseRobotEngineLogic))]
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
