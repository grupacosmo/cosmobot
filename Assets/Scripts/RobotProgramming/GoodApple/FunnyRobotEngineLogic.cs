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
        private ProgrammableFunctionWrapper wrapper;

        [SerializeField] private GoodApple video;
        [SerializeField] private Material white;
        [SerializeField] private Material black;

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
        {
            taskCompletedEvent = taskEvent;
            cancellationToken = token;
            wrapper = new ProgrammableFunctionWrapper(taskCompletedEvent, cancellationToken, commandQueue);
        }

        public IReadOnlyDictionary<string, Delegate> GetFunctions()
        {
            return new Dictionary<string, Delegate>() {
                { "isBlack", wrapper.Wrap(IsBlack)},
                { "ChangeBlack", wrapper.Wrap(ChangeBlack)},
                { "ChangeWhite", wrapper.Wrap(ChangeWhite)}
            };
        }

        //funny funnys
        public bool IsBlack()
        {
            if (video.CheckColor((transform.position.x + 15) / 31, (transform.position.z + 11) / 23))
            {
                taskCompletedEvent.Set();
                return false;
            }
            taskCompletedEvent.Set();
            return true;
        }

        public void ChangeWhite()
        {
            gameObject.GetComponent<MeshRenderer>().material = white;
            taskCompletedEvent.Set();
        }

        public void ChangeBlack()
        {
            gameObject.GetComponent<Renderer>().material = black;
            taskCompletedEvent.Set();
        }
    }
}
