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
            wrapper = new ProgrammableFunctionWrapper(taskEvent, token, commandQueue);
        }

        public IReadOnlyDictionary<string, Delegate> GetFunctions()
        {
            // Here you can expose the robot's functions in the game, use:
            // WrapOneFrame() for immediate functions and
            // WrapDeffered() for time-stretched functions (like coroutines)
            return new Dictionary<string, Delegate>()
            {
                { "moveToObject", wrapper.WrapDeffered<TypesInternal.Entity>(MoveToObject)},
                { "moveToPosition", wrapper.WrapDeffered<float, float>(MoveToPosition)},
                { "moveForward", wrapper.WrapDeffered<float>(MoveForward)},
                { "turnRight", wrapper.WrapDeffered<float>(TurnRight)},
                { "turnLeft", wrapper.WrapDeffered<float>(TurnLeft)},
                { "turnFacing", wrapper.WrapDeffered<float>(TurnFacing)},
            };
        }

        //FUNCTION IMPLEMENTATIONS
        // Implement Robot's functions here
        // functions must only return void, primitives or types in Cosmobot.Api.Types
        // functions also must have a unique name
        // (!)remember to expose functions ingame in Dictionary above
        // (!)remember to include "ManualResetEvent taskCompletedEvent" in arguments if using WrapDeffered and .Set() it at the end of action
        private void MoveToObject(ManualResetEvent taskCompletedEvent, TypesInternal.Entity obj)
        {
            if (obj.transform == null)
            {
                baseLogic.LogError("Item doesn't exist");
                taskCompletedEvent.Set();
                return;
            }

            Vector3 facingOffset = transform.position - obj.transform.position;
            StartCoroutine(MoveToPointCoroutine(taskCompletedEvent, obj.transform.position + facingOffset.normalized));
        }

        private void MoveToPosition(ManualResetEvent taskCompletedEvent, float x, float y)
        {
            Vec2 to = new Vec2(x, y);
            StartCoroutine(MoveToPointCoroutine(taskCompletedEvent, to));
        }

        private void MoveForward(ManualResetEvent taskCompletedEvent, float distance)
        {
            StartCoroutine(MoveToPointCoroutine(taskCompletedEvent, transform.position + transform.forward * distance));
        }

        private IEnumerator MoveToPointCoroutine(ManualResetEvent taskCompletedEvent, Vec3 to)
        {
            yield return TurnCoroutine(taskCompletedEvent, Vector3.SignedAngle(transform.forward, to - transform.position, Vector3.up), false);

            Vector3 dir = to - transform.position;
            while (dir.magnitude > 0.1f)
            {
                transform.position += speed * Time.deltaTime * dir.normalized;
                dir = to - transform.position;
                yield return null;
            }

            taskCompletedEvent.Set();
        }

        private void TurnRight(ManualResetEvent taskCompletedEvent, float degrees)
        {
            StartCoroutine(TurnCoroutine(taskCompletedEvent, degrees, true));
        }

        private void TurnLeft(ManualResetEvent taskCompletedEvent, float degrees)
        {
            StartCoroutine(TurnCoroutine(taskCompletedEvent, -degrees, true));
        }

        private void TurnFacing(ManualResetEvent taskCompletedEvent, float degrees)
        {
            Quaternion targetRotation = Quaternion.Euler(0, degrees, 0);
            StartCoroutine(TurnCoroutine(taskCompletedEvent, Vector3.SignedAngle(transform.forward, targetRotation * Vector3.forward, Vector3.up), true));
        }

        private IEnumerator TurnCoroutine(ManualResetEvent taskCompletedEvent, float degrees, bool completeEvent)
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
