using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Jint;
using System.Reflection;
using System.Collections.Concurrent;
using Codice.CM.Common;

namespace Cosmobot
{
    public class TestingThread : MonoBehaviour
    {
        Thread task;
        TestingCoroutine a;
        Engine engine;
        private ManualResetEvent _taskCompletedEvent; //for waiting for Unity thread
        private CancellationTokenSource _cancellationTokenSource; //for thread killing
        private static SynchronizationContext _mainThreadContext; //for handing Unity functions to main thread

        void Start()
        {
            a = gameObject.GetComponent<TestingCoroutine>();
            _taskCompletedEvent = new ManualResetEvent(false);
            engine = new Engine();
            engine.Execute("let a = 2+2;");
            task = new Thread(() => Thead());
            task.IsBackground = true;
            task.Start(); 
        }

        void Thead()
        {
            for (int i = 0; i < 100; i++)
            {
                a.CQ.Enqueue(() => { engine.Execute("a = 2+2;"); StartCoroutine(Move()); });
                _taskCompletedEvent.WaitOne();
                _taskCompletedEvent.Reset();
                a.CQ.Enqueue(() => { engine.Execute("a = 2+2;"); TurnLeft(); });
                _taskCompletedEvent.WaitOne();
                _taskCompletedEvent.Reset();
            }
        }

        IEnumerator Move()
        {
            Vector3 to = transform.position + transform.forward;
            while (transform.position != to)
            {
                Vector3 dir = to - transform.position;
                if (dir.magnitude <= 0.1) transform.position = to;
                else transform.position += dir.normalized * 5 * Time.deltaTime;
                yield return null;
            }
            _taskCompletedEvent.Set();
        }

        public void TurnLeft()
        {
            transform.Rotate(Vector3.up, -90);
            _taskCompletedEvent.Set();
        }
    }
}
