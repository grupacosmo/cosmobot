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
                { "moveToObject", wrapper.Wrap<TypesInternal.Entity>(MoveToObject)},
                { "moveToPosition", wrapper.Wrap<float, float>(MoveToPosition)},
                { "moveForward", wrapper.Wrap<float>(MoveForward)},
                { "turnRight", wrapper.Wrap<float>(TurnRight)},
                { "turnLeft", wrapper.Wrap<float>(TurnLeft)},
                { "turnFacing", wrapper.Wrap<float>(TurnFacing)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to call "taskCompletedEvent.Set();" when yours code is finished or robot will wait infinitely
        private void MoveToObject(TypesInternal.Entity obj)
        {
            Vector3 facingOffset = transform.position - obj.position;
            StartCoroutine(MoveToPointCoroutine(obj.position + facingOffset.normalized));
        }

        private void MoveToPosition(float x, float y)
        {
            vec2 to = new vec2(x, y);
            StartCoroutine(MoveToPointCoroutine(to));
        }

        private void MoveForward(float distance)
        {
            StartCoroutine(MoveToPointCoroutine(transform.position + transform.forward * distance));
        }

        private IEnumerator MoveToPointCoroutine(vec3 to)
        {
            yield return TurnCoroutine(Vector3.SignedAngle(transform.forward, to - transform.position, Vector3.up), false);

            Vector3 dir = to - transform.position;
            while (dir.magnitude > 0.1f)
            {
                transform.position += speed * Time.deltaTime * dir.normalized;
                dir = to - transform.position;
                yield return null;
            }

            taskCompletedEvent.Set();
        }

        private void TurnRight(float degrees)
        {
            StartCoroutine(TurnCoroutine(degrees, true));
        }

        private void TurnLeft(float degrees)
        {
            StartCoroutine(TurnCoroutine(-degrees, true));
        }

        private void TurnFacing(float degrees)
        {
            Quaternion targetRotation = Quaternion.Euler(0, degrees, 0);
            StartCoroutine(TurnCoroutine(Vector3.SignedAngle(transform.forward, targetRotation * Vector3.forward, Vector3.up), true));
        }

        private IEnumerator TurnCoroutine(float degrees, bool completeEvent)
        {
            Quaternion targetRotation = transform.rotation * Quaternion.Euler(Vector3.up *  degrees);

            while (Quaternion.Angle(transform.rotation, targetRotation) > 0.1f)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turningSpeed * Time.deltaTime);
                yield return null;
            }

            transform.rotation = targetRotation;
            if (completeEvent) taskCompletedEvent.Set();
        }
    }
}
