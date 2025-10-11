using Codice.CM.Common;
using Cosmobot.Api.Types;
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
    public class MovementEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ManualResetEvent taskCompletedEvent;
        private CancellationToken cancellationToken;
        private ProgrammableFunctionWrapper wrapper;

        private BaseEngineLogic baseLogic;

        [SerializeField] private float speed;
        [SerializeField] private float turningSpeed;

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
                { "MoveToObject", wrapper.Wrap<Types.Entity>(MoveToObject)},
                { "MoveToPosition", wrapper.Wrap<float, float>(MoveToPosition)},
                { "MoveForward", wrapper.Wrap<float>(MoveForward)},
                { "TurnRight", wrapper.Wrap<float>(TurnRight)},
                { "TurnLeft", wrapper.Wrap<float>(TurnLeft)},
                { "TurnFacing", wrapper.Wrap<float>(TurnFacing)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to call "taskCompletedEvent.Set();" when yours code is finished or robot will wait infinitely
        void MoveToObject(Types.Entity obj)
        {
            Vector3 facingOffset = transform.position - obj.position;
            StartCoroutine(MoveToPointCoroutine(obj.position + facingOffset.normalized));
        }

        void MoveToPosition(float x, float y)
        {
            Vec2 to = new Vec2(x, y);
            StartCoroutine(MoveToPointCoroutine(to));
        }

        void MoveForward(float distance)
        {
            StartCoroutine(MoveToPointCoroutine(transform.position + transform.forward * distance));
        }

        IEnumerator MoveToPointCoroutine(Vec3 to)
        {
            yield return TurnCoroutine(Vector3.Angle(to - transform.position, transform.forward), false);

            while (transform.position != to)
            {
                Vector3 dir = to - transform.position;
                if (dir.magnitude <= 0.1) transform.position = to;
                else transform.position += speed * Time.deltaTime * dir.normalized;
                yield return null;
            }

            taskCompletedEvent.Set();
        }

        void TurnRight(float degrees)
        {
            StartCoroutine(TurnCoroutine(degrees, true));
        }

        void TurnLeft(float degrees)
        {
            StartCoroutine(TurnCoroutine(-degrees, true));
        }

        void TurnFacing(float degrees)
        {
            Quaternion targetRotation = Quaternion.Euler(0, degrees, 0);
            StartCoroutine(TurnCoroutine(Vector3.Angle(transform.forward, targetRotation * Vector3.forward), true));
        }

        IEnumerator TurnCoroutine(float degrees, bool completeEvent)
        {
            Quaternion targetRotation = transform.rotation * Quaternion.Euler(Vector3.up *  degrees);
            
            while (transform.rotation != targetRotation)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turningSpeed * Time.deltaTime);
                yield return null;
            }

            transform.rotation = targetRotation;
            if(completeEvent) taskCompletedEvent.Set();
        }
    }
}