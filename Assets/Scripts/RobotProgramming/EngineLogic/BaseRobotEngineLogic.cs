using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cosmobot.Api.Types;
using UnityEngine;

namespace Cosmobot
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Programmable))]
    public class BaseRobotEngineLogic : MonoBehaviour, IEngineLogic
    {
        private ManualResetEvent taskCompletedEvent;
        private CancellationToken cancellationToken;
        private ProgrammableFunctionWrapper wrapper;

        [SerializeField] private Transform target; //temporary for testing
        [SerializeField] private float speed = 1; //temporary for testing

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue)
        {
            taskCompletedEvent = taskEvent;
            cancellationToken = token;
            wrapper = new ProgrammableFunctionWrapper(taskCompletedEvent, cancellationToken, commandQueue);
        }

        public IReadOnlyDictionary<string, Delegate> GetFunctions()
        {
            return new Dictionary<string, Delegate>() { // TODO: implement in #91 - Robots API 
                { "TurnLeft", wrapper.Wrap(TurnLeft)},
                { "TurnRight", wrapper.Wrap(TurnRight)},
                { "MoveForward", wrapper.Wrap(MoveForward)},
                { "Seek", wrapper.Wrap(Seek)},
                { "MoveToPoint", wrapper.Wrap<float, float, float>(MoveToPoint)},
                { "GetRobotSpeed", wrapper.Wrap(GetRobotSpeed)},
                { "GetRobotPosition", wrapper.Wrap(GetRobotPosition)},
                { "MoveInDirection", wrapper.Wrap<Vector3>(MoveInDirection)}
            };
        }

        //ROBOT FUNCTIONS
        public void TurnLeft()
        {
            transform.Rotate(Vector3.up, -90);
            taskCompletedEvent.Set();
        }

        public void TurnRight()
        {
            transform.Rotate(Vector3.up, 90);
            taskCompletedEvent.Set();
        }

        public float GetRobotSpeed()
        {
            taskCompletedEvent.Set();
            return speed;
        }

        public Vec3 GetRobotPosition()
        {
            taskCompletedEvent.Set();
            return transform.position;
        }

        public void MoveForward()
        {
            StartCoroutine(MoveToPointCoroutine(transform.position + transform.forward));
        }

        public void MoveToPoint(float x, float y, float z)
        {
            StartCoroutine(MoveToPointCoroutine(new Vector3(x, y, z)));
        }

        public void MoveInDirection(Vector3 dir)
        {
            StartCoroutine(MoveToPointCoroutine(transform.position + dir));
        }

        public IEnumerator MoveToPointCoroutine(Vector3 to)
        {
            while (transform.position != to)
            {
                Vector3 dir = to - transform.position;
                if (dir.magnitude <= 0.1) transform.position = to;
                else transform.position += speed * Time.deltaTime * dir.normalized;
                yield return null;
            }

            taskCompletedEvent.Set();
        }

        public void Seek()
        {
            StartCoroutine(SeekCoroutine());
        }

        public IEnumerator SeekCoroutine()
        {
            while (transform.position != target.position)
            {
                Vector3 dir = target.position - transform.position;
                if (dir.magnitude <= 0.1) transform.position = target.position;
                else transform.position += dir.normalized * speed * Time.deltaTime;
                yield return null;
            }
            taskCompletedEvent.Set();
        }

        /*
        issue: #91 - Robots API
        
        item find()
        List<item> findAll()

        pickup(item?)
        drop()

        collect(item?)
        insert()

        dig()
        build(building)

        goto(object)
        goToPosition(vec3)
        moveForward(float)
        moveBackwards(float)
        TurnLeft(float)
        TurnRight(float)

        wait(float)

        useBuilding(building?)

        attack(enemy?)

        log(string)
        logWarning(string)
        logError(string)

        dance()

        repair(object?)
        disassemble(object?)

        activate(object?)
        deactivate(object?)

        building.setParameter?
        // if object is too far return error

        */
    }
}
