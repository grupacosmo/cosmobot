using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cosmobot
{
    public interface EngineLogicInterface
    {
        Dictionary<string, Delegate> GetFunctions();
        void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue);
    }
}
