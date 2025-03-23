using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cosmobot
{
    [DisallowMultipleComponent]
    public class BaseRobotEngineLogic : MonoBehaviour, EngineLogicInterface
    {
        private ManualResetEvent _taskCompletedEvent;
        private CancellationToken _cancellationToken;
        private static SynchronizationContext _mainThreadContext;
        private Wrapper wrapper;

        [SerializeField] private Transform target; //temporary for testing
        [SerializeField] private float speed = 1; //temporary for testing

        public void SetupThread(ManualResetEvent taskEvent, CancellationToken token, SynchronizationContext threadContext)
        {
            _taskCompletedEvent = taskEvent;
            _cancellationToken = token;
            _mainThreadContext = threadContext;
            wrapper = new Wrapper(_taskCompletedEvent, _cancellationToken, _mainThreadContext);
        }

        public Dictionary<string, Delegate> GetFunctions()
        {
            return new Dictionary<string, Delegate>() {
                { "TurnLeft", wrapper.Wrap(TurnLeft)},
                { "TurnRight", wrapper.Wrap(TurnRight)},
                { "MoveForward", wrapper.Wrap(MoveForward)},
                { "Seek", wrapper.Wrap(Seek)},
                { "MoveToPoint", wrapper.Wrap<float, float, float>((x, y, z) => MoveToPoint(x, y, z))},
                { "GetRobotSpeed", wrapper.Wrap(GetRobotSpeed)},
                { "D5", wrapper.Wrap<float, float>((x) => d5(x))},
                { "GetRobotPosition", wrapper.Wrap(GetRobotPosition)},
                { "MoveInDirection", wrapper.Wrap<Vector3>((x) => MoveInDirection(x))},
                { "Log", wrapper.Wrap<object>((x) => log(x))}
            };
        }
            
        //ROBOT FUNCTIONS
        public float d5(float x) { _taskCompletedEvent.Set(); return x + 5; } //temporary for testing

        public void log(object x)
        {
            _taskCompletedEvent.Set();
            Debug.Log(x);
        }

        public void TurnLeft()
        {
            transform.Rotate(Vector3.up, -90);
            _taskCompletedEvent.Set();
        }

        public void TurnRight()
        {
            transform.Rotate(Vector3.up, -90);
            _taskCompletedEvent.Set();
        }

        public float GetRobotSpeed()
        {
            _taskCompletedEvent.Set();
            return speed;
        }

        public Vector3 GetRobotPosition()
        {
            _taskCompletedEvent.Set();
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
                else transform.position += dir.normalized * speed * Time.deltaTime;
                yield return null;
            }
            _taskCompletedEvent.Set();
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
            _taskCompletedEvent.Set();
        }
    }
}
