using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Jint;
using Jint.Runtime.Debugger;
using UnityEditor;
#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UIElements;
#endif


namespace Cosmobot
{
    public class SimpleTest : MonoBehaviour
    {

        public Transform testTarget;

        [TextArea(10, 20)]
        public string code = @"
            moveForward();
            moveLeft();
            moveRight();
        ";

        private Engine engine;

        bool insideInternalOperation = false;

        private ConcurrentQueue<Action> unityActionsQueue = new ConcurrentQueue<Action>();
        private int lastDeltaTimeMilis = 3000/60;


        void Start()
        {
            new Thread(() => {
                engine = new Engine(options => {
                    // options
                    //     .DebugMode()
                    //     .DebuggerStatementHandling(DebuggerStatementHandling.Script)
                    //     .InitialStepMode(StepMode.Out);
                    options.Constraint(new UnityConstraint(this));
                });
                // engine.Debugger.Step += EngineStep;
                engine.SetValue("findTarget", Wrap(FindTarget));
                engine.SetValue("moveToPoint", Wrap<Vector3>(MoveToPoint));
                engine.SetValue("moveForward", Wrap(MoveForward));
                engine.SetValue("moveLeft", Wrap(MoveLeft));
                engine.SetValue("moveRight", Wrap(MoveRight));
                
                engine.Execute(code);
                Debug.Log("done");
            }).Start();
        }

        class UnityConstraint : Constraint
        {
            private Component parent;

            public UnityConstraint(Component parent)
            {
                this.parent = parent;
            }
            

            public override void Check()
            {
                if(!parent) 
                {
                    throw new OperationCanceledException("Unity object destroyed");
                }

                // #if UNITY_EDITOR
                // if(EditorApplication.isPlaying) //only Unity Thread -.-
                // {
                //     throw new OperationCanceledException("Unity object destroyed");
                // }
                // #endif
            }

            public override void Reset()
            {
            }
        }

        void OnDestroy()
        {
            engine.Dispose();
        }
        
        void Update()
        {
            while (unityActionsQueue.TryDequeue(out Action action))
            {
                action();
                // based on current code structure, there will be at most only one action in the queue
                // so Interlocked will be called at most once per frame
                Interlocked.Exchange(ref lastDeltaTimeMilis, (int)(Time.deltaTime*1000));
            }
        }

        StepMode EngineStep(object sender, DebugInformation e) 
        {
           Task.WaitAll(EngineStepHandler(sender, e));
           return StepMode.Out;
        }

        async Task EngineStepHandler(object sender, DebugInformation e)
        {
            if (!insideInternalOperation)
            {
                return;
            }


            while (insideInternalOperation)
            {
                int currentDeltaTimeMilis = Volatile.Read(ref lastDeltaTimeMilis);
                await Task.Delay(currentDeltaTimeMilis);
            }
        }


        void PreInternalOperation()
        {
            insideInternalOperation = true;
            Debug.Log("PreInternalOperation");
        }

        void PostInternalOperation() 
        {
            insideInternalOperation = false;
            Debug.Log("PostInternalOperation");
        }

        async Task WaitForInternalOperationFinishAsync()
        {
            while (insideInternalOperation)
            {
                int currentDeltaTimeMilis = Volatile.Read(ref lastDeltaTimeMilis);
                await Task.Delay(currentDeltaTimeMilis);
            }
        }

        void WaitForInternalOperationFinish()
        {
            Task.WaitAll(WaitForInternalOperationFinishAsync());
        }

        void MoveForward(TimeConsumingFunctionFinisher done)
        {
            Transform t = transform;
            Move(t.position, t.position + t.forward * 1f, done);
        }

        void MoveRight(TimeConsumingFunctionFinisher done)
        {
            Transform t = transform;
            Move(t.position, t.position + t.right * 1f, done);
        }

        void MoveLeft(TimeConsumingFunctionFinisher done)
        {
            Transform t = transform;
            Move(t.position, t.position + t.right * -1f, done);
        }

        void MoveToPoint(TimeConsumingFunctionFinisher done, Vector3 point)
        {
            Vector3 direction = point - transform.position;
            transform.position += direction.normalized * Time.deltaTime;
            done();
        }

        Vector3 FindTarget()
        {
            return testTarget.position;
        }

        void Move(Vector3 from, Vector3 to, TimeConsumingFunctionFinisher done)
        {
            StartCoroutine(MoveCoroutine(from, to, done));
        }

        IEnumerator MoveCoroutine(Vector3 from, Vector3 to, TimeConsumingFunctionFinisher done) 
        {
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime;
                transform.position = Vector3.Lerp(from, to, t);
                yield return null;
            }
            done();
        }

        // struct TimeCosumingResult<T>
        // {
        //     public T result;
        //     public float time;
        // }


        private delegate void TimeConsumingFunctionFinisher();
        // private delegate void TimeConsumingFunctionFinisher(); 
        // private delegate T TimeConsumingFunctionFinisher<T>(T result); 

        void DoInUnityThread(Action action)
        {
            unityActionsQueue.Enqueue(action);
        }

        void DoInUnityThreadAndWait(Action action)
        {
            unityActionsQueue.Enqueue(() => {
                action();
                PostInternalOperation();
            });
            WaitForInternalOperationFinish();
        }

        TReturn DoInUnityThreadAndWait<TReturn>(Func<TReturn> action)
        {
            TReturn r = default(TReturn);
            unityActionsQueue.Enqueue(() => {
                r = action();
                PostInternalOperation();
            });
            WaitForInternalOperationFinish();
            return r;
        }

        Func<T> Wrap<T>(Func<T> action)
        {
            return () => {
                PreInternalOperation();
                T t = DoInUnityThreadAndWait(() => action());
                // `PostInternalOperation()` is called in `DoInUnityThreadAndWait` method
                return t;
            };
        }

        Action Wrap(Action action)
        {
            return () => {
                PreInternalOperation();
                DoInUnityThreadAndWait(() => action());
                // PostInternalOperation();
            };
        }

        Action<T> Wrap<T> (Action<T> action)
        {
            return (a) => {
                PreInternalOperation();
                DoInUnityThreadAndWait(() => action(a));
                // PostInternalOperation();
            };
        }

        Action Wrap (Action<TimeConsumingFunctionFinisher> action)
        {
            return () => {
                PreInternalOperation();
                DoInUnityThread(() => action(PostInternalOperation));
                WaitForInternalOperationFinish();
            };
        }

        Action<T> Wrap<T> (Action<TimeConsumingFunctionFinisher, T> action)
        {
            return (a) => {
                PreInternalOperation();
                DoInUnityThread(() => action(PostInternalOperation, a));
                WaitForInternalOperationFinish();
            };
        }
    }
}
/*

engine.SetValue("add", (a, b) => { preOperation(); r = add(a, b); postOperation(); return r;});
engine.SetValue("sin", (a) => { preOperation(); r = sin(a); postOperation(); return r;});
engine.SetValue("print", (a) => { preOperation(); print(a); postOperation();});



engine.SetValue("add", wrap(add));
engine.SetValue("sin", wrap(sin));
engine.SetValue("print", wrap(print));

*/