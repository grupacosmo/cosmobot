using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Cosmobot
{
    public static class RobotTaskManager
    {
        public static List<Thread> TaskList = new List<Thread>();
        public static ManualResetEvent allReady = new ManualResetEvent(false);

        public static int CountTasksReady()
        {
            allReady.Reset();
            int count = 0;
            for (int i = 0; i < TaskList.Count; i++)
            {
                if (TaskList[i].IsAlive)
                {
                    count++;
                }
            }
            if (count == TaskList.Count) allReady.Set();
            return count;
        }
    }
}
