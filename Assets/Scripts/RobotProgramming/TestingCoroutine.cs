using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Jint;
using System.Reflection;
using System.Collections.Concurrent;

namespace Cosmobot
{
    public class TestingCoroutine : MonoBehaviour
    {
        public ConcurrentQueue<Action> CQ = new ConcurrentQueue<Action>();
        private Action currAction;



        void Update()
        {
            if(CQ.TryDequeue(out currAction))
            {
                currAction();
            }
        }

        void Move()
        {
            transform.position += transform.forward * 5 * Time.deltaTime;
        }
    }
}
