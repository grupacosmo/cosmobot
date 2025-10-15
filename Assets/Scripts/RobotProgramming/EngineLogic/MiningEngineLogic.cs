using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cosmobot.ItemSystem;
using UnityEngine;

namespace Cosmobot.Api
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseEngineLogic))]
    public class MiningEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ManualResetEvent taskCompletedEvent;
        private CancellationToken cancellationToken;
        private ProgrammableFunctionWrapper wrapper;

        private BaseEngineLogic baseLogic;

        private void Start()
        {
            baseLogic = gameObject.GetComponent<BaseEngineLogic>();
        }

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
                { "dig", wrapper.Wrap(Dig)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to call "taskCompletedEvent.Set();" when yours code is finished or robot will wait infinitely
        private void Dig()
        {
            RaycastHit hit;
            Physics.Raycast(transform.position, Vector3.down, out hit, 1);

            OreVein deposit = hit.collider.gameObject.GetComponent<OreVein>();

            if (deposit == null)
            {
                baseLogic.LogInternal("There's no material deposit here");
                taskCompletedEvent.Set();
                return;
            }

            ItemInfo material = deposit.Mine();
            if (material == null)
            {
                baseLogic.LogErrorInternal("Gathered material was null");
                taskCompletedEvent.Set();
                return;
            }

            material.InstantiateItem(transform.position + transform.forward, Quaternion.identity);
            taskCompletedEvent.Set();
        }
    }
}
