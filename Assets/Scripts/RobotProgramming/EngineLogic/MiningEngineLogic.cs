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
        private ProgrammableFunctionWrapper wrapper;

        private BaseEngineLogic baseLogic;

        private void Start()
        {
            baseLogic = gameObject.GetComponent<BaseEngineLogic>();
        }

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
        {
            wrapper = new ProgrammableFunctionWrapper(taskEvent, token, commandQueue);
        }

        public IReadOnlyDictionary<string, Delegate> GetFunctions()
        {
            // Here you can expose the robot's functions in the game, use:
            // WrapOneFrame() for immediate functions and
            // WrapDeffered() for time-stretched functions (like coroutines)
            return new Dictionary<string, Delegate>()
            {
                { "dig", wrapper.WrapOneFrame(Dig)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to include "ManualResetEvent taskCompletedEvent" in arguments if using WrapDeffered and .Set() it at the end of action
        private void Dig()
        {
            RaycastHit hit;

            if (!Physics.Raycast(transform.position, Vector3.down, out hit, 1))
            {
                baseLogic.LogInternal("Nothing detected under me");
                return;
            }

            OreVein deposit = hit.collider.gameObject.GetComponent<OreVein>();

            if (deposit == null)
            {
                baseLogic.LogInternal("There's no material deposit here");
                return;
            }

            ItemInfo material = deposit.Mine();
            if (material == null)
            {
                baseLogic.LogErrorInternal("Gathered material was null");
                return;
            }

            material.InstantiateItem(transform.position + transform.forward, Quaternion.identity);
        }
    }
}
