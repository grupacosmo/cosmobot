using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Cosmobot.Api.Types;
using System.Collections.Concurrent;

namespace Cosmobot
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseRobotEngineLogic))]
    public class FunnyRobotEngineLogic : MonoBehaviour, EngineLogicInterface
    {
        private ManualResetEvent _taskCompletedEvent;
        private CancellationToken _cancellationToken;
        private SynchronizationContext _mainThreadContext;
        private Wrapper wrapper;

        [SerializeField] private badapple video;
        [SerializeField] private Material White;
        [SerializeField] private Material Black;

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
        {
            _taskCompletedEvent = taskEvent;
            _cancellationToken = token;
            wrapper = new Wrapper(_taskCompletedEvent, _cancellationToken, commandQueue);
        }

        public Dictionary<string, Delegate> GetFunctions()
        {
            return new Dictionary<string, Delegate>() {
                { "isBlack", wrapper.Wrap<bool>(isBlack)},
                { "ChangeBlack", wrapper.Wrap(ChangeBlack)},
                { "ChangeWhite", wrapper.Wrap(ChangeWhite)}
            };
        }

        //funny funny
        public bool isBlack()
        {
            if (video.checkColor((transform.position.x + 15) / 31, (transform.position.z + 11) / 23))
            {
                _taskCompletedEvent.Set();
                return false;
            }
            _taskCompletedEvent.Set();
            return true;
        }

        public void ChangeWhite()
        {
            gameObject.GetComponent<MeshRenderer>().material = White;
            _taskCompletedEvent.Set();
        }

        public void ChangeBlack()
        {
            gameObject.GetComponent<Renderer>().material = Black;
            _taskCompletedEvent.Set();
        }
    }
}
