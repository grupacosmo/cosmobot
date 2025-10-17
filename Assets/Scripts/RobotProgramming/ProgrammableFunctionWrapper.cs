using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace Cosmobot
{
    public class ProgrammableFunctionWrapper
    {
        private ManualResetEvent taskCompletedEvent;
        private CancellationToken token;
        private ConcurrentQueue<Action> commandQueue;

        public ProgrammableFunctionWrapper(ManualResetEvent taskEvent, CancellationToken cancelToken, ConcurrentQueue<Action> commandQueue)
        {
            taskCompletedEvent = taskEvent;
            token = cancelToken;
            this.commandQueue = commandQueue;
        }

        public Action WrapDeffered(Action<ManualResetEvent> action)
        {
            return () =>
            {
                commandQueue.Enqueue(() => action(this.taskCompletedEvent));
                WaitForSync();
            };
        }

        public Action<T> WrapDeffered<T>(Action<ManualResetEvent, T> action)
        {
            return (t) =>
            {
                commandQueue.Enqueue(() => action(this.taskCompletedEvent, t));
                WaitForSync();
            };
        }

        public Action<T1, T2> WrapDeffered<T1, T2>(Action<ManualResetEvent, T1, T2> action)
        {
            return (t1, t2) =>
            {
                commandQueue.Enqueue(() => action(this.taskCompletedEvent, t1, t2));
                WaitForSync();
            };
        }

        public Action<T1, T2, T3> WrapDeffered<T1, T2, T3>(Action<ManualResetEvent, T1, T2, T3> action)
        {
            return (t1, t2, t3) =>
            {
                commandQueue.Enqueue(() => action(this.taskCompletedEvent, t1, t2, t3));
                WaitForSync();
            };
        }

        public Action<T1, T2, T3, T4> WrapDeffered<T1, T2, T3, T4>(Action<ManualResetEvent, T1, T2, T3, T4> action)
        {
            return (t1, t2, t3, t4) =>
            {
                commandQueue.Enqueue(() => action(this.taskCompletedEvent, t1, t2, t3, t4));
                WaitForSync();
            };
        }

        public Action<T1, T2, T3, T4, T5> WrapDeffered<T1, T2, T3, T4, T5>(Action<ManualResetEvent, T1, T2, T3, T4, T5> action)
        {
            return (t1, t2, t3, t4, t5) =>
            {
                commandQueue.Enqueue(() => action(this.taskCompletedEvent, t1, t2, t3, t4, t5));
                WaitForSync();
            };
        }

        public Action<T1, T2, T3, T4, T5, T6> WrapDeffered<T1, T2, T3, T4, T5, T6>(Action<ManualResetEvent, T1, T2, T3, T4, T5, T6> action)
        {
            return (t1, t2, t3, t4, t5, t6) =>
            {
                commandQueue.Enqueue(() => action(this.taskCompletedEvent, t1, t2, t3, t4, t5, t6));
                WaitForSync();
            };
        }

        public Action WrapOneFrame(Action action)
        {
            return () =>
            {
                commandQueue.Enqueue(() => { action(); taskCompletedEvent.Set(); });
                WaitForSync();
            };
        }

        public Action<T1> WrapOneFrame<T1>(Action<T1> action)
        {
            return (t1) =>
            {
                commandQueue.Enqueue(() => { action(t1); taskCompletedEvent.Set(); });
                WaitForSync();
            };
        }

        public Action<T1, T2> WrapOneFrame<T1, T2>(Action<T1, T2> action)
        {
            return (t1, t2) =>
            {
                commandQueue.Enqueue(() => { action(t1, t2); taskCompletedEvent.Set(); });
                WaitForSync();
            };
        }

        public Action<T1, T2, T3> WrapOneFrame<T1, T2, T3>(Action<T1, T2, T3> action)
        {
            return (t1, t2, t3) =>
            {
                commandQueue.Enqueue(() => { action(t1, t2, t3); taskCompletedEvent.Set(); });
                WaitForSync();
            };
        }

        public Action<T1, T2, T3, T4> WrapOneFrame<T1, T2, T3, T4>(Action<T1, T2, T3, T4> action)
        {
            return (t1, t2, t3, t4) =>
            {
                commandQueue.Enqueue(() => { action(t1, t2, t3, t4); taskCompletedEvent.Set(); });
                WaitForSync();
            };
        }

        public Action<T1, T2, T3, T4, T5> WrapOneFrame<T1, T2, T3, T4, T5>(Action<T1, T2, T3, T4, T5> action)
        {
            return (t1, t2, t3, t4, t5) =>
            {
                commandQueue.Enqueue(() => { action(t1, t2, t3, t4, t5); taskCompletedEvent.Set(); });
                WaitForSync();
            };
        }

        public Action<T1, T2, T3, T4, T5, T6> WrapOneFrame<T1, T2, T3, T4, T5, T6>(Action<T1, T2, T3, T4, T5, T6> action)
        {
            return (t1, t2, t3, t4, t5, t6) =>
            {
                commandQueue.Enqueue(() => { action(t1, t2, t3, t4, t5, t6); taskCompletedEvent.Set(); });
                WaitForSync();
            };
        }

        public Func<TR> WrapOneFrame<TR>(Func<TR> action)
        {
            return () =>
            {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(); taskCompletedEvent.Set(); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, TR> WrapOneFrame<T1, TR>(Func<T1, TR> action)
        {
            return (t1) =>
            {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1); taskCompletedEvent.Set(); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, T2, TR> WrapOneFrame<T1, T2, TR>(Func<T1, T2, TR> action)
        {
            return (t1, t2) =>
            {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1, t2); taskCompletedEvent.Set(); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, T2, T3, TR> WrapOneFrame<T1, T2, T3, TR>(Func<T1, T2, T3, TR> action)
        {
            return (t1, t2, t3) =>
            {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1, t2, t3); taskCompletedEvent.Set(); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, T2, T3, T4, TR> WrapOneFrame<T1, T2, T3, T4, TR>(Func<T1, T2, T3, T4, TR> action)
        {
            return (t1, t2, t3, t4) =>
            {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1, t2, t3, t4); taskCompletedEvent.Set(); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, T2, T3, T4, T5, TR> WrapOneFrame<T1, T2, T3, T4, T5, TR>(Func<T1, T2, T3, T4, T5, TR> action)
        {
            return (t1, t2, t3, t4, t5) =>
            {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1, t2, t3, t4, t5); taskCompletedEvent.Set(); });
                WaitForSync();
                return tr;
            };
        }

        public Func<T1, T2, T3, T4, T5, T6, TR> WrapOneFrame<T1, T2, T3, T4, T5, T6, TR>(Func<T1, T2, T3, T4, T5, T6, TR> action)
        {
            return (t1, t2, t3, t4, t5, t6) =>
            {
                TR tr = default;
                commandQueue.Enqueue(() => { tr = action(t1, t2, t3, t4, t5, t6); taskCompletedEvent.Set(); });
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
