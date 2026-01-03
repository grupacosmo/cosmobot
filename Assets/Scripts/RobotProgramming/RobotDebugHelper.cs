using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using JetBrains.Annotations;

namespace Cosmobot
{
    public static class RobotDebugHelper
    {
        private static readonly AsyncLocal<StackTrace?> capturedStackTrace = new(); 

        public static void BeforeExecutionContextCapture()
        {
            StackTrace stackTraceNow = new StackTrace(1, true);
            capturedStackTrace.Value = stackTraceNow;
        }

        public static void AfterExecutionContextCapture()
        {
            capturedStackTrace.Value = null;
        }

        
        public static IEnumerable<StackFrame> GetCurrentRobotContextStackTrace()
        {
            StackTrace currentSt = new StackTrace(1, true);
            StackFrame[] currentFrames = currentSt.GetFrames() ?? Array.Empty<StackFrame>();
            if (capturedStackTrace.Value == null)
            {
                return currentFrames;
            }

            StackFrame mergeFrame = new StackFrame("[Programmable Context Switch]",-1);
            
            StackTrace capturedSt = capturedStackTrace.Value;
            StackFrame[] capturedFrames = capturedSt.GetFrames() ?? Array.Empty<StackFrame>();
            return 
                currentFrames
                    .Concat(Enumerable.Repeat(mergeFrame, 1))
                    .Concat(capturedFrames);
        }

        public static string GetCurrentRobotContextStackTraceAsString(ProgrammableData data)
        {
            return StackFramesToText(GetCurrentRobotContextStackTrace(), data);
        }
        
        [CanBeNull]
        public static StackTrace GetCapturedStackTrace()
        {
            return capturedStackTrace.Value;
        }

        /// <summary>
        /// Returns simplified StackTrace ToString based on provided StackFrames 
        /// </summary>
        /// <param name="stackFrames">frames to stringify</param>
        /// <returns>formatted stack trace</returns>
        public static string StackFramesToText(IEnumerable<StackFrame> stackFrames, ProgrammableData data = null)
        {
            StringBuilder outText = new();
            bool ignoredJintStackFrame = false;
            
            foreach (StackFrame stackFrame in stackFrames)
            {
                if (stackFrame.GetFileLineNumber() == -1) // special case
                {
                    outText.Append(stackFrame.GetFileName());
                    outText.Append('\n');
                    continue;
                }

                MethodBase methodBase = stackFrame.GetMethod();
                
                if (methodBase?.DeclaringType?.Namespace?.StartsWith("Jint") ?? false)
                {
                    ignoredJintStackFrame = true;
                    continue;
                }
                else if (ignoredJintStackFrame)
                {
                    outText.Append("[Internal Jint Calls]\n");
                    // TODO: append Jint stack trace (try: jsEngine.Advanced.StackTrace)
                    if (data != null)
                    {
                        string jintStackTrace = data.Unity.ProgrammableComponent.engineInstance.Advanced.StackTrace;
                        outText.Append("[Begin of JS StackTrace]\n");
                        outText.Append("[JS]    ");
                        outText.Append(jintStackTrace.Replace("\n", "\n[JS]    "));
                        outText.Append("\n[End of JS Stack Trace]\n");
                    }

                    ignoredJintStackFrame = false;
                }
                
                
                if (methodBase != null)
                {
                    if (!ShouldShowInStackTrace(methodBase)) 
                        continue;

                    Type declaringType = methodBase.DeclaringType;
                    if (declaringType != null)
                    {
                        outText.Append(declaringType.Namespace);
                        outText.Append(".");
                        outText.Append(declaringType.Name);
                        outText.Append(".");
                    }
                    
                    outText.Append(methodBase.Name);
            
                    if (methodBase.IsGenericMethod)
                    {
                        outText.Append('<');
                        outText.Append(string.Join(", ", methodBase.GetGenericArguments().Select(a => a.Name)));
                        outText.Append('>');
                    }
                    

                    ParameterInfo[] parameterInfos = methodBase.GetParameters();
                    outText.Append('(');
                    outText.Append(string.Join(", ", parameterInfos.Select(a => a.ToString())));
                    outText.Append(')');
                }
                else
                {
                    outText.Append("[unknown method]");
                }

                string fileName = stackFrame.GetFileName();

                if (fileName != null)
                {
#if UNITY_EDITOR
                    if (fileName.StartsWith(UnityEngine.Application.dataPath))
                        fileName = fileName.Substring(UnityEngine.Application.dataPath.Length - "Assets".Length);
#endif
                    outText.Append("(at ");
                    outText.Append(fileName);
                    outText.Append(':');
                    outText.Append(stackFrame.GetFileLineNumber());
                    outText.Append(')');
                }
                outText.Append('\n');
            }
            
            return outText.ToString();
        }



        private static readonly Type[] ignoredTypesInStackTrace = new Type[] {
                typeof(Delegate),
                typeof(MulticastDelegate),
                typeof(RobotDebugHelper)
        };
        private static readonly string[] ignoredNamespacesInStackTrace = new string[] {
            "System.Reflection"
        };
        
        private static bool ShouldShowInStackTrace(MethodBase methodBase)
        {
            // Check Method Base
            MethodImplAttributes hiddingFlags = 
                MethodImplAttributes.AggressiveInlining
                | MethodImplAttributes.IL
                | MethodImplAttributes.InternalCall;
            
            if ((methodBase.MethodImplementationFlags & hiddingFlags) != 0)
                return false;
            
            if (methodBase.IsDefined(typeof(CompilerGeneratedAttribute), false))
                return false;
            
            // Check declaring type
            Type declaringType = methodBase.DeclaringType;
            if (declaringType == null) return true; // true coz there is no declaring type to check
            
            if (declaringType.IsDefined(typeof(CompilerGeneratedAttribute), false))
                return false;

            if (ignoredTypesInStackTrace.Contains(declaringType)) 
                return false;
            
            // Check Namespace
            if (ignoredNamespacesInStackTrace.Contains(declaringType.Namespace))
                return false;
            
            return true;
        }
    }
}
