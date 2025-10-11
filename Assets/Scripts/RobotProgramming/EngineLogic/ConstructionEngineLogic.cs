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
    [RequireComponent(typeof(BaseEngineLogic))]
    public class ConstructionEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ManualResetEvent taskCompletedEvent;
        private CancellationToken cancellationToken;
        private ProgrammableFunctionWrapper wrapper;

        private BaseEngineLogic baseLogic;

        [SerializeField] private GameObject constructionSite;

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
                { "SetupConstructionSite", wrapper.Wrap(SetupConstructionSite)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to call "taskCompletedEvent.Set();" when yours code is finished or robot will wait infinitely
        void SetupConstructionSite()
        {
            // This will throw an error as construction system might need reworking, 
            // also team not sure if robot should be able to initialize builds
            Vector3 inFront = transform.position + transform.forward;

            inFront.x -= inFront.x % GlobalConstants.GRID_CELL_SIZE + 0.5f;
            inFront.y = 0;
            inFront.z -= inFront.z % GlobalConstants.GRID_CELL_SIZE + 0.5f;

            Instantiate(constructionSite, inFront, Quaternion.identity);
            taskCompletedEvent.Set();
        }
    }
}
