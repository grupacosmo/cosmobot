using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Collections.Concurrent;

namespace Cosmobot
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseRobotEngineLogic))]
    public class FunnyRobotEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ManualResetEvent taskCompletedEvent;
        private CancellationToken cancellationToken;
        private Wrapper wrapper;

        [SerializeField] private badapple video;
        [SerializeField] private Material White;
        [SerializeField] private Material Black;

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
        {
            taskCompletedEvent = taskEvent;
            cancellationToken = token;
            wrapper = new Wrapper(taskCompletedEvent, cancellationToken, commandQueue);
        }

        public Dictionary<string, Delegate> GetFunctions()
        {
            return new Dictionary<string, Delegate>() {
                { "isBlack", wrapper.Wrap(isBlack)},
                { "ChangeBlack", wrapper.Wrap(ChangeBlack)},
                { "ChangeWhite", wrapper.Wrap(ChangeWhite)}
            };
        }

        //funny funnys
        public bool isBlack()
        {
            if (video.checkColor((transform.position.x + 15) / 31, (transform.position.z + 11) / 23))
            {
                taskCompletedEvent.Set();
                return false;
            }
            taskCompletedEvent.Set();
            return true;
        }

        public void ChangeWhite()
        {
            gameObject.GetComponent<MeshRenderer>().material = White;
            taskCompletedEvent.Set();
        }

        public void ChangeBlack()
        {
            gameObject.GetComponent<Renderer>().material = Black;
            taskCompletedEvent.Set();
        }
    }
}
