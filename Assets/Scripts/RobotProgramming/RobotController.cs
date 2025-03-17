using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Jint;

namespace Cosmobot
{
    public class RobotController : MonoBehaviour
    {
        private Engine jsEngine;

        [TextArea(10, 20)]
        [SerializeField] private string code = @"
            MoveForward();
            TurnLeft();
            MoveForward();
            TurnRight();
        ";

        //UNITY HANGS WORKING THREAD WHEN NOT STOPPED MANUALLY !!! (shouldn't be a problem in-build where it's more controlled)(I think?)
        [SerializeField] private bool stop = false; //temporary for testing
        [SerializeField] private Transform target; //temporary for testing
        [SerializeField] private float speed = 1;

        private static SynchronizationContext _mainThreadContext; //for handing Unity functions to main thread
        private ManualResetEvent _taskCompletedEvent = new ManualResetEvent(false); //for waiting for Unity thread
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource(); //for thread killing

        void Start()
        {
            _mainThreadContext = SynchronizationContext.Current;
            Task.Run(() => jsThread(_cancellationTokenSource.Token));
        }

        void OnDestroy()
        {
            CancelAllRobotLogic();
        }

        private void CancelAllRobotLogic()
        {
            _taskCompletedEvent.Set();
            _cancellationTokenSource?.Cancel();
        }

        private void jsThread(CancellationToken token)
        {
            jsEngine = new Engine();

            jsEngine.SetValue("MoveForward", (Action)(() => {
                ExecuteOnMainThread(() => StartCoroutine(MoveToPoint(transform.position + transform.forward))); //lambda of function to be executed on Unity thread
                _taskCompletedEvent.WaitOne(); //waiting for unity thread
                _taskCompletedEvent.Reset(); //reseting waiting signal
                token.ThrowIfCancellationRequested(); //checking if thread was cancelled
            }));                                                                             //REMEMBER ABOUT _taskCompletedEvent.Set(); WHEN UNITY TASK IS DONE!!!

            jsEngine.SetValue("MoveToPoint", (Action)(() => {
                ExecuteOnMainThread(() => StartCoroutine(MoveToPoint(target.position)));
                _taskCompletedEvent.WaitOne();
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            }));

            jsEngine.SetValue("Seek", (Action)(() => {
                ExecuteOnMainThread(() => StartCoroutine(Seek()));
                _taskCompletedEvent.WaitOne();
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            }));

            jsEngine.SetValue("TurnLeft", (Action)(() => {
                ExecuteOnMainThread(TurnLeft);
                _taskCompletedEvent.WaitOne();
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            }));

            jsEngine.SetValue("TurnRight", (Action)(() => {
                ExecuteOnMainThread(TurnRight);
                _taskCompletedEvent.WaitOne();
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            }));

            try
            {
                jsEngine.Execute(code);
            }
            catch (OperationCanceledException) 
            {
                Debug.LogError("Operation was cancelled");
                jsEngine.Dispose();
                jsEngine = null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error: " + ex.Message);
                jsEngine.Dispose();
                jsEngine = null;
            }
            finally
            {
                Debug.Log("Done");
            }
        }

        public void ExecuteOnMainThread(Action action)
        {
            if(_mainThreadContext != null)
            {
                _mainThreadContext.Post(_ => action(), null);
            }
        }

        void Update()
        {
            if (stop)
            {
                CancelAllRobotLogic();
                stop = false;
            }
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

        public IEnumerator MoveToPoint(Vector3 to)
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

        public IEnumerator Seek()
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
