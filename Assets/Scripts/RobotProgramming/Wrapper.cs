using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Cosmobot
{
    public class Wrapper : MonoBehaviour
    {
        private ManualResetEvent _taskCompletedEvent;
        private CancellationToken token;
        private SynchronizationContext _mainThreadContext;

        public Wrapper(ManualResetEvent taskEvent, CancellationToken cancelToken, SynchronizationContext threadContext)
        {
            _taskCompletedEvent = taskEvent;
            token = cancelToken;
            _mainThreadContext = threadContext;
        }

        public Action Wrap(Action action)
        {
            return () => {
                ExecuteOnMainThread(action);
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            };
        }
        
        public Action<T> Wrap<T>(Action<T> action)
        {
            return (t) => {
                ExecuteOnMainThread(() => action(t));
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            };
        }

        public Action<T1, T2> Wrap<T1, T2>(Action<T1, T2> action)
        {
            return (t1, t2) => {
                ExecuteOnMainThread(() => action(t1, t2));
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            };
        }

        public Action<T1, T2, T3> Wrap<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            return (t1, t2, t3) => {
                ExecuteOnMainThread(() => action(t1, t2, t3));
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            };
        }

        public Action<T1, T2, T3, T4> Wrap<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            return (t1, t2, t3, t4) => {
                ExecuteOnMainThread(() => action(t1, t2, t3, t4));
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            };
        }

        public Action<T1, T2, T3, T4, T5> Wrap<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            return (t1, t2, t3, t4, t5) => {
                ExecuteOnMainThread(() => action(t1, t2, t3, t4, t5));
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            };
        }

        public Action<T1, T2, T3, T4, T5, T6> Wrap<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
        {
            return (t1, t2, t3, t4, t5, t6) => {
                ExecuteOnMainThread(() => action(t1, t2, t3, t4, t5, t6));
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
            };
        }

        public Func<T> Wrap<T>(Func<T> action)
        {
            return () => {
                T tr = default;
                ExecuteOnMainThread(() => { tr = action(); });
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
                return tr;
            };
        }

        public Func<T1, TR> Wrap<T1, TR>(Func<T1, TR> action)
        {
            return (t1) => {
                TR tr = default;
                ExecuteOnMainThread(() => { tr = action(t1); });
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
                return tr;
            };
        }

        public Func<T1, T2, TR> Wrap<T1, T2, TR>(Func<T1, T2, TR> action)
        {
            return (t1, t2) => {
                TR tr = default;
                ExecuteOnMainThread(() => { tr = action(t1, t2); });
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
                return tr;
            };
        }

        public Func<T1, T2, T3, TR> Wrap<T1, T2, T3, TR>(Func<T1, T2, T3, TR> action)
        {
            return (t1, t2, t3) => {
                TR tr = default;
                ExecuteOnMainThread(() => { tr = action(t1, t2, t3); });
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
                return tr;
            };
        }

        public Func<T1, T2, T3, T4, TR> Wrap<T1, T2, T3, T4, TR>(Func<T1, T2, T3, T4, TR> action)
        {
            return (t1, t2, t3, t4) => {
                TR tr = default;
                ExecuteOnMainThread(() => { tr = action(t1, t2, t3, t4); });
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
                return tr;
            };
        }

        public Func<T1, T2, T3, T4, T5, TR> Wrap<T1, T2, T3, T4, T5, TR>(Func<T1, T2, T3, T4, T5, TR> action)
        {
            return (t1, t2, t3, t4, t5) => {
                TR tr = default;
                ExecuteOnMainThread(() => { tr = action(t1, t2, t3, t4, t5); });
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
                return tr;
            };
        }

        public Func<T1, T2, T3, T4, T5, T6, TR> Wrap<T1, T2, T3, T4, T5, T6, TR>(Func<T1, T2, T3, T4, T5, T6, TR> action)
        {
            return (t1, t2, t3, t4, t5, t6) => {
                TR tr = default;
                ExecuteOnMainThread(() => { tr = action(t1, t2, t3, t4, t5, t6); });
                WaitHandle.WaitAny(new[] { _taskCompletedEvent, token.WaitHandle });
                _taskCompletedEvent.Reset();
                token.ThrowIfCancellationRequested();
                return tr;
            };
        }

        private void ExecuteOnMainThread(Action action)
        {
            _mainThreadContext.Post(_ => action(), null);
            //CQ.Enqueue(action);
        }
    }
}
