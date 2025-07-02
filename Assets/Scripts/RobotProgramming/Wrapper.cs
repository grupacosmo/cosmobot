using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Cosmobot
{
    public class Wrapper
    {
        private ManualResetEvent taskCompletedEvent;
        private CancellationToken token;
        private ConcurrentQueue<Action> commandQueue;

        public Wrapper(ManualResetEvent taskEvent, CancellationToken cancelToken, ConcurrentQueue<Action> commandQueue)
        {
            taskCompletedEvent = taskEvent;
            token = cancelToken;
            this.commandQueue = commandQueue;
        }

        public Action Wrap(Action action)
        {
            return () => {
                commandQueue.Enqueue(action);
                WaitForSync();
            };
        }
        
        public Action<T> Wrap<T>(Action<T> action)
        {
            return (t) => {
                commandQueue.Enqueue(() => action(t));
                WaitForSync();
            };
        }

        public Action<T1, T2> Wrap<T1, T2>(Action<T1, T2> action)
        {
            return (t1, t2) => {
                commandQueue.Enqueue(() => action(t1, t2));
                WaitForSync();
            };
        }

        public Action<T1, T2, T3> Wrap<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            return (t1, t2, t3) => {
                commandQueue.Enqueue(() => action(t1, t2, t3));
                WaitForSync();
            };
        }

        public Action<T1, T2, T3, T4> Wrap<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            return (t1, t2, t3, t4) => {
                commandQueue.Enqueue(() => action(t1, t2, t3, t4));
                WaitForSync();
            };
        }

        public Action<T1, T2, T3, T4, T5> Wrap<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            return (t1, t2, t3, t4, t5) => {
                commandQueue.Enqueue(() => action(t1, t2, t3, t4, t5));
                WaitForSync();
            };
        }

        public Action<T1, T2, T3, T4, T5, T6> Wrap<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
        {
            return (t1, t2, t3, t4, t5, t6) => {
                commandQueue.Enqueue(() => action(t1, t2, t3, t4, t5, t6));
                WaitForSync();
            };
        }

        public Func<T> Wrap<T>(Func<T> action)
        {
            return () => {
                T tr = default;
                commandQueue.Enqueue(() => { tr = action(); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, TR> Wrap<T1, TR>(Func<T1, TR> action)
        {
            return (t1) => {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, T2, TR> Wrap<T1, T2, TR>(Func<T1, T2, TR> action)
        {
            return (t1, t2) => {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1, t2); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, T2, T3, TR> Wrap<T1, T2, T3, TR>(Func<T1, T2, T3, TR> action)
        {
            return (t1, t2, t3) => {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1, t2, t3); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, T2, T3, T4, TR> Wrap<T1, T2, T3, T4, TR>(Func<T1, T2, T3, T4, TR> action)
        {
            return (t1, t2, t3, t4) => {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1, t2, t3, t4); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, T2, T3, T4, T5, TR> Wrap<T1, T2, T3, T4, T5, TR>(Func<T1, T2, T3, T4, T5, TR> action)
        {
            return (t1, t2, t3, t4, t5) => {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1, t2, t3, t4, t5); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, T2, T3, T4, T5, T6, TR> Wrap<T1, T2, T3, T4, T5, T6, TR>(Func<T1, T2, T3, T4, T5, T6, TR> action)
        {
            return (t1, t2, t3, t4, t5, t6) => {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1, t2, t3, t4, t5, t6); });
                WaitForSync();
                return tr;
            };
        }
        private void WaitForSync()
        {
            WaitHandle.WaitAny(new[] { taskCompletedEvent, token.WaitHandle });
            taskCompletedEvent.Reset();
            token.ThrowIfCancellationRequested();
        }
    }
}
