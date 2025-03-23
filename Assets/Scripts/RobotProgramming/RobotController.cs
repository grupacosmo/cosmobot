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

        [SerializeField] private Transform target; //temporary for testing
        [SerializeField] private float speed = 1; //temporary for testing

        private ManualResetEvent _taskCompletedEvent = new ManualResetEvent(false); //for waiting for Unity thread
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource(); //for thread killing
        private static SynchronizationContext _mainThreadContext; //for handing Unity functions to main thread
        private Wrapper wrapper;

        void Start()
        {
            _mainThreadContext = SynchronizationContext.Current;
            wrapper = new Wrapper(_taskCompletedEvent, _cancellationTokenSource.Token, _mainThreadContext);
            Task.Run(() => jsThread(_cancellationTokenSource.Token));
        }

        void OnDestroy()
        {
            CancelAllRobotLogic();
        }

        private void CancelAllRobotLogic()
        {
            StopAllCoroutines();
            _taskCompletedEvent.Set();
            _cancellationTokenSource?.Cancel();
        }

        private void jsThread(CancellationToken token)
        {
            jsEngine = new Engine();

            jsEngine.SetValue("TurnLeft", wrapper.Wrap(TurnLeft));
            jsEngine.SetValue("TurnRight", wrapper.Wrap(TurnRight));
            jsEngine.SetValue("MoveForward", wrapper.Wrap(MoveForward));
            jsEngine.SetValue("Seek", wrapper.Wrap(Seek));
            jsEngine.SetValue("MoveToPoint", wrapper.Wrap<float, float, float>((x, y, z) => MoveToPoint(x, y, z)));
            jsEngine.SetValue("GetRobotSpeed", wrapper.Wrap(GetRobotSpeed));
            jsEngine.SetValue("D5", wrapper.Wrap<float, float>((x) => d5(x)));
            jsEngine.SetValue("GetRobotPosition", wrapper.Wrap(GetRobotPosition));
            jsEngine.SetValue("MoveInDirection", wrapper.Wrap<Vector3>((x) => MoveInDirection(x)));
            jsEngine.SetValue("Log", wrapper.Wrap<object>((x) => log(x)));

            try
            {
                jsEngine.Execute(code);
            }
            catch (OperationCanceledException)
            {
                Debug.Log("Operation was cancelled");
                jsEngine.Dispose();
                jsEngine = null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("JS Error: " + ex.Message);
                jsEngine.Dispose();
                jsEngine = null;
            }
            finally
            {
                Debug.Log("Done");
            }
        }

        // ROBOT FUNCTIONS
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
