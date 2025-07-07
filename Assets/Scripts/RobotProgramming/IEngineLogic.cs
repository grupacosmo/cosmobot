using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Cosmobot
{
    /// <summary>
    /// Implement this interface to create a custom logic available in <see cref="Programmable"/> code
    /// </summary>
    public interface IEngineLogic
    {
        /// <summary>
        /// Provides a dictionary of functions that can be called from the robot programming environment.
        /// <code>{"jsFunctionName", FunctionToCall}</code>
        /// where
        /// <list type="bullet">
        ///   <item><c>jsFunctionName</c> is the name of the function that will be called from JavaScript code</item>
        ///   <item><c>FunctionToCall</c> is a delegate that will be executed. It is strongly recommended to use the
        ///   <see cref="ProgrammableFunctionWrapper"/> class to wrap the functions, as it handles synchronization with the Unity thread
        ///   </item>
        /// </list>
        /// </summary>
        /// <returns> <c>jsFunctionName -> FunctionToCall</c> dictionary </returns>
        IReadOnlyDictionary<string, Delegate> GetFunctions();
        
        /// <summary>
        /// Used to provide implementing class with necessary data to run programming environment.
        ///
        /// It is the best place to initialize the <see cref="ProgrammableFunctionWrapper"/> class, which will be used in <see cref="GetFunctions"/>
        /// </summary>
        void SetupThread(ManualResetEvent taskEvent, CancellationToken token, ConcurrentQueue<Action> commandQueue);
    }
}
