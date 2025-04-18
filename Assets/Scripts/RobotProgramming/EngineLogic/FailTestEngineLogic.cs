using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cosmobot.Api.Types;

namespace Cosmobot
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseRobotEngineLogic))]
    public class FailTestEngineLogic : MonoBehaviour, EngineLogicInterface
    {
        private ManualResetEvent _taskCompletedEvent;
        private CancellationToken _cancellationToken;
        private SynchronizationContext _mainThreadContext;
        private Wrapper wrapper;

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, SynchronizationContext threadContext)
        {
            _taskCompletedEvent = taskEvent;
            _cancellationToken = token;
            _mainThreadContext = threadContext;
            wrapper = new Wrapper(_taskCompletedEvent, _cancellationToken, _mainThreadContext);
        }

        public Dictionary<string, Delegate> GetFunctions()
        {
            return new Dictionary<string, Delegate>() {
                { "VectorTest", wrapper.Wrap(TestFun)},
                { "TurnLeft", wrapper.Wrap(TurnLeft)},
                { "TurnRight", wrapper.Wrap(TurnRight)}
            };
        }

        //ROBOT FUNCTIONS
        public void TurnLeft()
        {
            transform.Rotate(Vector3.up, -90);
            _taskCompletedEvent.Set();
        }

        public void TurnRight()
        {
            transform.Rotate(Vector3.up, -90);
            _taskCompletedEvent.Set();
        }

        Vector3 TestFun()
        {
            _taskCompletedEvent.Set();
            return Vector3.one;
        }
    }
}
