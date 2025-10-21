using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cosmobot.Utils;
using UnityEngine;

namespace Cosmobot.Api
{
    //
    // This class in under WIP
    // Waiting for construction fix
    // `abstract` from `public abstract class` and this comment should be removed when it starts working properly
    //
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseEngineLogic))]
    public abstract class ConstructionEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ProgrammableFunctionWrapper wrapper;

        private BaseEngineLogic baseLogic;

        [SerializeField] private GameObject constructionSite;

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
                { "setupConstructionSite", wrapper.WrapOneFrame(SetupConstructionSite)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to include "ManualResetEvent taskCompletedEvent" in arguments if using WrapDeffered and .Set() it at the end of action
        private void SetupConstructionSite()
        {
            // This will throw an error as construction system might need reworking, 
            // also team not sure if robot should be able to initialize builds
            Vector3 inFront = transform.position + transform.forward;

            inFront.x = Mathf.Floor(inFront.x / GlobalConstants.GRID_CELL_SIZE) * GlobalConstants.GRID_CELL_SIZE + 0.5f;
            inFront.y = 0;
            inFront.z = Mathf.Floor(inFront.z / GlobalConstants.GRID_CELL_SIZE) * GlobalConstants.GRID_CELL_SIZE + 0.5f;

            Instantiate(constructionSite, inFront, Quaternion.identity);
        }
    }
}
