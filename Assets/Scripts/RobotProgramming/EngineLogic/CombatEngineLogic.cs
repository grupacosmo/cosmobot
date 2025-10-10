using Cosmobot.Api.Types;
using Cosmobot.Entity;
using Cosmobot.ItemSystem;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cosmobot.Api
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BaseEngineLogic))]
    public class CombatEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ManualResetEvent taskCompletedEvent;
        private CancellationToken cancellationToken;
        private ProgrammableFunctionWrapper wrapper;

        private BaseEngineLogic baseLogic;
        [SerializeField] private float detectionRange;
        [SerializeField] private float attackRange;
        [SerializeField] private float demage;

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
                //{ "ExampleFunctionARG", wrapper.Wrap(ExampleFunctionARG)},
                //{ ... },
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to call "taskCompletedEvent.Set();" when yours code is finished or robot will wait infinitely
        Hostile? FindEnemy(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, detectionRange, 1 << GlobalConstants.ENEMY_LAYER);

            if (objects.Length == 0)
            {
                taskCompletedEvent.Set();
                return null;
            }

            if (string.IsNullOrEmpty(type))
            {
                Enemy enemyComponent = objects[0].GetComponent<Enemy>();
                if (enemyComponent == null)
                {
                    taskCompletedEvent.Set();
                    return null;
                }

                taskCompletedEvent.Set();
                Vector2 pos = new Vector2(objects[0].transform.position.x, objects[0].transform.position.z);
                return new Hostile(enemyComponent, pos);
            }

            foreach (Collider collider in objects)
            {
                Enemy enemyComponent = collider.GetComponent<Enemy>();

                if (enemyComponent == null)
                    continue;

                /*if (enemyComponent. == type)
                {
                    taskCompletedEvent.Set();
                    Vector2 pos = new Vector2(objects[0].transform.position.x, objects[0].transform.position.z);
                    return new Hostile(enemyComponent, pos);
                }*/
            }

            taskCompletedEvent.Set();
            return null;
        }

        Hostile? FindClosestItem(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, detectionRange, 1 << GlobalConstants.ENEMY_LAYER);

            if (objects.Length == 0)
            {
                taskCompletedEvent.Set();
                return null;
            }

            Hostile? closest = null;
            float distance = detectionRange;

            foreach (Collider collider in objects)
            {
                Enemy enemyComponent = collider.GetComponent<Enemy>();

                if (enemyComponent == null)
                    continue;

                /*
                if (enemyComponent == type || string.IsNullOrEmpty(type))
                {
                    float currentDist = Vector3.Distance(transform.position, collider.transform.position);
                    if (currentDist < distance)
                    {
                        distance = currentDist;
                        Vector2 pos = new Vector2(collider.transform.position.x, collider.transform.position.z);
                        closest = new Hostile(enemyComponent, pos);
                    }
                }*/
            }

            taskCompletedEvent.Set();
            return closest;
        }

        List<Hostile> FindAllItems(string type = "")
        {
            Collider[] objects = Physics.OverlapSphere(gameObject.transform.position, detectionRange, 1 << GlobalConstants.ENEMY_LAYER);
            List<Hostile> hostiles = new List<Hostile>();

            foreach (Collider collider in objects)
            {
                Enemy enemyComponent = collider.GetComponent<Enemy>();

                if (enemyComponent == null)
                    continue;
                /*
                if (enemyComponent == type || string.IsNullOrEmpty(type))
                {
                    Vector2 pos = new Vector2(collider.transform.position.x, collider.transform.position.z);
                    hostiles.Add(new Hostile(enemyComponent, pos));
                }*/
            }

            taskCompletedEvent.Set();
            return hostiles;
        }
    }
}